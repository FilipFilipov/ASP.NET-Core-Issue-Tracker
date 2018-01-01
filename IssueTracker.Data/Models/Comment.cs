using System;
using System.ComponentModel.DataAnnotations;
using IssueTracker.Models;

namespace IssueTracker.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int IssueId { get; set; }

        public string AuthorId { get; set; }

        public User Author { get; set; }

        public DateTime Created { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; }

        public Issue Issue { get; set; }
    }
}
