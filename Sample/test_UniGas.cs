using System.Collections.Generic;
using UniGasLogger;
using UnityEngine;

public class test_UniGas : MonoBehaviour
{
    private GasServiceManager gasServiceManager;

    private void Start()
    {
        gasServiceManager = GasServiceManager.Instance;

        SendTestLog();
    }

    private void SendTestLog()
    {
        var logDict = new Dictionary<string, object>();
        logDict.Add("name", "hoge");
        logDict.Add("score", 123456);
        var _ = gasServiceManager.SendLog(logDict, "Ranking");
    }
}
