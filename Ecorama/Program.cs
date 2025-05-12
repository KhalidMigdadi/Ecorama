using Ecorama.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecorama
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // ? ????? ??????
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // ??? ???? ??????
                options.Cookie.HttpOnly = true; // ????? ?????? ?? ??????? ??? ???????????
                options.Cookie.IsEssential = true; // ????? ?????? ?????? ??? ?? ?? ???? ???????? ???????
            });



            // Add services to the container.

            builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("YourConnectionString")));



            builder.Services.AddControllersWithViews();

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

            // ? ????? ?????? ??? Authentication ? Authorization
            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",

                pattern: "{controller=Home}/{action=Index}/{id?}");
                //pattern: "{controller=Admin}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
