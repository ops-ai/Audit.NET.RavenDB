# Audit.NET.RavenDB

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ops-ai_audit-net-ravendb&metric=alert_status)](https://sonarcloud.io/dashboard?id=ops-ai_audit-net-ravendb)
[![Build Status](https://opsai.visualstudio.com/BeyondAuth/_apis/build/status/ops-ai.Audit.NET.RavenDB?branchName=develop)](https://opsai.visualstudio.com/BeyondAuth/_build/latest?definitionId=7&branchName=develop)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=ops-ai_audit-net-ravendb&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=ops-ai_audit-net-ravendb)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ops-ai_audit-net-ravendb&metric=coverage)](https://sonarcloud.io/dashboard?id=ops-ai_audit-net-ravendb)
![CodeQL](https://github.com/ops-ai/Audit.NET.RavenDB/workflows/CodeQL/badge.svg)


## Overview

RavenDB storage provider for Audit.NET

## Usage

Set up the Audit.NET RavenDB provider during application startup
```csharp
Audit.Core.Configuration.Setup()
  .UseRavenDB(configuration.GetSection("RavenSettings:Urls").Get<string[]>(),
    new X509Certificate2(Path.Combine(HttpContext.Current.Server.MapPath(""), configuration["RavenSettings:CertFilePath"]), configuration["RavenSettings:CertPassword"]),
    configuration["RavenSettings:DatabaseName"], null);
```

## Known Bugs

