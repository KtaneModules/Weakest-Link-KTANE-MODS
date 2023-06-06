let json;

const init = () => {
    //const jsonURL = "../JSON/data.json";
    const jsonURL = "https://ktane-mods.github.io/Weakest-Link-Data/data.json";
    
  downloadFile(jsonURL, (str) => {
    console.log(jsonURL);
    json = JSON.parse(str);
    initalizeKtaneTables(json);
    initalizeGeographyTables(json);
    /*initalizeLanguageTables(json);
    initalizeWildlifeTables(json);
    initalizeBiologyTables(json);
    initalizeHistoryTables(json);
    initalizeMathTables(json);
    initalizeOtherTables(json); */
  });
  


};

const downloadFile = (url, callbackRef) => {
  const xhr = new XMLHttpRequest();
  xhr.onerror = (e) => console.log("error");

  xhr.onload = (e) => {
    const headers = e.target.getAllResponseHeaders();
    const jsonString = e.target.response;
    callbackRef(jsonString);
  };

  xhr.open("GET", url);
  xhr.send();
};

const initalizeKtaneTables = (json) => {
  let table1 = document.querySelector("#ktane-table1");
  let table2 = document.querySelector("#ktane-table2");
  let table3 = document.querySelector("#ktane-table3");
  let table4 = document.querySelector("#ktane-table4");
  let table5 = document.querySelector("#ktane-table5");


  let ktaneData = json.QuizBank.filter((s) => s.Category == "KTANE");

  let num1 = initialzeTable(table1, ktaneData, 0, 19);
  let num2 = initialzeTable(table2, ktaneData, num1, 20);
  let num3 = initialzeTable(table3, ktaneData, num1 + num2, 19);
  let num4 = initialzeTable(table4, ktaneData, num1 + num2 + num3, 15);
  let num5 = initialzeTable(table5, ktaneData, num1 + num2 + num3 + num4, 15);


  console.log("KTANE questions shown: ", num1 + num2 + num3 + num4 + num5)
  console.log("Total KTANE questions: ", ktaneData.length)
};

const initalizeGeographyTables = (json) => {
  let table1 = document.querySelector("#geography-table1");

  let geographyData = json.QuizBank.filter((s) => s.Category == "Geography");

  initialzeTable(table1, geographyData, 0, 1000);
};

const initalizeLanguageTables = (json) => {
  let table1 = document.querySelector("#language-table1");

  let languageData = json.QuizBank.filter((s) => s.Category == "Language");

  initialzeTable(table1, languageData, 0, 1000);
};

const initalizeWildlifeTables = (json) => {
  let table1 = document.querySelector("#wildlife-table1");

  let wildlifeData = json.QuizBank.filter((s) => s.Category == "Wildlife");

  initialzeTable(table1, wildlifeData, 0, 1000);
};

const initalizeBiologyTables = (json) => {
  let table1 = document.querySelector("#biology-table1");
  let biologyData = json.QuizBank.filter((s) => s.Category == "Biology");

  initialzeTable(table1, biologyData, 0, 1000);
};

const initalizeMathTables = (json) => {
  let table1 = document.querySelector("#math-table1");

  let mathData = json.QuizBank.filter((s) => s.Category == "Maths");

  initialzeTable(table1, mathData, 0, 1000);
};

const initalizeHistoryTables = (json) => {
  let table1 = document.querySelector("#history-table1");

  let historyData = json.QuizBank.filter((s) => s.Category == "History");

  initialzeTable(table1, historyData, 0, 1000);
};

const initalizeOtherTables = (json) => {
  let table1 = document.querySelector("#other-table1");

  let otherData = json.QuizBank.filter((s) => s.Category == "Other");

  initialzeTable(table1, otherData, 0, 1000);
};

const initialzeTable = (tableElement, dataArr, startIndex = 0, length = 0) => {
  let html = "<tbody> <tr> <th>Question</th> <th>Accepted Answers </th> </tr>";
  let dataNum = dataArr.length;

  let num = 0;

  for (let i = startIndex; i < length + startIndex && i < dataNum; i++) {
    let q = dataArr[i];

    let question = q.Question;
    let answers = q.Answers.join(", ");

    html += `<tr><td>${question}</td><td>${answers}</td></tr>`;
    num++;
  }

  html += "</tbody>";
  tableElement.innerHTML = html;
  return num;
};

window.onload = () => {
  init();
};