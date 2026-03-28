namespace UniGasLogger.Data
{
    /// <summary>
    /// Gasに問い合わせる際のエラー出力用
    /// </summary>
    [System.Serializable]
    internal class GasErrorResponse
    {
        public string error;
        public string details;
    }
}
