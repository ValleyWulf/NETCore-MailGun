using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MailGun.Data;
using MailGun.Models;
using MailGun.Services;

namespace MailGun
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

             // Set up mechanism for sending email.
             IConfigurationSection sectionEmailProvider = 
                 Configuration.GetSection("EmailProvider");

             string emailProvider = sectionEmailProvider["Provider"];

             string emailConnectionType = 
                 sectionEmailProvider["ConnectionType"];

             if (null == emailProvider)
             {

                 throw new ArgumentNullException(
                     "EmailProvider",
                     "Missing email service provider configuration"
                 );

             }

             IConfiguration emailConfig = LoadEmailConfig();

             switch (emailProvider.ToLower())
             {

                 case "mailgun":

                     Console.WriteLine(
                         "Starting send mail via MailGun email service...");

                     if ("api" == emailConnectionType.ToLower())
                     {

                         services.AddTransient<IEmailSender, 
                             MailGunApiEmailSender>();

                         services.Configure<MailGunApiEmailSettings>(
                             emailConfig);

                     }

                     if ("smtp" == emailConnectionType.ToLower())
                     {

                         services.AddTransient<IEmailSender, 
                             MailGunSmtpEmailSender>();

                         services.Configure<MailGunSmtpEmailSettings>(
                             emailConfig);

                     }

                     break;

                 case "gmail":

                     Console.WriteLine(
                         "Starting send mail via Google email service...");

                    services.AddTransient<IEmailSender, GmailSmtpEmailSender>();

                    services.Configure<GmailSmtpEmailSettings>(emailConfig);

                     break;

                 default:

                     Console.WriteLine("Error: Unknown email service.");

                     throw new ArgumentException(
                         "Unknown email service provider in configuration.",
                         "EmailProvider:Provider"
                     );

                     // break;

             }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // 
        // Load Mailgun configurations based on connection type.
        // 
        protected IConfiguration LoadEmailConfig()
        {

             IConfigurationSection sectionEmailProvider = 
                 Configuration.GetSection("EmailProvider");

             string emailProvider = sectionEmailProvider["Provider"];

             if (null == emailProvider)
             {

                 throw new ArgumentException(
                     "Empty email provider.",
                         "EmailProvider:Provider"
                 );

             }

             IConfiguration emailConfig = null;

             switch (emailProvider.ToLower())
             {

                 case "mailgun":

                     Console.WriteLine("Loading MailGun configuration...");

                     emailConfig = LoadMailGunEmailConfig();

                     break;

                 case "gmail":

                     Console.WriteLine("Loading Gamil configuration...");

                     emailConfig = LoadGmailSmtpEmailSettings();

                     break;

                 default:

                     throw new ArgumentException(
                         "Unknown email provider.",
                         "EmailProvider:Provider"
                     );

                     // break;

             }

             return emailConfig;

         }

         protected IConfiguration LoadMailGunEmailConfig()
         {

             IConfigurationSection sectionEmailProvider = 
                 Configuration.GetSection("EmailProvider");

             string emailConnectionType = 
                 sectionEmailProvider["ConnectionType"];

             IConfiguration emailConfig = null;

             if (null == emailConnectionType)
             {

                 throw new ArgumentException(
                     "Invalid email connection type.",
                     "EmailProvider:ConnectionType"
                 );

             }

             switch (emailConnectionType.ToLower())
             {

                 case "api":

                     emailConfig = LoadMailGunApiEmailSettings();

                     break;

                 case "smtp":

                     emailConfig = LoadMailGunSmtpEmailSettings();

                     break;

                 default:

                     throw new ArgumentException(
                         "Unknown email connection type.",
                         "EmailProvider:ConnectionType"
                     );

                     // break;

             }

             return emailConfig;

         }

        //
        // Configure email sender based on Mailgun configurations for REST API.
        // 
         protected IConfiguration LoadMailGunSmtpEmailSettings()
         {

             IConfigurationSection sectionEmailProvider = 
                 Configuration.GetSection("EmailProvider");

             string configFile = sectionEmailProvider["ConfigFile"];

             var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                 .AddEnvironmentVariables();

             IConfiguration config = builder.Build();

             IConfigurationSection sectionSmtp = 
                 config.GetSection("SMTPSettings");

             return sectionSmtp;

         }

        //
        // Configure email sender based on Mailgun configurations for Smtp.
        // 
         protected IConfiguration LoadMailGunApiEmailSettings()
         {

             IConfigurationSection sectionEmailProvider = 
                 Configuration.GetSection("EmailProvider");

             string configFile = sectionEmailProvider["ConfigFile"];

             var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                 .AddEnvironmentVariables();

             IConfiguration config = builder.Build();

             IConfigurationSection sectionRestApi = 
                 config.GetSection("RestAPISettings");

             return sectionRestApi;

         }

        //
        // Configure email sender based on Gmail configurations for Smtp.
        // 
        protected IConfiguration LoadGmailSmtpEmailSettings()
        {

            IConfigurationSection sectionEmailProvider = 
                Configuration.GetSection("EmailProvider");

            string configFile = sectionEmailProvider["ConfigFile"];

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            IConfiguration config = builder.Build();

            IConfigurationSection sectionSmtp = 
                config.GetSection("SMTPSettings");

            return sectionSmtp;

        }

    }

}
