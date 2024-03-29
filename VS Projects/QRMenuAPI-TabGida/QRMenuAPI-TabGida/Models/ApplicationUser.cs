﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using QRMenuAPI_TabGida.Models;

namespace QRMenuAPI_TabGida.Models
{
	public class ApplicationUser : IdentityUser
	{
        [StringLength(100, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; } = "";

        [StringLength(100, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(100)")]
        public override string? UserName { get => base.UserName; set => base.UserName = value; }

        [EmailAddress]
        [StringLength(100, MinimumLength = 5)]
        [Column(TypeName = "nvarchar(100)")]
        public override string Email { get; set; } = "";

        [Phone]
        [StringLength(30)]
        public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

        public DateTime RegisterationDate { get; set; }

        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        [JsonIgnore]
        public Company? Company { get; set; }

        [Column(TypeName = "tinyint")]
        public byte StateId { get; set; }
        [ForeignKey("StateId")]
        [JsonIgnore]
        public State? State { get; set; }

        public virtual ICollection<RestaurantUser>? RestaurantUsers { get; set; }
    }
}

