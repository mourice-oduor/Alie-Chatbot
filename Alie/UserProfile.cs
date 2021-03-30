using System;
using System.Net.Mail;

namespace Alie
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
        public int PhoneNumber { get; set; }
        public decimal Amount { get; set; }
        public int PaymentPeriod { get; set; }
        public Attachment Picture { get; set; }
        public bool IsRegistered { get; set; }
        public DateTime TimeAccessed { get; set; }
        public Microsoft.Bot.Schema.Activity Activity { get; }
    }
}
