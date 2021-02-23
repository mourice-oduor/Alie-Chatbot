using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alie
{
    public class UserProfile
    {
        public string FullNames { get; set; }

        public string Email { get; set; }

        public string Location { get; set; }

        public int PhoneNumber { get; set; }

        public int Age { get; set; }

        public Attachment Picture { get; set; }

    }
}
