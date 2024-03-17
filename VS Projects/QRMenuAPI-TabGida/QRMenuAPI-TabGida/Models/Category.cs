using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QRMenuAPI_TabGida.Models;

namespace QRMenuAPI_TabGida.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(100)")]
        public String Name { get; set; } = "";

        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string? Description { get; set; }

        //public List<Food>? Foods { get; set; }

        public int MenuId { get; set; }
        [ForeignKey("MenuId")]
        public Menu? Menu { get; set; }

        [Column(TypeName = "tinyint")]
        public byte StateId { get; set; }
        [ForeignKey("StateId")]
        public State? State { get; set; }
    }
}