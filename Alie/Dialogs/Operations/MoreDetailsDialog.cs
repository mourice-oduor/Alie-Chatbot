﻿using Microsoft.Bot.Builder.Dialogs;
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
using Alie.Models;

namespace Alie.Dialogs.Details
{
    public class MoreDetailsDialog : ComponentDialog
    {

        public MoreDetailsDialog() : base(nameof(MoreDetailsDialog))
        {
            AddDialog(new FaqsDialog());
            AddDialog(new ContactDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));


            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hello, What else can I do for you?"), cancellationToken);


            List<string> operationList = new List<string> { "1. Back To Previous Menu",
                                                            "2. FAQs",
                                                            "3. Visit Nearest Office",
                                                            "4. Contact Us!",
                                                            "5. Want to try a Riddle?",
                                                            "6. Main Menu"};

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

            var userProfile = new UserProfile()
            {
            };

            if ("1. Back To Previous Menu".Equals(operation))
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
            }
            else if ("2. FAQs".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(FaqsDialog), userProfile, cancellationToken);
            }
            else if ("3. Visit Nearest Office".Equals(operation))
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 2;
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), userProfile, cancellationToken);
            }
            else if ("4. Contact Us!".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(ContactDialog), userProfile, cancellationToken);
            }
            else if ("5. Want to try a Riddle?".Equals(operation))
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 2;
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), userProfile, cancellationToken);
            }
            else if ("6. Main Menu".Equals(operation))
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 2;
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), userProfile, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Wrong User Input. Please try again!"), cancellationToken);
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
