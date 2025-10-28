using UniGasLogger.Data;
using UniGasLogger.Service;
using UniGasLogger.Http;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace UniGasLogger
{
    public class GasServiceManager : MonoBehaviour
    {
        public static GasServiceManager Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }


            var gasSettings = GasSettingsService.LoadSettings();
            var httpClient = new GasHttpClient(gasSettings);

            loggerService = new GasLoggerService(httpClient);
            rankingService = new RankingClientService(httpClient);
        }


        private GasLoggerService loggerService;
        private RankingClientService rankingService;


        public async Task SendLog<T>(T datas, string sheetName = null)
        {
            if (datas == null)
            {
                Debug.LogError("datas is null. so can not send log.");
                return;
            }

            await loggerService.SendLog(datas, sheetName);
        }

        public async Task SendLog(Dictionary<string, object> datas, string sheetName = null)
        {
            if (datas == null)
            {
                Debug.LogError("datas is null. so can not send log.");
                return;
            }

            await loggerService.SendLog(datas, sheetName);
        }

        public async Task<RankingResponse> GetTopNRanking(int n, string sheetName = null)
        {
            return await rankingService.GetTopNRanking(n, sheetName);
        }

        public async Task<ScoreRankResponse> GetScoreRanking(BigInteger score, string sheetName = null)
        {
            return await rankingService.GetScoreRank(score, sheetName);
        }
    }
}
