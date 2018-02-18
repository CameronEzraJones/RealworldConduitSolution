using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Models
{
    public class UserFavoriteArticles
    {
        public string UserId { get; set; }
        public int ArticleId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [ForeignKey("ArticleId")]
        public Article Article { get; set; }
    }
}
