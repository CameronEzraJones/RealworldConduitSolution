using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Models
{
    public class Article
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        public string Slug { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Body { get; set; }

        [NotMapped]
        public List<String> TagList { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddThh:mm:ss.fffZ}")]
        public DateTime CreatedAt { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddThh:mm:ss.fffZ}")]
        public DateTime UpdatedAt { get; set; }
        
        [NotMapped]
        public bool Favorited { get; set; }

        [NotMapped]
        public int FavoritesCount { get; set; }

        [JsonIgnore]
        public string AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public Profile Author { get; set; }

        [NotMapped]
        [JsonIgnore]
        public List<Comment> Comments { get; set; }
    }
}
