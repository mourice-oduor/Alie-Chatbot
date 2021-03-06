using System;
using System.Net.Mail;

namespace Alie
{
    public class UserProfile
    {
        public String Id { get; set; }

        public string FullNames { get; set; }

        public string Email { get; set; }

        public string Location { get; set; }

        public int PhoneNumber { get; set; }

        public int Amount { get; set; }


        public int Age { get; set; }

        public Attachment Picture { get; set; }

    }
}
