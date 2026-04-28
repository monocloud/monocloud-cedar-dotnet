---
"@monocloud/monocloud-cedar-dotnet": patch
---

**Initial Release of the MonoCloud Cedar SDK for .NET**

This is the first public release of the MonoCloud Cedar SDK, bringing the power of the AWS Cedar Policy Engine natively to .NET applications via high-performance Rust FFI bindings. 

### Highlights
- **Native Cedar Execution**: Author, evaluate, and validate Cedar policies completely within your .NET process—no JVM or external microservices required.
- **Cross-Platform Support**: Prebuilt native libraries included automatically via NuGet for:
  - Windows (`win-x64`, `win-arm64`)
  - Linux (`linux-x64`, `linux-arm64`)
  - macOS (`osx-x64`, `osx-arm64`)
- **API Parity**: This SDK mirrors the official upstream `cedar-policy/cedar-java` bindings, providing full support for Entities, Policies, Templates, Schema validation, and Partial Evaluation.
- **Seamless Integration**: Seamless translation between Cedar's internal entity representations, JSON, and native C# types for rapid authorization integration.
