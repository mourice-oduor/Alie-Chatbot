using Microsoft.Bot.Builder.Dialogs;

namespace Alie.Dialogs.Operations
{
    public class MasomoBoostDialog : ComponentDialog
    {
        public MasomoBoostDialog() : base(nameof(MasomoBoostDialog))
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
