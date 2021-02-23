using Microsoft.Azure.Documents;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Alie.Dialogs.Details
{
    public class LoanApplicationDetailsDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public LoanApplicationDetailsDialog(UserState userState) : base(nameof(LoanApplicationDetailsDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                NameConfirmStepAsync,
                EmailAddressStepAsync,
                PhoneNumberStepAsync,
                LocationStepAsync,
                AgeStepAsync,
                AmountStepAsync,
                PictureStepAsync,
                ConfirmStepAsync,
                SummaryStepAsync,
            };


            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new MainMenuDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        public LoanApplicationDetailsDialog()
        {
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your full name.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please provide your email address") }, cancellationToken);

        }
        private async Task<DialogTurnResult> EmailAddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //stepContext.Values["name"] = (string)stepContext.Result
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your email address.") }, cancellationToken);
        }
        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["email"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your Phone Number .") }, cancellationToken);
        }
        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phone"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please Provide your Location.") }, cancellationToken);
        }
        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["location"] = (string)stepContext.Result;

            if ((bool)stepContext.Result)
            {
                // User said "yes" so we will be prompting for the age.
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter your age."),
                    RetryPrompt = MessageFactory.Text("The value entered must be greater than 0 and less than 150."),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
            }
            else
            {
                // User said "no" so we will skip the next step. Give -1 as the age.
                return await stepContext.NextAsync(-1, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> AmountStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (string)stepContext.Result;
            if ((bool)stepContext.Result)
            {
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter the loan amount."),
                    RetryPrompt = MessageFactory.Text("The value entered must be greater than 0 and less than 300,000."),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(-1, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> PictureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["amount"] = (string)stepContext.Result;
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please attach a profile picture (or type any message to skip)."),
                RetryPrompt = MessageFactory.Text("The attachment must be a jpeg/png image file."),
            };

            return await stepContext.PromptAsync(nameof(AttachmentPrompt), promptOptions, cancellationToken);

        }
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["picture"] = ((IList<Microsoft.Bot.Schema.Attachment>)stepContext.Result)?.FirstOrDefault();

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is this ok?") }, cancellationToken);

        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // Get the current profile object from user state.
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

                userProfile.Email = (string)stepContext.Values["email"];
                userProfile.FullNames = (string)stepContext.Values["name"];
                userProfile.Age = (int)stepContext.Values["age"];
                userProfile.Picture = (Microsoft.Azure.Documents.Attachment)stepContext.Values["picture"];

                var msg = $"I have your Email Address as {userProfile.Email} and your name as {userProfile.FullNames}";

                if (userProfile.Age != -1)
                {
                    msg += $" and your age as {userProfile.Age}";
                }

                msg += ".";

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

                if (userProfile.Picture != null)
                {
                    try
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment((IEnumerable<Microsoft.Bot.Schema.Attachment>)userProfile.Picture, "This is your profile picture."), cancellationToken);
                    }
                    catch
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("A profile picture was saved but could not be displayed here."), cancellationToken);
                    }
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks. Your profile will not be saved and sent to one of our agents for loan processing."), cancellationToken);
            }

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }


        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 150);
        }

        private static async Task<bool> PicturePromptValidatorAsync(PromptValidatorContext<IList<Microsoft.Bot.Schema.Attachment>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var attachments = promptContext.Recognized.Value;
                var validImages = new List<Microsoft.Bot.Schema.Attachment>();

                foreach (var attachment in attachments)
                {
                    if (attachment.ContentType == "image/jpeg" || attachment.ContentType == "image/png")
                    {
                        validImages.Add(attachment);
                    }
                }

                promptContext.Recognized.Value = validImages;

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


        public string FullNames { get; set; }

        public string EmailAddress { get; set; }

        public int PhoneNumber { get; set; }

        public string Location { get; set; }

        public int Age { get; set; }

        public int Amount { get; set; }

        public Microsoft.Bot.Schema.Attachment Picture { get; set; }

    }
}
