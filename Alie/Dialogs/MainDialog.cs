using AdaptiveCards;
using Alie.Dialogs.Details;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Alie.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Alie.Services;

namespace Alie.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        private readonly StateService _stateService;
        private readonly UserRepository _userRepository;
        protected readonly IConfiguration Configuration;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ILogger<MainDialog> logger, StateService stateService, UserRepository userRepository, IConfiguration configuration)
            : base(nameof(MainDialog))
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));
            Logger = logger;
            _userRepository = userRepository;
            Configuration = configuration;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new EmailAuthenticationDialog(_stateService, userRepository, Configuration));
            AddDialog(new ProductsDialog());
            AddDialog(new CheckBalDialog());
            AddDialog(new MoreDetailsDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NameStepAsync,
                EmailAuthenticationStepAsync,
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("What is your name?")
                }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> EmailAuthenticationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                userProfile.Name = (string)stepContext.Result;

                await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            }


            if (string.IsNullOrEmpty(userProfile.Email))
            {
                return await stepContext.BeginDialogAsync(nameof(EmailAuthenticationDialog), null, cancellationToken);
            }
            else
            {
                if (userProfile.EmailVerified)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Welcome back {userProfile.Name}, How can I help you today?"), cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hello {userProfile.Name}, Your email is not verified."), cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(EmailAuthenticationDialog), null, cancellationToken);
                }

            }
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("What type of product would like to chose from?"), cancellationToken);

            List<string> operationList = new List<string> { "1. LOAN PRODUCTS",
                                                            "2. CHECK BALANCE",
                                                            "3. TRANSACTION HISTORY",
                                                            "4. REFER CLIENT",
                                                            "5. TERMS AND CONDITION",
                                                            "6. MORE"
                                                           };
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
                Prompt = (Microsoft.Bot.Schema.Activity)MessageFactory.Attachment(new Attachment
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
            //if(innerDc.stepContext.Activity.Type == ActivityTypes.Message)
            stepContext.Values["Operation"] = ((FoundChoice)stepContext.Result).Value;
            string operation = (string)stepContext.Values["Operation"];
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("You have selected - " + operation), cancellationToken);

            if ("1. LOAN PRODUCTS".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(ProductsDialog), new UserProfile(), cancellationToken);
            }
            else if ("2. CHECK BALANCE".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(CheckBalDialog), new UserProfile(), cancellationToken);
            }
            else if ("3. TRANSACTION HISTORY".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(MainDialog), new UserProfile(), cancellationToken);
            }
            else if ("4. REFER CLIENT".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(MainDialog), new UserProfile(), cancellationToken);
            }
            else if ("5. TERMS AND CONDITION".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(MainDialog), new UserProfile(), cancellationToken);
            }
            else if ("6. MORE".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(MoreDetailsDialog), new UserProfile(), cancellationToken);
            }

            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Wrong User Input. Please try again!"), cancellationToken);
                return await stepContext.NextAsync(cancellationToken: cancellationToken);

            }

        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
            //return await stepContext.EndDialogAsync(InitialDialogId, cancellationToken);

        }
    }

}



















//if ("Auto LogBook Loans".Equals(operation))
//{
//    return await stepContext.BeginDialogAsync(nameof(AutoLogBookLoansDialog), new UserProfile(), cancellationToken);
//}
//else if ("Asset Finance".Equals(operation))
//{
//    return await stepContext.BeginDialogAsync(nameof(AssetFinanceDialog), new UserProfile(), cancellationToken);
//}
//else if ("Loan Against Shares".Equals(operation))
//{
//    return await stepContext.BeginDialogAsync(nameof(LoanAgainstSharesDialog), new UserProfile(), cancellationToken);
//}
//else if ("Masomo Boost".Equals(operation))
//{
//    return await stepContext.BeginDialogAsync(nameof(MasomoBoostDialog), new UserProfile(), cancellationToken);
//}
//else if ("Jijenge Loan".Equals(operation))
//{
//    return await stepContext.BeginDialogAsync(nameof(JijengeLoanDialog), new UserProfile(), cancellationToken);
//}
//else if ("Import Finance".Equals(operation))
//{
//    return await stepContext.BeginDialogAsync(nameof(ImportFinanceDialog), new UserProfile(), cancellationToken);
//}
//else
//{
//switch (operation)
//{
//    case "Auto LogBook Loans":
//        return await stepContext.BeginDialogAsync(nameof(AutoLogBookLoansDialog), new UserProfile(), cancellationToken);

//    case "Asset Finance":
//        return await stepContext.BeginDialogAsync(nameof(AssetFinanceDialog), new UserProfile(), cancellationToken);

//    case "Loan Against Shares":
//        return await stepContext.BeginDialogAsync(nameof(LoanAgainstSharesDialog), new UserProfile(), cancellationToken);

//    case "Masomo Boost":
//        return await stepContext.BeginDialogAsync(nameof(MasomoBoostDialog), new UserProfile(), cancellationToken);

//    case "Jijenge Loan":
//        return await stepContext.BeginDialogAsync(nameof(JijengeLoanDialog), new UserProfile(), cancellationToken);

//    case "Import Finance":
//        return await stepContext.BeginDialogAsync(nameof(ImportFinanceDialog), new UserProfile(), cancellationToken);

//    default:
//        break;
//}

//return await stepContext.BeginDialogAsync(nameof(AutoLogBookLoansDialog), new UserProfile(), cancellationToken);


//    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Wrong User Input. Please try again!"), cancellationToken);
//return await stepContext.BeginDialogAsync(nameof(AutoLogBookLoansDialog), new UserProfile(), cancellationToken);

//return await stepContext.ReplaceDialogAsync(InitialDialogId, cancellationToken);
//return await stepContext.NextAsync(cancellationToken: cancellationToken);

//return await stepContext.NextAsync(cancellationToken: cancellationToken);