using System.Collections.Generic;
using UnityEngine;

namespace UniGasLogger.Samples
{
    public class test_SendLog : MonoBehaviour
    {
        [System.Serializable]
        public class PlayLog
        {
            public string PlayerName { get; set; }
            public int Score { get; set; }
            public bool IsClear { get; set; }
        }

        public async void OnGameEnd(int finalScore)
        {
            var log = new PlayLog
            {
                PlayerName = "Hero_Alpha",
                Score = finalScore,
                IsClear = true
            };

            // GASへ送信。GAS側で "Stage1Clear" というシート名として扱われます。
            await GasServiceManager.Instance.SendLog(log, "Stage1Clear");
        }

        // Dictionary を送信する例
        public async void OnGameStart()
        {
            var log = new Dictionary<string, object>
        {
            { "Event", "GameStart" },
            { "SceneName", "Level_01" }
        };

            await GasServiceManager.Instance.SendLog(log, "EventLog");
        }
    }
}
