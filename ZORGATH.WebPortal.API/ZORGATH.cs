namespace ZORGATH.WebPortal.API;

public class ZORGATH
{
    public static void Main(string[] args)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Map User-Defined Configuration Section
        builder.Services.Configure<OperationalConfiguration>(builder.Configuration.GetRequiredSection(OperationalConfiguration.ConfigurationSection));

        // Add Aspire Service Defaults
        builder.AddServiceDefaults();

        // Add The Database Context
        builder.AddSqlServerDbContext<MerrickContext>("MERRICK", configureSettings: null, configureDbContextOptions: options =>
        {
            // Enable Detailed Error Messages In Development Environment
            options.EnableDetailedErrors(builder.Environment.IsDevelopment());

            // Suppress Warning Regarding Enabled Sensitive Data Logging, Since It Is Only Enabled In The Development Environment
            // https://github.com/dotnet/efcore/blob/main/src/EFCore/Properties/CoreStrings.resx (LogSensitiveDataLoggingEnabled)
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .ConfigureWarnings(warnings => warnings.Log((Id: CoreEventId.SensitiveDataLoggingEnabledWarning, Level: LogLevel.Trace)));

            // Enable Thread Safety Checks For Entity Framework
            options.EnableThreadSafetyChecks();
        });

        // Add Memory Cache Service
        builder.Services.AddMemoryCache();

        // Add Rate Limiting Service To Protect Against Abuse And DoS Attacks
        builder.Services.AddRateLimiter(options =>
        {
            // Relaxed Limits For General API Endpoints
            options.AddSlidingWindowLimiter(policyName: RateLimiterPolicies.Relaxed, policy =>
            {
                policy.PermitLimit = 100;
                policy.Window = TimeSpan.FromMinutes(1);
                policy.SegmentsPerWindow = 6; // 10 Seconds Per Sliding Window Segment
                policy.QueueLimit = 10;
                policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            
            // Strict Limits For Authentication And Other Sensitive Endpoints
            options.AddSlidingWindowLimiter(policyName: RateLimiterPolicies.Strict, policy =>
            {
                policy.PermitLimit = 5;
                policy.Window = TimeSpan.FromMinutes(1);
                policy.SegmentsPerWindow = 6; // 10 Seconds Per Sliding Window Segment
                policy.QueueLimit = 0;
                policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        // Add HTTP Request/Response Logging For Debugging In Development
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders;
                options.RequestBodyLogLimit = 4096; // 4KB Request Body Limit
                options.ResponseBodyLogLimit = 4096; // 4KB Response Body Limit
            });
        }

        // Set CORS Policy Name
        const string corsPolicyName = "Allow-All";

        // Set CORS Origins
        string[] corsOrigins = builder.Environment.IsDevelopment()
            ? [ "https://localhost:55550",      "https://localhost:55551",      "https://localhost:55552",      "https://localhost:55553",     "https://localhost:55554", "http://localhost:55555", "https://localhost:55556",       "https://localhost:55557"      ]
            : [ "https://dashboard.kongor.net", "https://telemetry.kongor.net", "https://resources.kongor.net", "https://database.kongor.net", "https://chat.kongor.net", "http://api.kongor.net",  "https://portal.api.kongor.net", "https://portal.ui.kongor.net" ];

        // Add CORS Policy To Allow Cross-Origin Requests
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(corsPolicyName,
                configuration => configuration.WithOrigins(corsOrigins)
                    .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
        });

        // Add Server-Side Output Caching
        builder.Services.AddOutputCache(options =>
        {
            options.AddPolicy(OutputCachePolicies.CacheForThirtySeconds, policy => policy.Cache().Expire(TimeSpan.FromSeconds(30)));
            options.AddPolicy(OutputCachePolicies.CacheForFiveMinutes, policy => policy.Cache().Expire(TimeSpan.FromMinutes(5)));
            options.AddPolicy(OutputCachePolicies.CacheForOneDay, policy => policy.Cache().Expire(TimeSpan.FromDays(1)));
            options.AddPolicy(OutputCachePolicies.CacheForOneWeek, policy => policy.Cache().Expire(TimeSpan.FromDays(7)));
        });

        // TODO: Implement Username And Password Validation Policies
        //if (builder.Environment.IsDevelopment())
        //{
        //    builder.Services.Configure<IdentityOptions>(options =>
        //    {
        //        options.User.RequireUniqueEmail = false;
        //        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_`";

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
        //        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_`";

        //        options.Password.RequiredLength = 8;
        //        options.Password.RequiredUniqueChars = 4;
        //        options.Password.RequireNonAlphanumeric = true;
        //        options.Password.RequireLowercase = true;
        //        options.Password.RequireUppercase = true;
        //        options.Password.RequireDigit = true;

        //        options.Lockout.MaxFailedAccessAttempts = 3;
        //        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        //    });
        //}

        // Add JWT Bearer Authentication Configuration
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Get Operational Configuration For JWT Settings
                OperationalConfiguration configuration = builder.Configuration.GetRequiredSection(OperationalConfiguration.ConfigurationSection)
                    .Get<OperationalConfiguration>() ?? throw new NullReferenceException("Operational Configuration Is NULL");

                // Configure JWT Validation Parameters
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.JWT.SigningKey)), // TODO: Put The Signing Key In A Secrets Vault
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.JWT.Issuer,
                    ValidateIssuer = true,
                    ValidAudience = configuration.JWT.Audience,
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = true
                };
            });

        // Add Authorization Policies For Role-Based Access Control
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(UserRoles.Administrator, policy => policy.RequireClaim(Claims.UserRole, UserRoles.Administrator))
            .AddPolicy(UserRoles.User, policy => policy.RequireClaim(Claims.UserRole, UserRoles.User))
            .AddPolicy(UserRoles.AllRoles, policy => policy.RequireClaim(Claims.UserRole, UserRoles.AllRoles.Split(',')));

        // Enable MVC Controllers
        builder.Services.AddControllers();

        // Add Comprehensive Error Response Detail In Development Environment
        if (builder.Environment.IsDevelopment())
            builder.Services.AddProblemDetails();

        // Add Swagger/OpenAPI Documentation Generation
        builder.Services.AddSwaggerGen(options =>
        {
            // Configure API Documentation Metadata
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ZORGATH Web Portal API",
                Version = "v1",

                License = new OpenApiLicense
                {
                    Name = "Project KONGOR Open-Source License",
                    Url = new Uri("https://github.com/Project-KONGOR-Open-Source/ASPIRE/blob/main/license")
                },

                Contact = new OpenApiContact
                {
                    Name = "[K]ONGOR",
                    Url = new Uri("https://github.com/K-O-N-G-O-R"),
                    Email = "project.kongor@proton.me"
                }
            });

            // Add JWT Bearer Authentication To Swagger UI
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "Insert A Valid JSON Web Token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header,
                BearerFormat = "JWT"
            });

            // Configure Security Requirements For All Endpoints
            options.AddSecurityRequirement(document =>
            {
                OpenApiSecuritySchemeReference schemeReference = new (JwtBearerDefaults.AuthenticationScheme, document);

                List<string> requiredScopes = []; // No Scopes Required, Just A Valid JWT

                OpenApiSecurityRequirement securityRequirement = new ()
                {
                    { schemeReference, requiredScopes }
                };

                return securityRequirement;
            });
        });

        // Add Email Service
        builder.Services.AddSingleton<IEmailService, EmailService>();

        // Build The Application
        WebApplication application = builder.Build();

        // Configure Development-Specific Middleware
        if (application.Environment.IsDevelopment())
        {
            // Show Detailed Error Pages In Development
            application.UseDeveloperExceptionPage();

            // Enable HTTP Request/Response Logging
            application.UseHttpLogging();

            // Enable Swagger API Documentation
            application.UseSwagger();

            // Configure Swagger UI With Custom Styling
            application.UseSwaggerUI(options =>
            {
                options.InjectStylesheet("swagger.css");
                options.DocumentTitle = "ZORGATH Web Portal API";
            });

            // Serve Static Files For Swagger CSS
            application.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Resources", "CSS")),
                RequestPath = "/swagger"
            });
        }

        else
        {
            // Use Global Exception Handler In Production
            application.UseExceptionHandler("/error");
        }

        // Enable Rate Limiting (Before Other Processing)
        application.UseRateLimiter();

        // Enforce HTTPS With Strict Transport Security
        application.UseHsts();

        // Automatically Redirect HTTP Requests To HTTPS
        application.UseHttpsRedirection();

        // Enable CORS Policy
        application.UseCors(corsPolicyName);

        // Enable Output Caching
        application.UseOutputCache();

        // Add Security Headers Middleware
        application.Use(async (context, next) =>
        {
            IHeaderDictionary headers = context.Response.Headers;

            // Prevent MIME Type Sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Control Referrer Information
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Apply Restrictive CSP Only To API Endpoints
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none';";
            }

            await next();
        });

        // Require Authentication To Access Non-Public Resources
        application.UseAuthentication();

        // Require Authorization To Access Role-Specific Resources
        application.UseAuthorization();

        // Map Aspire Default Health Check Endpoints
        application.MapDefaultEndpoints();

        // Map MVC Controllers With Rate Limiting
        application.MapControllers().RequireRateLimiting(RateLimiterPolicies.Relaxed);

        // Run The Application
        application.Run();
    }
}
