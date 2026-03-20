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

            if (!(string.IsNullOrEmpty(deployId) ||
                string.IsNullOrEmpty(authToken) ||
                string.IsNullOrEmpty(sheetId)))
            {
                Debug.Log("[UniGasLogger] 環境変数を検知しました。ビルド用に GasSettings.asset を自動生成します...");

                SaveGasSettings(deployId, authToken, sheetId);
                return;
            }

            string[] args = Environment.GetCommandLineArgs();

            deployId = GetArgument(args, "-UNIGAS_DEPLOY_ID");
            authToken = GetArgument(args, "UNIGAS_AUTH_TOKEN");
            sheetId = GetArgument(args, "UNIGAS_SHEET_ID");

            if (!(string.IsNullOrEmpty(deployId) ||
                string.IsNullOrEmpty(authToken) ||
                string.IsNullOrEmpty(sheetId)))
            {
                Debug.Log("[UniGasLogger] コマンドライン引数による環境変数を検知しました。ビルド用に GasSettings.asset を自動生成します...");

                SaveGasSettings(deployId, authToken, sheetId);
                return;
            }


            Debug.Log("[UniGasLogger] 環境変数が設定されていないため、既存の GasSettings.asset を使用してビルドします。");
        }

        private void SaveGasSettings(string deployId, string authToken, string sheetId)
        {
            var settings = GasSettingsService.LoadOrCreateSettings();
            GasSettingsService.UpdateSettings(settings, deployId, authToken, sheetId);
            GasSettingsService.SaveSettingsToDisk();
        }

        private static string GetArgument(string[] args, string name)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
#endif
