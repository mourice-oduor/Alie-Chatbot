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

namespace Alie.Dialogs.Details
{
    public class LoanApplicationDetailsDialog : ComponentDialog
    {
        //private readonly IStatePropertyAccessor<UserData> _userDataAccessor;
        //private readonly IStatePropertyAccessor<ConversationData> _conversationDataAccessor;

        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        public LoanApplicationDetailsDialog() : base(nameof(LoanApplicationDetailsDialog))
        {
            //_userDataAccessor = userState.CreateProperty<UserData>("UserData");
            //_conversationDataAccessor = conversationState.CreateProperty<ConversationData>("ConversationData");
            //_userProfileAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), PhoneNumberPromptValidaton));
            AddDialog(new NumberPrompt<decimal>(nameof(NumberPrompt<decimal>), AmountPromptValidatorAsync));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt), PicturePromptValidatorAsync));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NameStepAsync,
                //ThankYouStepAsync,
                AgeStepAsync,
                EmailAddressStepAsync,
                PhoneNumberStepAsync,
                LocationStepAsync,
                AmountStepAsync,
                AttachmentStepAsync,
                ConfirmAttachmentStepAsync,
                ConfirmLoanStepAsync,
                SummaryStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
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
        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Retrieve the user profile from the UserState via the userProfileAccessor
            //var userProfile = await _userProfileAccessor.GetAsync(
            //    stepContext.Context, () => new UserProfile(),
            //    cancellationToken);

            // Set the Is Registered property to TRUE
            //userProfile.IsRegistered = true;
            // Create an object in which to collect the user's information within the dialog.
            //stepContext.Values[UserInfo] = new UserProfile();

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") };

            // Ask the user to enter their name.
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        //private async Task<DialogTurnResult> ThankYouStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    stepContext.Values["name"] = (string)stepContext.Result;

        //    // We can send messages to the user at any point in the WaterfallStep.
        //    //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thank You! {stepContext.Result}."), cancellationToken);
        //    //var promptOptions = new PromptOptions { Prompt = MessageFactory.Text($"Thank You! {stepContext.Result}.") };

        //    //await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Thank You! {stepContext.Result}.") }, cancellationToken);
        //    //return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);

        //    await stepContext.Context.SendActivityAsync(
        //            MessageFactory.Text($"Thank You! {stepContext.Result}."),
        //            cancellationToken);

        //    // User said "no" so we will skip the next step. Give -1 as the age
        //    return await stepContext.NextAsync(cancellationToken, cancellationToken);
        //    // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
        //    //return await stepContext.PromptAsync(nameof(ChoicePrompt),
        //    //    new PromptOptions
        //    //    {
        //    //        Prompt = MessageFactory.Text("Do you want to provide your age?"),
        //    //        Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
        //    //    },
        //    //    cancellationToken);
        //}

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;
            //var value = ((FoundChoice)stepContext.Result).Value;
            //if (value == "Yes")
            //{
                // If the user has declared that he wants to communicate the age, I present a "NumberPrompt" dialog to acquire it.
                // The NumberPrompt dialog is similar to the TextPrompt, but only accepts numeric values:
                // if the user enters a non-numeric value, or the set value is not validated by the validator method
                // we defined as a parameter when instantiating the NumberPrompt Dialog,
                // there is a RetryPrompt to be presented on the screen.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(" Please Enter your age. "),
                    RetryPrompt = MessageFactory.Text(" WARNING!: you must enter a number value between 18 and 150. "),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
            //}
            //else
            //{
            //    // If the user has declared that he does not want to communicate his age, jump directly to the next WaterfallStep.
            //    await stepContext.Context.SendActivityAsync(
            //        MessageFactory.Text($"No problem, { stepContext.Values["name"]} !Let's continue with your application. "),
            //        cancellationToken);

            //    // User said "no" so we will skip the next step. Give -1 as the age
            //    return await stepContext.NextAsync(-1, cancellationToken);
            //}
        }
        private async Task<DialogTurnResult> EmailAddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (int)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your email address.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["email"] = (string)stepContext.Result;
            //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Enter your Phone Number .") }, cancellationToken);

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(" Enter your Phone Number. "),
                RetryPrompt = MessageFactory.Text(" Incorect Value!: Please enter the valid Phone Number!. ")
            };

            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phone"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please Provide your Location.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AmountStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["location"] = (string)stepContext.Result;
            //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Enter the amount between 50,000 to 300,000") }, cancellationToken);
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter the loan amount."),
                RetryPrompt = MessageFactory.Text("The value entered must be greater than 50,000 and less than 300,000.")
            };

            //var result = ((decimal)stepContext.Context.);
            //var value = (decimal)(50000);

            //var value = Convert.ToDecimal(stepContext.Context.Responded);
            ////var value = (decimal)50000;
            //if (value < 50000)
            //{
            //    await stepContext.Context.SendActivityAsync(
            //        MessageFactory.Text("The value entered must be greater than 50,000 and less than 300,000. "),
            //        cancellationToken);
            //    //Prompt = MessageFactory.Text("The value entered must be greater than 50,000 and less than 300,000.");
            //}
            //else
            //{
            //    return await stepContext.NextAsync(null, cancellationToken);
            //}

            // Ask the user to enter their loan amount.
            return await stepContext.PromptAsync(nameof(NumberPrompt<decimal>), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> AttachmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["amount"] = (decimal)stepContext.Result;
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please attach a profile picture (or type any message to skip)."),
                RetryPrompt = MessageFactory.Text("The attachment must be a jpeg/png image file."),
            };

            return await stepContext.PromptAsync(nameof(AttachmentPrompt), promptOptions, cancellationToken);

        }
        private async Task<DialogTurnResult> ConfirmAttachmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["picture"] = ((IList<Microsoft.Bot.Schema.Attachment>)stepContext.Result)?.FirstOrDefault();

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                new PromptOptions 
                { 
                    Prompt = MessageFactory.Text("Is this ok?"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { " Yes ", " No " })
                }, cancellationToken);

        }

        private async Task<DialogTurnResult> ConfirmLoanStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["attachment"] = (Attachment)stepContext.Result;

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text( $"Thanks for the replies, { stepContext.Values ["name"]}. Here is a summary of the data you entered: "),
                cancellationToken);

            // Retrieve the user profile from the UserState so as to be able to present the summary of the data entered.
            var userProfile = await _userProfileAccessor.GetAsync(
                stepContext.Context, () => new UserProfile(),
                cancellationToken);

            userProfile.FullName = (string)stepContext.Values["name"];
            userProfile.Age = (int)stepContext.Values["age"];
            userProfile.Email = (string)stepContext.Values["email"];
            userProfile.Location = (string)stepContext.Values["location"];
            userProfile.PhoneNumber = (int)stepContext.Values["phone"];
            userProfile.Amount = (decimal)stepContext.Values["amount"];

            var msg = $" Your name is { userProfile.FullName } ";

            if (userProfile.Age!= -1)
                msg += $", you are { userProfile . Age } years old ";

            msg += $", you are from { userProfile.Location } and your loan amount is { userProfile.Amount }. ";

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
            var userProfile = await _userProfileAccessor.GetAsync(
                stepContext.Context, () => new UserProfile(),
                cancellationToken);

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
                    MessageFactory.Text($" Thanks for your application, { userProfile.FullName }: " +
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
            userProfile.IsRegistered = false;

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

        

        private static async Task<bool> AmountPromptValidatorAsync(PromptValidatorContext<decimal> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            //var result = ((decimal)stepContext.Context.);
            //var value = (decimal)(50000);

            var value = Convert.ToDecimal(promptContext.Recognized.Value);
            //var value = (decimal)50000;
            if (value < 50000)
            {
                TextPrompt = MessageFactory.Text("The value entered must be greater than 50,000 and less than 300,000.");
            }
            else
            {
                return false;
            }
            return await Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 49000 && promptContext.Recognized.Value < 300001);
        }

        private static async Task<bool> PicturePromptValidatorAsync(PromptValidatorContext<IList<Microsoft.Bot.Schema.Attachment>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var attachments = promptContext.Recognized.Value;
                var validImages = new List<System.Net.Mail.Attachment>();

                foreach (var attachment in attachments)
                {
                    if (attachment.ContentType == "image/jpeg" || attachment.ContentType == "image/png")
                    {
                        validImages.Add((System.Net.Mail.Attachment)attachments);
                    }
                }

                promptContext.Recognized.Value = (IList<Microsoft.Bot.Schema.Attachment>)validImages;

                // If none of the attachments are valid images, the retry prompt should be sent.
                return validImages.Any();
            }
            else
            {
                await promptContext.Context.SendActivityAsync("No attachments received. Proceeding without a profile picture...");

                // We can return true from a validator function even if Recognized.Succeeded is false.
                return true;
            }
        }


        public string FullName { get; set; }

        public int Age { get; set; }

        public string Email { get; set; }

        public int PhoneNumber { get; set; }

        public string Location { get; set; }

        public decimal Amount { get; set; }

        public System.Net.Mail.Attachment Picture { get; set; }

        public bool IsRegistered { get; set; }
        public Microsoft.Bot.Schema.Activity Activity { get; }
        public static Activity TextPrompt { get; private set; }
    }
}