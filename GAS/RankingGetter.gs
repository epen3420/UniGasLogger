/**
 * Webアプリとしてアクセスされた際（GETリクエスト）に実行されます。
 * 認証、シートの準備、モード分岐を制御します。
 * * @param {GoogleAppsScript.Events.DoGet} e - イベントオブジェクト
 * @return {GoogleAppsScript.Content.TextOutput} JSON形式のテキスト出力
 */
function doGet(e) {
  try {
    // --- 1. 認証チェック ---
    const receivedToken = e.parameter.authToken;
    if (!isAuthenticated(receivedToken)) {
      return createJsonResponse({ error: `Unauthorized access (Invalid or missing authToken).` });
    }
    const sheetID = e.parameter.sheetID;
    const sheetName = e.parameter.sheetName;
    const topCount = e.parameter.topCount != null ? e.parameter.topCount : 5;

    const spreadsheet = SpreadsheetApp.openById(sheetID);
    const sheet = spreadsheet.getSheetByName(sheetName);

    if (!sheet) {
      return createJsonResponse({ error: `Sheet "${sheetName}" not found.` });
    }

    // --- 2. データとヘッダーの取得・解析 ---
    const { sheetHeader, data, scoreColumnNum, nameColumnNum } = fetchAndAnalyzeData(sheet);

    if (scoreColumnNum === -1) {
      return createJsonResponse({ error: "'Score' column not found in sheet." });
    }

    // --- 3. モード分岐と実行 ---
    const targetScoreParam = e.parameter.score;

    if (targetScoreParam !== undefined) {
      return calculateRank(targetScoreParam, data, scoreColumnNum);
    } else {
      if (nameColumnNum === -1) {
        return createJsonResponse({ error: `"name" column not found (required for top ${topCount} ranking).` });
      }
      return getTopRanking(data, scoreColumnNum, nameColumnNum, topCount);
    }
  } 
  catch (error) {
    // エラーハンドリング
    console.log('Exception Error in doGet: ' + error.stack);
    // C#規則: nameColumnNum をチェック
    return createJsonResponse({ error: "An unexpected error occurred.", details: error.message });
  }
}

/**
 * スプレッドシートからデータ全体を取得し、必要な列番号を解析します。
 * @param {GoogleAppsScript.Spreadsheet.Sheet} sheet - 対象シート
 * @return {{sheetHeader: any[], data: any[][], scoreColumnNum: number, nameColumnNum: number}} 解析結果
 */
function fetchAndAnalyzeData(sheet) {
  const startRow = 2;
  const numRows = sheet.getLastRow() - startRow + 1;
  const lastColumn = sheet.getLastColumn();

  // ヘッダーとデータ行が共に存在しない場合
  if (sheet.getLastRow() < 1) {
      return { sheetHeader: [], data: [], scoreColumnNum: -1, nameColumnNum: -1 };
  }
  
  // ヘッダーの取得
  const sheetHeader = sheet.getRange(1, 1, 1, lastColumn).getValues()[0];
  
  let nameColumnNum = -1;
  let scoreColumnNum = -1;

  // ヘッダーから列番号を特定
  for (let i = 0; i < sheetHeader.length; i++) {
    const header = sheetHeader[i];
    if (header === 'name') {
      nameColumnNum = i;
    } 
    else if (header === 'Score') {
      scoreColumnNum = i;
    }
  }

  // データ行が0行の場合
  if (numRows <= 0) {
      return { sheetHeader, data: [], scoreColumnNum, nameColumnNum };
  }
  
  // データ本体の取得
  const data = sheet.getRange(startRow, 1, numRows, lastColumn).getValues();

  return { sheetHeader, data, scoreColumnNum, nameColumnNum };
}

/**
 * トップN件のランキングデータを取得し、JSON応答を作成します。
 * @param {any[][]} data - ランキングデータ本体
 * @param {number} scoreColumnNum - Score列のインデックス
 * @param {number} nameColumnNum - name列のインデックス
 * @param {number} topN - 取得する上位件数
 * @return {GoogleAppsScript.Content.TextOutput} JSON形式のテキスト出力
 */
function getTopRanking(data, scoreColumnNum, nameColumnNum, topN) {
  if (data.length === 0) {
    return createJsonResponse({ ranking: [] });
  }
  
  // Score列で降順ソート
  const sortedData = data.sort(function(a, b) {
    const scoreA = parseFloat(a[scoreColumnNum]);
    const scoreB = parseFloat(b[scoreColumnNum]);
    
    // 非数値はソートの最後に移動 (元のロジックを維持)
    if (isNaN(scoreA)) return 1;
    if (isNaN(scoreB)) return -1;

    return scoreB - scoreA;
  });

  // 上位N件を取得
  const topResults = sortedData.slice(0, topN);

  // JSON形式に整形
  const rankingList = topResults.map(function(row) {
    return {
      name: row[nameColumnNum],
      score: row[scoreColumnNum]
    };
  });

  return createJsonResponse({ ranking: rankingList });
}

/**
 * 指定されたスコアの順位を計算し、JSON応答を作成します。
 * @param {string} targetScoreParam - クエリパラメータのスコア文字列
 * @param {any[][]} data - ランキングデータ本体
 * @param {number} scoreColumnNum - Score列のインデックス
 * @return {GoogleAppsScript.Content.TextOutput} JSON形式のテキスト出力
 */
function calculateRank(targetScoreParam, data, scoreColumnNum) {
  const targetScore = parseFloat(targetScoreParam);
  
  if (isNaN(targetScore)) {
    return createJsonResponse({ error: "Invalid score parameter. Must be a number." });
  }

  // データがない場合、1位を返す
  if (data.length === 0) {
    return createJsonResponse({ score: targetScore, rank: 1 });
  }
  
  // 'Score'列の数値データだけを抽出
  const allScores = data.map(row => parseFloat(row[scoreColumnNum]))
                        .filter(score => !isNaN(score));
  
  // 自分より高いスコアの数を数える
  let higherScoresCount = 0;
  for (const score of allScores) {
    if (score > targetScore) {
      higherScoresCount++;
    }
  }
  
  // 順位 = (自分より高いスコアの数) + 1
  const rank = higherScoresCount + 1;

  return createJsonResponse({ score: targetScore, rank: rank });
}