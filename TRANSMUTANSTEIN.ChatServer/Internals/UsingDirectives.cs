global using ASPIRE.Common.Extensions.Cryptography;

global using KONGOR.MasterServer.Models.ServerManagement; // TODO: Move Models To A Shared Project And Remove Inter-Project Dependencies

global using MERRICK.Database.Context;
global using MERRICK.Database.Entities.Core;
global using MERRICK.Database.Enumerations;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.Extensions.Diagnostics.HealthChecks;

global using OneOf;

global using StackExchange.Redis;

global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Net;
global using System.Net.Sockets;
global using System.Reflection;
global using System.Text;

global using TRANSMUTANSTEIN.ChatServer.Attributes;
global using TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;
global using TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;
global using TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;
global using TRANSMUTANSTEIN.ChatServer.Communication;
global using TRANSMUTANSTEIN.ChatServer.Contracts;
global using TRANSMUTANSTEIN.ChatServer.Core;
global using TRANSMUTANSTEIN.ChatServer.Extensions;
global using TRANSMUTANSTEIN.ChatServer.Internals;
global using TRANSMUTANSTEIN.ChatServer.Matchmaking;
global using TRANSMUTANSTEIN.ChatServer.Services;
global using TRANSMUTANSTEIN.ChatServer.Utilities;
