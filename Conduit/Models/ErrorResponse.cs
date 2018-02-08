using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Models
{
    public class ErrorResponse
    {
        public List<string> Errors { get; set; }

        public ErrorResponse()
        {
            Errors = new List<string>();
        }

        public void addErrorKey(string key)
        {
            Errors.Add(key);
        }
    }
}
