using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QRMenu.Models;

namespace QRMenu.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }	  //[Key] attribute koyulması gerekmiyor çünkü EF otomatik algılıyor.

        [StringLength(200, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; } = "";

        [DataType(DataType.PostalCode)]
        [StringLength(5, MinimumLength = 5)]
        [Column(TypeName = "char(5)")]
        public string PostalCode { get; set; } = "";

        [StringLength(300, MinimumLength = 5)]
        [Column(TypeName = "nvarchar(300)")]
        public string AddressDetails { get; set; } = "";

        [EmailAddress]
        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string EMail { get; set; } = "";

        [Phone]
        [StringLength(30, MinimumLength = 2)]
        [Column(TypeName = "varchar(30)")]
        public string Phone { get; set; } = "";

        [StringLength(11, MinimumLength = 10)]
        [Column(TypeName = "varchar(11)")]
        public string TaxNumber { get; set; } = "";

        [Column(TypeName = "smalldatetime")]
        public DateTime RegisterationDate { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? WebAddress { get; set; }

        [Column(TypeName ="tinyint")]
        public byte StateId { get; set; }
        [ForeignKey("StateId")]
        public State? State { get; set; }

	}
}

