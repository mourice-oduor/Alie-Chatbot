using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Alie.Models
{
    public class Validations : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // your validation logic
            if (value is >= 0 and < 11)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Invalid Phone Number!.");
            }
        }

        public int Id { get; set; }


        [StringLength(10, ErrorMessage = "FullName should be less than or equal to five characters.")]
        public string FullName { get; set; }
        public int Age { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "Email is not valid.")]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Email is not valid.")]
        public string Email { get; set; }
        public string Location { get; set; }


        [Range(0, 11, ErrorMessage = "Phone Number should range from 0 to 10.")]
        public int PhoneNumber { get; set; }

        [Range(50000, 300000, ErrorMessage = "The value entered must be greater than 50,000 and less than 300,000.")]
        public decimal Amount { get; set; }
        public int PaymentPeriod { get; set; }
        public Attachment Picture { get; set; }
        public bool IsRegistered { get; set; }

        public DateTime TimeAccessed { get; set; }
        public Microsoft.Bot.Schema.Activity Activity { get; }
    }
}
