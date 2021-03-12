using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Alie.Models
{
    public class UserData
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public int Age { get; set; }

        public string Email { get; set; }

        public string Location { get; set; }

        public int PhoneNumber { get; set; }

        public int Amount { get; set; }
        public int PaymentPeriod { get; set; }

        public Attachment Picture { get; set; }

        //public List<Products> Products { get; set; } = new List<Products>();
    }
}
