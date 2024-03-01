using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRMenu.Models
{
	public class State
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]  // Db otomatik Id atamaması için EF kodu.
		public byte Id { get; set; }                      //  EF Byte Id'leri otomatik Primary Key olarak görmez fakat short,int ve long tipinde Id var ise EF otomatik primary key olarak ayarlar
		[Required]                                        // Kullanıcı State.Name'i entera basıp geçemez bir girdi vermek zorunda
		[StringLength(10)]								  // Veri tipi max 10 olduğu için karakter sınırı koyarak db'nin patlamamasını sağlar
        [Column(TypeName ="nvarchar(10)")]
		public string Name { get; set; } = "";
	}
}

