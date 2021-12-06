namespace Buaa.AIBot.Controllers.Models
{
    public class UserBody
    {
        public string Name {get; set;}

        public string Email {get; set;}

        public string Password {get; set;}

        public int? Uid {get; set;}

        // 0 is Traveler, 1 is User, 2 is Administrator
        public int? Auth {get; set;}

        public string Token {get; set;}

        public int? ProfilePhoto { get; set; }
    }
}