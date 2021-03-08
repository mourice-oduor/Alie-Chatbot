using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Alie.Dialogs.Details
{
    public class LoanApplicationDetailsDialog : ComponentDialog
    {
        //private readonly IStatePropertyAccessor<UserData> _userDataAccessor;
        //private readonly IStatePropertyAccessor<ConversationData> _conversationDataAccessor;

        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;


        public LoanApplicationDetailsDialog(UserState userState, ConversationState conversationState) : base(nameof(LoanApplicationDetailsDialog))
        {
            //_userDataAccessor = userState.CreateProperty<UserData>("UserData");
            //_conversationDataAccessor = conversationState.CreateProperty<ConversationData>("ConversationData");
            _userProfileAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));

            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                NameConfirmStepAsync,
                AgeStepAsync,
                EmailAddressStepAsync,
                PhoneNumberStepAsync,
                LocationStepAsync,
                AmountStepAsync,
                //PictureStepAsync,
                ConfirmStepAsync,
                SummaryStepAsync
            };

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            //AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt), PicturePromptValidatorAsync));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Recovery from the user profile 'User State through' the userprofileaccessor
            var userProfile = await _userProfileAccessor.GetAsync(
                stepContext.Context, () => new UserProfile(),
                cancellationToken);

            // Set the Is Registered property to TRUE
            userProfile.IsRegistered = true;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your full name.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thank You! {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            //return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please confirm if you entered your name correctly!") }, cancellationToken);

            //Ask user for the age>>>> Response to be either YES OR NO!
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Do you want to provide your age?"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var value = ((FoundChoice)stepContext.Result).Value;
            if (value == " Yes ")
            {
                // If the user has declared that he wants to communicate the age, I present a "NumberPrompt" dialog to acquire it.
                // The NumberPrompt dialog is similar to the TextPrompt, but only accepts numeric values:
                // if the user enters a non-numeric value, or the set value is not validated by the validator method
                // we defined as a parameter when instantiating the NumberPrompt Dialog,
                // there is a RetryPrompt to be presented on the screen.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(" Perfect! Enter your age. "),
                    RetryPrompt = MessageFactory.Text(" ATTENTION: you must enter a number value between 0 and 150. "),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
            }
            else
            {
                // If the user has declared that he does not want to communicate his age, jump directly to the next WaterfallStep.

                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text( $" No problem,{ stepContext.Values[" name "]} !Let's go ahead. "),
                    cancellationToken);

                // User said "no" so we will skip the next step. Give -1 as the age
                return await stepContext.NextAsync(-1, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> EmailAddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[" age "] = (int)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your email address.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["email"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your Phone Number .") }, cancellationToken);
        }
        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phone"] = (int)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please Provide your Location.") }, cancellationToken);
        }
        
        private async Task<DialogTurnResult> AmountStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["location"] = (string)stepContext.Result;
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter the loan amount."),
                RetryPrompt = MessageFactory.Text("The value entered must be greater than 50000 and less than 300,000."),
            };

            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }
        //private async Task<DialogTurnResult> PictureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    stepContext.Values["amount"] = (string)stepContext.Result;
        //    // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
        //    var promptOptions = new PromptOptions
        //    {
        //        Prompt = MessageFactory.Text("Please attach a profile picture (or type any message to skip)."),
        //        RetryPrompt = MessageFactory.Text("The attachment must be a jpeg/png image file."),
        //    };

        //    return await stepContext.PromptAsync(nameof(AttachmentPrompt), promptOptions, cancellationToken);

        //}
        //private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    stepContext.Values["picture"] = ((IList<Microsoft.Bot.Schema.Attachment>)stepContext.Result)?.FirstOrDefault();

        //    // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
        //    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is this ok?") }, cancellationToken);

        //}

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[" amount "] = (int)stepContext.Result;

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text( $"Thanks for the replies, { stepContext.Values [ " name " ]}. Here is a summary of the data you entered: "),
                cancellationToken);

            // Retrieve the user profile from the UserState so as to be able to present the summary of the data entered.
            var userProfile = await _userProfileAccessor.GetAsync(
                stepContext.Context, () => new UserProfile(),
                cancellationToken);

            userProfile.FullName = (string)stepContext.Values[" name "];
            userProfile.Age = (int)stepContext.Values[" age "];
            userProfile.Email = (string)stepContext.Values[" email "];
            userProfile.Location = (string)stepContext.Values[" location "];
            userProfile.PhoneNumber = (int)stepContext.Values[" phone "];
            userProfile.Amount = (int)stepContext.Values[" amount "];

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
                    MessageFactory.Text( $" Thanks for your application, { userProfile.FullName }: " +
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
            //if ((bool)stepContext.Result)
            //{
            //    // Get the current profile object from user state.
            //var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            //userProfile.Email = (string)stepContext.Values["email"];
            //userProfile.FullNames = (string)stepContext.Values["name"];
            //userProfile.Age = (int)stepContext.Values["age"];
            ////userProfile.Picture = (System.Net.Mail.Attachment)stepContext.Values["picture"];

            //var msg = $"I have your Email Address as {userProfile.Email} and your name as {userProfile.FullNames}";

            //if (userProfile.Age != -1)
            //{
            //    msg += $" and your age as {userProfile.Age}";
            //}

            //msg += ".";

            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            //    if (userProfile.Picture != null)
            //    {
            //        try
            //        {
            //            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment((IEnumerable<Microsoft.Bot.Schema.Attachment>)userProfile.Picture, "This is your profile picture."), cancellationToken);
            //        }
            //        catch
            //        {
            //            await stepContext.Context.SendActivityAsync(MessageFactory.Text("A profile picture was saved but could not be displayed here."), cancellationToken);
            //        }
            //    }
            //}
            //else
            //{
            //    // Save Completed Order in the UserProfile on success
            //    var userProfile = await _userDataAccessor.GetAsync(stepContext.Context, null, cancellationToken);

            //    //userProfile.Products.Add(newProducts);

            //    await _userDataAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

            //    await stepContext.Context.SendActivityAsync("Thank you. Your Loan application details have been captured and sent to one of our agents for loan processing.");

            //    return await stepContext.EndDialogAsync(null, cancellationToken);
            //    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks. Your profile will not be saved and sent to one of our agents for loan processing."), cancellationToken);
        

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is the end.
            //return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 150);
        }

        //private static async Task<bool> PicturePromptValidatorAsync(PromptValidatorContext<IList<Microsoft.Bot.Schema.Attachment>> promptContext, CancellationToken cancellationToken)
        //{
        //    if (promptContext.Recognized.Succeeded)
        //    {
        //        var attachments = promptContext.Recognized.Value;
        //        var validImages = new List<System.Net.Mail.Attachment>();

        //        foreach (var attachment in attachments)
        //        {
        //            if (attachment.ContentType == "image/jpeg" || attachment.ContentType == "image/png")
        //            {
        //                //validImages.Add(attachment);
        //            }
        //        }

        //        promptContext.Recognized.Value = (IList<Microsoft.Bot.Schema.Attachment>)validImages;

        //        // If none of the attachments are valid images, the retry prompt should be sent.
        //        return validImages.Any();
        //    }
        //    else
        //    {
        //        await promptContext.Context.SendActivityAsync("No attachments received. Proceeding without a profile picture...");

        //        // We can return true from a validator function even if Recognized.Succeeded is false.
        //        return true;
        //    }
        //}


        public string FullName { get; set; }

        public int Age { get; set; }

        public string Email { get; set; }

        public int PhoneNumber { get; set; }

        public string Location { get; set; }

        public int Amount { get; set; }

        public System.Net.Mail.Attachment Picture { get; set; }

        public bool IsRegistered { get; set; }
    }
}