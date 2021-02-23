using AdaptiveCards;
using Alie.Dialogs.Details;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Attachment = Microsoft.Bot.Schema.Attachment;
using Activity = Microsoft.Bot.Schema.Activity;
using Microsoft.Extensions.Logging;

namespace Alie.Dialogs.Operations
{
    public class AutoLogBookLoansDialog : ComponentDialog
    {

        public AutoLogBookLoansDialog() : base(nameof(AutoLogBookLoansDialog))
        {

            var waterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            };

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new AutoLogBookDetailsDialog());
            AddDialog(new LoanApplicationDetailsDialog());
            AddDialog(new BackDialog());
            AddDialog(new MainMenuDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));


            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Please select one of the item below!"), cancellationToken);

            List<string> operationList = new List<string> { "1. Loan Details",
                                                            "2. Apply This Loan",
                                                            "3. Back To Previous Menu",
                                                            "4. Main Menu"};
            // Create card
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = operationList.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,  // This will be a string
                }).ToList<AdaptiveAction>(),
            };
            // Prompt
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    // Convert the AdaptiveCard to a JObject
                    Content = JObject.FromObject(card),
                }),
                Choices = ChoiceFactory.ToChoices(operationList),
                // Don't render the choices outside the card
                Style = ListStyle.None,
            },
        cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Operation"] = ((FoundChoice)stepContext.Result).Value;
            string operation = (string)stepContext.Values["Operation"];
            //await stepContext.Context.SendActivityAsync((operation));


            if ("Loan Details".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(AutoLogBookDetailsDialog), new UserProfile(), cancellationToken);
            }

            else if ("Apply This Loan".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(LoanApplicationDetailsDialog), new UserProfile(), cancellationToken);
            }
            else if ("Back To Previous Menu".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(BackDialog), new UserProfile(), cancellationToken);
            }
            else if ("Main Menu".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(MainMenuDialog), new UserProfile(), cancellationToken);
            }

            else
            {
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Wrong User Input. Please try again!"), cancellationToken);
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            //var promptMessage = "What else can I do for you?";
            //return await stepContext.NextAsync(promptMessage, cancellationToken: cancellationToken);


            return await stepContext.BeginDialogAsync(nameof(AutoLogBookDetailsDialog), new UserProfile(), cancellationToken);
            //return await stepContext.BeginDialogAsync(nameof(LoanApplicationDetailsDialog), new UserProfile(), cancellationToken);



            //string operation = (string)stepContext.Values["Operation"];


            //switch (operation)
            //{
            //    case "Auto LogBook Loans":
            //        return await stepContext.BeginDialogAsync(nameof(AutoLogBookLoansDialog), cancellationToken);

            //    case "Asset Finance":
            //        return await stepContext.BeginDialogAsync(nameof(AssetFinanceDialog), cancellationToken);

            //    case "Loan Against Shares":
            //        return await stepContext.BeginDialogAsync(nameof(LoanAgainstSharesDialog), cancellationToken);
            //}

            //// We shouldn't get here, but fail gracefully if we do.
            //await stepContext.Context.SendActivityAsync("I don't recognize that option.", cancellationToken: cancellationToken);

            //// Continue through to the next step without starting a child dialog.
            //return await stepContext.NextAsync(cancellationToken: cancellationToken);

        }
    }
}
