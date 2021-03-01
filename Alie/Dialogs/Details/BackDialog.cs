using Alie.Dialogs.Operations;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace Alie.Dialogs.Details
{
    public class BackDialog : ComponentDialog
    {
        public BackDialog() : base(nameof(BackDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                ActStepAsync
            };


            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new MainDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }



        private static async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(MainDialog), new UserProfile(), cancellationToken);
        }
    }
}
