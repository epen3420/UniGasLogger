/**
 * Unityからのログを受け取り、スプレッドシートに書き込む
 */
function doPost(e) {
  const lock = LockService.getScriptLock();

  try {
    lock.waitLock(1000 * 20);

    const receivedToken = e.parameter.authToken;

    if (!isAuthenticated(receivedToken)){
      throw new Error("Authentication Failed: Invalid Token");
    }

    const payload = e.parameter;
    const dataKeys = payload.keys.split(',');
    const sheetID = payload.sheetID;
    const sheetName = payload.sheetName;
    const spreadsheet = SpreadsheetApp.openById(sheetID);
    const expectedHeader = ['TimeStamp', ...dataKeys];

    const { targetSheet, finalHeaderArray } = determineTargetSheet(spreadsheet, sheetName, expectedHeader);

    // シートが空（新規作成含む）の時だけヘッダーを書き込む
    if (targetSheet.getLastRow() === 0) {
      targetSheet.getRange(1, 1, 1, finalHeaderArray.length).setValues([finalHeaderArray]);
    }

    const rowToAppend = finalHeaderArray.map(headerName => {
      if (headerName === 'TimeStamp') {
        return new Date();
      }
      else if (payload.hasOwnProperty(headerName)) {
        return payload[headerName];
      }
      return null;
    });

    targetSheet.appendRow(rowToAppend);
  }
  catch (error) {
    console.log('Exception Error: ' + error.stack);
    return createStatusResponse(STATUS.Error, `Error: ${error.message}`)
  }
  finally {
    if (lock != null)
      lock.releaseLock();
  }

  return createStatusResponse(STATUS.Success, 'Completed append to spread sheet');
}

function determineTargetSheet(spreadsheet, sheetName, expectedHeader) {
  // 1: 指定された名前のシートを処理
  if (sheetName) {
    const sheet = spreadsheet.getSheetByName(sheetName);
    if (sheet) {
      const currentHeader = getHeader(sheet);
      if (isSubset(expectedHeader, currentHeader)) {
        return { targetSheet: sheet, finalHeaderArray: currentHeader };
      }

      console.log(`Header mismatch in '${sheetName}'. Creating a new sheet.`);
    }

    // シートが存在しない、またはヘッダー不一致の場合：
    // 指定された名前をベースに「名前1」「名前2」などの空き番号シートを作成
    const nextName = findNextSheetName(spreadsheet, sheetName);
    const newSheet = spreadsheet.insertSheet(nextName);
    return { targetSheet: newSheet, finalHeaderArray: expectedHeader };
  }

  // 2: 同じヘッダーがあるシート
  // 3: 空のシート
  // 2 & 3 を同時に探す
  const allSheets = spreadsheet.getSheets();
  let emptySheetFallback = null;

  for (const sheet of allSheets) {
    if (sheet.getLastRow() === 0) {
      // 空シートを見つけたらフォールバック
      if (!emptySheetFallback)
        emptySheetFallback = sheet;
    } else {
      // なにかデータがあるシートならヘッダーを検証
      const sheetHeader = getHeader(sheet);
      if (isSubset(expectedHeader, sheetHeader)) {
        return { targetSheet: sheet, finalHeaderArray: sheetHeader };
      }
    }
  }

  if (emptySheetFallback) {
    return { targetSheet: emptySheetFallback, finalHeaderArray: expectedHeader };
  }

  // 4: 1~3を全て通らなかったら、新しいシートを作成して返す
  const newSheetName = findNextSheetName(spreadsheet);
  return { targetSheet: spreadsheet.insertSheet(newSheetName), finalHeaderArray: expectedHeader };
}
