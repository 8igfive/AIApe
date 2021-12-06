using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace Buaa.AIBot.Repository.Models
{
    public class IndependentDatabaseContext : DatabaseContext
    {
        public static string ConnectingString { get; set; }
        private ILogger<IndependentDatabaseContext> logger;

        public IndependentDatabaseContext(ILogger<IndependentDatabaseContext> logger)
        {
            this.logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseMySQL(ConnectingString)
                .LogTo(s => logger.LogWarning(s), LogLevel.Warning)
                ;
        }
    }

    public class DatabaseContext : DbContext
    {
        public DbSet<UserData> Users { get; set; }
        public DbSet<QuestionData> Questions { get; set; }
        public DbSet<AnswerData> Answers { get; set; }
        public DbSet<TagData> Tags { get; set; }
        public DbSet<LikeQuestion> LikeQuestions { get; set; }
        public DbSet<LikeAnswer> LikeAnswers { get; set; }
        public DbSet<QuestionTagRelation> QuestionTagRelations { get; set; }
        public DbSet<QuestionHotData> QuestionHotDatas { get; set; }
        public DbSet<NatrualAnswer> NatrualAnswers { get; set; }
        public DbSet<NaturalQuestion> NatrualQuestions { get; set; }
        public DbSet<NatrualQuestionAnswerRelation> NatrualQuestionAnswerRelations { get; set; }

        // public string ConnectString { get; }

        protected DatabaseContext() { }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseMySql("server=localhost;database=aiape;user=root;password=DDXXYY0105");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region User--<n>--LikeAnswer--<n>--Answer

            modelBuilder.Entity<LikeAnswer>()
                .HasKey(la => new { la.AnswerId, la.UserId });

            modelBuilder.Entity<LikeAnswer>()
                .HasOne(la => la.Answer)
                .WithMany(a => a.LikedInfo);

            modelBuilder.Entity<LikeAnswer>()
                .HasOne(la => la.User)
                .WithMany(u => u.LikeAnswers);

            #endregion

            #region User--<n>--LikeQuestion--<n>--Question

            modelBuilder.Entity<LikeQuestion>()
                .HasKey(la => new { la.QuestionId, la.UserId });

            modelBuilder.Entity<LikeQuestion>()
                .HasOne(lq => lq.Question)
                .WithMany(q => q.LikedInfo);

            modelBuilder.Entity<LikeQuestion>()
                .HasOne(lq => lq.User)
                .WithMany(u => u.LikeQuestions);

            #endregion

            #region Tag--<n>--HasTag--<n>--Question

            modelBuilder.Entity<QuestionTagRelation>()
                .HasKey(qt => new { qt.QuestionId, qt.TagId });

            modelBuilder.Entity<QuestionTagRelation>()
                .HasOne(qt => qt.Question)
                .WithMany(q => q.QuestionTagRelation);

            modelBuilder.Entity<QuestionTagRelation>()
                .HasOne(qt => qt.Tag)
                .WithMany(t => t.QuestionTagRelation);

            #endregion

            #region NatrualQuestion--<n>----<n>NatrualAnswer

            modelBuilder.Entity<NatrualQuestionAnswerRelation>()
                .HasKey(nqa => new { nqa.NaturalQuestionId, nqa.NatrualAnswerId });

            modelBuilder.Entity<NatrualQuestionAnswerRelation>()
                .HasOne(nqa => nqa.NatrualAnswer)
                .WithMany(na => na.NatrualQuestionAnswerRelations);

            modelBuilder.Entity<NatrualQuestionAnswerRelation>()
                .HasOne(nqa => nqa.NaturalQuestion)
                .WithMany(nq => nq.NatrualQuestionAnswerRelations);

            #endregion

            // User--<1?>--Create--<n>--Question
            modelBuilder.Entity<QuestionData>()
                .HasOne(q => q.User)
                .WithMany(u => u.Questions)
                .OnDelete(DeleteBehavior.SetNull)
                ;

            // User--<1?>--Create--<n>--Answer
            modelBuilder.Entity<AnswerData>()
                .HasOne(a => a.User)
                .WithMany(u => u.Answers)
                .OnDelete(DeleteBehavior.SetNull)
                ;

            // Question --<1>--hot--<1> QuestionHot
            modelBuilder.Entity<QuestionHotData>()
                .HasOne(qh => qh.Question)
                .WithOne(q => q.HotData)
                .OnDelete(DeleteBehavior.Cascade)
                ;

            modelBuilder.Entity<UserData>()
                .HasAlternateKey(u => u.Email);
        }
    }
}
