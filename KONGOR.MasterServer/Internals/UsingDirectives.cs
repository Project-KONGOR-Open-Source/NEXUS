global using ASPIRE.Common.Communication;
global using ASPIRE.Common.Extensions.Cryptography;

global using KONGOR.MasterServer.Extensions.Cache;
global using KONGOR.MasterServer.Extensions.Collections;
global using KONGOR.MasterServer.Handlers.Patch;
global using KONGOR.MasterServer.Handlers.SRP;
global using KONGOR.MasterServer.Models.Configuration;
global using KONGOR.MasterServer.Models.RequestResponse.GameData;
global using KONGOR.MasterServer.Models.RequestResponse.Patch;
global using KONGOR.MasterServer.Models.RequestResponse.ServerManagement;
global using KONGOR.MasterServer.Models.RequestResponse.SRP;
global using KONGOR.MasterServer.Models.RequestResponse.Stats;
global using KONGOR.MasterServer.Models.RequestResponse.Store;
global using KONGOR.MasterServer.Models.ServerManagement;

global using MERRICK.Database.Context;
global using MERRICK.Database.Entities.Core;
global using MERRICK.Database.Entities.Game;
global using MERRICK.Database.Entities.Statistics;
global using MERRICK.Database.Enumerations;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Options;
global using Microsoft.OpenApi.Models;

global using OneOf;

global using PhpSerializerNET;

global using SecureRemotePassword;

global using StackExchange.Redis;

global using System.Security.Cryptography;
global using System.Linq.Expressions;
global using System.Text;
global using System.Text.Json;
global using System.Text.RegularExpressions;
