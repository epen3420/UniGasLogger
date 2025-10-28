using UnityEditor;
using UnityEngine;
using System.IO;

namespace UniGasLogger.Data
{
    internal static class GasSettingsService
    {
        private const string ASSET_NAME = "GasSettings.asset";
        private const string RESOURCES_DIR_NAME = "Resources";
        private const string SERVICE_SCRIPT_NAME = "GasSettingsService";


        public static GasSettings LoadSettings()
        {
            GasSettings settings = Resources.Load<GasSettings>("GasSettings");
            if (settings == null)
            {
                return null;
            }

            return settings;
        }

        public static GasSettings CreateSettings()
        {
            string libraryRootPath = ResolveLibraryRootPath();
            if (string.IsNullOrEmpty(libraryRootPath))
            {
                Debug.LogError("Failed to create GasSettings: Library root path could not be resolved.");
                return null;
            }

            string resourcesFolderPath = Path.Combine(libraryRootPath, RESOURCES_DIR_NAME);
            string assetPath = Path.Combine(resourcesFolderPath, ASSET_NAME);

            Debug.Log($"GasSettings asset not found. Creating one at '{assetPath}'");

            EnsureFoldersExist(libraryRootPath, resourcesFolderPath);

            var settings = ScriptableObject.CreateInstance<GasSettings>();
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return settings;
        }

        /// <summary>
        /// ScriptableObject の設定値を更新し、ダーティ（変更済み）としてマークする
        /// </summary>
        public static void UpdateSettings(GasSettings settings, string newDeployId, string newAuthToken, string newSheetId)
        {
            Undo.RecordObject(settings, "Change LogSender Settings");

            // 渡された値で settings オブジェクトを更新
            settings.Init(newDeployId, newAuthToken, newSheetId);

            // 変更を保存対象としてマーク
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
        /// フォルダパスが存在することを確認し、なければ作成する
        /// </summary>
        private static void EnsureFoldersExist(string libraryRootPath, string resourcesFolderPath)
        {
            if (!Directory.Exists(libraryRootPath))
            {
                string parent = Path.GetDirectoryName(libraryRootPath);
                string folder = Path.GetFileName(libraryRootPath);
                AssetDatabase.CreateFolder(parent, folder);
            }

            if (!Directory.Exists(resourcesFolderPath))
            {
                AssetDatabase.CreateFolder(libraryRootPath, RESOURCES_DIR_NAME);
            }
        }

        /// <summary>
        /// このスクリプトファイル (GasSettingsService.cs) の場所を基準に、
        /// ライブラリのルートパスを自動解決します。
        /// </summary>
        /// <returns>ライブラリのルートパス (例: "Assets/UniGasClient")。失敗時は null。</returns>
        private static string ResolveLibraryRootPath()
        {
            // 1. "GasSettingsService" という名前のスクリプトアセットを検索
            // "t:Script" でスクリプトのみを対象
            string[] guids = AssetDatabase.FindAssets($"t:Script {SERVICE_SCRIPT_NAME}");

            if (guids.Length == 0)
            {
                Debug.LogError($"Could not find script file: {SERVICE_SCRIPT_NAME}.cs");
                return null;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);

            if (string.IsNullOrEmpty(scriptPath) || !scriptPath.EndsWith(".cs"))
            {
                Debug.LogError($"Failed to resolve path for script: {SERVICE_SCRIPT_NAME}");
                return null;
            }

            string dataFolderPath = Path.GetDirectoryName(scriptPath);

            string coreFolderPath = Path.GetDirectoryName(dataFolderPath);

            string scriptFolderPath = Path.GetDirectoryName(coreFolderPath);

            string libraryRootPath = Path.GetDirectoryName(scriptFolderPath);

            return libraryRootPath;
        }
    }
}
