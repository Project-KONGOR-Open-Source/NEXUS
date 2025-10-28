﻿global using MERRICK.DatabaseContext.Entities.Utility;
global using MERRICK.DatabaseContext.Helpers;
global using MERRICK.DatabaseContext.Persistence;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using TUnit.Assertions;
global using TUnit.Assertions.Extensions;
global using TUnit.Core;

global using System.IdentityModel.Tokens.Jwt;
global using System.Net.Http.Headers;
global using System.Text;

global using ZORGATH.WebPortal.API.Contracts;
global using ZORGATH.WebPortal.API.Controllers;
global using ZORGATH.WebPortal.API.Extensions;
global using ZORGATH.WebPortal.API.Handlers;
global using ZORGATH.WebPortal.API.Internals;
global using ZORGATH.WebPortal.API.Models.Configuration;
global using ZORGATH.WebPortal.API.Services.Email;
