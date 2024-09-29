using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.S3;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Payroll.Database;
using Payroll.Filters;
using Payroll.Models;
using Payroll.Services;
using Rotativa.AspNetCore;
using Serilog;
using Serilog.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Linq;

namespace Payroll
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            // services.AddMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None; //SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
                // options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
                // options.Secure = CookieSecurePolicy.Always;
                // options.MinimumSameSitePolicy = SameSiteMode.None; 
                options.OnAppendCookie = cookieContext =>  
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions); 
                options.OnDeleteCookie = cookieContext =>  
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions); 
            });


            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddDbContext<PayrollDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("PayrollConnection"))
                .EnableSensitiveDataLogging());

            ////First register a custom made db context provider
            //services.AddTransient<IPayrolDbContextFactory, PayrolDbContextFactory>();
            ////Then use implementation factory to get the one you need
            //services.AddTransient(provider => provider.GetService<IPayrolDbContextFactory>().CreateApplicationDbContext());

            services.AddDbContext<AccountDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("AccountConnection"),
                x => x.MigrationsAssembly("Payroll.Models"))
                .EnableSensitiveDataLogging());

            services.AddDbContext<JobScrapeDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("JobScrapeConnection"))
                .EnableSensitiveDataLogging());

            services.AddDbContext<LogDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LogConnection"), b => b.MigrationsAssembly("Payroll.Models"))
                .EnableSensitiveDataLogging());


            //services.AddDefaultIdentity<AppUser>().AddRoles<AppRole>().AddEntityFrameworkStores<AccountDbContext>()
            //    .AddDefaultTokenProviders();

            services.AddIdentity<AppUser, AppRole>()
                .AddEntityFrameworkStores<AccountDbContext>()
                //.AddDefaultUI(Microsoft.AspNetCore.Identity.UI.UIFramework.Bootstrap4)
                .AddDefaultTokenProviders();

            // Add Custom Claims processor
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, CustomClaimsPrincipalFactory>();

            
    //          services.AddAuthentication(options => {               
    // options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //             // options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //             // options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    //             // options.DefaultAuthenticateScheme = IdentityConstants.ExternalScheme;
    //             // options.DefaultScheme = IdentityConstants.ApplicationScheme;
    //             // options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    //          })
    //         // .AddCookie(IdentityConstants.ExternalScheme)
            services.AddAuthentication()
            .AddGoogle(googleOptions =>  
            {  
                // googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
                // googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
                googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];  
                googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                
                googleOptions.Scope.Add("profile");
                // googleOptions.Events.OnCreatingTicket = (context) =>
                // {
                //     context.Identity.AddClaim(new Claim("image", context.User.GetValue("image").SelectToken("url").ToString()));

                //     return Task.CompletedTask;
                // };  

                googleOptions.ClaimActions.Clear();
                googleOptions.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                googleOptions.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                googleOptions.ClaimActions.MapJsonKey("urn:google:profile", "link");
                googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                googleOptions.ClaimActions.MapJsonKey("image", "picture");
                // googleOptions.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v1/certs";
                // googleOptions.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";    
                // googleOptions.Scope.Add("email");
                // googleOptions.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
                // googleOptions.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");
                // googleOptions.SaveTokens = true;
                // googleOptions.Events.OnTicketReceived = (context) =>
                // {
                //     Console.WriteLine(context.HttpContext.User);
                //     return Task.CompletedTask;
                // };
                // googleOptions.Events.OnCreatingTicket = (context) =>
                // {
                //     Console.WriteLine(context.Identity);
                //     return Task.CompletedTask;
                // };

                // googleOptions.Events.OnCreatingTicket = ctx =>
                // {
                //     List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();

                //     tokens.Add(new AuthenticationToken()
                //     {
                //         Name = "TicketCreated",
                //         Value = DateTime.UtcNow.ToString()
                //     });

                //     ctx.Properties.StoreTokens(tokens);

                //     return Task.CompletedTask;
                // };

                // googleOptions.ClaimActions.MapAllExcept("iss", "nbf", "exp", "aud", "nonce", "iat", "c_hash");

                // googleOptions.ClaimActions.Clear();
                // googleOptions.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                // googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                // googleOptions.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                // googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                // googleOptions.ClaimActions.MapJsonKey("urn:google:profile", "link");
                // googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

            });


            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });



            // services.ConfigureApplicationCookie(options =>
            // {
            //     // Cookie settings
            //     options.Cookie.HttpOnly = true;
            //     options.Cookie.Expiration = TimeSpan.FromDays(150);
            //     // If the LoginPath isn't set, ASP.NET Core defaults 
            //     // the path to /Account/Login.
            //     options.LoginPath = "/Account/Login";
            //     // If the AccessDeniedPath isn't set, ASP.NET Core defaults 
            //     // the path to /Account/AccessDenied.
            //     options.AccessDeniedPath = "/Account/AccessDenied";
            //     options.SlidingExpiration = true;

            //     // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            //     // options.Cookie.SameSite = SameSiteMode.Strict;
            //     // // options.Cookie.HttpOnly = true;

            // });

            services.AddHangfire(config =>
                config.UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new Hangfire.SqlServer.SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    // QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true,
                    PrepareSchemaIfNecessary = true,
                }));


            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(120);
                options.Cookie.HttpOnly = true;
            });


            services.AddControllersWithViews().AddNewtonsoftJson();
            // AddSessionStateTempDataProvider()
            services.AddRazorPages();
            //services.AddMvc()
            //.AddSessionStateTempDataProvider()
            //.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();

            services.AddTransient<UserResolverService>();
            services.AddTransient<PayAdjustmentService>();
            services.AddTransient<PayrollService>(); ;
            services.AddTransient<SearchService>();
            services.AddTransient<EmployeeService>();
            services.AddTransient<AccessGrantService>();
            services.AddTransient<FileUploadService>();
            services.AddTransient<SynchronizationService>();
            services.AddTransient<ScheduleService>();
            services.AddTransient<RequestService>();
            services.AddTransient<CompanyService>();
            services.AddTransient<NotificationService>();
            services.AddTransient<ScheduledSystemTaskService>();
            services.AddTransient<EventLogService>();

            services.AddScoped<BackgroundJobService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<AuditLogService>();
            //services.AddTransient<ZeekboxService>();


            //services.AddCors(options => options.AddPolicy("CorsPolicy",
            //builder =>
            //{
            //    builder.AllowAnyMethod().AllowAnyHeader()
            //           .WithOrigins("http://localhost:44302")
            //           .AllowCredentials();
            //}));


            services.AddSignalR();
        }

        private void CheckSameSite(HttpContext httpContext, CookieOptions options) 
        { 
            if (options.SameSite == SameSiteMode.None) 
            { 
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString(); 
                if ( DisallowsSameSiteNone(userAgent)) 
                { 
                    options.SameSite = SameSiteMode.None; 
                } 
            } 
        }


 //  Read comments in https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
        public bool DisallowsSameSiteNone(string userAgent)
        {
            // Check if a null or empty string has been passed in, since this
            // will cause further interrogation of the useragent to fail.
            if (String.IsNullOrWhiteSpace(userAgent))
                return false;

            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS networking
            // stack.
            if (userAgent.Contains("CPU iPhone OS 12") ||
                userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. 
            // This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }

            return false;
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            //}

            app.UseStatusCodePages(context => {
                var request = context.HttpContext.Request;
                var response = context.HttpContext.Response;

                if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    response.Redirect("/account/login?returnUrl=" + context.HttpContext.Request.Path);
                }

                return Task.CompletedTask;
            });

            // app.UseForwardedHeaders();


            //app.UseSerilogRequestLogging();

            // Seed(app);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession(new SessionOptions { IdleTimeout = TimeSpan.FromSeconds(60) });

            app.UseRouting();

            //app.UseCookiePolicy(); // Before UseAuthentication or anything else that writes cookies.
            //app.UseAuthentication();
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseSerilogRequestLogging();
            app.Use(async (httpContext, next) =>
            {
                var userName = httpContext.User.Identity.IsAuthenticated ? httpContext.User.Identity.Name : "unknown";
                LogContext.PushProperty("UserId", httpContext.User.Identity.IsAuthenticated ? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) : "unknown");
                LogContext.PushProperty("UserName", httpContext.User.Identity.IsAuthenticated ? httpContext.User.Identity.Name : "");
                LogContext.PushProperty("Environment", env?.EnvironmentName ?? "NA");
                await next.Invoke();
            });

            //app.UseCors("CorsPolicy");

            // app.UseSignalR(route =>{route.MapHub<SignalServer>("/SignalServer");});

            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
            });

            HangfireJobScheduler.ScheduleRecurringJobs();

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});

            // RotativaConfiguration.Setup(env);



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        [NonAction]
        public void PushSeriLogProperties(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            LogContext.PushProperty("UserId", httpContext.User.Identity.Name);
            diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
            diagnosticContext.Set("Environment", httpContext.RequestServices.GetService<IHostingEnvironment>().EnvironmentName);
        }


        //public void Seed(IApplicationBuilder applicationBuilder)
        //{
        //    using (var serviceScope = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
        //            .CreateScope())
        //    {
        //        JobScrapeDbContext js_context = serviceScope.ServiceProvider.GetService<JobScrapeDbContext>();
        //        PayrollDbContext context = serviceScope.ServiceProvider.GetService<PayrollDbContext>();
        //        AccountDbContext accountDbContext = serviceScope.ServiceProvider.GetService<AccountDbContext>();
        //        PayrollService payrollService = serviceScope.ServiceProvider.GetService<PayrollService>();
        //        PayAdjustmentService payAdjustmentService = serviceScope.ServiceProvider.GetService<PayAdjustmentService>();

        //        var RoleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        //        var UserManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        //        js_context.Database.EnsureCreated();

        //        CompanyAccount companyAccount = null;
        //        if (!accountDbContext.AppUsers.Any())
        //        {

        //            /// ---------- ROLES AND AUTH --------------- //
        //            IdentityResult roleResult;
        //            //Adding Admin Role
        //            var task = Task.Run(async () => await RoleManager.RoleExistsAsync(Roles.PayAll.admin));
        //            var roleCheck = task.Result;
        //            if (!roleCheck)
        //            {
        //                //create the roles and seed them to the database
        //                RoleManager.CreateAsync(new IdentityRole(Roles.PayAll.admin)).Wait();
        //            }

        //            var userId = "7fadeeb8-f941-11e9-aad5-362b9e155667";
        //            //Assign Admin role to the main User here we have given our newly registered 
        //            //login id for Admin management
        //            AppUser sysUser = new AppUser
        //            {
        //                UserName = "system@server.com",
        //                Email = "system@server.com",
        //                SecurityStamp = DateTime.Now.Ticks.ToString(),
        //                Id = userId,
        //                IsActive = true,
        //                LockoutEnabled = true,
        //                Avatar = "~/img/default-image.png"
        //            };

        //            UserManager.CreateAsync(sysUser, sysUser.UserName).Wait();
        //            UserManager.AddToRoleAsync(sysUser, Roles.PayAll.admin).Wait();

        //            AppUser normalUser = new AppUser
        //            {
        //                UserName = "normal@server.com",
        //                Email = "normal@server.com",
        //                SecurityStamp = DateTime.Now.Ticks.ToString(),
        //                Id = Guid.NewGuid().ToString(),
        //                IsActive = true,
        //                LockoutEnabled = true,
        //                Avatar = "~/img/default-image.png"
        //            };

        //            UserManager.CreateAsync(normalUser, normalUser.UserName).Wait();
        //            UserManager.AddToRoleAsync(sysUser, Roles.PayAll.admin).Wait();

        //            companyAccount = new CompanyAccount
        //            {
        //                Address = "Cecilia Chapman 711-2880 Nulla St. Mankato Mississippi 96522",
        //                Hotline = "(372) 587-2335",
        //                Name = "Raajje Televison Pvt Ltd",
        //                TaxCode = "GST (6%)",
        //                TaxPercentValue = 6,
        //                Website = "https://www.summet.com/",
        //                LogoUrl = "~/img/default-image.png",
        //                //InitialCatalog = "Raajje Televison Pvt Ltd".GenerateSlug(),
        //                //DataSource = ".",
        //                //IntegratedSecurity = true,
        //            };
        //            //companyAccount.CONNECTION_STRING = $"Server=.;Initial Catalog={companyAccount.InitialCatalog};Integrated Security=True;MultipleActiveResultSets=True";
        //            //companyAccount.ConnectionStatus = true;
        //            accountDbContext.CompanyAccounts.Add(companyAccount);
        //            accountDbContext.SaveChanges();

        //        }


        //        if (context.Employees.Any()) return;

        //        /// Company And Deptments
        //        var company = new Company
        //        {
        //            Id = companyAccount.Id,
        //            Address = "Cecilia Chapman 711-2880 Nulla St. Mankato Mississippi 96522",
        //            Hotline = "(372) 587-2335",
        //            Name = "Raajje Televison Pvt Ltd",
        //            TaxCode = "GST (6%)",
        //            TaxPercentValue = 6,
        //            Website = "https://www.summet.com/",
        //            LogoUrl = "~/img/default-image.png",
        //            Departments = new List<Department>
        //            {
        //                new Department
        //                {
        //                    DeptCode = "ADMIN",
        //                    Name = "Admin",
        //                    TotalHeadCount = 10,
        //                },
        //                new Department
        //                {
        //                    DeptCode = "FINANCE",
        //                    Name = "FINANCE",
        //                    TotalHeadCount = 10,
        //                }
        //            }
        //        };
        //        context.Companies.Add(company);
        //        context.SaveChanges();


        //        // ------- DEDUCTION ---------------------
        //        var add_excepPerformAll = new PayAdjustment { Name = "Exceptional Performance Allowance" };
        //        var add_basicSalary = new PayAdjustment { Name = "Basic Salary", IsFilledByEmployee = true, VariationType = VariationType.ConstantAddition, CalculationOrder = 0 };
        //        var add_phoneAll = new PayAdjustment { Name = "Phone Allowance", IsFilledByEmployee = true, VariationType = VariationType.ConstantAddition, CalculationOrder = 2 };
        //        var add_logngSerivAll = new PayAdjustment { Name = "Service Allowance", IsFilledByEmployee = true, VariationType = VariationType.ConstantAddition, CalculationOrder = 1 };
        //        var add_ServiceAll = new PayAdjustment
        //        { Name = "Production Sheet Add",
        //            Fields = new List<PayAdjustmentFieldConfig> {
        //                new PayAdjustmentFieldConfig
        //                {
        //                    BaseType = BaseType.ComputedList,
        //                    ListType = ListType.Employee,
        //                    ListSelect = "Name",
        //                    DisplayName = "Name"
        //                },
        //                new PayAdjustmentFieldConfig
        //                {
        //                    BaseType = BaseType.Calculated,
        //                    DisplayName = "Basic Salary",
        //                    Calculation = "{employee.Basic Salary}",
        //                    CalculationIdentifier = "BasicSalary",
        //                },
        //                new PayAdjustmentFieldConfig
        //                {
        //                    BaseType = BaseType.Calculated,
        //                    FieldType = FieldType.Number,
        //                    Calculation = "{employee.Basic Salary}/2",
        //                    DisplayName = "Salary/2",
        //                    CalculationIdentifier = "Salary",
        //                },
        //                new PayAdjustmentFieldConfig
        //                {
        //                    BaseType = BaseType.ManualEntry,
        //                    FieldType = FieldType.Number,
        //                    DisplayName = "No. of Articles",
        //                    CalculationIdentifier = "No. of Articles",
        //                },
        //                new PayAdjustmentFieldConfig
        //                {
        //                    BaseType = BaseType.Calculated,
        //                    Calculation = "{field.no-of-articles}*15",
        //                    IsClientCalculatable = true,
        //                    DisplayName = "Total",
        //                    CalculationIdentifier = "Total",
        //                    IsReturn =true
        //                }
        //            }
        //        };
        //        var add_extraNews = new PayAdjustment
        //        {
        //            Name = "Extra News",
        //            Fields = new List<PayAdjustmentFieldConfig> {
        //                new PayAdjustmentFieldConfig
        //                {
        //                    BaseType = BaseType.ComputedList,
        //                    ListType = ListType.Employee,
        //                    ListSelect = "Name",
        //                    DisplayName = "Emp"
        //                },
        //                new PayAdjustmentFieldConfig
        //                {
        //                    BaseType = BaseType.ManualEntry,
        //                    FieldType = FieldType.Number,
        //                    DisplayName = "No. of Extra News",
        //                    CalculationIdentifier = "no-of-extra-news",
        //                },
        //                new PayAdjustmentFieldConfig
        //                {
        //                    BaseType = BaseType.Calculated,
        //                    Calculation = "{field.no-of-extra-news}*15",
        //                    IsClientCalculatable = true,
        //                    DisplayName = "Total",
        //                    CalculationIdentifier = "Total",
        //                    IsReturn =true
        //                }
        //            }
        //        };


        //        var additions = new List<PayAdjustment> { add_basicSalary, add_excepPerformAll, add_ServiceAll, add_phoneAll, add_extraNews, add_logngSerivAll };


        //        // ------- DEDUCTION ---------------------
        //        var ded_pensionCharges = new PayAdjustment { Name = "Pension Charges", CalculationOrder = 1 };
        //        var ded_LateDeductions = new PayAdjustment { Name = "Late Deductions", CalculationOrder = 2 };
        //        var ded_newLatePresenterProducer = new PayAdjustment { Name = "News Late Presenter Producer Fine" };
        //        var ded_prodSheetFine = new PayAdjustment { Name = "Production Sheet Fine" };
        //        var ded_programFine = new PayAdjustment
        //        {
        //            Name = "Program Fine",
        //            Fields = new List<PayAdjustmentFieldConfig> {
        //                 new PayAdjustmentFieldConfig { BaseType = BaseType.ManualEntry, FieldType = FieldType.Date, DisplayName = "Date" },
        //                 new PayAdjustmentFieldConfig { BaseType = BaseType.ComputedList, ListType = ListType.Employee, ListSelect = "Name", DisplayName="Name" },
        //                 new PayAdjustmentFieldConfig { BaseType = BaseType.ManualEntry, FieldType = FieldType.Text, DisplayName = "Program name" },
        //                 new PayAdjustmentFieldConfig { BaseType = BaseType.ManualEntry, FieldType = FieldType.Number, DisplayName = "No. Of Programs", CalculationIdentifier = "Program-count" },
        //                 new PayAdjustmentFieldConfig { BaseType = BaseType.Calculated, Calculation = "{field.no-of-programs} * 100", DisplayName= "Total", IsReturn =true }
        //             }
        //        };

        //        var dedcutions = new List<PayAdjustment> { ded_pensionCharges, ded_LateDeductions, ded_newLatePresenterProducer, ded_prodSheetFine, ded_programFine };

        //        additions.ForEach(d => { if(!d.IsFilledByEmployee.HasValue) d.VariationType = VariationType.VariableAddition; d.Fields.ForEach(f => f.CalculationIdentifier = f.DisplayName.GenerateSlug()); });
        //        dedcutions.ForEach(d => { d.VariationType = VariationType.VariableDeduction; d.Fields.ForEach(f => f.CalculationIdentifier = f.DisplayName.GenerateSlug()); });

        //        context.PayAdjustments.AddRange(additions);
        //        context.PayAdjustments.AddRange(dedcutions);
        //        context.SaveChanges();


        //        for (int i = 0; i < additions.Count; i++)
        //        {
        //            if (additions[i].Fields.Count > 0)
        //                for (int j = 0; j < additions[i].Fields.Count; j++)
        //                    additions[i].Fields[j] = payAdjustmentService.SetEvaluationFields(additions[i].Fields[j]);
        //        }
        //        for (int i = 0; i < dedcutions.Count; i++)
        //        {
        //            if (dedcutions[i].Fields.Count > 0)
        //                for (int j = 0; j < dedcutions[i].Fields.Count; j++)
        //                    dedcutions[i].Fields[j] = payAdjustmentService.SetEvaluationFields(dedcutions[i].Fields[j]);
        //        }

        //        var compileAdjFielsd = additions.SelectMany(x => x.Fields).ToList();
        //        compileAdjFielsd.AddRange(dedcutions.SelectMany(x => x.Fields).ToList());
                
        //        context.PayAdjustmentFieldConfigs.UpdateRange(compileAdjFielsd);
        //        context.SaveChanges();


        //        // ------- EMPLOYEES ---------------------
        //        var emp_husham = new Employee { EmpID = "1", FirstName = "Mohamed",LastName= "Husham", JobTitle = "News Director", DateOfJoined = new DateTime(2017, 07, 30), JobType = JobType.FullTime, Department = company.Departments.First(), DepartmentId = context.Departments.First().Id };
        //        var emp_uhaamath = new Employee { EmpID = "2", FirstName = "Aishath", LastName = "Jumaan", JobTitle = "HR & Admin Assistant", DateOfJoined = new DateTime(2017, 09, 04), JobType = JobType.FullTime, Department = company.Departments.First(), DepartmentId = context.Departments.First().Id };
        //        var emp_shan = new Employee { EmpID = "3", FirstName = "Shan", LastName = "Ahmed", JobTitle = "Office Assistant", DateOfJoined = new DateTime(2017, 09, 04), JobType = JobType.FullTime, Department = company.Departments.Last(), DepartmentId = context.Departments.Last().Id };

        //        var random = new Random();
        //        var employees = new List<Employee> { emp_husham, emp_shan, emp_uhaamath };
        //        foreach (var emp in employees)
        //        {
        //            emp.DepartmentId = company.Departments.First().Id;
        //            emp.EmployeePayComponents.AddRange(additions.Where(x => x.IsFilledByEmployee.HasValue && x.IsFilledByEmployee.Value== true)
        //                .Select(ajusments => new EmployeePayComponent
        //                {
        //                    //Adjustment = ajusments.Name,
        //                    EmployeeId = emp.Id,
        //                    Employee = emp,
        //                    Total = random.Next(400, 5000),
        //                    //VariationType = ajusments.VariationType,
        //                    PayAdjustment = ajusments,
        //                    PayAdjustmentId = ajusments.Id
        //                }).ToList());
        //        }

        //        context.Employees.AddRange(employees);
        //        context.SaveChanges();



        //        /// ----------------- PAYROLL STATRTING BELOW -----------------//////////////

        //        var payrollPeriodAug2019 = new PayrollPeriod
        //        {
        //            CompanyId = company.Id,
        //            Name = "August 2019",
        //            StartDate = new DateTime(2019, 8, 1),
        //            EndDate = new DateTime(2019, 8, 30),

        //            PayrollPeriodPayAdjustments = new List<PayrollPeriodPayAdjustment>
        //                    {
        //                        // phone allowance
        //                        new PayrollPeriodPayAdjustment
        //                        {
        //                            Employee = emp_husham,
        //                            EmployeeName = emp_husham.Name,
        //                            EmployeeId = emp_husham.Id,
        //                            Adjustment = add_phoneAll.Name,
        //                            PayAdjustmentId = add_phoneAll.Id,
        //                            PayAdjustment = add_phoneAll,
        //                            CalculationOrder = add_phoneAll.CalculationOrder,
        //                            Total = 2000,
        //                            VariationType = VariationType.VariableAddition,
        //                        },

        //                        // Service allowance
        //                        new PayrollPeriodPayAdjustment
        //                        {
        //                            Employee = emp_husham,
        //                            EmployeeName = emp_husham.Name,
        //                            EmployeeId = emp_husham.Id,
        //                            Adjustment = add_ServiceAll.Name,
        //                            PayAdjustmentId = add_ServiceAll.Id,
        //                            PayAdjustment = add_ServiceAll,
        //                            CalculationOrder = add_ServiceAll.CalculationOrder,
        //                            Total = 4000,
        //                            VariationType = VariationType.VariableAddition
        //                        },
        //                         new PayrollPeriodPayAdjustment
        //                        {
        //                            Employee = emp_uhaamath,
        //                            EmployeeName = emp_uhaamath.Name,
        //                            EmployeeId = emp_uhaamath.Id,
        //                            Adjustment = add_phoneAll.Name,
        //                            PayAdjustmentId = add_phoneAll.Id,
        //                            PayAdjustment = add_phoneAll,
        //                            CalculationOrder = add_phoneAll.CalculationOrder,
        //                            Total = 750,
        //                            VariationType = VariationType.VariableAddition
        //                        },

        //                        new PayrollPeriodPayAdjustment
        //                        {
        //                            Employee = emp_uhaamath,
        //                            EmployeeName = emp_uhaamath.Name,
        //                            EmployeeId = emp_uhaamath.Id,
        //                            Adjustment = add_ServiceAll.Name,
        //                            PayAdjustmentId = add_ServiceAll.Id,
        //                            PayAdjustment = add_ServiceAll,
        //                            CalculationOrder = add_ServiceAll.CalculationOrder,
        //                            Total = 200,
        //                            VariationType = VariationType.VariableAddition
        //                        },
        //                        new PayrollPeriodPayAdjustment
        //                        {
        //                            Employee = emp_husham,
        //                            EmployeeName = emp_husham.Name,
        //                            EmployeeId = emp_husham.Id,
        //                            Adjustment = ded_LateDeductions.Name,
        //                            PayAdjustmentId = ded_LateDeductions.Id,
        //                            PayAdjustment = ded_LateDeductions,
        //                            CalculationOrder = ded_LateDeductions.CalculationOrder,
        //                            Total = 1269.44m,
        //                            VariationType = VariationType.VariableAddition
        //                            // week1, week2
        //                        },
        //                        new PayrollPeriodPayAdjustment
        //                        {
        //                            Employee = emp_husham,
        //                            EmployeeName = emp_husham.Name,
        //                            EmployeeId = emp_husham.Id,
        //                            Adjustment = ded_newLatePresenterProducer.Name,
        //                            PayAdjustmentId = ded_newLatePresenterProducer.Id,
        //                            PayAdjustment = ded_newLatePresenterProducer,
        //                            CalculationOrder = ded_newLatePresenterProducer.CalculationOrder,
        //                            Total = 1200m,
        //                            VariationType = VariationType.VariableAddition
        //                            // week1, week2
        //                        },
        //                        new PayrollPeriodPayAdjustment
        //                        {
        //                            Employee = emp_husham,
        //                            EmployeeName = emp_husham.Name,
        //                            EmployeeId = emp_husham.Id,
        //                            Adjustment = ded_pensionCharges.Name,
        //                            PayAdjustment = ded_pensionCharges,
        //                            PayAdjustmentId = ded_pensionCharges.Id,
        //                            CalculationOrder = ded_pensionCharges.CalculationOrder,
        //                            Total = 140,
        //                            VariationType = VariationType.VariableAddition
        //                        },

        //                        new PayrollPeriodPayAdjustment
        //                        {
        //                            Employee = emp_uhaamath,
        //                            EmployeeName = emp_uhaamath.Name,
        //                            EmployeeId = emp_uhaamath.Id,
        //                            Adjustment = ded_LateDeductions.Name,
        //                            PayAdjustment = ded_LateDeductions,
        //                            PayAdjustmentId = ded_LateDeductions.Id,
        //                            CalculationOrder = add_ServiceAll.CalculationOrder,
        //                            Total = 0,
        //                            VariationType = VariationType.VariableAddition
        //                            // week1, week2
        //                        },
        //                        new PayrollPeriodPayAdjustment
        //                        {
        //                            Employee = emp_uhaamath,
        //                            EmployeeName = emp_uhaamath.Name,
        //                            EmployeeId = emp_uhaamath.Id,
        //                            Adjustment = ded_newLatePresenterProducer.Name,
        //                            PayAdjustment = ded_newLatePresenterProducer,
        //                            PayAdjustmentId = ded_newLatePresenterProducer.Id,
        //                            CalculationOrder = ded_newLatePresenterProducer.CalculationOrder,
        //                            Total = 0,
        //                            VariationType = VariationType.VariableAddition
        //                            // week1, week2
        //                        },
        //                        new PayrollPeriodPayAdjustment
        //                        {
        //                            Employee = emp_uhaamath,
        //                            EmployeeName = emp_uhaamath.Name,
        //                            EmployeeId = emp_uhaamath.Id,
        //                            Adjustment = ded_pensionCharges.Name,
        //                            PayAdjustment = ded_pensionCharges,
        //                            PayAdjustmentId = ded_pensionCharges.Id,
        //                            CalculationOrder = ded_pensionCharges.CalculationOrder,
        //                            Total = 175,
        //                            VariationType = VariationType.VariableAddition
        //                        },
        //                    },
        //        };


        //        // add constand additions and contant deductions (ex; Basic Salary, Phoen Allow)
        //        payrollPeriodAug2019.PayrollPeriodPayAdjustments.AddRange(

        //                employees.SelectMany(x => x.EmployeePayComponents)
        //                .Select(x => new PayrollPeriodPayAdjustment
        //                {
        //                    Adjustment = x.PayAdjustment.Name,
        //                    Employee = x.Employee,
        //                    EmployeeId = x.EmployeeId,
        //                    CalculationOrder = x.PayAdjustment.CalculationOrder,
        //                    EmployeeName = x.Employee.Name,
        //                    PayAdjustmentId = x.PayAdjustmentId,
        //                    PayAdjustment = x.PayAdjustment,
        //                    Total = x.Total,
        //                    VariationType = x.PayAdjustment.VariationType
        //                })
        //                .ToList()
        //            );


        //        /// FILLING UP PAY ADJUSTMENTS AND SOME FIELD VALUES
        //        //for (int i = 0; i < payrollPeriodAug2019.PayrollPeriodPayAdjustments.Count; i++)
        //        //{
        //        //    if(payrollPeriodAug2019.PayrollPeriodPayAdjustments[i].PayAdjustment.Fields.Count > 0)
        //        //    {
        //        //        var fieldConfigs =  payrollPeriodAug2019.PayrollPeriodPayAdjustments[i].PayAdjustment.Fields;

        //        //        var fieldValues = payAdjustmentService.GetFieldValues(fieldConfigs, payrollPeriodAug2019.PayrollPeriodPayAdjustments[i].Employee, payrollPeriodAug2019.PayrollPeriodPayAdjustments[i].PayAdjustment);

        //        //        payrollPeriodAug2019.PayrollPeriodPayAdjustments[i]
        //        //            .PayrollPeriodPayAdjustmentFieldValues.AddRange(fieldValues);
        //        //    }
        //        //}
        //        context.PayrollPeriods.Add(payrollPeriodAug2019);
        //        context.SaveChanges();


        //        // RUNNNING PAYROL:L
        //        var allEmployees = new Employee[] { emp_husham, emp_uhaamath };
        //        var listPayrolEmplee = new List<PayrollPeriodEmployee>();
        //        PayrollPeriodEmployee _payEmployeeItem = null;
        //        foreach (var emp in allEmployees)
        //        {
        //            // create employer model
        //            _payEmployeeItem = new PayrollPeriodEmployee
        //            {
        //                EmployeeId = emp.Id,
        //                EmpID = emp.EmpID,
        //                Name = emp.Name,
        //                Designation = emp.JobTitle,
        //                BasicSalary = 2000m
        //            };

        //            payrollService.RunPayroll(ref _payEmployeeItem, payrollPeriodAug2019);
        //            // calculate Gross and Net Pay
        //            //_payEmployeeItem.GrossPay = _payEmployeeItem.BasicSalary + payrollPeriodAug2019.PayrollPeriodPayAdjustments.Where(x => x.VariationType == VariationType.Addition && x.EmployeeId == emp.Id)
        //            //    .OrderBy(x => x.PayAdjustment?.CalculationOrder)
        //            //    .Sum(x => x.Total);

        //            //_payEmployeeItem.NetSalary = _payEmployeeItem.GrossPay - payrollPeriodAug2019.PayrollPeriodPayAdjustments.Where(x=> x.VariationType == VariationType.Deduction)
        //            //    .Where(x => x.EmployeeId == emp.Id)
        //            //    .OrderBy(x => x.PayAdjustment?.CalculationOrder)
        //            //    .Sum(x => x.Total);


        //            //// add varition key values (pay adjustments)
        //            //_payEmployeeItem.VariationKeyValues.AddRange(
        //            //    payrollPeriodAug2019.PayrollPeriodPayAdjustments
        //            //    .Where(x=> x.EmployeeId == emp.Id)
        //            //    .OrderBy(x => x.CalculationOrder)
        //            //    .GroupBy(x=> new { x.Adjustment, x.VariationType, x.PayAdjustmentId, x.CalculationOrder})
        //            //    .Select(x => new VariationKeyValue
        //            //    {
        //            //        KeyId = x.Key.PayAdjustmentId,
        //            //        Value = x.Sum(z=> z.Total),
        //            //        Key = x.Key.Adjustment,
        //            //        MultiOrder = x.Key.CalculationOrder,
        //            //        Type = x.Key.VariationType
        //            //    }));

        //            // add varition key values (deduction)  ^^ MERGED WITH... ^^
        //            //_payEmployeeItem.VariationKeyValues.AddRange(
        //            //    payrollPeriodAug2019.PayrollPeriodDeductions
        //            //    .Where(x => x.EmployeeId == emp.Id)
        //            //    .OrderBy(x => x.CalculationOrder)
        //            //    .GroupBy(x => new { x.Deduction, x.DeductionId, x.CalculationOrder })
        //            //    .Select(x => new VariationKeyValue
        //            //    {
        //            //        KeyId = x.Key.DeductionId,
        //            //        Value = x.Sum(z => z.Total),
        //            //        Key = x.Key.Deduction,
        //            //        MultiOrder = x.Key.CalculationOrder,
        //            //        Type = VariationType.Deduction
        //            //    }));



        //            listPayrolEmplee.Add(_payEmployeeItem);
        //            _payEmployeeItem = null;
        //            // get employee unique variations
        //            // get addition--
        //            // if (payrollPeriodAug2019.PayrollPeriodAdditions)
        //        }

        //        payrollPeriodAug2019.PayrollPeriodEmployees = listPayrolEmplee;
        //        context.PayrollPeriods.Update(payrollPeriodAug2019);
        //        context.SaveChanges();
        //    }
        //}


        private ICollection<PayrollPeriodPayAdjustmentFieldValue> GetFieldValues(PayAdjustment payAdjustment, Employee emp_uhaamath)
        {
            var fieldValueList = new List<PayrollPeriodPayAdjustmentFieldValue>();
            var fieldValue = new PayrollPeriodPayAdjustmentFieldValue();
            foreach (var item in payAdjustment.Fields)
            {
                fieldValue = new PayrollPeriodPayAdjustmentFieldValue();
                //fieldValue.FieldType = item.FieldType;
                
                switch (item.BaseType)
                {
                    case BaseType.ComputedList:
                        switch (item.ListType)
                        {
                            case ListType.Employee:
                                fieldValue.ListSelect =  emp_uhaamath.GetType().GetProperty(item.ListSelect).GetValue(emp_uhaamath, null)?.ToString();
                                break;
                            default:
                            break;
                        }
                        break;
                    
                    default:
                    break;
                }
            }

            return fieldValueList;
        }
    }
    
}



/*
 * CompanySector
 * Agency,banking,Communications,Construction,Consulting,Engineering,Finance,Htoels and Restaurants,Media,Non Profit Organization,Other,Public,Startup,Technology
 * Employee Count 
 *  1-10, 11-20, 21-50, 50-150,150-300,300+
 *  Country
 * 
 * 
 */ 