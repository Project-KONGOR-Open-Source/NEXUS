global using MERRICK.Database.Constants;
global using MERRICK.Database.Context;
global using MERRICK.Database.Entities;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;

global using SecureRemotePassword;

global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.RegularExpressions;

global using ZORGATH.WebPortal.API.Contracts;
global using ZORGATH.WebPortal.API.Handlers;
global using ZORGATH.WebPortal.API.Helpers;
global using ZORGATH.WebPortal.API.Services.Email;
