---
"@monocloud/monocloud-cedar-dotnet": minor
---

Add a size-optimized [profile.release] to MonoCloud.Cedar.FFI:
opt-level = "z", lto = "fat", codegen-units = 1, strip = "symbols",
panic = "abort". Drop default features from miette since the FFI
boundary doesn't need fancy terminal rendering, backtraces, or
color support.

Rebuild all six native runtimes from a single toolchain pass so the
artifacts are consistent. Notable changes:

- osx-arm64: 12 MB → ~2 MB (cargo's default macOS release build was
  embedding full DWARF debug info into the dylib)
- osx-x64: replaced a 4 KB stub with a real build
- linux-arm64: replaced a 71 KB stub with a real 3.25 MB build
- linux-x64 / win-x64 / win-arm64: rebuilt with matching settings

Net effect: the NuGet package no longer carries the 12 MB unstripped
macOS binary and ships consistent, fully optimized artifacts across
all platforms.
