/**
 * Tokenを作成する際に使う関数
 * APIとして使うことはなく、このプロジェクト内で実行して使用する
 * @return {string}
 */
function generateSecretKey() {
  const TIMESTAMP = new Date().getTime().toString();

  // 乱数とタイムスタンプを組み合わせたシークレット（真のランダムシードとして）
  const RANDOM_SEED = TIMESTAMP + Math.random().toString();

  // HMAC-SHA256でハッシュ値を生成
  const hash = Utilities.computeHmacSha256Signature(RANDOM_SEED, TIMESTAMP);

  // Base64でエンコードしてキーとして使用
  const key = Utilities.base64Encode(hash);

  Logger.log(key);
}

/**
 * receivedTokenがScriptPropertyに登録してあるものと同じかどうかを返す関数
 * @return {boolean}
 */
function isAuthenticated(receivedToken) {
  // 引数がnullだったらfalse
  if (receivedToken == null) return false;

  const SECRET_TOKEN_KEY = 'AUTH_TOKEN';

  // Propertyにキーが存在しなかったらfalse
  if (!PropertiesService.getScriptProperties().getKeys().find(key => key == SECRET_TOKEN_KEY)) return false;

  // 受け取ったのとPropertyが一緒かどうかを返す
  return receivedToken == PropertiesService.getScriptProperties().getProperty(SECRET_TOKEN_KEY)
}

/**
 * headerAのすべての要素がheaderBに含まれているか（headerA⊆headerB）をチェックするヘルパー関数
 * @return {boolean}
 */
function isSubset(headerA, headerB) {
  // Aの長さがBより大きい場合は、AはBの部分集合ではありえない (高速化)
  if (headerA.length > headerB.length) {
    return false;
  }

  const superSet = new Set(headerB);
  // Aのすべての要素がsuperSetに含まれているかチェック
  return headerA.every(val => superSet.has(val));
}

/**
 * 指定されたベース名のシートで、存在しない最小の番号（欠番）を探して新しいシート名を返す
 * @param {SpreadsheetApp.Spreadsheet} spreadsheet - スプレッドシートオブジェクト
 * @param {string} baseName - シート名のベース（デフォルトは "シート"）
 * @returns {string} 新しいシート名（例: "Log1", "Data2"）
 */
function findNextSheetName(spreadsheet, baseName = 'シート') {
  const sheets = spreadsheet.getSheets();
  const sheetNumbers = [];

  // 1. 変数から正規表現を動的に作成
  // baseNameに正規表現の特殊文字（. * + ? ^ $ { } ( ) | [ ] \）が含まれていた場合、ただの文字として扱うためにエスケープする
  const escapedBaseName = baseName.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const regex = new RegExp(`^${escapedBaseName}(\\d+)$`);

  // 2. 既存のシート名から番号を抽出
  sheets.forEach(sheet => {
    const sheetName = sheet.getName();
    const match = sheetName.match(regex);
    if (match) {
      sheetNumbers.push(parseInt(match[1], 10));
    }
  });

  // 3. 1から順番にチェックして、存在しない最小の番号（欠番）を探す
  let nextNumber = 1;
  sheetNumbers.sort((a, b) => a - b); // 番号を昇順にソート

  for (const num of sheetNumbers) {
    if (num === nextNumber) {
      nextNumber++;
    }
    else if (num > nextNumber) {
      // 欠番が見つかった
      break;
    }
  }

  return `${baseName}${nextNumber}`;
}

/**
 * sheetのヘッダー配列を返す関数
 * @retrun {string[]}
 */
function getHeader(sheet){
  const lastColumn = sheet.getLastColumn();

  return (sheet.getLastRow() > 0 && lastColumn > 0)
          ? sheet.getRange(1, 1, 1, lastColumn).getValues()[0]
          : [];
}

const STATUS = Object.freeze({
  Success: 'success',
  Warning: 'warning',
  Error: 'error'
});

function createStatusResponse(status, message){
  return createJsonResponse({"status": status, "message": message});
}

/**
 * オブジェクトをJSON文字列に変換し、ContentServiceで返すヘルパー関数
 * @param {Object} obj - JSONに変換するオブジェクト
 * @return {GoogleAppsScript.Content.TextOutput}
 */
function createJsonResponse(obj) {
  const jsonString = JSON.stringify(obj, null, 2);
  console.log(jsonString);
  return ContentService.createTextOutput(jsonString)
    .setMimeType(ContentService.MimeType.JSON);
}
