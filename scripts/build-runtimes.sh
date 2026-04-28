#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
FFI_DIR="$ROOT_DIR/MonoCloud.Cedar.FFI"
RUNTIMES_DIR="$ROOT_DIR/MonoCloud.Cedar/Runtimes"

PROFILE="release"
BUILD_ALL=false
CLEAN=false
USE_CROSS=false
USE_DOCKER=false
INSTALL_TARGETS=false
FEATURES=()
TARGET_RIDS=()

WINDOWS_TARGET="${WINDOWS_TARGET:-x86_64-pc-windows-gnu}"
DOCKER_RUST_IMAGE="${DOCKER_RUST_IMAGE:-rust:1-bookworm}"
DOCKER_WINDOWS_ARM64_IMAGE="${DOCKER_WINDOWS_ARM64_IMAGE:-monocloud-cedar-rust-llvm-mingw:latest}"

usage() {
  cat <<'USAGE'
Usage:
  scripts/build-runtimes.sh [options]

Options:
  --all                    Build all configured runtime IDs.
  --target <rid>           Build one runtime ID. Can be repeated.
                          Supported: win-x64, win-arm64, linux-x64, linux-arm64,
                                     osx-x64, osx-arm64.
  --profile <name>         Cargo profile to build. Defaults to release.
  --features <features>    Cargo feature list to pass through.
  --use-cross              Use cross instead of cargo for compilation.
  --use-docker             Use a Rust Docker image for Linux/Windows builds.
  --install-targets        Run rustup target add for missing Rust targets.
  --clean                  Remove MonoCloud.Cedar/Runtimes before copying outputs.
  -h, --help               Show this help.

Environment:
  WINDOWS_TARGET           Rust target used for win-x64. Defaults to x86_64-pc-windows-gnu.
  DOCKER_RUST_IMAGE        Rust Docker image used by --use-docker. Defaults to rust:1-bookworm.
  DOCKER_WINDOWS_ARM64_IMAGE
                          Docker image used by --use-docker for win-arm64.
                          Defaults to monocloud-cedar-rust-llvm-mingw:latest.

Examples:
  scripts/build-runtimes.sh --all --use-docker --install-targets
  scripts/build-runtimes.sh --target osx-arm64
  WINDOWS_TARGET=x86_64-pc-windows-msvc scripts/build-runtimes.sh --target win-x64
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --all)
      BUILD_ALL=true
      shift
      ;;
    --target)
      [[ $# -ge 2 ]] || { echo "error: --target requires a runtime ID" >&2; exit 1; }
      TARGET_RIDS+=("$2")
      shift 2
      ;;
    --profile)
      [[ $# -ge 2 ]] || { echo "error: --profile requires a value" >&2; exit 1; }
      PROFILE="$2"
      shift 2
      ;;
    --features)
      [[ $# -ge 2 ]] || { echo "error: --features requires a value" >&2; exit 1; }
      FEATURES+=(--features "$2")
      shift 2
      ;;
    --use-cross)
      USE_CROSS=true
      shift
      ;;
    --use-docker)
      USE_DOCKER=true
      shift
      ;;
    --install-targets)
      INSTALL_TARGETS=true
      shift
      ;;
    --clean)
      CLEAN=true
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "error: unknown option: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

HOST_OS="$(uname -s)"
HOST_RID=""
case "$HOST_OS-$(uname -m)" in
  Darwin-arm64) HOST_RID="osx-arm64" ;;
  Darwin-x86_64) HOST_RID="osx-x64" ;;
  Linux-x86_64) HOST_RID="linux-x64" ;;
  MINGW*-x86_64|MSYS*-x86_64|CYGWIN*-x86_64) HOST_RID="win-x64" ;;
esac

if [[ "$BUILD_ALL" == false && ${#TARGET_RIDS[@]} -eq 0 ]]; then
  if [[ -z "$HOST_RID" ]]; then
    echo "error: could not infer host runtime ID; pass --target or --all" >&2
    exit 1
  fi
  TARGET_RIDS=("$HOST_RID")
fi

if [[ "$BUILD_ALL" == true ]]; then
  TARGET_RIDS=(win-x64 win-arm64 linux-x64 linux-arm64 osx-x64 osx-arm64)
fi

if [[ "$CLEAN" == true ]]; then
  rm -rf "$RUNTIMES_DIR"
fi

mkdir -p "$RUNTIMES_DIR"

cargo_profile_args=()
target_dir_profile="$PROFILE"
if [[ "$PROFILE" == "release" ]]; then
  cargo_profile_args+=(--release)
else
  cargo_profile_args+=(--profile "$PROFILE")
fi

resolve_target() {
  case "$1" in
    win-x64) echo "$WINDOWS_TARGET|cedar_c_ffi.dll|cedar_dotnet_ffi.dll" ;;
    win-arm64) echo "aarch64-pc-windows-gnullvm|cedar_c_ffi.dll|cedar_dotnet_ffi.dll" ;;
    linux-x64) echo "x86_64-unknown-linux-gnu|libcedar_c_ffi.so|libcedar_dotnet_ffi.so" ;;
    linux-arm64) echo "aarch64-unknown-linux-gnu|libcedar_c_ffi.so|libcedar_dotnet_ffi.so" ;;
    osx-x64) echo "x86_64-apple-darwin|libcedar_c_ffi.dylib|libcedar_dotnet_ffi.dylib" ;;
    osx-arm64) echo "aarch64-apple-darwin|libcedar_c_ffi.dylib|libcedar_dotnet_ffi.dylib" ;;
    *)
      echo "error: unsupported runtime ID: $1" >&2
      return 1
      ;;
  esac
}

builder_for_target() {
  local rust_target="$1"

  if [[ "$USE_DOCKER" == true && "$rust_target" != *-apple-darwin ]]; then
    echo "docker"
  elif [[ "$USE_CROSS" == true && "$rust_target" != *-apple-darwin ]]; then
    echo "cross"
  else
    echo "cargo"
  fi
}

docker_setup_for_target() {
  case "$1" in
    x86_64-unknown-linux-gnu) echo "" ;;
    aarch64-unknown-linux-gnu) echo "apt-get update && apt-get install -y --no-install-recommends gcc-aarch64-linux-gnu libc6-dev-arm64-cross && /usr/local/cargo/bin/rustup target add aarch64-unknown-linux-gnu && export CARGO_TARGET_AARCH64_UNKNOWN_LINUX_GNU_LINKER=aarch64-linux-gnu-gcc &&" ;;
    x86_64-pc-windows-gnu) echo "apt-get update && apt-get install -y --no-install-recommends gcc-mingw-w64-x86-64 && /usr/local/cargo/bin/rustup target add x86_64-pc-windows-gnu &&" ;;
    aarch64-pc-windows-gnullvm) echo "export CC_aarch64_pc_windows_gnullvm=/opt/llvm-mingw/bin/aarch64-w64-mingw32-clang CARGO_TARGET_AARCH64_PC_WINDOWS_GNULLVM_LINKER=/opt/llvm-mingw/bin/aarch64-w64-mingw32-clang &&" ;;
    *)
      echo "error: no Docker setup configured for Rust target: $1" >&2
      return 1
      ;;
  esac
}

docker_image_for_target() {
  case "$1" in
    aarch64-pc-windows-gnullvm) echo "$DOCKER_WINDOWS_ARM64_IMAGE" ;;
    *) echo "$DOCKER_RUST_IMAGE" ;;
  esac
}

docker_platform_args_for_target() {
  case "$1" in
    aarch64-pc-windows-gnullvm) ;;
    *) echo "linux/amd64" ;;
  esac
}

ensure_docker_image_for_target() {
  case "$1" in
    aarch64-pc-windows-gnullvm)
      if ! docker image inspect "$DOCKER_WINDOWS_ARM64_IMAGE" >/dev/null 2>&1; then
        docker build \
          -f "$ROOT_DIR/scripts/Dockerfile.windows-arm64" \
          -t "$DOCKER_WINDOWS_ARM64_IMAGE" \
          "$ROOT_DIR"
      fi
      ;;
  esac
}

for rid in "${TARGET_RIDS[@]}"; do
  IFS="|" read -r rust_target source_file dest_file < <(resolve_target "$rid")
  builder="$(builder_for_target "$rust_target")"

  if [[ "$rust_target" == *-apple-darwin && "$HOST_OS" != "Darwin" ]]; then
    echo "error: Apple targets require macOS and the Apple linker: $rust_target" >&2
    exit 1
  fi

  if ! command -v "$builder" >/dev/null 2>&1; then
    echo "error: '$builder' is not installed or not on PATH" >&2
    exit 1
  fi

  if [[ "$builder" == "cross" || "$builder" == "docker" ]]; then
    :
  elif [[ "$INSTALL_TARGETS" == true ]]; then
    rustup target add "$rust_target"
  elif command -v rustup >/dev/null 2>&1 && ! rustup target list --installed | grep -qx "$rust_target"; then
    echo "error: Rust target '$rust_target' is not installed." >&2
    echo "       Install it with 'rustup target add $rust_target' or rerun with --install-targets." >&2
    exit 1
  fi

  echo "building $rid ($rust_target)"
  (
    cd "$FFI_DIR"
    build_args=(build --target "$rust_target")
    build_args+=("${cargo_profile_args[@]}")
    if [[ ${#FEATURES[@]} -gt 0 ]]; then
      build_args+=("${FEATURES[@]}")
    fi

    if [[ "$builder" == "docker" ]]; then
      ensure_docker_image_for_target "$rust_target"
      docker_image="$(docker_image_for_target "$rust_target")"
      docker_platform="$(docker_platform_args_for_target "$rust_target")"
      docker_setup="$(docker_setup_for_target "$rust_target")"
      docker_command="$docker_setup /usr/local/cargo/bin/cargo ${build_args[*]}"
      if [[ -n "$docker_platform" ]]; then
        docker run --rm --platform "$docker_platform" \
          -v "$ROOT_DIR:/work" \
          -w /work/MonoCloud.Cedar.FFI \
          "$docker_image" \
          bash -c "$docker_command"
      else
        docker run --rm \
          -v "$ROOT_DIR:/work" \
          -w /work/MonoCloud.Cedar.FFI \
          "$docker_image" \
          bash -c "$docker_command"
      fi
    else
      "$builder" "${build_args[@]}"
    fi
  )

  source_path="$FFI_DIR/target/$rust_target/$target_dir_profile/$source_file"
  dest_dir="$RUNTIMES_DIR/$rid/native"
  dest_path="$dest_dir/$dest_file"

  if [[ ! -f "$source_path" ]]; then
    echo "error: expected build output was not found: $source_path" >&2
    exit 1
  fi

  mkdir -p "$dest_dir"
  cp "$source_path" "$dest_path"
  echo "copied $dest_path"
done
