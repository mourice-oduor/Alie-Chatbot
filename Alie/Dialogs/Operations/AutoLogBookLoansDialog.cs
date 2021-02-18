using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
