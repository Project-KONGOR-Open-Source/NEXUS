global using ASPIRE.Tests.Infrastructure.Container;
global using ASPIRE.Tests.Infrastructure.DependencyInversion;
global using ASPIRE.Tests.Infrastructure.Factory;
global using ASPIRE.Tests.Infrastructure.Hooks;

global using ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;
global using ASPIRE.Tests.ZORGATH.WebPortal.API.Models;
global using ASPIRE.Tests.ZORGATH.WebPortal.API.Services;

global using KONGOR.MasterServer.Configuration;
global using KONGOR.MasterServer.Configuration.Economy;

global using MERRICK.DatabaseContext.Constants;
global using MERRICK.DatabaseContext.Entities.Core;
global using MERRICK.DatabaseContext.Entities.Utility;
global using MERRICK.DatabaseContext.Enumerations;
global using MERRICK.DatabaseContext.Persistence;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using System.Collections.Concurrent;
global using System.IdentityModel.Tokens.Jwt;
global using System.Text;
global using System.Text.Json;

global using TUnit.Assertions;
global using TUnit.Assertions.Extensions;
global using TUnit.Core;

global using ZORGATH.WebPortal.API.Contracts;
global using ZORGATH.WebPortal.API.Controllers;
global using ZORGATH.WebPortal.API.Extensions;
global using ZORGATH.WebPortal.API.Internals;
global using ZORGATH.WebPortal.API.Models.Configuration;
global using ZORGATH.WebPortal.API.Services.Email;
