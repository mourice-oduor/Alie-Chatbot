using Microsoft.Bot.Builder.Dialogs;

namespace Alie.Dialogs.Operations
{
    public class AssetFinanceDialog : ComponentDialog
    {
        public AssetFinanceDialog() : base(nameof(AssetFinanceDialog))
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
