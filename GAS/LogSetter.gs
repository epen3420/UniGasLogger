/**
 * Unityからのログを受け取り、スプレッドシートに書き込む
 */
function doPost(e) {
  const lock = LockService.getScriptLock();

  try {
    lock.waitLock(1000 * 20);
    
    const receivedToken = e.parameter.authToken;

    if (!isAuthenticated(receivedToken)){
      throw new Error("何らかのエラーが発生しました。");
    }
  
    const sentDatas = e.parameter;
    const dataKeys = sentDatas.keys.split(',');
    const sheetID = sentDatas.sheetID;
    const sheetName = sentDatas.sheetName; 
    const spreadsheet = SpreadsheetApp.openById(sheetID);
    const expectedHeader = ['TimeStamp', ...dataKeys];
    
    let finalHeaderArray = expectedHeader;
    let targetSheet = null;

    // 1: 指定された名前のシートを指定
    if (sheetName != null && sheetName != ""){
      targetSheet = spreadsheet.getSheetByName(sheetName);

      // 1a: 存在しなければ作成
      if (targetSheet != null){
        if (!isSubset(expectedHeader, getHeader(targetSheet))){
          throw new Error(`指定された名前のシートに必要なヘッダーが存在しません。シート: ${sheetName}, 必要なヘッダー: ${expectedHeader}`);
        }
      }
      else{
        targetSheet = spreadsheet.insertSheet(sheetName);
      }
    }

    // 2: 必要なデータのヘッダーが含まれているシートを指定
    if (targetSheet === null) {
      const allSheets = spreadsheet.getSheets();

      for (const sheet of allSheets) {
        const sheetHeader = getHeader(sheet);
        
        if (isSubset(expectedHeader, sheetHeader)) {
          targetSheet = sheet;
          finalHeaderArray = sheetHeader;
          break;
        }
      }
    }

    // 3: 何もデータのないシートを指定
    if(targetSheet == null){
      const allSheets = spreadsheet.getSheets();

      for (const sheet of allSheets) {
        if (sheet.getLastRow() < 1) {
          targetSheet = sheet;
          break;
        }
      }
    }

    // 4: 1~3を通らなかったら、新しいシートを作成
    if (targetSheet === null) {
      const newSheetName = findNextSheetName(spreadsheet);
      targetSheet = spreadsheet.insertSheet(newSheetName);
    }

    targetSheet.getRange(1, 1, 1, finalHeaderArray.length).setValues([finalHeaderArray]);
    
    const rowToAppend = finalHeaderArray.map(headerName => {
      if (headerName === 'TimeStamp') {
        return new Date();
      }
      else if (sentDatas.hasOwnProperty(headerName)) {
        return sentDatas[headerName];
      }
      return null;
    });

    targetSheet.appendRow(rowToAppend);

    const output = ContentService.createTextOutput('Completed append to spread sheet');
    return output.setMimeType(ContentService.MimeType.TEXT);

  } 
  catch (error) {
    console.log('Exception Error: ' + error.stack);
    const output = ContentService.createTextOutput('Error: ' + error.message);
    return output.setMimeType(ContentService.MimeType.TEXT);

  } 
  finally {
    lock.releaseLock();
  }
}
