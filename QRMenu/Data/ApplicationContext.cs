using System;
using Microsoft.EntityFrameworkCore;
using QRMenu.Models;

namespace QRMenu.Data
{
	public class ApplicationContext  : DbContext
    {   // DbContext Base classının constructor'una :base(parameter) yazarak parametre gönderiyoruz.
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) 
		{

		}
        public DbSet<QRMenu.Models.Company> Companies { get; set; } = default!;
		public DbSet<State>? States { get; set; }
		public DbSet<Food>? Foods { get; set; }
		// üstteki kodlarda namespace yazılsa da olur yazılmasa da.
	}
}

