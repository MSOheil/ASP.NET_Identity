using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data.Context;
using WebApplication1.Models;
using WebApplication1.Security;

namespace WebApplication1
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
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
            // Add DbContext
            services.AddDbContextPool<DbContextIdentity>(option =>
            option.UseSqlServer(Configuration.GetConnectionString("Connection"))
            );
            // Identity Configure
            services.AddIdentity<ApplicationUser, IdentityRole>(option =>
            {
                option.Password.RequiredLength = 10;
                option.Password.RequiredUniqueChars = 3;


                option.SignIn.RequireConfirmedEmail = true;

                option.Lockout.MaxFailedAccessAttempts = 5;
                option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

            }).AddEntityFrameworkStores<DbContextIdentity>()
                .AddDefaultTokenProviders();
                //.AddTokenProvider<CustomEmailConfirmationTokenProvider
                //<ApplicationUser>>("CustomeEmailConfirmation");
            // Access Denied
            services.ConfigureApplicationCookie(option =>
            {
                option.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });
            //Claims Policy
            services.AddAuthorization(option =>
            {
                option.AddPolicy("DeleteRolePolicy",
                    policy => policy.RequireClaim("Delete Role" , "true"));
                option.AddPolicy("EditRolePolicy",
                   policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));
            });
            //Roles Policy
            services.AddAuthorization(option =>
            {
                option.AddPolicy("AdminRolePolicy",
                    policy => policy.RequireRole("Admin"));

                option.InvokeHandlersAfterFailure = false;

                option.AddPolicy("AdminMainRolePolicy",
                   policy => policy.RequireRole("MainAdmin"));
            });
            services.Configure<DataProtectionTokenProviderOptions>(o =>
            o.TokenLifespan = TimeSpan.FromHours(5));
            services.AddSingleton<IAuthorizationHandler, CanEdiyOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
