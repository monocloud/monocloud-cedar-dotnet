<div align="center">
  <a href="https://www.monocloud.com?utm_source=github&utm_medium=cedar_dotnet" target="_blank" rel="noopener noreferrer">
    <picture>
      <img src="https://raw.githubusercontent.com/monocloud/monocloud-cedar-dotnet/refs/heads/main/banner.svg" alt="MonoCloud Banner">
    </picture>
  </a>
  <div align="right">
    <a href="https://www.nuget.org/packages/MonoCloud.Cedar" target="_blank">
      <img src="https://img.shields.io/nuget/v/MonoCloud.Cedar" alt="NuGet" />
    </a>
    <a href="https://www.apache.org/licenses/LICENSE-2.0">
      <img src="https://img.shields.io/:license-Apache--2.0-blue.svg?style=flat" alt="License: Apache-2.0" />
    </a>
    <a href="https://github.com/monocloud/monocloud-cedar-dotnet/actions/workflows/build.yml">
      <img src="https://github.com/monocloud/monocloud-cedar-dotnet/actions/workflows/build.yml/badge.svg" alt="Build Status" />
    </a>
  </div>
</div>

## Introduction

**MonoCloud Cedar SDK for .NET — author, validate, and evaluate [Cedar](https://www.cedarpolicy.com/) policies natively from .NET applications.**

[MonoCloud](https://www.monocloud.com?utm_source=github&utm_medium=cedar_dotnet) is a modern, developer-friendly Identity & Access Management platform.

This SDK exposes the Cedar policy engine to .NET via prebuilt native bindings, so you can parse policies and entities, run authorization decisions, and validate schemas without spawning a separate process or pulling in a JVM.

> This project is a fork of [`cedar-policy/cedar-java`](https://github.com/cedar-policy/cedar-java) — the official Java bindings published by the Cedar Contributors. The .NET surface, type names, and serialization shape mirror the Java SDK closely. See [Acknowledgments](#-acknowledgments) below.

## 📘 Documentation

- **Cedar language:** [https://docs.cedarpolicy.com](https://docs.cedarpolicy.com)
- **Cedar tutorials & examples:** [https://www.cedarpolicy.com](https://www.cedarpolicy.com)
- **MonoCloud docs:** [https://www.monocloud.com/docs](https://www.monocloud.com/docs?utm_source=github&utm_medium=cedar_dotnet)

## Supported Platforms

This SDK targets **.NET 10** and ships native runtime libraries for:

- `win-x64`, `win-arm64`
- `linux-x64`, `linux-arm64`
- `osx-x64`, `osx-arm64`

The right native binary is selected automatically at runtime based on the host RID — no extra setup required.

## 🚀 Getting Started

### Installation

```powershell
Install-Package MonoCloud.Cedar

# or

dotnet add package MonoCloud.Cedar
```

### Usage

```csharp
namespace MyPackage;

using MonoCloud.Cedar;
using MonoCloud.Cedar.Model;
using MonoCloud.Cedar.Model.Entity;
using MonoCloud.Cedar.Model.Policy;
using MonoCloud.Cedar.Value;

public class SimpleAuthorization
{
    public static void Main()
    {
        // Build entities
        Entity principal = new Entity(EntityUID.Parse("User::\"Alice\"")!);
        Entity action = new Entity(EntityUID.Parse("Action::\"view\"")!);
        Entity resource = new Entity(EntityUID.Parse("Photo::\"alice_photo\"")!);

        // Build policies
        PolicySet policySet = PolicySet.ParsePolicies("""
            permit(
                principal == User::"Alice",
                action == Action::"view",
                resource == Photo::"alice_photo"
            );

            forbid(
                principal == User::"Alice",
                action == Action::"view",
                resource == Photo::"bob_photo"
            );
            """);

        // Authorization request
        AuthorizationEngine ae = new BasicAuthorizationEngine();
        Entities entities = new Entities();
        Context context = new Context();
        AuthorizationRequest request = new AuthorizationRequest(principal, action, resource, context);
        AuthorizationResponse authorizationResponse = ae.IsAuthorized(request, policySet, entities);
    }
}
```

For richer scenarios — schemas, templates, validation, partial evaluation — see the unit tests under `MonoCloud.Cedar.UnitTests/` for working examples.

## 🤝 Contributing & Support

### Issues & Feedback

- Use **GitHub Issues** for bug reports and feature requests.
- For tenant or account-specific help, contact MonoCloud Support through your dashboard.

### Security

Do **not** report security issues publicly. Please follow the contact instructions at: [https://www.monocloud.com/contact](https://www.monocloud.com/contact?utm_source=github&utm_medium=cedar_dotnet)

## 🙏 Acknowledgments

This project is a fork of [`cedar-policy/cedar-java`](https://github.com/cedar-policy/cedar-java) and would not exist without the work of the Cedar Contributors:

- **Cedar policy engine:** [`cedar-policy/cedar`](https://github.com/cedar-policy/cedar) — the Rust crate that powers authorization decisions.
- **Cedar Java SDK:** [`cedar-policy/cedar-java`](https://github.com/cedar-policy/cedar-java) — the upstream Java bindings whose API surface this SDK mirrors.
- **Cedar website & docs:** [https://www.cedarpolicy.com](https://www.cedarpolicy.com)

Cedar and the Java SDK are licensed under the Apache License 2.0; this fork preserves that license. Many thanks to the original authors and maintainers.

## 📄 License

Licensed under the **Apache License, Version 2.0**. See the included [`LICENSE`](LICENSE) file.
