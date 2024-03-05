﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRMenu.Models
{
	public class Food
	{
		public int Id { get; set; }

		[StringLength(100, MinimumLength = 2)]
		[Column(TypeName = "nvarchar(100)")]
		public required string Name { get; set; } = "";

		[Range(0,float.MaxValue)]
		public float Price { get; set; }

        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string? Description { get; set; }

        [Column(TypeName = "tinyint")]
        public byte StateId { get; set; }
        [ForeignKey("StateId")]  //[ForeignKey(nameof(StateId))] 
        public State? State { get; set; }

        //fotoğraf ve indirim yazılacak
    }
}
