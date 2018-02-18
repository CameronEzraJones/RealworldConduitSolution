using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [Required]
        public string Body { get; set; }

        public string AuthorId { get; set; }

        public int ArticleId { get; set; }

        [ForeignKey("AuthorId")]
        public Profile Author { get; set; }

        [ForeignKey("ArticleId")]
        public Article Article { get; set; }
    }
}
