using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Models
{
    public class UserIsFollowing
    {
        public string UserId { get; set; }
        public string IsFollowingId { get; set; }
    }
}
