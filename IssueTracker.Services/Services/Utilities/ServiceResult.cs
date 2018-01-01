namespace IssueTracker.Services.Services.Utilities
{
    public class ServiceResult<T> where T: class
    {
        public ServiceResult(T value, string message = null)
        {
            Value = value;
            Message = message;
            NotificationType = NotificationType.Success;
        }

        public ServiceResult(string message)
        {
            Message = message;
            NotificationType = NotificationType.Error;
        }

        public T Value { get; set; }

        public string Message { get; set; }

        public NotificationType NotificationType { get; set; }
    }
}
