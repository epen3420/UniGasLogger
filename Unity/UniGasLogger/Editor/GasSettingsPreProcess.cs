#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System;

namespace UniGasLogger.Editor
{
    public class GasSettingsPreProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            string deployId = Environment.GetEnvironmentVariable("UNIGAS_DEPLOY_ID");
            string authToken = Environment.GetEnvironmentVariable("UNIGAS_AUTH_TOKEN");
            string sheetId = Environment.GetEnvironmentVariable("UNIGAS_SHEET_ID");

            if (string.IsNullOrEmpty(deployId) ||
                string.IsNullOrEmpty(authToken) ||
                string.IsNullOrEmpty(sheetId))
            {
                Debug.Log("[UniGasLogger] 環境変数が設定されていないため、既存の GasSettings.asset を使用してビルドします。");
                return;
            }

            Debug.Log("[UniGasLogger] 環境変数を検知しました。ビルド用に GasSettings.asset を自動生成します...");

            var settings = GasSettingsService.LoadOrCreateSettings();
            GasSettingsService.UpdateSettings(settings, deployId, authToken, sheetId);
            GasSettingsService.SaveSettingsToDisk();
        }
    }
}
#endif
