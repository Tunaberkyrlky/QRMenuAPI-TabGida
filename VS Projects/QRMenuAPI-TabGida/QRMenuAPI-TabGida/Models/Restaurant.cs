using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using QRMenuAPI_TabGida.Models;


namespace QRMenuAPI_TabGida.Models
{
    public class Restaurant
    {
        [JsonIgnore]
        [Key]
        public int Id { get; set; }

        [StringLength(200, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; } = "";

        [StringLength(30)]
        [Phone]
        public string Phone { get; set; } = "";

        [EmailAddress]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string EMail { get; set; } = "";

        [StringLength(5, MinimumLength = 5)]
        [Column(TypeName = "char(5)")]
        public string PostalCode { get; set; } = "";

        [StringLength(200, MinimumLength = 5)]
        [Column(TypeName = "nvarchar(200)")]
        public string AddressDetails { get; set; } = "";

        [JsonIgnore]
        [Column(TypeName = "smalldatetime")]
        public DateTime RegisterationDate { get; set; }


        public int CompanyId { get; set; }
        [JsonIgnore]
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        [Column(TypeName = "tinyint")]
        public byte StateId { get; set; }
        [JsonIgnore]
        [ForeignKey("StateId")]
        public State? State { get; set; }

    }
}