# Audit.NET.RavenDB

Audit.NET.RavenDB is now part of the Audit.NET repository and can be found here https://github.com/thepirat000/Audit.NET/tree/master/src/Audit.NET.RavenDB


[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ops-ai_audit-net-ravendb&metric=alert_status)](https://sonarcloud.io/dashboard?id=ops-ai_audit-net-ravendb)
[![Build Status](https://opsai.visualstudio.com/BeyondAuth/_apis/build/status/ops-ai.Audit.NET.RavenDB?branchName=develop)](https://opsai.visualstudio.com/BeyondAuth/_build/latest?definitionId=7&branchName=develop)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=ops-ai_audit-net-ravendb&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=ops-ai_audit-net-ravendb)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ops-ai_audit-net-ravendb&metric=coverage)](https://sonarcloud.io/dashboard?id=ops-ai_audit-net-ravendb)
![CodeQL](https://github.com/ops-ai/Audit.NET.RavenDB/workflows/CodeQL/badge.svg)

[![NuGet Status](https://img.shields.io/nuget/v/Audit.NET.RavenDB.svg?style=flat)](https://www.nuget.org/packages/Audit.NET.RavenDB/)
[![NuGet Count](https://img.shields.io/nuget/dt/Audit.NET.RavenDB.svg)](https://www.nuget.org/packages/Audit.NET.RavenDB/)

## Overview

RavenDB storage provider for Audit.NET

## Setup

### Set up the Audit.NET RavenDB provider during application startup with defaults storing object diffs
```csharp
Audit.Core.Configuration.Setup()
  .UseRavenDB(configuration.GetSection("RavenSettings:Urls").Get<string[]>(),
    new X509Certificate2(Path.Combine(HttpContext.Current.Server.MapPath(""), configuration["RavenSettings:CertFilePath"]), configuration["RavenSettings:CertPassword"]),
    configuration["RavenSettings:DatabaseName"]);
```

### Alternatively, set up the Audit.NET RavenDB provider during application startup storing full objects
```csharp
Audit.Core.Configuration.Setup()
  .UseRavenDB(configuration.GetSection("RavenSettings:Urls").Get<string[]>(),
    new X509Certificate2(Path.Combine(HttpContext.Current.Server.MapPath(""), configuration["RavenSettings:CertFilePath"]), configuration["RavenSettings:CertPassword"]),
    configuration["RavenSettings:DatabaseName"], null, false);
```

## Usage

Create an audit scope around the objects you are changing. Audit.NET takes a snapshot of the referenced object as it enters the scope, and compares it with the referenced object as it leaves the using block.

### Audit creation of a new application, setting a custom field called Id to the app's Id after it's created

```csharp
Application? app = null;
using (var audit = await AuditScope.CreateAsync("Application:Create", () => app))
{
    app = new Application
    {
        OwnerId = userId,
        Description = model.Description,
        Id = $"Applications/{Guid.NewGuid()}",
        Name = model.Name
    };
    await session.StoreAsync(app, ct);
    await session.SaveChangesAsync(ct);
    audit.SetCustomField(nameof(app.Id), app.Id);
}
```

### Audit updating an application

```csharp
using (var audit = await AuditScope.CreateAsync("Application:Update", () => app, new { app.Id }))
{
    app.Description = model.Description;
    app.Name = model.Name;
    app.Domain = model.Domain;
    app.DefaultEnvironment = model.DefaultEnvironment;

    await session.SaveChangesAsync(ct);
}
```

## Known Bugs

No known bugs at this time. Please submit an issue if you encounter issues

## Example audit records

### Change property called Description on an object
```json
{
    "Environment": {
        "UserName": "root",
        "MachineName": "7b7c96b853-ftlkw",
        "DomainName": "7b7c96b853-ftlkw",
        "CallingMethodName": "System.Runtime.CompilerServices.AsyncMethodBuilderCore.Start()",
        "AssemblyName": "System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bdd7798e",
        "Culture": ""
    },
    "EventType": "Application:Update",
    "Target": {
        "Type": "Application",
        "Old": {
            "Description": "My Old Description"
        },
        "New": {
            "Description": "My New Description"
        }
    },
    "StartDate": "2022-03-30T13:51:45.3090112Z",
    "EndDate": "2022-03-30T13:51:45.3305599Z",
    "Duration": 22,
    "Id": "Applications/c9fa19a0-dafa-4f77-b751-3119475d0815",
    "UserId": "ApplicationUsers/1-A",
    "@metadata": {
        "@collection": "AuditEvents",
        "Raven-Clr-Type": "Audit.Core.AuditEvent, Audit.NET"
    }
}
```

### Remove an object from a deeply nested array
```json
{
    "Environment": {
        "UserName": "root",
        "MachineName": "7b7c96b855-rm8bl",
        "DomainName": "7b7c96b855-rm8bl",
        "CallingMethodName": "System.Runtime.CompilerServices.AsyncMethodBuilderCore.Start()",
        "AssemblyName": "System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bdd7798e",
        "Culture": ""
    },
    "EventType": "Application:UpdateEnvironment",
    "Target": {
        "Type": "Application",
        "Old": {
            "Environments": {
                "Production": {
                    "Definitions": {
                        "Test": [
                            {
                                "Name": "AlwaysOn",
                                "Parameters": null
                            }
                        ]
                    }
                }
            }
        },
        "New": {
            "Environments": {
                "Production": {
                    "Definitions": {
                        "Test": []
                    }
                }
            }
        }
    },
    "StartDate": "2022-03-30T13:52:18.1276474Z",
    "EndDate": "2022-03-30T13:52:18.3096561Z",
    "Duration": 182,
    "Id": "Applications/c91119ad-aaaa-2277-b751-311aaaad0815",
    "UserId": "ApplicationUsers/1232-C",
    "@metadata": {
        "@collection": "AuditEvents",
        "Raven-Clr-Type": "Audit.Core.AuditEvent, Audit.NET"
    }
}
```
