namespace KONGOR.MasterServer;

internal class KONGOR
{
    internal static void Main(string[] args)
    {
        //KongorContext.ServerStartEpochTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

        // Build The Application
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add Aspire Service Defaults
        builder.AddServiceDefaults();

        // Add The Database Context
        builder.Services.AddDbContext<MerrickContext>(options => { options.UseSqlServer("MERRICK Database"); });

        // Add MVC Controllers
        builder.Services.AddControllers();

        //builder.Services.AddEndpointsApiExplorer();

        //builder.Services.AddIdentityCore<User>()
        //    .AddRoles<Role>()
        //    .AddEntityFrameworkStores<MerrickContext>()
        //    .AddDefaultTokenProviders();

        // builder.Services.AddEndpointsApiExplorer();
        // builder.Services.AddProblemDetails();

        // Add Swagger
        builder.Services.AddSwaggerGen();

        //builder.Services.AddSwaggerGen(options =>
        //{
        //    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Project KONGOR", Version = "v1" });

        //    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        //    {
        //        Description = @"Insert Your JWT In The Format ""Bearer {token}""",
        //        Name = "Authorization",
        //        In = ParameterLocation.Header,
        //        Type = SecuritySchemeType.ApiKey,
        //        Scheme = JwtBearerDefaults.AuthenticationScheme
        //    });

        //    options.AddSecurityRequirement(new OpenApiSecurityRequirement
        //    {
        //        {
        //            new OpenApiSecurityScheme
        //            {
        //                Reference = new OpenApiReference
        //                {
        //                    Type = ReferenceType.SecurityScheme,
        //                    Id = JwtBearerDefaults.AuthenticationScheme
        //                },
        //                Scheme = "oauth2",
        //                Name = JwtBearerDefaults.AuthenticationScheme,
        //                In = ParameterLocation.Header
        //            },
        //            new List<string>()
        //        }
        //    });
        //});

        //builder.Services.AddExceptionHandler();

        //if (builder.Environment.IsDevelopment())
        //{
        //    builder.Services.Configure<IdentityOptions>(options =>
        //    {
        //        options.User.RequireUniqueEmail = false;
        //        options.User.AllowedUserNameCharacters = string.Concat(Enumerable.Range(char.MinValue, char.MaxValue).Select(Convert.ToChar).Where(character => char.IsControl(character).Equals(false)));

        //        options.Password.RequiredLength = 0;
        //        options.Password.RequiredUniqueChars = 0;
        //        options.Password.RequireNonAlphanumeric = false;
        //        options.Password.RequireLowercase = false;
        //        options.Password.RequireUppercase = false;
        //        options.Password.RequireDigit = false;

        //        options.Lockout.MaxFailedAccessAttempts = 5;
        //        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Zero;
        //    });
        //}

        //else
        //{
        //    builder.Services.Configure<IdentityOptions>(options =>
        //    {
        //        options.User.RequireUniqueEmail = true;
        //        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";

        //        options.Password.RequiredLength = 8;
        //        options.Password.RequiredUniqueChars = 4;
        //        options.Password.RequireNonAlphanumeric = true;
        //        options.Password.RequireLowercase = true;
        //        options.Password.RequireUppercase = true;
        //        options.Password.RequireDigit = true;

        //        options.Lockout.MaxFailedAccessAttempts = 3;
        //        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        //    });
        //}

        //builder.Services.AddAuthentication(options =>
        //{
        //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //}).AddJwtBearer(options =>
        //{
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuerSigningKey = true,
        //        ValidateIssuer = true,
        //        ValidateAudience = true,
        //        ValidateLifetime = true,
        //        ClockSkew = TimeSpan.Zero,
        //        ValidIssuer = builder.Configuration["JWT:Issuer"],
        //        ValidAudience = builder.Configuration["JWT:Audience"],
        //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])) // TODO: Put The Key In A Secrets File And Move That File To A Separate Repository
        //    };
        //});

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.InjectStylesheet("/swagger/custom.css"); // https://github.com/Amoenus/SwaggerDark/releases
                // https://blog.elijahlopez.ca/posts/aspnet-swagger-dark-theme/
                // https://amoenus.dev/swagger-dark-theme
            });
        }
        else
        {
            app.UseExceptionHandler();
        }

        //app.UseHttpsRedirection();

        //app.UseAuthorization();

        // Map Aspire Default Endpoints
        app.MapDefaultEndpoints();

        // Map MVC Controllers
        app.MapControllers();

        // Run The Application
        app.Run();
    }
}
