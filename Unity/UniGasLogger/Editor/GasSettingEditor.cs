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
        private string newDeployId;
        private string newAuthToken;
        private string newSheetId;

        private void OnEnable()
        {
            settings = Resources.Load<GasSettings>("GasSettings");

            if (settings == null)
            {
                settings = GasSettingsService.CreateSettings();
            }

            newDeployId = settings.DeployId;
            newAuthToken = settings.AuthToken;
            newSheetId = settings.SheetId;
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

            GUILayout.Label("Deploy ID");
            newDeployId = GUILayout.TextField(newDeployId);

            GUILayout.Label("Auth Token");
            newAuthToken = GUILayout.TextField(newAuthToken);

            GUILayout.Label("Sheet ID");
            newSheetId = GUILayout.TextField(newSheetId);


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(settings, "Change LogSender Settings");

                GasSettingsService.UpdateSettings(settings, newDeployId, newAuthToken, newSheetId);
            }

            if (GUILayout.Button("Save Settings to Disk"))
            {
                GasSettingsService.SaveSettingsToDisk();
            }
        }
    }
}
