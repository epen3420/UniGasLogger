using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UniGasLogger.Data;
using UniGasLogger.Http;

namespace UniGasLogger.Service
{
    internal class RankingClientService
    {
        private readonly GasHttpClient gasHttpClient;

        public RankingClientService(GasHttpClient gasHttpClient)
        {
            this.gasHttpClient = gasHttpClient;
        }


        /// <summary>
        /// トップ5ランキングを取得します。
        /// </summary>
        public async Task<RankingResponse> GetTop5Ranking(string sheetName = "Ranking")
        {
            var queryParams = new Dictionary<string, string>
        {
            {"sheetName", sheetName}
        };

            return await gasHttpClient.GetGas<RankingResponse>(queryParams);
        }

        /// <summary>
        /// 指定したスコアの順位を取得します。
        /// </summary>
        /// <param name="score">問い合わせたいスコア</param>
        public async Task<ScoreRankResponse> GetScoreRank(BigInteger score, string sheetName = "Ranking")
        {
            var queryParams = new Dictionary<string, string>
        {
            {"score", score.ToString()},
            {"sheetName", sheetName}
        };

            return await gasHttpClient.GetGas<ScoreRankResponse>(queryParams);
        }
    }
}
