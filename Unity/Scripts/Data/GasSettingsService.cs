using UnityEditor;
using UnityEngine;
using System.IO;

namespace UniGasLogger.Data
{
    internal static class GasSettingsService
    {
        private const string AssetName = "GasSettings.asset";
        private const string ResourcesDirName = "Resources";
        private const string BaseDirPath = "Assets/Plugins";
        private const string FinalResourcesPath = BaseDirPath + "/" + ResourcesDirName;
        private const string FinalAssetPath = FinalResourcesPath + "/" + AssetName;


        /// <summary>
        /// Resourcesフォルダから設定ファイルをロードします。
        /// </summary>
        /// <returns>見つかったGasSettings、または null</returns>
        public static GasSettings LoadSettings()
        {
            GasSettings settings = Resources.Load<GasSettings>("GasSettings");

            return settings;
        }


        /// <summary>
        /// 新しい GasSettings アセットを作成し、指定の場所に保存します。
        /// </summary>
        /// <returns>新しく作成されたGasSettings</returns>
        public static GasSettings CreateSettings()
        {
            if (!FinalAssetPath.StartsWith("Assets/"))
            {
                Debug.LogError("Failed to create GasSettings: Asset path is invalid.");
                return null;
            }

            Debug.Log($"GasSettings asset not found. Creating a new one at '{FinalAssetPath}'");

            EnsureFoldersExist();

            var settings = ScriptableObject.CreateInstance<GasSettings>();
            AssetDatabase.CreateAsset(settings, FinalAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return settings;
        }


        /// <summary>
        /// ScriptableObject の設定値を更新し、ダーティ（変更済み）としてマークする
        /// </summary>
        public static void UpdateSettings(GasSettings settings, string newDeployId, string newAuthToken, string newSheetId)
        {
            // settingsがnullの場合は処理を中断
            if (settings == null) return;

            Undo.RecordObject(settings, "Change LogSender Settings");

            settings.Init(newDeployId, newAuthToken, newSheetId);

            EditorUtility.SetDirty(settings);
        }


        /// <summary>
        /// 変更されたアセット（settings を含む）をディスクに即時保存する
        /// </summary>
        public static void SaveSettingsToDisk()
        {
            AssetDatabase.SaveAssets();
            Debug.Log("LogSender Settings saved to disk.");
        }


        /// <summary>
        /// アセット保存先のフォルダ構造（Assets/Plugins/Resources）を確保する
        /// </summary>
        private static void EnsureFoldersExist()
        {
            if (!Directory.Exists(BaseDirPath))
            {
                AssetDatabase.CreateFolder("Assets", "Plugins");
            }

            if (!Directory.Exists(FinalResourcesPath))
            {
                AssetDatabase.CreateFolder(BaseDirPath, ResourcesDirName);
            }
        }
    }
}
