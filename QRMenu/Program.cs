using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace QRMenu;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<QRMenu.Data.ApplicationContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDatabase")));
        // Builder configuration, appsettings.json dosyasından "ApplicationDatabase" bilgisini çekerek sql'e bağlanır.

        builder.Services.AddSession(); //session servisini uygulamada kullanmak için servis olarak eklememiz gerekiyor
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.UseSession();  //uygulamada session kullanıyorsak burada belirtmemiz gerekiyor.
        
        app.Run();
    }
}

