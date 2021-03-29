using Microsoft.Bot.Builder.Dialogs;

namespace Alie.Dialogs.Operations
{
    public class LoanAgainstSharesDialog : ComponentDialog
    {
        public LoanAgainstSharesDialog() : base(nameof(LoanAgainstSharesDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {

            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }
    }
}
