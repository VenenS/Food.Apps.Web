using System.Collections.Generic;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using ITWebNet.Food.Site;
using ITWebNet.Food.Site.Areas.Administrator;
using ITWebNet.Food.Site.Areas.Curator;
using ITWebNet.Food.Site.Areas.Manager;
using ITWebNet.Food.Site.Hubs;
using ITWebNet.Food.Site.Routing;
using ITWebNet.Food.Site.Services;
using ITWebNet.Food.Site.Services.AuthorizationService;
using ITWebNet.Food.Site.Services.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NLog;

namespace FoodApps.Web.NetCore
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }
        private ILoggerFactory _loggerFactory { get; }
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            var logger = _loggerFactory.CreateLogger(typeof(Logger));
            services.Configure<RouteOptions>(config =>
            {
                config.AppendTrailingSlash = true;
                config.LowercaseUrls = true;
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options =>
            {
                options.LoginPath = new PathString("/Account/Login");
                options.Cookie.Domain = "." + Configuration.GetValue<string>("AppSettings:Domain");
            });

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                // Должно создавать одну сессию для основного и субдоменов
                options.Cookie.Domain = "." + Configuration.GetValue<string>("AppSettings:Domain");

            });

            services.AddSignalR();
            services.AddCors();
            services.AddMemoryCache();
            services.AddHttpContextAccessor();

            // Чтобы кириллица не эскейпилась в отрендеренном html, создаем IHtmlEncoder вручную.
            services.AddSingleton(HtmlEncoder.Create(allowedRanges: new[] {
                UnicodeRanges.BasicLatin,
                UnicodeRanges.Cyrillic
            }));

            services.AddTransient<INotificationsHub, NotificationsHub>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddScoped<IAddressesService, AddressesService>();
            services.AddScoped<IAuthenticationSessionSevice, AuthenticationSessionSevice>();
            services.AddScoped<IBanketsService, BanketsService>();
            services.AddScoped<ICafeOrderNotificationService, CafeOrderNotificationService>();
            services.AddScoped<ICafeService, CafeService>();
            services.AddScoped<ICategoriesService, CategoriesService>();
            services.AddScoped<ICompanyOrderService, CompanyOrderService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IContentServiceClient, ContentServiceClient>();
            services.AddScoped<IDiscountService, DiscountService>();
            services.AddScoped<IDishCategoryService, DishCategoryService>();
            services.AddScoped<IDishService, DishService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IKitchenService, KitchenService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IOrderItemService, OrderItemService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddTransient<IProfileService, ProfileService>();
            services.AddScoped<IRatingService, RatingService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<ICityService, CityService>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //Добавляем локализацию
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            services.AddMvc(options =>
                {
                    options.Filters.Add(new LogErrorAttribute(logger));
                    options.EnableEndpointRouting = false;

                    //Параметры локализации
                    var F = services.BuildServiceProvider().GetService<IStringLocalizerFactory>();
                    var L = F.Create("ModelbindingMessages", "Food.Apps.Web");
                    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
                        (x) => L["The value '{0}' is invalid.", x]);
                    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
                        (x) => L["The field {0} must be a number.", x]);
                    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
                        (x) => L["A value for the '{0}' property was not provided.", x]);
                    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
                        (x, y) => L["The value '{0}' is not valid for {1}.", x, y]);
                    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
                        () => L["A value is required."]);
                    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(
                        (x) => L["The supplied value is invalid for {0}.", x]);
                    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                        (x) => L["Null value is invalid.", x]);
                    options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(
                        () => L["A non-empty request body is required."]);
                    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(
                        (x) => L["The value '{0}' is not valid."]);
                    options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(
                        () => L["The supplied value is invalid."]);
                    options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(
                        () => L["The field must be a number"]);
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddDataAnnotationsLocalization()
                .AddViewLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportCultures = new[]
                {
                    new CultureInfo("en"), new CultureInfo("ru"),
                };
                options.DefaultRequestCulture = new RequestCulture("en", "en");
                options.SupportedCultures = supportCultures;
                options.SupportedUICultures = supportCultures;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration config)
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en"), new CultureInfo("ru")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture(new CultureInfo("en")),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // Обработка кодов
                app.UseStatusCodePagesWithReExecute("/Error/NotFound");
            }
            else
            {
                app.UseExceptionHandler("/Error/Index");
                // Обработка кодов
                app.UseStatusCodePagesWithReExecute("/Error/NotFound");
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseSignalR(conf =>
            {
                conf.MapHub<NotificationsHub>(new PathString("/signalr"));
            });
            app.UseCors(conf =>
            {
                conf.AllowAnyOrigin();
                conf.AllowAnyMethod();
                conf.AllowAnyHeader();
                conf.AllowCredentials();
            });
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRequestLocalization(new RequestLocalizationOptions()
            {
                SupportedCultures = new List<CultureInfo> {
                    new CultureInfo("ru"),
                    new CultureInfo("en")
                },
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("ru", "ru"),
            });
            app.UseAuthentication();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.DefaultHandler = new SubdomainRouter(routes.DefaultHandler, config);
                routes
                    .MapAdministratorRoutes()
                    .MapManagerRoutes()
                    .MapCuratorRoutes()
                    .MapCommonRoutes();
            });
        }
    }
}
