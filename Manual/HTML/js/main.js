let json;

const init = () => {
    //const jsonURL = "../JSON/data.json";
    const jsonURL = "https://ktane-mods.github.io/Weakest-Link-Data/data.json";
    
  downloadFile(jsonURL, (str) => {
    console.log(jsonURL);
    json = JSON.parse(str);
    initalizeKtaneTables(json);
    initalizeHistoryTables(json);
    initalizeGeographyTables(json);
    initalizeLanguageTables(json);
    initalizeWildlifeTables(json);
    initalizeBiologyTables(json);
    initalizeMathTables(json);
    initalizeOtherTables(json);
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
  let num5 = initialzeTable(table5, ktaneData, num1 + num2 + num3 + num4, 1000);


  console.log(`KTANE ${num1 + num2 + num3 + num4 + num5}/${ktaneData.length}`)
};

const initalizeHistoryTables = (json) => {
  let table1 = document.querySelector("#history-table1");
  let table2 = document.querySelector("#history-table2");
  let table3 = document.querySelector("#history-table3");



  let historyData = json.QuizBank.filter((s) => s.Category == "History");

  let num1 = initialzeTable(table1, historyData, 0, 16);
  let num2 = initialzeTable(table2, historyData, num1, 16);
  let num3 = initialzeTable(table3, historyData, num1 + num2, 1000);

  console.log(`History ${num1 + num2 + num3}/${historyData.length}`);

};


const initalizeGeographyTables = (json) => {
  let table1 = document.querySelector("#geography-table1");
  let table2 = document.querySelector("#geography-table2");
  let table3 = document.querySelector("#geography-table3");


  let geographyData = json.QuizBank.filter((s) => s.Category == "Geography");

  let num1 = initialzeTable(table1, geographyData, 0, 17);
  let num2 = initialzeTable(table2, geographyData, num1, 19);
  let num3 = initialzeTable(table3, geographyData, num1 + num2, 1000);


  console.log(`Geography ${num1 + num2 + num3}/${geographyData.length}`);
};

const initalizeLanguageTables = (json) => {
  let table1 = document.querySelector("#language-table1");
  let table2 = document.querySelector("#language-table2");
  let table3 = document.querySelector("#language-table3");
  let table4 = document.querySelector("#language-table4");


  let languageData = json.QuizBank.filter((s) => s.Category == "Language");


  let num1 = initialzeTable(table1, languageData, 0, 17);
  let num2 = initialzeTable(table2, languageData, num1, 15);
  let num3 = initialzeTable(table3, languageData, num1 + num2, 12);
  let num4 = initialzeTable(table4, languageData, num1 + num2 + num3, 1000);



  console.log(`Language ${num1 + num2 + num3 + num4}/${languageData.length}`);

};

const initalizeWildlifeTables = (json) => {
  let table1 = document.querySelector("#wildlife-table1");
  let table2 = document.querySelector("#wildlife-table2");
  let table3 = document.querySelector("#wildlife-table3");




  let wildlifeData = json.QuizBank.filter((s) => s.Category == "Wildlife");
  let num1 = initialzeTable(table1, wildlifeData, 0, 18);
  let num2 = initialzeTable(table2, wildlifeData, num1, 24);
  let num3 = initialzeTable(table3, wildlifeData, num1 + num2, 1000);

  console.log(`Wildlife ${num1 + num2 + num3}/${wildlifeData.length}`);

};

const initalizeBiologyTables = (json) => {
  let table1 = document.querySelector("#biology-table1");
  let table2 = document.querySelector("#biology-table2");

  let biologyData = json.QuizBank.filter((s) => s.Category == "Biology");
  let num1 = initialzeTable(table1, biologyData, 0, 14);
  let num2 = initialzeTable(table2, biologyData, num1, 1000);

  console.log(`Biology ${num1 + num2}/${biologyData.length}`);

};

const initalizeMathTables = (json) => {
  let table1 = document.querySelector("#math-table1");
  let table2 = document.querySelector("#math-table2");

  let mathData = json.QuizBank.filter((s) => s.Category == "Maths");
  let num1 = initialzeTable(table1, mathData, 0, 21);
  let num2 = initialzeTable(table2, mathData, num1, 1000);

  

  console.log(`Maths ${num1 + num2}/${mathData.length}`);

};


const initalizeOtherTables = (json) => {
  let table1 = document.querySelector("#other-table1");
  let table2 = document.querySelector("#other-table2");
  let table3 = document.querySelector("#other-table3");
  let table4 = document.querySelector("#other-table4");


  let otherData = json.QuizBank.filter((s) => s.Category == "Other");

  let num1 = initialzeTable(table1, otherData, 0, 17);
  let num2 = initialzeTable(table2, otherData, num1, 15);
  let num3 = initialzeTable(table3, otherData, num1 + num2, 15);  
  let num4 = initialzeTable(table4, otherData, num1 + num2 + num3, 1000);  



  console.log(`Other ${num1 + num2 + num3 + num4}/${otherData.length}`);

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