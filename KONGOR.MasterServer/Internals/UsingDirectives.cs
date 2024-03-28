global using KONGOR.MasterServer.Extensions;
global using KONGOR.MasterServer.Handlers.Patch;
global using KONGOR.MasterServer.Handlers.SRP;
global using KONGOR.MasterServer.Models.Configuration;
global using KONGOR.MasterServer.Models.RequestResponse.Patch;
global using KONGOR.MasterServer.Models.RequestResponse.SRP;
global using KONGOR.MasterServer.Models.RequestResponse.Stats;
global using KONGOR.MasterServer.Models.RequestResponse.Store;

global using MERRICK.Database.Context;
global using MERRICK.Database.Entities.Core;
global using MERRICK.Database.Enumerations;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Options;

global using OneOf;

global using PhpSerializerNET;

global using SecureRemotePassword;

global using System.Security.Cryptography;
global using System.Text;
global using System.Text.RegularExpressions;
