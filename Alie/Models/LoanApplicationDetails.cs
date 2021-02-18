using Microsoft.Azure.Documents;

namespace Alie.Models
{
    public class LoanApplicationDetails
    {
        public string FullName { get; set; }

        public string EmailAddress { get; set; }

        public int PhoneNumber  { get; set; }

        public string Location { get; set; }

        public int Age { get; set; }

        public int Amount { get; set; }

        public string AdditionalInformation { get; set; }

        public Attachment Picture { get; set; }

    }
}
