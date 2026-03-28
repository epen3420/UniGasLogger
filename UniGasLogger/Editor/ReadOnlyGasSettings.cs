using UnityEditor;
using UniGasLogger.Data;

namespace UniGasLogger.Editor
{
    [CustomEditor(typeof(GasSettings))]
    public class ReadOnlyGasSettings : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            DrawDefaultInspector();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "設定の変更は、[Tools > LogSenderSetting] ウィンドウから行ってください。\n" +
                "このアセットは直接編集できないように保護されています。",
                MessageType.Info
            );
        }
    }
}
