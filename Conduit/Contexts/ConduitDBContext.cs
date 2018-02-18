using Conduit.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Contexts
{
    public class ConduitDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<UserIsFollowing> UserIsFollowing { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ArticleTags> ArticleTags { get; set; }
        public DbSet<UserFavoriteArticles> UserFavoriteArticles { get; set; }

        public ConduitDbContext(DbContextOptions<ConduitDbContext> options)
            : base(options)
        { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserIsFollowing>()
                .HasKey(e => new { e.UserId, e.IsFollowingId });
            builder.Entity<Article>()
                .HasKey(e => new { e.Id });
            builder.Entity<Comment>()
                .HasKey(e => new { e.Id });
            builder.Entity<Tag>()
                .HasIndex(e => e.TagName)
                .IsUnique();
            builder.Entity<ArticleTags>()
                .HasKey(e => new { e.ArticleId, e.TagId });
            builder.Entity<UserFavoriteArticles>()
                .HasKey(e => new { e.ArticleId, e.UserId });
        }
    }
}
