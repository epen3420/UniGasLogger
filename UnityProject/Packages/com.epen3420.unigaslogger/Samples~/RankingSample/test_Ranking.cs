using UnityEngine;
using System.Numerics;
using UniGasLogger.Data;

namespace UniGasLogger.Samples
{
    public class test_Ranking : MonoBehaviour
    {
        // トップ5ランキングを取得する例
        public async void ShowTop5Ranking()
        {
            // "RankingSheet" はGAS側でデータソースとして使用するシート名
            RankingResponse response = await GasServiceManager.Instance.GetTopNRanking(5, "RankingSheet");

            if (response?.ranking != null)
            {
                foreach (var entry in response.ranking)
                {
                    Debug.Log($"Rank Entry: {entry.name}, Score: {entry.score}");
                }
            }
        }

        // 自分のスコアが何位か問い合わせる例
        public async void CheckMyRank(BigInteger myScore)
        {
            ScoreRankResponse response = await GasServiceManager.Instance.GetScoreRanking(myScore, "RankingSheet");

            if (response != null && response.rank > 0)
            {
                Debug.Log($"Your score {response.score} is Rank {response.rank}!");
            }
        }
    }
}
