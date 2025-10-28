namespace UniGasLogger.Data
{
    /// <summary>
    /// スコアのランキングを受け取る際に使用
    /// </summary>
    [System.Serializable]
    public class RankingResponse
    {
        public RankEntry[] ranking;
        public string error; // エラーメッセージも受信できるように追加
    }
}
