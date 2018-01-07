namespace IssueTracker.Data.Models
{
    public class IssueLabel
    {
        public int IssueId { get; set; }

        public int LabelId { get; set; }

        public Issue Issue { get; set; }

        public Label Label { get; set; }
    }
}
