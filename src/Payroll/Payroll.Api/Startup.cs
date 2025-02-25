using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Payroll.Api.Models;
using Payroll.Api.Services;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;

namespace Payroll.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<PayrollDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("PayrollConnection"), b => b.MigrationsAssembly("Payroll.Models"))
                .EnableSensitiveDataLogging());

            services.AddDbContext<AccountDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("AccountConnection"), b => b.MigrationsAssembly("Payroll.Models"))
                .EnableSensitiveDataLogging());

            services.AddDbContext<LogDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LogConnection"), b => b.MigrationsAssembly("Payroll.Models"))
                .EnableSensitiveDataLogging());

            services.AddIdentity<AppUser, AppRole>()
                .AddEntityFrameworkStores<AccountDbContext>()
                .AddDefaultTokenProviders();


            //services.AddHangfire(config =>
            //    config.UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new Hangfire.SqlServer.SqlServerStorageOptions
            //    {
            //        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            //        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            //        // QueuePollInterval = TimeSpan.Zero,
            //        UseRecommendedIsolationLevel = true,
            //        UsePageLocksOnDequeue = true,
            //        DisableGlobalLocks = true,
            //        PrepareSchemaIfNecessary = true,
            //    }));


            services.AddCors();

            //FirebaseApp.Create();
            //services.AddMvc();
            //services.AddMvcCore()
            //    .AddApiExplorer()
            //    .AddAuthorization()
            //    //.AddJsonFormatters()
            //    .AddJsonOptions(options => {
            //        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            //        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //        options.SerializerSettings.FloatFormatHandling = Newtonsoft.Json.FloatFormatHandling.DefaultValue;

            //        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            //    });

            // order is vital, this *must* be called *after* AddNewtonsoftJson()
            services.AddSwaggerGenNewtonsoftSupport();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(name: "v1", info: new OpenApiInfo
                {
                    Title = "PayAll API",
                    Version = "v1",
                    Description = "A simple example .NET Core Web API",
                    TermsOfService = new Uri("https://example.com/terms"),
                });


                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);


                // add jwt token to authorization header
                var securityScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Description = "JWT Token gerekiyor!",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                };
                c.AddSecurityDefinition("Bearer", securityScheme);

                // Security Requirement
                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                    { securityScheme, Array.Empty<string>() }
                });
            });

            // add enum convertor support
            services.AddSwaggerGenNewtonsoftSupport();

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            // code taken from link below
            // https://jasonwatmore.com/post/2020/05/25/aspnet-core-3-api-jwt-authentication-with-refresh-tokens
            //services.AddAuthentication(x =>
            //{
            //    //x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    //x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            //.AddJwtBearer(x =>
            //{
            //    x.RequireHttpsMetadata = false;
            //    x.SaveToken = true;
            //    x.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuerSigningKey = true,
            //        IssuerSigningKey = new SymmetricSecurityKey(key),
            //        ValidateIssuer = false,
            //        ValidateAudience = false,
            //        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
            //        ClockSkew = TimeSpan.Zero
            //    };
            //});


            //services.AddHangfire(config =>
            //    config.UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new Hangfire.SqlServer.SqlServerStorageOptions
            //    {
            //        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            //        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            //        // QueuePollInterval = TimeSpan.Zero,
            //        UseRecommendedIsolationLevel = true,
            //        UsePageLocksOnDequeue = true,
            //        DisableGlobalLocks = true,
            //        PrepareSchemaIfNecessary = true,
            //    }));


            // configure DI for application services
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, Payroll.Services.CustomClaimsPrincipalFactory>();
            services.AddScoped<IUserAuthService, UserAuthService>();
            services.AddScoped<EmployeeService>();
            services.AddScoped<PayrollService>();
            services.AddScoped<CompanyService>();
            services.AddScoped<ScheduleService>();
            services.AddScoped<RequestService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<AccessGrantService>();
            services.AddScoped<BackgroundJobService>();
            services.AddScoped<UserResolverService>();
            // services.AddScoped<ZeekboxService>();

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
        {
            app.UseDeveloperExceptionPage();

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}


            app.UseAuthentication();
            //app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(url: "v1/swagger.json", name: "PayAll API v1");
            });

            // global cors policy
            app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            //app.ConfigureExceptionHandler(logger);


            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
