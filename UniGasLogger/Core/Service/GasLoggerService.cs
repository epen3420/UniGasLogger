using BindingFlags = System.Reflection.BindingFlags;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniGasLogger.Http;

namespace UniGasLogger.Service
{
    internal class GasLoggerService
    {
        private readonly GasHttpClient gasHttpClient;

        public GasLoggerService(GasHttpClient gasHttpClient)
        {
            this.gasHttpClient = gasHttpClient;
        }


        public async Task SendLog<T>(T data, string sheetName)
        {
            if (data == null)
            {
                throw new System.ArgumentNullException(nameof(data), "can not send log. because data is null.");
            }

            if (sheetName == null)
            {
                throw new System.ArgumentNullException(nameof(sheetName), "can not send log. because sheetName is null.");
            }

            // **修正**: GetPropertiesを使って、インスタンスのpublicプロパティのみを取得
            var dataProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (dataProperties.Length == 0)
            {
                throw new System.ArgumentException($"Can't send log. No public properties found", typeof(T).Name);
            }

            // T型をDictionaryに変換
            var dataDict = new Dictionary<string, object>();
            foreach (var prop in dataProperties)
            {
                dataDict[prop.Name] = prop.GetValue(data);
            }

            await SendLog(dataDict, sheetName);
        }

        /// <summary>
        /// Dictionary<string, object> のデータをログとして送信します。
        /// </summary>
        public async Task SendLog(Dictionary<string, object> data, string sheetName)
        {
            if (data == null || data.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(data), "Can't send log. Because data dictionary is null or empty");
            }

            await PostData(data, sheetName);
        }

        /// <summary>
        /// [共通化] 辞書データをWWWFormに変換し、GASにPOSTする内部メソッド
        /// </summary>
        private async Task PostData(Dictionary<string, object> data, string sheetName)
        {
            WWWForm form = new WWWForm();
            form.AddField("sheetName", sheetName ?? "");

            var keys = data.Keys.ToArray();
            form.AddField("keys", string.Join(',', keys));

            string logStr = "";
            foreach (var pair in data)
            {
                // nullの場合も "null" という文字列を安全に送る
                string valueStr = pair.Value?.ToString() ?? "null";
                form.AddField(pair.Key, valueStr);
                logStr += $"[{pair.Key}] {valueStr}, ";
            }

            if (logStr.Length > 2)
            {
                logStr = logStr.Remove(logStr.Length - 2, 2);
                Debug.Log($"Sending Log: {logStr}");
            }

            try
            {
                await gasHttpClient.PostGas(form);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Log send failed with exception: {e.Message}");
            }
        }
    }
}
