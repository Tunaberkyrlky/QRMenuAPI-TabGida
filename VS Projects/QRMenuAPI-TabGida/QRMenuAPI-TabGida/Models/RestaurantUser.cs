using System;
using System.ComponentModel.DataAnnotations.Schema;
using QRMenuAPI_TabGida.Models;

namespace QRMenuAPI_TabGida.Models
{
    public class RestaurantUser
    {
        public int RestaurantId { get; set; }
        [ForeignKey("RestaurantId")]
        public Restaurant? Restaurant { get; set; }

        public string UserId { get; set; } = "";
        [ForeignKey("UserId")]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
