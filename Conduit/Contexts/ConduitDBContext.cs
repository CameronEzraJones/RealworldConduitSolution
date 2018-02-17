﻿using Conduit.Models;
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

        public ConduitDbContext(DbContextOptions<ConduitDbContext> options)
            : base(options)
        { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserIsFollowing>()
                .HasKey(e => new {e.UserId, e.IsFollowingId });
        }
    }
}
