using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Services;

namespace Buaa.AIBot.Bot.WorkingModule
{
    public class QuestionBuilder
    {
        public static readonly string SimpleDescribe = ConstantStrings.SimpleDescribe;
        public static readonly string DetailDescribe = ConstantStrings.DetailDescribe;
        public static readonly string OS = "OS";
        public static readonly string OS_detail = "OS_detail";
        public static readonly string IDE = "IDE";
        public static readonly string IDE_detail = "IDE_detail";
        public static readonly string Compiler = "Compiler";
        public static readonly string Compiler_detail = "Compiler_Detail";

        public static readonly string Category = "Category";

        public enum QuestionCategory
        {
            EnvInstalling,
            EnvUsing,
            StdLib,
            Statement,
            Keywords,
            Code
        }

        public static readonly IReadOnlyDictionary<QuestionCategory, string> CategoryToChinese = new Dictionary<QuestionCategory, string>()
        {
            [QuestionCategory.EnvInstalling] = "环境配置",
            [QuestionCategory.EnvUsing] = "工具使用",
            [QuestionCategory.StdLib] = "标准库",
            [QuestionCategory.Statement] = "语句",
            [QuestionCategory.Keywords] = "关键字",
            [QuestionCategory.Code] = "代码",
        };

        private IQuestionService questionService;

        public QuestionBuilder(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public static void TryAddTag(IReadOnlyDictionary<string, int> source, List<int> target, string name)
        {
            int tid;
            if (source.TryGetValue(name, out tid))
            {
                target.Add(tid);
            }
        }

        public async Task<Tuple<List<int>, string>> SearchStatusAsync(IBotStatusContainer status)
        {
            var tagsInDatabase = await questionService.GetTagListAsync();
            var tagsOfQuestion = new List<int>();
            var remarksFromStatus = status.Get<string>(DetailDescribe);
            StringBuilder remarks = new StringBuilder();
            remarks.Append(remarksFromStatus);
            QuestionCategory category;
            if (status.TryGet(Category, out category))
            {
                TryAddTag(tagsInDatabase, tagsOfQuestion, CategoryToChinese[category]);
            }
            string os;
            if (status.TryGet(OS, out os))
            {
                TryAddTag(tagsInDatabase, tagsOfQuestion, os);
                remarks.Append("\n* OS:");
                string os_detail;
                if (status.TryGet(OS_detail, out os_detail))
                {
                    remarks.Append(os_detail);
                }
                else
                {
                    remarks.Append(os);
                }
            }
            string ide;
            if (status.TryGet(IDE, out ide))
            {
                TryAddTag(tagsInDatabase, tagsOfQuestion, ide);
                remarks.Append("\n* IDE");
                string ide_detail;
                if (status.TryGet(IDE_detail, out ide_detail))
                {
                    remarks.Append(ide_detail);
                }
                else
                {
                    remarks.Append(ide);
                }
            }
            string compiler;
            if (status.TryGet(Compiler, out compiler))
            {
                TryAddTag(tagsInDatabase, tagsOfQuestion, compiler);
                remarks.Append("\n* 编译器");
                string compiler_detail;
                if (status.TryGet(Compiler_detail, out compiler_detail))
                {
                    remarks.Append(compiler_detail);
                }
                else
                {
                    remarks.Append(compiler);
                }
            }
            return Tuple.Create(tagsOfQuestion, remarks.ToString());
        }

        public async Task<int> BuildAsync(IBotStatusContainer status)
        {
            int uid = status.UserId;
            string title = status.Get<string>(SimpleDescribe);
            var res = await SearchStatusAsync(status);
            var tags = res.Item1;
            var remarks = res.Item2;
            int qid = await questionService.AddQuestionAsync(uid, title, remarks, tags);
            return qid;
        }
    }
}
