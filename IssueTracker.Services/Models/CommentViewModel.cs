using System;
using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Services.Models
{
    public class CommentViewModel
    {
        public int Id { get; set; }

        public int IssueId { get; set; }

        public string Author { get; set; }

        public DateTime Created { get; set; }

        [Required]
        [StringLength(1000)]
        public string Text { get; set; }
    }
}
