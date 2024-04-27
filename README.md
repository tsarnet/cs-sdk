# 📦Tsar.Net - Development Kit Documentation

## Introduction

Tsar.Net Is A Development Kit For Building Applications That Interact With The Tsar Network For Application Management, Authentication And More.

## Getting Started

To Get Started With Tsar.Net, You Will Need To Install Tsar.Net NuGet Package. You Can Do This By Running The Following Command In The NuGet Package Manager Console Or Visit The Nuget Page:

```
Install-Package Tsar.Net
```

Alternatively, You Can Install The Package Using The .NET CLI:

```
dotnet add package Tsar.Net
```

Once You Have Installed The Package, You Can Start Using The Tsar.Net SDK In Your Application.
If You Have Any Questions Or Need Help, Please Reference The Demo Application Or Contact Us.

## Documentation - Table Of Contents

- [Client](#Client)
  - [Constructor](#Constructor)
  - [Properties](#Properties)
  - [Methods](#Methods)

# Constructor

```csharp
public Client(ClientOptions Options)
```

## Parameters

- `Options` - The Options To Use When Creating The Client.


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

- `Id` - The Id Of The User To Validate.

```csharp
public Async Task<Data> ValidateUserAsync(string Id)
```

## Parameters

- `Id` - The Id Of The User To Get.
