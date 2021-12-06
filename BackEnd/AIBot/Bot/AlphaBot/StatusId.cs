using Buaa.AIBot.Bot.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.AlphaBot.Status;

namespace Buaa.AIBot.Bot.AlphaBot
{
    public enum StatusId
    {
        Welcome,

            Environment,

                // OnInstalling,
                    GetOSForInstalling,
                        GetIDEForInstalling,
                            ShowGovernmentLinkForInstalling,
                                
                
                // OnUsing,
                    GetOSForUsing,
                        GetIDEForUsing,
                            GetCompilerForUsing,
                                ShowDocumentLinkForUsing,

            Gramma,
                // OnStandardLibary,
                    ShowLinksForStandardLibary,

                // OnStatement,
                    GetStatementTypeForStatement,
                        ShowLinksForStatement,

                // OnKeywords,
                    GetKeywordForKeywords,
                        ShowLinksForKeywords,

            // WithCode,
                GetCode,
                    AskIfHaveWrongCase,
                        GetWrongCaseInput,
                            GetWrongCaseExpectOutput,
                                TrySolveWithCode,
                            
        GetSimpleDescribe,
            ShowSearchResult,
                ShowDatabaseResult,
                    GetDetails,
                        RunAddQuestion,
    }

    public static class Configuration
    {
        public static Dictionary<StatusId, IBotStatusBehaviour<StatusId>> GetStatusBehaviours()
        {
            var list = new List<IBotStatusBehaviour<StatusId>>()
            {
                new WelcomeStatus(),
                    new EnvironmentStatus(),
                        // On Installing
                            new GetOSForInstallingStatus(),
                                new GetIDEForInstallingStatus(),
                                    new ShowGovernmentLinkForInstallingStatus(),
                        // On Using
                            new GetOSForUsingStatus(),
                                new GetIDEForUsingStatus(),
                                    new GetCompilerForUsingStatus(),
                                        new ShowDocumentLinkForUsingStatus(),

                    new GrammaStatus(),
                        // OnStandardLibary,
                            new ShowLinksForStandardLibaryStatus(),

                        // OnStatement,
                            new GetStatementTypeForStatementStatus(),
                                new ShowLinksForStatementStatus(),

                        // OnKeywords,
                            new GetKeywordForKeywordsStatus(),
                                new ShowLinksForKeywordsStatus(),

                    new GetCodeStatus(),
                        new AskIfHaveWrongCaseStatus(),
                            new GetWrongCaseInputStatus(),
                                new GetWrongCaseExpectOutputStatus(),
                                    new TrySolveWithCodeStatus(),


                new GetSimpleDescribeStatus(),
                    new ShowSearchResultStatus(),
                        new GetDetailsStatus(),
                            new AddQuestionStatus()
            };
            var ret = new Dictionary<StatusId, IBotStatusBehaviour<StatusId>>();
            foreach (var status in list)
            {
                ret[status.Id] = status;
            }
            return ret;
        }
    }
}
