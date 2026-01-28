global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Net;
global using System.Net.Sockets;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Text;

global using ASPIRE.Common.Constants;
global using ASPIRE.Common.Extensions.Cryptography;
global using ASPIRE.Common.ServiceDefaults;
// TODO: Move These To A Shared Project And Remove Inter-Project Dependencies
global using KONGOR.MasterServer.Extensions.Cache;
global using KONGOR.MasterServer.Handlers.SRP;
global using KONGOR.MasterServer.Models.ServerManagement;

global using MERRICK.DatabaseContext.Entities.Core;
global using MERRICK.DatabaseContext.Entities.Relational;
global using MERRICK.DatabaseContext.Enumerations;
global using MERRICK.DatabaseContext.Persistence;

global using Microsoft.AspNetCore.HttpOverrides;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.Extensions.Diagnostics.HealthChecks;

global using OneOf;

global using StackExchange.Redis;

global using TRANSMUTANSTEIN.ChatServer.Attributes;
global using TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;
global using TRANSMUTANSTEIN.ChatServer.Contracts;
global using TRANSMUTANSTEIN.ChatServer.Domain.Communication;
global using TRANSMUTANSTEIN.ChatServer.Domain.Core;
global using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;
global using TRANSMUTANSTEIN.ChatServer.Domain.Social;
global using TRANSMUTANSTEIN.ChatServer.Extensions.Protocol;
global using TRANSMUTANSTEIN.ChatServer.Internals;
global using TRANSMUTANSTEIN.ChatServer.Services;
global using TRANSMUTANSTEIN.ChatServer.Utilities;