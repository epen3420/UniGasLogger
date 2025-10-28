namespace UniGasLogger.Data
{
    /// <summary>
    /// スコアのランキングを問い合わせる際に使用
    /// </summary>
    [System.Serializable]
    public class ScoreRankResponse
    {
        public string score;
        public int rank;
        public string error;
    }
}
