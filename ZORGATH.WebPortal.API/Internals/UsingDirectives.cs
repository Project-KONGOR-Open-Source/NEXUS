global using ASPIRE.Common.ServiceDefaults;

global using FluentValidation;
global using FluentValidation.Results;

global using MERRICK.DatabaseContext.Constants;
global using MERRICK.DatabaseContext.Entities.Core;
global using MERRICK.DatabaseContext.Entities.Utility;
global using MERRICK.DatabaseContext.Persistence;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.HttpLogging;
global using Microsoft.AspNetCore.HttpOverrides;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.Extensions.FileProviders;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi;

global using SecureRemotePassword;

global using System.IdentityModel.Tokens.Jwt;
global using System.Net;
global using System.Net.Sockets;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Threading.RateLimiting;

global using ZORGATH.WebPortal.API.Constants;
global using ZORGATH.WebPortal.API.Contracts;
global using ZORGATH.WebPortal.API.Extensions;
global using ZORGATH.WebPortal.API.Handlers;
global using ZORGATH.WebPortal.API.Helpers;
global using ZORGATH.WebPortal.API.Models.Configuration;
global using ZORGATH.WebPortal.API.Services.Email;
global using ZORGATH.WebPortal.API.Validators;