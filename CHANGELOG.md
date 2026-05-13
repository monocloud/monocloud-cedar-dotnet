# @monocloud/monocloud-cedar-dotnet

## 0.1.0

### Minor Changes

- 016c31f: Add a size-optimized [profile.release] to MonoCloud.Cedar.FFI:
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

## 0.0.3

### Patch Changes

- b5c6c53: Added Support for .NET 8/9/netstandard20

## 0.0.2

### Patch Changes

- 339c2ce: **Initial Release of the MonoCloud Cedar SDK for .NET**

  This is the first public release of the MonoCloud Cedar SDK, bringing the power of the AWS Cedar Policy Engine natively to .NET applications via high-performance Rust FFI bindings.

  ### Highlights

  - **Native Cedar Execution**: Author, evaluate, and validate Cedar policies completely within your .NET process—no JVM or external microservices required.
  - **Cross-Platform Support**: Prebuilt native libraries included automatically via NuGet for:
    - Windows (`win-x64`, `win-arm64`)
    - Linux (`linux-x64`, `linux-arm64`)
    - macOS (`osx-x64`, `osx-arm64`)
  - **API Parity**: This SDK mirrors the official upstream `cedar-policy/cedar-java` bindings, providing full support for Entities, Policies, Templates, Schema validation, and Partial Evaluation.
  - **Seamless Integration**: Seamless translation between Cedar's internal entity representations, JSON, and native C# types for rapid authorization integration.
