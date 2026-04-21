global using Aspire.Hosting.Testing;
global using Aspire.Hosting;

global using ASPIRE.SourceGenerator.Attributes;

global using ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;
global using ASPIRE.Tests.KONGOR.MasterServer.Models;
global using ASPIRE.Tests.KONGOR.MasterServer.Services;

global using ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;
global using ASPIRE.Tests.ZORGATH.WebPortal.API.Models;
global using ASPIRE.Tests.ZORGATH.WebPortal.API.Services;

global using ASPIRE.Common.Configuration;
global using ASPIRE.Common.Configuration.Economy;
global using ASPIRE.Common.Configuration.Matchmaking;
global using ASPIRE.Common.Configuration.Plinko;
global using ASPIRE.Common.Configuration.Store;
global using ASPIRE.Common.Enumerations.Match;
global using ASPIRE.Common.Enumerations.Statistics;

global using KONGOR.MasterServer.Attributes.Serialisation;
global using KONGOR.MasterServer.Extensions.Cache;
global using KONGOR.MasterServer.Helpers.Stats;
global using KONGOR.MasterServer.Models.ServerManagement;
global using KONGOR.MasterServer.Handlers.SRP;
global using KONGOR.MasterServer.Internals;
global using KONGOR.MasterServer.Models.RequestResponse.SRP;
global using KONGOR.MasterServer.Models.RequestResponse.Store;

global using MERRICK.DatabaseContext.Constants;
global using MERRICK.DatabaseContext.Entities.Core;
global using MERRICK.DatabaseContext.Entities.Statistics;
global using MERRICK.DatabaseContext.Entities.Utility;
global using MERRICK.DatabaseContext.Enumerations;
global using MERRICK.DatabaseContext.Helpers;
global using MERRICK.DatabaseContext.Persistence;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using OneOf;

global using PhpSerializerNET;

global using SecureRemotePassword;

global using StackExchange.Redis;

global using System.Collections;
global using System.Collections.Concurrent;
global using System.IdentityModel.Tokens.Jwt;
global using System.Text.RegularExpressions;
global using System.Text;

global using TUnit.Assertions.Extensions;
global using TUnit.Assertions;
global using TUnit.Core;

global using ZORGATH.WebPortal.API.Contracts;
global using ZORGATH.WebPortal.API.Controllers;
global using ZORGATH.WebPortal.API.Extensions;
global using ZORGATH.WebPortal.API.Handlers;
global using ZORGATH.WebPortal.API.Internals;
global using ZORGATH.WebPortal.API.Models.Configuration;
global using ZORGATH.WebPortal.API.Services.Email;
