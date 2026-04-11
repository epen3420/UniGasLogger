using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UniGasLogger.Data;

namespace UniGasLogger.Core
{
    internal class GasHttpClient
    {
        private readonly GasSettings settings;

        public GasHttpClient(GasSettings gasSettings)
        {
            this.settings = gasSettings;
        }

        /// <summary>
        /// GASへのPOSTリクエストを非同期で実行
        /// </summary>
        public async Task PostGas(WWWForm form)
        {
            form.AddField("authToken", settings.AuthToken);
            form.AddField("sheetID", settings.SheetId);

            using UnityWebRequest www = UnityWebRequest.Post(settings.GasUrl, form);
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            string responseText = www.downloadHandler.text;
            StatusResponse response = JsonUtility.FromJson<StatusResponse>(responseText);

            if (www.result == UnityWebRequest.Result.Success &&
                response.GetStatus() == Status.success)
            {
                Debug.Log($"[Success to send log: UniGasLogger]: {response.message}");
            }
            else
            {
                throw new System.Exception($"{response.message}");
            }
        }

        /// <summary>
        /// GETリクエストを共通で処理する内部メソッド。クエリパラメータを付与します。
        /// </summary>
        public async Task<T> GetGas<T>(Dictionary<string, string> queryParams)
        {
            string fullUrl = settings.GasUrl + "?";

            // 認証キーとシートIDを必須パラメータとして追加
            queryParams["authToken"] = settings.AuthToken;
            queryParams["sheetID"] = settings.SheetId;

            // クエリ文字列を構築
            fullUrl += string.Join("&", queryParams.Select(p =>
                {
                    string key = UnityWebRequest.EscapeURL(p.Key);
                    string value = UnityWebRequest.EscapeURL(p.Value);
                    return $"{key}={value}";
                })
            );

            using UnityWebRequest webRequest = UnityWebRequest.Get(fullUrl);
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                string responseText = webRequest.downloadHandler.text;
                string errorMessage = $"Error: {webRequest.error}. Response: {responseText}";

                // GASからのエラー応答（JSON）をパースし、詳細なエラーをログに出力
                try
                {
                    GasErrorResponse errorResponse = JsonUtility.FromJson<GasErrorResponse>(responseText);
                    errorMessage = $"GAS Error: {errorResponse.error} (Details: {errorResponse.details})";
                }
                catch
                {
                    // パースに失敗した場合はそのまま
                }

                Debug.LogError(errorMessage);
                return default;
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log($"GAS Response: {jsonResponse}");

                T response = JsonUtility.FromJson<T>(jsonResponse);
                return response;
            }
        }
    }

}
}
