using UnityEditor;
using UnityEngine;
using UniGasLogger.Data;

namespace UniGasLogger.Editor
{
    internal static class GasSettingsService
    {
        private const string AssetName = "GasSettings.asset";
        private const string ResourcesDirName = "Resources";
        private const string BaseDirPath = "Assets/Plugins/UniGasLogger";
        private const string FinalResourcesPath = BaseDirPath + "/" + ResourcesDirName;
        private const string FinalAssetPath = FinalResourcesPath + "/" + AssetName;


        /// <summary>
        /// 新しい GasSettings アセットを作成し、指定の場所に保存します。
        /// </summary>
        /// <returns>新しく作成されたGasSettings</returns>
        public static GasSettings CreateSettings()
        {
            Debug.Log($"GasSettings asset not found. Creating a new one at '{FinalAssetPath}'");

            // ★ 修正点: 汎用ヘルパーを呼び出す
            EnsureAssetPathExists(FinalResourcesPath);

            // これで親フォルダが確実に存在するので、アセットを作成できる
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
        /// "Assets/..." から始まる任意のパスを再帰的に作成するヘルパー関数
        /// </summary>
        /// <param name="path">例: "Assets/Plugins/UniGasLogger/Resources"</param>
        private static void EnsureAssetPathExists(string path)
        {
            // "Assets" 自体は作成不要なので除外
            string[] folders = path.Split('/');
            if (folders.Length == 0) return;

            // "Assets" から始めて1階層ずつ確認
            string currentPath = folders[0];

            // i=1 (Assetsの次) からループ
            for (int i = 1; i < folders.Length; i++)
            {
                string folderName = folders[i];
                string newPath = currentPath + "/" + folderName;

                // AssetDatabase.IsValidFolder は "Assets/..." 形式のパスを受け取る
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    // AssetDatabase.CreateFolder は (親パス, 新フォルダ名) を受け取る
                    AssetDatabase.CreateFolder(currentPath, folderName);
                }

                // 次のループのためにパスを更新
                currentPath = newPath;
            }
        }
    }
}
