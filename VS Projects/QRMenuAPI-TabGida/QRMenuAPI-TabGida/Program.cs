
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI_TabGida.Models;
using QRMenuAPI_TabGida.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace QRMenuAPI_TabGida
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationContext>(options =>
            options.UseSqlServer(builder.Configuration["ConnectionStrings:ApplicationDatabase"]));
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();

            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            {
                ApplicationContext? Context = app.Services.CreateScope().ServiceProvider.GetService<ApplicationContext>();
                RoleManager<IdentityRole>? roleManager = app.Services.CreateScope().ServiceProvider.GetService<RoleManager<IdentityRole>>();
                UserManager<ApplicationUser>? userManager = app.Services.CreateScope().ServiceProvider.GetService<UserManager<ApplicationUser>>();
                DataInitialization dataInitialization = new DataInitialization(Context,roleManager,userManager);

                
            }
            app.Run();
        }
    }
}
