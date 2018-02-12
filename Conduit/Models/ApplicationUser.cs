using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conduit.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApplicationUser : IdentityUser
    {
        [JsonProperty]
        public override string Email { get; set; }

        [JsonProperty]
        [NotMapped]
        public string Token { get; set; }

        [JsonProperty("username")]
        public override string UserName { get; set; }

        [JsonProperty]
        public string Bio { get; set; }

        [JsonProperty]
        public string Image { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [NotMapped]
        public string Password { get; set; }
    }
}
