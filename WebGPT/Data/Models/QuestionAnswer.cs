namespace WebGPT.Data.Models
{
    public class QuestionAnswer
    {
        public QuestionAnswer(string question)
        {
            Question = question;
        }

        public string Question { get; set; }
        public string? Answer { get; set; }
    }
}
