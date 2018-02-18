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
        [NotMapped]
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
        public List<String> Tags { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int MyProperty { get; set; }

        [NotMapped]
        public bool Favorited { get; set; }

        [NotMapped]
        public int FavoritesCount { get; set; }

        public string AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public Profile Author { get; set; }

        public List<Comment> Comments { get; set; }
    }
}
