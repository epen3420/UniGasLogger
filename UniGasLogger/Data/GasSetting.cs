using UnityEngine;

namespace UniGasLogger.Data
{
    [CreateAssetMenu(fileName = "NewGasSettings", menuName = "GasSettings")]
    internal class GasSettings : ScriptableObject
    {
        [SerializeField]
        private bool isEnable = true;
        [SerializeField]
        private string authToken;
        [SerializeField]
        private string deployId;
        [SerializeField]
        private string sheetId;

        public bool IsEnable => isEnable;
        public string AuthToken => authToken;
        public string DeployId => deployId;
        public string GasUrl => $"https://script.google.com/macros/s/{deployId}/exec";
        public string SheetId => sheetId;

        public void Init(string deployId, string authToken, string sheetId, bool isEnable = true)
        {
            this.isEnable = isEnable;
            this.deployId = deployId;
            this.authToken = authToken;
            this.sheetId = sheetId;
        }
    }
}
