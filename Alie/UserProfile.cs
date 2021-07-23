using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace Alie
{
    public class UserProfile
    {
        public int Id { get; set; }

        [StringLength(20, ErrorMessage = "Name should be less than or equal to twenty characters.")]  
        public string Name { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "Email is not valid.")]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Email is not valid.")]
        public string Email { get; set; }

        public int OTP { get; set; }

        internal bool EmailVerified { get; set; } = false;

        public int Age { get; set; }
        public string Location { get; set; }

        [Phone]
        [Range(0, 11, ErrorMessage = "Phone Number should range from 0 to 10." )] 
        public string PhoneNumber { get; set; }

        [Range(50000, 300000, ErrorMessage = "The value entered must be greater than 50,000 and less than 300,000.")]
        public int Amount { get; set; }
        [Range(1, 12, ErrorMessage ="Payment pperiod should range from 1 to 12 months ")]
        public int PaymentPeriod { get; set; }
        public Attachment Picture { get; set; }
        public bool IsRegistered { get; set; }

        public DateTime TimeAccessed { get; set; }
        public Microsoft.Bot.Schema.Activity Activity { get; }
    }
}
