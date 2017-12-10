using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
