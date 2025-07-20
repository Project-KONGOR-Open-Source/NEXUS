namespace ZORGATH.WebPortal.API;

public class ZORGATH
{
    // TRUE If The Application Is Running In Development Mode Or FALSE If Not
    public static bool RunsInDevelopmentMode { get; private set; } = true;

    public static void Main(string[] args)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Set Static RunsInDevelopmentMode Property
        RunsInDevelopmentMode = builder.Environment.IsDevelopment();

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
            ? [ "https://localhost:55550",   "https://localhost:55551",      "https://localhost:55552",      "https://localhost:55553",     "https://localhost:55554", "http://localhost:55555", "https://localhost:55556",       "https://localhost:55557"      ]
            : [ "https://aspire.kongor.net", "https://telemetry.kongor.net", "https://resources.kongor.net", "https://database.kongor.net", "https://chat.kongor.net", "http://api.kongor.net",  "https://portal.api.kongor.net", "https://portal.ui.kongor.net" ];

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
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },

                    Array.Empty<string>() // No Scopes Required, Just A Valid JWT
                }
            });
        });

        // Add Email Service
        builder.Services.AddSingleton<IEmailService, EmailService>();

        // Build The Application
        WebApplication app = builder.Build();

        // Configure Development-Specific Middleware
        if (app.Environment.IsDevelopment())
        {
            // Show Detailed Error Pages In Development
            app.UseDeveloperExceptionPage();

            // Enable HTTP Request/Response Logging
            app.UseHttpLogging();

            // Enable Swagger API Documentation
            app.UseSwagger();

            // Configure Swagger UI With Custom Styling
            app.UseSwaggerUI(options =>
            {
                options.InjectStylesheet("swagger.css");
                options.DocumentTitle = "ZORGATH Web Portal API";
            });

            // Serve Static Files For Swagger CSS
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Resources", "CSS")),
                RequestPath = "/swagger"
            });
        }

        else
        {
            // Use Global Exception Handler In Production
            app.UseExceptionHandler("/error");
        }

        // Enable CORS Policy
        app.UseCors(corsPolicyName);

        // Enable Output Caching
        app.UseOutputCache();

        // Enable Rate Limiting (Before Authentication)
        app.UseRateLimiter();

        // Automatically Redirect HTTP Requests To HTTPS
        app.UseHttpsRedirection();

        // Enforce HTTPS With Strict Transport Security
        app.UseHsts();

        // Add Security Headers Middleware
        app.Use(async (context, next) =>
        {
            // Prevent MIME Type Sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            // Prevent Page From Being Displayed In Frames
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            
            // Enable XSS Protection
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            
            // Control Referrer Information
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Content Security Policy For API
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none';");
            
            await next();
        });

        // Require Authentication To Access Non-Public Resources
        app.UseAuthentication();

        // Require Authorization To Access Role-Specific Resources
        app.UseAuthorization();

        // Map Aspire Default Health Check Endpoints
        app.MapDefaultEndpoints();

        // Map MVC Controllers With Rate Limiting
        app.MapControllers().RequireRateLimiting(RateLimiterPolicies.Relaxed);

        // Run The Application
        app.Run();
    }
}
