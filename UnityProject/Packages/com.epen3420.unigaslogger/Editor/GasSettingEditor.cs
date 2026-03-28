using UniGasLogger.Data;
using UnityEditor;
using UnityEngine;

namespace UniGasLogger.Editor
{
    public class GasSettingEditor : EditorWindow
    {
        [MenuItem("Tools/LogSenderSetting")]
        private static void OpenWindow()
        {
            GetWindow<GasSettingEditor>("LogSender Settings");
        }

        private GasSettings settings;
        private bool isEnable;
        private string deployId;
        private string authToken;
        private string sheetId;

        private void OnEnable()
        {
            settings = GasSettingsService.LoadOrCreateSettings();

            isEnable = settings.IsEnable;
            deployId = settings.DeployId;
            authToken = settings.AuthToken;
            sheetId = settings.SheetId;
        }

        private void OnGUI()
        {
            GUILayout.Label("LogSender Settings", EditorStyles.boldLabel);

            if (settings == null)
            {
                GUILayout.Label("Settings asset could not be loaded.", EditorStyles.helpBox);
                if (GUILayout.Button("Retry Load/Resolve Paths"))
                {
                    OnEnable();
                }
                return;
            }

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Disable Logging");
            isEnable = !GUILayout.Toggle(!isEnable, "");

            EditorGUI.BeginDisabledGroup(!isEnable);
            GUILayout.Label("Deploy ID");
            deployId = GUILayout.TextField(deployId);

            GUILayout.Label("Auth Token");
            authToken = GUILayout.TextField(authToken);

            GUILayout.Label("Sheet ID");
            sheetId = GUILayout.TextField(sheetId);
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(settings, "Change LogSender Settings");

                GasSettingsService.UpdateSettings(settings, deployId, authToken, sheetId, isEnable);
            }

            if (GUILayout.Button("Save Settings to Disk"))
            {
                GasSettingsService.SaveSettingsToDisk();
            }
        }
    }
}
