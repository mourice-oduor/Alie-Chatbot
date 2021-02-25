using Microsoft.Bot.Builder.Dialogs;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Alie.Dialogs.Details;

namespace Alie.Dialogs.Operations
{
    public class ImportFinanceDialog : ComponentDialog
    {
        public ImportFinanceDialog() : base(nameof(ImportFinanceDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            };

            AddDialog(new LoanApplicationDetailsDialog());
            AddDialog(new MainMenuDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("With Motisha creadit from Ngao, you CHOOSE the car you want to buy, we'll finance up to 70% the value of the car, sit back and RELAX while we handle your vehicle’s documentation, processing, and importation. Our seamless end to end solution will deliver your dream car in just 60 days! When you import with Motisha, you get flexible repayment periods of up to 2 years with Interests as low as 3.5%," +
                "all in three easy steps: " + "  " +
                                                          ">CHOOSE your dream car from our partner websites  " + "  " +
                                                          ">RELAX, as we handle the entire process of importing your dream car. " + "  " +
                                                          ">Get to DRIVE your car as we will deliver it to you within 60 days after your order. "), cancellationToken);


            List<string> operationList = new List<string> { "1. Apply This Loan",
                                                            "2. Back To Previous Menu",
                                                            "3. Main Menu"};

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

            if ("Apply This Loan".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(LoanApplicationDetailsDialog), new UserProfile(), cancellationToken);
            }
            else if ("Back To Previous Menu".Equals(operation))
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, cancellationToken);
            }
            else if ("Main Menu".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(MainMenuDialog), new UserProfile(), cancellationToken);
            }

            else
            {
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Wrong User Input. Please try again!"), cancellationToken);
                return await stepContext.ReplaceDialogAsync(InitialDialogId, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.BeginDialogAsync(promptMessage, InitialDialogId, cancellationToken);
        }
    }
}
