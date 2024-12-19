using CacheManager.Core;
using Microsoft.AspNetCore.Identity;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using NLog;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UpRaise.Entities;
using UpRaise.Helpers;
using UpRaise.Models;
using UpRaise.Services;
using UpRaise.Services.EF;
using System.Net;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.ResponseCompression;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Stripe;
using Microsoft.AspNetCore.Rewrite;
using UpRaise.Helpers.Rules;
using EasyCaching.Core;

namespace UpRaise
{
    public class Startup
    {
        private readonly Logger _logger;

        readonly string CORSSpecificOrigins = "_corsSpecificOrigins";

        public Startup(IConfiguration configuration, Microsoft.Extensions.Hosting.IHostEnvironment hostEnvironment)
        {
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 8;

            _logger = LogManager.GetCurrentClassLogger(typeof(Startup));
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        public IConfiguration Configuration { get; }
        private Microsoft.Extensions.Hosting.IHostEnvironment HostEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                _logger.Info($"ConfigureServices Started");

                if (HostEnvironment.IsProduction())
                {
                    // The following will configure the channel to use the given folder to temporarily
                    // store telemetry items during network or Application Insights server issues.
                    // User should ensure that the given folder already exists
                    // and that the application has read/write permissions.
                    //services.AddSingleton(typeof(ITelemetryChannel),
                    //new ServerTelemetryChannel() { StorageFolder = "/tmp/appinsightupraise" });

                    // The following line enables Application Insights telemetry collection.
                    services.AddApplicationInsightsTelemetry();
                }

                services.AddHttpClient();

                var secret = Configuration["AuthenticationSecret"];
                var key = Encoding.ASCII.GetBytes(secret);
                services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                    .AddGoogle(options =>
                    {
                        options.ClientId = Configuration["OAuth:GoogleClientId"];
                        options.ClientSecret = Configuration["OAuth:GoogleClientSecret"];
                    })
                .AddJwtBearer(x =>
                {
                    x.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            //var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                            //var userId = int.Parse(context.Principal.Identity.Name);
                            //var user = userService.GetById(userId);

                            //var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<Entities.IDUser>>();
                            //var user = await userManager.GetUserAsync(context.HttpContext.User);
                            if (context.HttpContext.User == null)
                            {
                                // return unauthorized if user no longer exists
                                context.Fail("Unauthorized");
                            }

                            return Task.CompletedTask;
                        }
                    };
                    //x.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];

                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),

                        ValidateIssuer = false,
                        //ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                        ValidateAudience = false
                        //ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                        //RequireExpirationTime = false,
                        //ValidateLifetime = true,
                        //ClockSkew = TimeSpan.Zero
                    };
                })
                .AddIdentityCookies(o =>
                {
                    o.TwoFactorRememberMeCookie.Configure(a =>
                    {
                        a.Cookie.Name = "upraise.2fa.rememberme";
                        a.ExpireTimeSpan = TimeSpan.FromDays(14);
                    });
                });



                services.AddIdentityCore<Entities.IDUser>(options =>
                {
                    //options.SignIn.RequireConfirmedAccount = true;
                    options.SignIn.RequireConfirmedEmail = true;

                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequiredUniqueChars = 1;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;

                })
                    .AddSignInManager<SignInManager<IDUser>>()
                    .AddEntityFrameworkStores<AppDatabaseContext>()
                    .AddDefaultTokenProviders();


                /*
                services.AddIdentity<Entities.IDUser, Entities.IDRole>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequiredUniqueChars = 1;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                })
                    .AddEntityFrameworkStores<AppDatabaseContext>()
                    .AddDefaultTokenProviders();
                */



                #region register ef core second level cache


                const string providerName1 = "Redis1";

                services.AddEFSecondLevelCache(options =>
                   options.UseEasyCachingCoreProvider(providerName1, isHybridCache: false)
                       .DisableLogging(true)
                       .UseCacheKeyPrefix("EF_")
               );

                // More info: https://easycaching.readthedocs.io/en/latest/Redis/

                var redis_host = Configuration["ConnectionString:RedisHost"];
                var redis_port = Configuration["ConnectionString:RedisPort"];
                var redis_password = Configuration["ConnectionString:RedisPassword"];

                services.AddEasyCaching(option =>
                {
                    option.UseRedis(config =>
                    {
                        config.DBConfig.AllowAdmin = true;
                        config.DBConfig.AbortOnConnectFail = true;
                        config.DBConfig.IsSsl = true;
                        config.DBConfig.SyncTimeout = 10000;
                        config.DBConfig.AsyncTimeout = 10000;
                        config.DBConfig.Endpoints.Add(new EasyCaching.Core.Configurations.ServerEndPoint(redis_host, Convert.ToInt32(redis_port)));
                        config.DBConfig.Password = redis_password;
                    }, providerName1);
                });

                var sql_azure_connection = Configuration["ConnectionString:SQLAzure"];

                services.AddDbContextPool<AppDatabaseContext>((serviceProvider, optionsBuilder) =>
                        optionsBuilder
                            //.ConfigureWarnings(warnings => warnings.Throw(CoreEventId.ig))
                            .UseSqlServer(
                                sql_azure_connection,
                                sqlServerOptionsBuilder =>
                                {
                                    sqlServerOptionsBuilder
                                        .CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds)
                                        .EnableRetryOnFailure()
                                        .UseNetTopologySuite();
                                    //.MigrationsAssembly(typeof(MsSqlServiceCollectionExtensions).Assembly.FullName);

                                })
                                .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>()));
                #endregion

                services.AddHealthChecks();


                services.AddResponseCaching();

                /*
                services.AddHttpClient<AzureCognitiveServicesFormRecognizer>()
                     .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
                    .AddPolicyHandler(GetRetryPolicy())
                    .AddPolicyHandler(GetCircuitBreakerPolicy());
                */


                // configure strongly typed settings objects
                services.Configure<SESSettings>(Configuration.GetSection("SESSettings"));
                services.Configure<StorageAccountSettings>(Configuration.GetSection("StorageAccountSettings"));

                services.AddMvc()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });


                services.AddAutoMapper(typeof(UpRaise.Helpers.AutoMapperProfile));

                // Add functionality to inject IOptions<T>
                services.AddOptions();

                /*
                // Get options from app settings
                var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

                // Configure JwtIssuerOptions
                services.Configure<JwtIssuerOptions>(options =>
                {
                    options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                    options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                    options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
                });
                */
                // configure jwt authentication


                // api user claim policy
                //services.AddAuthorization(options =>
                //{
                //options.AddPolicy("ApiUser", policy => policy.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess));
                //});


                var origins = new List<string>() {"https://upraise.fund",
                                                  "https://www.upraise.fund",
                                                  "https://app.upraise.fund",
                                                  "http://app.upraise.fund",
                                                  "https://upraise-web.azureedge.net",
                                                  "https://upraise.z5.web.core.windows.net",
                                                  "http://localhost:3000" };

#if DEBUG
                origins.Add("http://localhost");
#endif

                services.AddCors(options =>
                {
                    options.AddPolicy(name: CORSSpecificOrigins,
                        builder =>
                        {
                            builder
                            .WithOrigins(origins.ToArray())
                            //.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                        });
                });


                services.AddResponseCompression(options =>
                {
                    options.MimeTypes =
                        ResponseCompressionDefaults.MimeTypes.Concat(
                            new[] { "image/svg+xml", "application/json" });
                });

                // configure DI for application services
                services.AddScoped<IUserService, UserService>();
                services.AddTransient<IBlobManager, BlobManager>();
                services.AddTransient<IEmail, AWSEmail>();
                services.AddTransient<EmailHelper>();
                services.AddTransient<SearchService>();

                services.AddControllersWithViews();
                //services.AddMvc()
                //.AddNewtonsoftJson(options =>
                //{
                //options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                //options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //});

                services.AddRazorPages();
                //services.AddControllers();

                //https://localhost:5002/swagger/index.html
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UpRaise", Version = "v1" });
                });


#if DEBUG
                //https://localhost:5001/profiler/results

                services.AddMiniProfiler(options =>
                {
                    // All of this is optional. You can simply call .AddMiniProfiler() for all defaults

                    // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                    options.RouteBasePath = "/profiler";

                    /*
                                        // (Optional) Control storage
                                        // (default is 30 minutes in MemoryCacheStorage)
                                        // Note: MiniProfiler will not work if a SizeLimit is set on MemoryCache!
                                        //   See: https://github.com/MiniProfiler/dotnet/issues/501 for details
                                        (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                                        // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                                        options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

                                        // (Optional) To control authorization, you can use the Func<HttpRequest, bool> options:
                                        // (default is everyone can access profilers)
                                        options.ResultsAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                                        options.ResultsListAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                                        // Or, there are async versions available:
                                        options.ResultsAuthorizeAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfiler;
                                        options.ResultsAuthorizeListAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfilerLists;

                                        // (Optional)  To control which requests are profiled, use the Func<HttpRequest, bool> option:
                                        // (default is everything should be profiled)
                                        options.ShouldProfile = request => MyShouldThisBeProfiledFunction(request);

                                        // (Optional) Profiles are stored under a user ID, function to get it:
                                        // (default is null, since above methods don't use it by default)
                                        options.UserIdProvider = request => MyGetUserIdFunction(request);

                                        // (Optional) Swap out the entire profiler provider, if you want
                                        // (default handles async and works fine for almost all applications)
                                        options.ProfilerProvider = new MyProfilerProvider();
                    */

                    options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();
                    //options.EnableServerTimingHeader = true;


                    // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                    // (defaults to true, and connection opening/closing is tracked)
                    options.TrackConnectionOpenClose = true;

                    // (Optional) Use something other than the "light" color scheme.
                    // (defaults to "light")
                    options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

                    // The below are newer options, available in .NET Core 3.0 and above:

                    // (Optional) You can disable MVC filter profiling
                    // (defaults to true, and filters are profiled)
                    options.EnableMvcFilterProfiling = true;
                    // ...or only save filters that take over a certain millisecond duration (including their children)
                    // (defaults to null, and all filters are profiled)
                    // options.MvcFilterMinimumSaveMs = 1.0m;

                    // (Optional) You can disable MVC view profiling
                    // (defaults to true, and views are profiled)
                    options.EnableMvcViewProfiling = true;
                    // ...or only save views that take over a certain millisecond duration (including their children)
                    // (defaults to null, and all views are profiled)
                    // options.MvcViewMinimumSaveMs = 1.0m;

                    // (Optional) listen to any errors that occur within MiniProfiler itself
                    // options.OnInternalError = e => MyExceptionLogger(e);

                    // (Optional - not recommended) You can enable a heavy debug mode with stacks and tooltips when using memory storage
                    // It has a lot of overhead vs. normal profiling and should only be used with that in mind
                    // (defaults to false, debug/heavy mode is off)
                    //options.EnableDebugMode = true;
                });
#endif

                _logger.Info($"ConfigureServices Finished");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"ConfigureServices - {ex.ToString()}");
            }
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                              IWebHostEnvironment env,
                              IServiceProvider serviceProvider)
        {
            StripeConfiguration.ApiKey = "sk_test_51Ja4hEDwqq579CoGIPLrKaE2zCgsf0vfy7VO7vmHWsgjSDe6VM3MiLUX5QUUlqIlwag1jszfxaXMGFFr4kgXG3VG00bqnxYTad";

            /*
            app.Use(async (c, next) =>
            {
                _logger.Info($"**** Request Path -- {c.Request.Path}");
                if (c.Request.Path == "/")
                {
                    c.Response.Headers["Cache-Control"] = "no-cache,no-store,must-revalidate,max-age=0";
                    c.Response.Headers["Pragma"] = "no-cache";
                    c.Response.Headers["Expires"] = "0";
                }
                await next();
            });
            */

            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UpRaise v1"));
                    app.UseMiniProfiler();
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHealthChecks("/hc");



                /*
                app.UseStaticFiles(new StaticFileOptions()
                {
                    OnPrepareResponse = ctx =>
                    {
                        if (ctx.Context.Request.Path.StartsWithSegments("/assets"))
                        {
                            // Cache all static resources for 1 year (versioned filenames)
                            var headers = ctx.Context.Response.GetTypedHeaders();
                            headers.CacheControl = new CacheControlHeaderValue
                            {
                                Public = true,
                                MaxAge = TimeSpan.FromDays(365)
                            };
                        }
                        else
                        {
                            // Do not cache explicit `/index.html` or any other files.  See also: `DefaultPageStaticFileOptions` below for implicit "/index.html"
                            var headers = ctx.Context.Response.GetTypedHeaders();
                            headers.CacheControl = new CacheControlHeaderValue
                            {
                                Public = true,
                                MaxAge = TimeSpan.FromDays(0)
                            };
                        }
                    }
                });
                */

                /*
                if (!env.IsDevelopment())
                {
                    // Serve static file like image, css, js in asset folder of angular app
                    app.UseSpaStaticFiles(new StaticFileOptions()
                    {
                        OnPrepareResponse = ctx =>
                        {
                            if (ctx.Context.Request.Path.StartsWithSegments("/assets"))
                            {
                                // Cache all static resources for 1 year (versioned filenames)
                                var headers = ctx.Context.Response.GetTypedHeaders();
                                headers.CacheControl = new CacheControlHeaderValue
                                {
                                    Public = true,
                                    MaxAge = TimeSpan.FromDays(365)
                                };
                            }
                            else
                            {
                                // Do not cache explicit `/index.html` or any other files.  See also: `DefaultPageStaticFileOptions` below for implicit "/index.html"
                                var headers = ctx.Context.Response.GetTypedHeaders();
                                headers.CacheControl = new CacheControlHeaderValue
                                {
                                    Public = true,
                                    MaxAge = TimeSpan.FromDays(0)
                                };
                            }
                        }
                    });
                }
                */

                app.UseResponseCompression();

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                var options = new RewriteOptions()
                                  .Add(new CampaignDetailsRewriteRule(
                                      serviceProvider.GetService<IEasyCachingProvider>()
                                      ));
                app.UseRewriter(options);

                app.UseRouting();

                app.UseCors(CORSSpecificOrigins);

                app.UseResponseCaching();

                app.UseAuthentication();
                app.UseAuthorization();
                app.UseHttpsRedirection();


                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();

                    /*
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller}/{action=Index}/{id?}");
                    */

                    // Add a new endpoint that uses the VersionMiddleware
                    endpoints.Map("/version", endpoints.CreateApplicationBuilder()
                        .UseMiddleware<VersionMiddleware>()
                        .Build())
                        .WithDisplayName("Version number");


                    endpoints.MapFallbackToFile("index.html");
                });



            }
            catch (Exception ex)
            {
                _logger.Error($"Configure - {ex.ToString()}");
            }
        }


        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {

            var jitterer = new Random(Guid.NewGuid().GetHashCode());

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                         + TimeSpan.FromMilliseconds(jitterer.Next(0, 100))
                );
        }

        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }


    }
}
