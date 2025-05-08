using ExSystemProject.MappinConfig;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExSystemProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddAutoMapper(cfg => {
                cfg.AddProfile<MappingProfile>();
            });
            builder.Services.AddAutoMapper(typeof(MmappingProfile));
             builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(s =>
            {
                s.LoginPath = "/Account/Login";
                s.LogoutPath = "/Account/Login";
                //s.AccessDeniedPath = "/Account/AccessDenied";
                s.ExpireTimeSpan = TimeSpan.FromDays(7);

            });
            builder.Services.AddDbContext<ExSystemTestContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("sc")).UseLazyLoadingProxies());
            builder.Services.AddScoped<UnitOfWork>(); // i could apply here the Dependancy Inversion instead of injection
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
