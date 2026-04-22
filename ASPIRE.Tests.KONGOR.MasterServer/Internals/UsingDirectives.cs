global using ASPIRE.Common.Enumerations.Match;
global using ASPIRE.Common.Enumerations.Statistics;

global using ASPIRE.Tests.Infrastructure.Container;
global using ASPIRE.Tests.Infrastructure.DependencyInversion;
global using ASPIRE.Tests.Infrastructure.Factory;
global using ASPIRE.Tests.Infrastructure.Hooks;

global using ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;
global using ASPIRE.Tests.KONGOR.MasterServer.Models;
global using ASPIRE.Tests.KONGOR.MasterServer.Services;

global using KONGOR.MasterServer.Attributes.Serialisation;
global using KONGOR.MasterServer.Configuration;
global using KONGOR.MasterServer.Configuration.Economy;
global using KONGOR.MasterServer.Configuration.Plinko;
global using KONGOR.MasterServer.Configuration.Store;
global using KONGOR.MasterServer.Extensions.Cache;
global using KONGOR.MasterServer.Handlers.SRP;
global using KONGOR.MasterServer.Helpers.Stats;
global using KONGOR.MasterServer.Internals;
global using KONGOR.MasterServer.Models.RequestResponse.SRP;
global using KONGOR.MasterServer.Models.RequestResponse.Store;
global using KONGOR.MasterServer.Models.ServerManagement;
global using KONGOR.MasterServer.Services;

global using MERRICK.DatabaseContext.Constants;
global using MERRICK.DatabaseContext.Entities.Core;
global using MERRICK.DatabaseContext.Entities.Statistics;
global using MERRICK.DatabaseContext.Entities.Utility;
global using MERRICK.DatabaseContext.Enumerations;
global using MERRICK.DatabaseContext.Persistence;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.Extensions.Options;

global using OneOf;

global using PhpSerializerNET;

global using SecureRemotePassword;

global using StackExchange.Redis;

global using System.Collections;
global using System.IdentityModel.Tokens.Jwt;
global using System.Net;
global using System.Net.Http.Json;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;

global using TUnit.Assertions;
global using TUnit.Assertions.Extensions;
global using TUnit.Core;

global using WireMock.Admin.Mappings;
