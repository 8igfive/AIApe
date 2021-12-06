using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Repository.Models
{
    public class UserData
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "varchar(254) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci")]
        public string Email { get; set; }

        [Required]
        [Column(TypeName = "char(60)")]
        public string Bcrypt { get; set; }

        [Required]
        [Column(TypeName = "varchar(18) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci UNIQUE")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public AuthLevel Auth { get; set; }

        public int ProfilePhoto { get; set; }

        // relation references
        public List<QuestionData> Questions { get; set; }
        public List<AnswerData> Answers { get; set; }
        public List<LikeAnswer> LikeAnswers { get; set; }
        public List<LikeQuestion> LikeQuestions { get; set; }
    }

    public class QuestionData
    {
        [Key]
        public int QuestionId { get; set; }
        public int? BestAnswerId { get; set; }
        public int? UserId { get; set; }

        [Required]
        [Column(TypeName = "varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci")]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "longtext")]
        public string Remarks { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ModifyTime { get; set; }

        // relation references
        public UserData User { get; set; }
        public List<AnswerData> Answers { get; set; }
        public List<QuestionTagRelation> QuestionTagRelation { get; set; }
        public List<LikeQuestion> LikedInfo { get; set; }
        public QuestionHotData HotData { get; set; }
    }

    public class QuestionHotData
    {
        [Key]
        public int QuestionId { get; set; }
        public int HotValue { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ModifyTime { get; set; }

        public QuestionData Question { get; set; }
    }

    public class AnswerData
    {
        [Key]
        public int AnswerId { get; set; }
        public int? UserId { get; set; }
        public int QuestionId { get; set; }

        [Required]
        [Column(TypeName = "longtext")]
        public string Content { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ModifyTime { get; set; }


        // relation references
        public UserData User { get; set; }
        public QuestionData Question { get; set; }
        public List<LikeAnswer> LikedInfo { get; set; }
    }

    public class TagData
    {
        [Key]
        public int TagId { get; set; }

        public int Category { get; set; }

        [Required]
        [Column(TypeName = "varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci UNIQUE")]
        public string Name { get; set; }

        [Required]
        public string Desc { get; set; }

        // relation references
        public List<QuestionTagRelation> QuestionTagRelation { get; set; }
    }

    public class QuestionTagRelation
    {
        public int TagId { get; set; }
        public int QuestionId { get; set; }

        // relation references
        public QuestionData Question { get; set; }
        public TagData Tag { get; set; }

        public override bool Equals(object obj)
        {
            return obj is QuestionTagRelation relation &&
                   TagId == relation.TagId &&
                   QuestionId == relation.QuestionId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TagId, QuestionId);
        }
    }

    public class LikeAnswer
    {
        public int UserId { get; set; }
        public int AnswerId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTime { get; set; }

        // relation references
        public AnswerData Answer { get; set; }
        public UserData User { get; set; }

        public override bool Equals(object obj)
        {
            return obj is LikeAnswer relation &&
                   UserId == relation.UserId &&
                   AnswerId == relation.AnswerId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, AnswerId);
        }
    }

    public class LikeQuestion
    {
        public int UserId { get; set; }
        public int QuestionId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTime { get; set; }

        // relation references
        public QuestionData Question { get; set; }
        public UserData User { get; set; }

        public override bool Equals(object obj)
        {
            return obj is LikeQuestion relation &&
                   UserId == relation.UserId &&
                   QuestionId == relation.QuestionId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, QuestionId);
        }
    }

    public class NaturalQuestion
    {
        [Key]
        public int NaturalQuestionId { get; set; }

        [Required]
        [Column(TypeName = "longtext")]
        public string Content { get; set; }

        public List<NatrualQuestionAnswerRelation> NatrualQuestionAnswerRelations { get; set; }
    }

    public class NatrualAnswer
    {
        [Key]
        public int NatrualAnswerId { get; set; }

        [Required]
        [Column(TypeName = "longtext")]
        public string Content { get; set; }

        public List<NatrualQuestionAnswerRelation> NatrualQuestionAnswerRelations { get; set; }
    }

    public class NatrualQuestionAnswerRelation
    {
        public int NaturalQuestionId { get; set; }
        public int NatrualAnswerId { get; set; }

        public NatrualAnswer NatrualAnswer { get; set; }
        public NaturalQuestion NaturalQuestion { get; set; }
    }
}
