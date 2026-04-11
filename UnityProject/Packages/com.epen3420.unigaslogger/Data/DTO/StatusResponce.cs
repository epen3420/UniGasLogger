namespace UniGasLogger.Data
{
    public enum Status
    {
        success,
        Warning,
        error,
    }

    [System.Serializable]
    public class StatusResponse
    {
        public string status;
        public string message;

        public Status GetStatus() => (Status)System.Enum.Parse(typeof(Status), status);
    }
}
