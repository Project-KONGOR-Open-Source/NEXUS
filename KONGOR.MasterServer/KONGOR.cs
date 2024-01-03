namespace KONGOR.MasterServer;

internal class KONGOR
{
    internal static void Main(string[] args)
    {
        // Sets The Server Start Epoch Time (This Will Stop Working In 2038)
        // KongorContext.ServerStartEpochTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add Aspire Service Defaults
        builder.AddServiceDefaults();

        // Add The Database Context
        builder.AddSqlServerDbContext<MerrickContext>("MERRICK", configureSettings: null, configureDbContextOptions: options =>
        {
            options.EnableDetailedErrors(builder.Environment.IsDevelopment());
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
            options.EnableThreadSafetyChecks();
        });

        // Add Memory Cache
        builder.Services.AddMemoryCache();

        // Add MVC Controllers
        builder.Services.AddControllers();

        // Add Comprehensive Error Responses
        if (builder.Environment.IsDevelopment())
            builder.Services.AddProblemDetails();

        // Add Swagger
        builder.Services.AddSwaggerGen();

        // TODO: Clean This Up

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
        //                    ID = JwtBearerDefaults.AuthenticationScheme
        //                },
        //                Scheme = "oauth2",
        //                Name = JwtBearerDefaults.AuthenticationScheme,
        //                In = ParameterLocation.Header
        //            },
        //            new List<string>()
        //        }
        //    });

        /*
         *
         *    options.SwaggerDoc("v1", new OpenApiInfo
           {
               Version = "v1",
               Title = "ToDo API",
               Description = "An ASP.NET Core Web API for managing ToDo items",
               TermsOfService = new Uri("https://example.com/terms"),
               Contact = new OpenApiContact
               {
                   Name = "Example Contact",
                   Url = new Uri("https://example.com/contact")
               },
               License = new OpenApiLicense
               {
                   Name = "Example License",
                   Url = new Uri("https://example.com/license")
               }
           });
         *
         */
        //});

        // builder.Services.AddAntiforgery(); ???

        // Build The Application
        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.InjectStylesheet("/swagger/custom.css");
                // https://github.com/Amoenus/SwaggerDark/releases
                // https://blog.elijahlopez.ca/posts/aspnet-swagger-dark-theme/
                // https://amoenus.dev/swagger-dark-theme
            });
        }

        else
        {
            app.UseExceptionHandler();
        }

        // Map Aspire Default Endpoints
        app.MapDefaultEndpoints();

        // Map MVC Controllers
        app.MapControllers();

        // Run The Application
        app.Run();
    }
}
