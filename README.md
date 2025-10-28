# UniGasLogger (Unity Google Apps Script Logger)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**UniGasLogger** は、UnityとGoogle Apps Script (GAS) 間で**非同期通信**を行うためのライブラリです。ゲームのログ収集、スコアランキングシステム構築を簡単に行うために設計されています。

## 🚀 特徴 (Features)

  * **柔軟なログ送信**: C\#のデータクラス または `Dictionary<string, object>` 形式のデータを、GASを経由してスプレッドシートに自動的に記録します。
  * **ランキング機能**: スプレッドシートからトップNランキングや、特定のスコアの順位を瞬時にJSON形式で取得します。

-----

## 📦 Unityへの導入方法 (Installation)

### Unity Package Manager (UPM)

Unityエディタの `Window > Package Manager` を開き、左上の「+」ボタンから `Add package from git URL...` を選択し、本リポジトリのURLを入力してください。

```
https://github.com/epen3420/UniGasLogger.git?path=/Unity
```

*(注: UPMでサブディレクトリを指定するには `?path=/Unity` が必要です)*

-----

## ⚙️ セットアップ (Configuration)

ライブラリを使用するには、**Google Apps Script (GAS)** 側と **Unityクライアント** 側の両方で設定が必要です。

### 1\. Google Apps Script (GAS) 側の設定

1.  **プロジェクトの作成とコードの配置**:
      * [Apps Script dashboard](https://script.google.com/home) で新しいプロジェクトを作成します。
      * 本リポジトリの `GAS/` フォルダにあるすべての `.gs` ファイルをコピー&ペーストします。
2.  **認証トークンの生成**:
      * `utils.gs` 内の `generateSecretKey()` 関数を実行し、ログに表示されたトークン文字列をコピーします。（または、自前で強力なトークンを生成してください。）
3.  **認証トークンの設定**:
      * GASエディタの `プロジェクトの設定（⚙️） > スクリプト プロパティ` を開きます。
      * **キー**: `AUTH_TOKEN`
      * **値**: **手順2でコピーしたトークン** を設定し、保存します。
4.  **Webアプリとしてデプロイ**:
      * `デプロイ > 新しいデプロイ` を選択します。
      * 種類は「Webアプリ」を選択します。
      * **アクセスできるユーザー**を「**全員**」に設定します。
      * デプロイ後、表示される**デプロイID**をコピーします。

### 2\. Unity 側の設定

1.  Unityエディタの上部メニューから `Tools > LogSenderSetting` を選択します。
2.  専用の `EditorWindow` が開き、ライブラリフォルダ内に設定ファイル（`GasSettings.asset`）が自動的に作成されます。
3.  ウィンドウに、GAS側で取得した情報を入力します。
      * **GAS Deploy ID**: GASのデプロイID
      * **Auth Token**: GASで設定した認証トークン
      * **Sheet ID**: ログを書き込むスプレッドシートのID（URLの `/d/` と `/edit` の間の文字列）
4.  `Save Settings to Disk` ボタンを押して設定を保存します。

### 🚨【セキュリティ警告】秘密鍵の保護

`GasSettings.asset` ファイルには、**GASの認証トークンなどの秘密鍵**が平文で含まれます。

**このファイルをパブリックなリポジトリにコミットしないことを** **強く推奨します。**

プロジェクトのルートにある `.gitignore` ファイルに、以下のパスを追記して保護してください。

```gitignore
# UniGasLogger Settings
/Assets/UniGasLogger/Settings/GasSettings.asset
/Assets/UniGasLogger/Settings/GasSettings.asset.meta
```

## 📖 使い方 (Usage)

シーンに `GasServiceManager.cs` をアタッチしたGameObjectを配置してください。このクラスはシングルトン (`Instance`) として機能し、各機能にアクセスできます。

### 1\. ログを送信する (`SendLog`)

**`GasServiceManager.Instance.SendLog<T>(data, sheetName)`**

`sheetName` を指定すると、GAS側で異なるシートへデータを振り分けることができます。

```csharp
using UnityEngine;
using UniGasLogger;
using System.Collections.Generic;

public class MyGameController : MonoBehaviour
{
    [System.Serializable]
    public class PlayLog
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public bool IsClear { get; set; }
    }

    public async void OnGameEnd(int finalScore)
    {
        var log = new PlayLog
        {
            PlayerName = "Hero_Alpha",
            Score = finalScore,
            IsClear = true
        };

        // GASへ送信。GAS側で "Stage1Clear" というシート名として扱われます。
        await GasServiceManager.Instance.SendLog(log, "Stage1Clear");
    }

    // Dictionary を送信する例
    public async void OnGameStart()
    {
        var log = new Dictionary<string, object>
        {
            { "Event", "GameStart" },
            { "SceneName", "Level_01" }
        };

        await GasServiceManager.Instance.SendLog(log, "EventLog");
    }
}
```

### 2\. ランキングを取得する (`Get...Ranking`)

GAS側で `RankingResponse` または `ScoreRankResponse` の **DTO (Data Transfer Object)** 形式でJSON応答が返されることを想定しています。

```csharp
using UnityEngine;
using UniGasLogger;
using UniGasLogger.Core.DTO; // DTOを参照
using System.Numerics;

public class RankingViewController : MonoBehaviour
{
    // トップ5ランキングを取得する例
    public async void ShowTop5Ranking()
    {
        // "RankingSheet" はGAS側でデータソースとして使用するシート名
        RankingResponse response = await GasServiceManager.Instance.GetTop5Ranking("RankingSheet");

        if (response?.ranking != null)
        {
            foreach (var entry in response.ranking)
            {
                Debug.Log($"Rank Entry: {entry.name}, Score: {entry.score}");
            }
        }
    }

    // 自分のスコアが何位か問い合わせる例
    public async void CheckMyRank(BigInteger myScore)
    {
        ScoreRankResponse response = await GasServiceManager.Instance.GetScoreRanking(myScore, "RankingSheet");

        if (response != null && response.rank > 0)
        {
            Debug.Log($"Your score {response.score} is Rank {response.rank}!");
        }
    }
}
```

-----

## 📜 ライセンス (License)

このプロジェクトは **MIT License** のもとで公開されています。詳細については、リポジトリ内の `LICENSE` ファイルを参照してください。
