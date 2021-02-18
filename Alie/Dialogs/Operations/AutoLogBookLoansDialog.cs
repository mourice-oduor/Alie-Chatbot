using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alie.Dialogs.Operations
{
    public class AutoLogBookLoansDialog : ComponentDialog
    {
        public AutoLogBookLoansDialog() : base(nameof(AutoLogBookLoansDialog))
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
