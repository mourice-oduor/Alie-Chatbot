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
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Alie.Dialogs.Details
{
    public class LoanApplicationDetailsDialog : ComponentDialog
    {
        private readonly string EmailDialogID = "EmailDlg";
        private readonly string PhoneDialogID = "PhoneDlg";
        private readonly string PeriodDialogID = "PeriodDlg";
        private readonly string AgeDialogID = "AgeDlg";
        private readonly string AmountDialogID = "AmountDlg";

        public object Name { get; private set; }

        public LoanApplicationDetailsDialog() : base(nameof(LoanApplicationDetailsDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(EmailDialogID, EmailValidation));
            AddDialog(new TextPrompt(PhoneDialogID, PhoneValidation));
            AddDialog(new NumberPrompt<int>(PeriodDialogID, PeriodValidationAsync));
            AddDialog(new NumberPrompt<int>(AgeDialogID, AgePromptValidatorAsync));
            AddDialog(new NumberPrompt<int>(AmountDialogID, AmountPromptValidationAsync));
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
            return await stepContext.PromptAsync(AgeDialogID, promptOptions, cancellationToken);
        }
        private async Task<DialogTurnResult> EmailAddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (int)stepContext.Result;
            return await stepContext.PromptAsync(EmailDialogID, new PromptOptions { Prompt = MessageFactory.Text("Please enter your email address.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["email"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(PhoneDialogID, new PromptOptions { Prompt = MessageFactory.Text("Enter Phone Number.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phone"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please Provide your Location.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AmountStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["location"] = (string)stepContext.Result;
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(" Please enter the loan amount. "),
                RetryPrompt = MessageFactory.Text("The value entered must be greater than 50,000 and less than 300,000.. "),
            };
            return await stepContext.PromptAsync(AmountDialogID, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult>PaymentPeriodStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["amount"] = (int)stepContext.Result;
            return await stepContext.PromptAsync(PeriodDialogID, new PromptOptions { Prompt = MessageFactory.Text("Please enter the payment period. (1 to 12 months)") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmLoanStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["period"] = (int)stepContext.Result;
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Thanks for the replies, { stepContext.Values["name"]}, here is a summary of your loan application: "),
                cancellationToken);

            UserProfile userProfile = new UserProfile()
            {
                Name = (string)stepContext.Values["name"],
                Age = (int)stepContext.Values["age"],
                Email = (string)stepContext.Values["email"],
                Location = (string)stepContext.Values["location"],
                PhoneNumber = (string)stepContext.Values["phone"],
                Amount = (int)stepContext.Values["amount"],
                PaymentPeriod = (int)stepContext.Values["period"]
            };

            var msg = $" Your name is { userProfile.Name } ";

            if (userProfile.Age != -1)
                msg += $", you are { userProfile.Age } years old ";

            msg += $", you are from { userProfile.Location } and your loan amount is { userProfile.Amount }," +
                   $" to be paid within { userProfile.PaymentPeriod} months. ";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

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
            var userProfile = new UserProfile()
            {
            };

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
                    MessageFactory.Text($" Thanks for your application, { Name }: " +
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
                    MessageFactory.Text(" Received: the information you entered will not be stored. Reach to us via the contact section if you need a loan. "),
                    cancellationToken);
            }

            // Regardless of the user's choice, I set the IsRegistering property to FALSE since the registration procedure has finished.
            //userProfile.IsRegistered = false;


            stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 2;
            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), userProfile, cancellationToken);
            // I return the end of the Waterfall
            //return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            int i;
            bool isInteger = Int32.TryParse(promptContext.Recognized.Value.ToString(), out i);
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 17 && promptContext.Recognized.Value < 150);
        }

        private async Task<bool> EmailValidation(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            string email = promptcontext.Recognized.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                await promptcontext.Context.SendActivityAsync("The email you entered is not valid, please enter a valid email.", cancellationToken: cancellationtoken);
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address == email)
                {
                    return true;
                }
                else
                {
                    await promptcontext.Context.SendActivityAsync("The email you entered is not valid, please enter a valid email.", cancellationToken: cancellationtoken);
                    return false;
                }
            }
            catch
            {
                await promptcontext.Context.SendActivityAsync("The email you entered is not valid, please enter a valid email.", cancellationToken: cancellationtoken);
                return false;
            }
        }

        private static async Task<bool> PhoneValidation(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            string number = promptcontext.Recognized.Value;
            if (Regex.IsMatch(number, @"^\d+$"))
            {
                int count = promptcontext.Recognized.Value.Length;
                if (count != 10)
                {
                    await promptcontext.Context.SendActivityAsync("Hello, you are missing some number !!!",
                        cancellationToken: cancellationtoken);
                    return false;
                }
                return true;
            }
            await promptcontext.Context.SendActivityAsync("The phone number is not valid. Please enter a valid number.",
                        cancellationToken: cancellationtoken);
            return false;
        }

        private static async Task<bool> AmountPromptValidationAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            int i;
            bool isint = int.TryParse(promptContext.Recognized.Value.ToString(), out i);
            return await Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 49000 && promptContext.Recognized.Value < 300001);
        }

        private static async Task<bool> PeriodValidationAsync(PromptValidatorContext<int> promptcontext, CancellationToken cancellationtoken)
        {
            int number = promptcontext.Recognized.Value;
            if (number < 1)
            {
                await promptcontext.Context.SendActivityAsync("Warning! Payment Period must be between 1 to 12 months.",
                    cancellationToken: cancellationtoken);
                return false;
            }
            if (number > 12)
            {
                await promptcontext.Context.SendActivityAsync("Warning! Payment Period must be between 1 to 12 months.",
                    cancellationToken: cancellationtoken);
                return false;
            }

            return true;
        }
    }
}