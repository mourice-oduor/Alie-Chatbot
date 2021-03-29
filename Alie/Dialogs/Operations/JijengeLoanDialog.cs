using Microsoft.Bot.Builder.Dialogs;

namespace Alie.Dialogs.Operations
{
    public class JijengeLoanDialog : ComponentDialog
    {
        public JijengeLoanDialog() : base(nameof(JijengeLoanDialog))
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
