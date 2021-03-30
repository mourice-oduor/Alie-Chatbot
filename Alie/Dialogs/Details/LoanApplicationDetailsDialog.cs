using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Alie.Models;
using System;
using System.Linq;
using Microsoft.Recognizers.Text.Number;
using Microsoft.Recognizers.Text;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Alie.Dialogs.Details
{
    public class LoanApplicationDetailsDialog : ComponentDialog
    {
        public LoanApplicationDetailsDialog() : base(nameof(LoanApplicationDetailsDialog))
        {
            

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), PhoneNumberPromptValidaton));
            AddDialog(new NumberPrompt<decimal>(nameof(NumberPrompt<decimal>), AmountPromptValidatorAsync));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NameStepAsync,
                AgeStepAsync,
                EmailAddressStepAsync,
                PhoneNumberStepAsync,
                LocationStepAsync,
                AmountStepAsync,
                PaymentPeriodStepAsync,
                ConfirmLoanStepAsync,
                SummaryStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Ask the user to enter their name.
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;
            
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(" Please Enter your age. "),
                    RetryPrompt = MessageFactory.Text(" WARNING!: you must enter a number value between 18 and 150. "),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }
        private async Task<DialogTurnResult> EmailAddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (int)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your email address.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["email"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Enter Phone Number!.") }, cancellationToken);

            //var promptOptions = new PromptOptions
            //{
            //    Prompt = MessageFactory.Text(" Enter your Phone Number. "),
            //    RetryPrompt = MessageFactory.Text(" Incorect Value!: Please enter the valid Phone Number!. ")
            //};

            //return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phone"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please Provide your Location.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AmountStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["location"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Enter Loan Amount.") }, cancellationToken);

            //var promptOptions = new PromptOptions
            //{
            //    Prompt = MessageFactory.Text("Please enter the loan amount."),
            //    RetryPrompt = MessageFactory.Text("The value entered must be greater than 50,000 and less than 300,000.")
            //};

            //return await stepContext.PromptAsync(nameof(NumberPrompt<decimal>), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> PaymentPeriodStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["amount"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter the payment period. (1 to 12 months)") }, cancellationToken);

            //var promptOptions = new PromptOptions
            //{
            //    Prompt = MessageFactory.Text("Please enter your Payment Period."),
            //    RetryPrompt = MessageFactory.Text("Warning!: The period must range between 1 to 12 months. "),
            //};

            //return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);

        }


        private async Task<DialogTurnResult> ConfirmLoanStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["period"] = (string)stepContext.Result;

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text( $"Thanks for the replies, { stepContext.Values ["name"]}. Here is a summary of the data you entered: "),
                cancellationToken);

            // Retrieve the user profile from the UserState so as to be able to present the summary of the data entered.
            //var userProfile = await _userProfileAccessor.GetAsync(
            //    stepContext.Context, () => new UserProfile(),
            //    cancellationToken);
            //var userProfile =  new UserProfile()
            //{ 
            //};

            //userProfile.FullName = (string)stepContext.Values["name"];
            //userProfile.Age = (int)stepContext.Values["age"];
            //userProfile.Email = (string)stepContext.Values["email"];
            //userProfile.Location = (string)stepContext.Values["location"];
            //userProfile.PhoneNumber = (int)stepContext.Values["phone"];
            //userProfile.Amount = (decimal)stepContext.Values["amount"];

            //var msg = $" Your name is { userProfile.FullName } ";

            //if (userProfile.Age!= -1)
            //    msg += $", you are { userProfile . Age } years old ";

            //msg += $", you are from { userProfile.Location } and your loan amount is { userProfile.Amount }. ";

            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            return await stepContext.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Do you want confirm your choices? "),
                    Choices = ChoiceFactory.ToChoices(new List<string> { " Yes ", " No " })
                },
                cancellationToken);
        }


        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Retrieve the user profile from the UserState
            //var userProfile = await _userProfileAccessor.GetAsync(
            //    stepContext.Context, () => new UserProfile(),
            //    cancellationToken);

            var value = ((FoundChoice)stepContext.Result).Value;
            if (value == " Yes ")
            {
                // If the user has confirmed the registration:
                // TODO: register the user in the database (or something like that)
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(" OPERATION COMPLETED! "),
                    cancellationToken);

                // TODO: send the user an e-mail/text confirming registration.

                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text($" Thanks for your application, { FullName }: " +
                        "you will shortly receive a confirmation text containing a summary of the loan application. "),
                    cancellationToken);

                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(" We can't wait to have you grow with us: see you soon! "),
                    cancellationToken);
            }
            else
            {
                // If the user has not confirmed the registration:

                // Delete the data entered
                stepContext.Values.Clear();

                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(" Received: the information you entered will not be stored. Type SIGN UP if you want to try again. "),
                    cancellationToken);
            }

            // Regardless of the user's choice, I set the IsRegistering property to FALSE since the registration procedure has finished.
            //userProfile.IsRegistered = false;

            // I return the end of the Waterfall
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            int i;
            bool isInteger = Int32.TryParse(promptContext.Recognized.Value.ToString(), out i);
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 17 && promptContext.Recognized.Value < 150);
        }

        private async Task<bool> PhoneNumberPromptValidaton(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {

            if (!promptContext.Recognized.Succeeded)
            {
                await promptContext.Context.SendActivityAsync("Hello, Please enter the valid mobile number",
                    cancellationToken: cancellationToken);

                return false;
            }

            int count = Convert.ToString(promptContext.Recognized.Value).Length;
            if (count > 0 && count < 11)
            {
                await promptContext.Context.SendActivityAsync("Hello , you are missing some number !!!",
                    cancellationToken: cancellationToken);
                return false;
            }

            return true;
            //return await Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 11);
        }

        private static async Task<bool> AmountPromptValidatorAsync(PromptValidatorContext<decimal> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.

            //var value = Convert.ToDecimal(promptContext.Recognized.Value);
            ////var value = (decimal)50000;
            //if (value < 50000)
            //{
            //    TextPrompt = MessageFactory.Text("The value entered must be greater than 50,000 and less than 300,000.");
            //}
            //else
            //{
            //    return false;
            //}

            decimal i;
            bool isDecimal = Decimal.TryParse(promptContext.Recognized.Value.ToString(), out i);
            return await Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 49000 && promptContext.Recognized.Value < 300001);
        }

        public string FullName { get; set; }

        public int Age { get; set; }

        public string Email { get; set; }

        public int PhoneNumber { get; set; }

        public string Location { get; set; }

        public decimal Amount { get; set; }

        public int PaymentPeriod { get; set; }

        public System.Net.Mail.Attachment Picture { get; set; }

        public bool IsRegistered { get; set; }
        public Microsoft.Bot.Schema.Activity Activity { get; }
        public static Activity TextPrompt { get; private set; }
    }
}