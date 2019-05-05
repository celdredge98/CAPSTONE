using System.Reflection;
using CanTheSpam.Data;
using CanTheSpam.Data.CanTheSpamRepository;
using CanTheSpam.Data.Repository.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CanTheSpam.Log4Net.Data;
using CanTheSpam.Log4Net.Extensions;
using CanTheSpam.Data.CanTheSpamRepository.Interfaces;
using Swashbuckle.AspNetCore.Swagger;

namespace CanTheSpam
{
   public class Startup
   {
      public IConfiguration Configuration { get; }
      public IHostingEnvironment Environment { get; }

      public Startup(IHostingEnvironment env)
      {
         IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

         Environment = env;
         Configuration = builder.Build();
      }

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
         services.Configure<CookiePolicyOptions>(options =>
         {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
         });

         services.AddDbContext<Log4NetDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Log4NetConnection")));
         services.AddDbContext<UserDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("AppConnection")));
         //services.AddDbContext<CanTheSpamContext>(options => options.UseSqlServer(Configuration.GetConnectionString("AppConnection")));

         services.AddEntityFrameworkSqlServer()
            .AddDbContext<CanTheSpamContext>(options =>
               {
                  options.UseSqlServer(Configuration.GetConnectionString("AppConnection"),
                     sqlOptions => sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().
                        Assembly.GetName().Name));
               },
               // Note that Scoped is the default choice in AddDbContext. It is shown here only for pedagogic purposes.
               ServiceLifetime.Scoped
            );

         services.AddScoped<IUnitOfWork, CanTheSpamContext>(
            sp => new CanTheSpamContext(
               new DbContextOptionsBuilder<CanTheSpamContainer>()
                  .UseSqlServer(Configuration.GetConnectionString("AppConnection")).Options));

         services.AddDefaultIdentity<IdentityUser>()
            .AddDefaultUI(UIFramework.Bootstrap4)
            .AddEntityFrameworkStores<UserDbContext>();

         services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

         // Register the Swagger generator, defining 1 or more Swagger documents
         services.AddSwaggerGen(c =>
         {
            c.SwaggerDoc("v1", new Info { Title = "Can The Spam API", Version = "v1" });
         });
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IHostingEnvironment env, Log4NetDbContext log4NetDbContext, ILoggerFactory loggerFactory)
      {
         string log4NetConnectionString = Configuration.GetConnectionString("Log4NetConnection");
         string tableName = Configuration["Logging:TableName"];

         if (env.IsDevelopment())
         {
            string logFilePath = Configuration["Logging:LogFilePath"];
            loggerFactory.AddLog4Net(new[]
            {
               Log4Net.Appenders.Configuration.CreateConsoleAppender(),
               Log4Net.Appenders.Configuration.CreateRollingFileAppender(logFilePath),
               Log4Net.Appenders.Configuration.CreateTraceAppender(),
               Log4Net.Appenders.Configuration.CreateAdoNetAppender(log4NetConnectionString, tableName)
            });
         }
         else
         {
            loggerFactory.AddLog4Net(new[]
            {
               Log4Net.Appenders.Configuration.CreateAdoNetAppender(log4NetConnectionString, tableName)
            });
         }

         Log4NetDbInitializer.Initialize(log4NetDbContext);

         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
         }
         else
         {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
         }

         app.UseHttpsRedirection();
         app.UseStaticFiles();
         app.UseCookiePolicy();

         app.UseAuthentication();

         // Enable middleware to serve generated Swagger as a JSON endpoint.
         app.UseSwagger();

         // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
         // specifying the Swagger JSON endpoint.
         app.UseSwaggerUI(c =>
         {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Can The Spam API V1");
         });

         app.UseMvc(routes =>
         {
            routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
         });
      }
   }
}
