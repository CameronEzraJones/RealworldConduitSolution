using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Profile : IdentityUser
    {
        public override string Id { get; set; }

        [JsonProperty("username")]
        public override string UserName { get; set; }

        [JsonProperty]
        public string Bio { get; set; }

        [JsonProperty]
        public string Image { get; set; }

        [JsonProperty]
        [NotMapped]
        public bool IsFollowing { get; set; }

        public static explicit operator Profile(ApplicationUser v)
        {
            Profile profile = new Profile();
            profile.UserName = v.UserName;
            profile.Bio = v.Bio;
            profile.Image = v.Image;
            return profile;
        }
    }
}
