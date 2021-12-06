using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Implement;
using Microsoft.EntityFrameworkCore;

namespace Buaa.AIBot.Repository
{
    public static class Extensions
    {
        public static IServiceCollection AddRepository(this IServiceCollection services, string ConnctingString)
        {
            IndependentDatabaseContext.ConnectingString = ConnctingString;
            services
                // .AddScoped(sp => new DatabaseContext(ConnctingString))
                .AddDbContext<DatabaseContext>(options =>
                {
                    options.UseMySQL(ConnctingString);
                })
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IQuestionRepository, QuestionRepository>()
                .AddScoped<IAnswerRepository, AnswerRepository>()
                .AddScoped<ITagRepostory, TagRepository>()
                .AddScoped<ILikeRepository, LikeRepository>()
                .AddTransient<ICrawlerOuterRepository, BaiduCrawlerRepository>()
                .AddSingleton<ICachePool<int>>(CachePool<int>.DEFAULT)
                ;

            return services;
        }
    }
}
