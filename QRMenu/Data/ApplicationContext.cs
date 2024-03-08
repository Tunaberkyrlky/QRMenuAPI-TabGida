using System;
using Microsoft.EntityFrameworkCore;
using QRMenu.Models;

namespace QRMenu.Data
{
	public class ApplicationContext : DbContext
	{   // DbContext Base classının constructor'una :base(parameter) yazarak parametre gönderiyoruz.
		public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
		{

		}
		public DbSet<QRMenu.Models.Company> Companies { get; set; } = default!;
		public DbSet<State>? States { get; set; }
		public DbSet<Food>? Foods { get; set; }
		public DbSet<Category>? Categories { get; set; }
		public DbSet<Restaurant>? Restaurants { get; set; }
		public DbSet<User>? Users { get; set; }
		//public DbSet<RestaurantUser> RestaurantUsers { get; set; }
        // üstteki kodlarda namespace yazılsa da olur yazılmasa da.

		//RestaurantUser için RestaurantId ve UserId kombinasyonunun primaryKey olması için;
   //     protected override void OnModelCreating(ModelBuilder modelBuilder)
   //     {
			//modelBuilder.Entity<RestaurantUser>().HasKey(r => new { r.RestaurantId, r.UserId });
   //     }
	}
}

