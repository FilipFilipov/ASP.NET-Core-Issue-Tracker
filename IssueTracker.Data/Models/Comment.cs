using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int IssueId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; }
    }
}
