using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QRMenuAPI_TabGida.Models;
using System.Text.Json.Serialization;

namespace QRMenuAPI_TabGida.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(100)")]
        public String Name { get; set; } = "";

        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string? Description { get; set; }


        public int RestaurantId { get; set; }
        [JsonIgnore]
        [ForeignKey("RestaurantId")]
        public Restaurant? Restaurant { get; set; }

        [Column(TypeName = "tinyint")]
        public byte StateId { get; set; }
        [JsonIgnore]
        [ForeignKey("StateId")]
        public State? State { get; set; }

        public virtual ICollection<Category>? Categories { get; set; }
    }
}
