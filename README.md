# 📦Tsar.Net - Development kit documentation

## Introduction

Tsar.Net is a development kit for building applications that interact with the Tsar Network for application management, authentication and more.

## Getting started

To get started with Tsar.Net, you will need to install Tsar.Net NuGet package. You can do this by running the following command in the NuGet package manager console or visit the Nuget page:

```
Install-Package Tsar.Net
```

Alternatively, you can install the package using the .NET CLI:

```
dotnet add package Tsar.Net
```

Once you have installed the package, you can start using the Tsar.Net SDK in your application.
If you have any questions or need help, please reference the demo application or contact us.

## Documentation - Table of contents

- [Client](#Client)
  - [Constructor](#Constructor)
  - [Properties](#Properties)
  - [Methods](#Methods)

# Constructor

```csharp
public Client(ClientOptions Options)
```

## Parameters

- `Options` - The options to use when creating the client.

# Properties

```csharp

public string ApplicationId { get; set; }

public string ClientKey { get; set; }

public string Session { get; set; }

public string HardwareId { get; set; }

public Subscription Subscription { get; }
```

# Methods

```csharp
public Data ValidateUser(string Id)
```

## Parameters

- `Id` - The id of the user to validate.

```csharp
public Async Task<Data> ValidateUserAsync(string Id)
```

## Parameters

- `Id` - The id of the user to get.
