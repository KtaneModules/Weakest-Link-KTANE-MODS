
let json;

const init = () => {
	downloadFile("../JSON/data.json", (str) => {
	    json = JSON.parse(str);
        initalizeKtaneTables();
		initalizeGeographyTables();
		initalizeLanguageTables();
		initalizeWildlifeTables();
		initalizeBiologyTables();
		initalizeHistoryTables();
		initalizeMathTables();
		initalizeOtherTables();
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
	
	xhr.open("GET",url);
	xhr.send();
};

const initalizeKtaneTables = () => {
    let table1 = document.querySelector("#ktane-table1");
    let table2 = document.querySelector("#ktane-table2");
    let table3 = document.querySelector("#ktane-table3");
    let table4 = document.querySelector("#ktane-table4");
    let table5 = document.querySelector("#ktane-table5");

    let ktaneData = json.QuizBank.filter(s => s.Category == "KTANE");

    let table1Length = 19;
	let table2Start = table1Length;
	let table2Length = 20;
	let table3Start = table2Start + table2Length;
	let table3Length = 17;
	let table4Start = table3Start + table3Length;
	let table4Length = 15;
	let table5Start = table4Start + table4Length;
	let table5Length = 100;

    initialzeTable(table1, ktaneData, 0, table1Length);
	initialzeTable(table2, ktaneData, table2Start, table2Length);
	initialzeTable(table3, ktaneData, table3Start, table3Length);
	initialzeTable(table4, ktaneData, table4Start, table4Length);
	initialzeTable(table5, ktaneData, table5Start, table5Length);
}

const initalizeGeographyTables = () => {
    let table1 = document.querySelector("#geography-table1");
    let table2 = document.querySelector("#geography-table2");
    let table3 = document.querySelector("#geography-table3");

    let geographyData = json.QuizBank.filter(s => s.Category == "Geography");

    let table1Length = 17;
	let table2Start = table1Length;
	let table2Length = 19;
	let table3Start = table2Start + table2Length;
	let table3Length = 100;

	initialzeTable(table1, geographyData, 0, table1Length);
	initialzeTable(table2, geographyData, table2Start, table2Length);
	initialzeTable(table3, geographyData, table3Start, table3Length);
}

const initalizeLanguageTables = () => {
    let table1 = document.querySelector("#language-table1");
    let table2 = document.querySelector("#language-table2");
    let table3 = document.querySelector("#language-table3");

    let languageData = json.QuizBank.filter(s => s.Category == "Language");
	
    let table1Length = 17;
	let table2Start = table1Length;
	let table2Length = 16;
	let table3Start = table2Start + table2Length;
	let table3Length = 100;

	initialzeTable(table1, languageData, 0, table1Length);
	initialzeTable(table2, languageData, table2Start, table2Length);
	initialzeTable(table3, languageData, table3Start, table3Length);
}

const initalizeWildlifeTables = () => {
    let table1 = document.querySelector("#wildlife-table1");
    let table2 = document.querySelector("#wildlife-table2");

    let wildlifeData = json.QuizBank.filter(s => s.Category == "Wildlife");
	
    let table1Length = 18;
	let table2Start = table1Length;
	let table2Length = 16;

	initialzeTable(table1, wildlifeData, 0, table1Length);
	initialzeTable(table2, wildlifeData, table2Start, table2Length);
}

const initalizeBiologyTables = () => {
    let table1 = document.querySelector("#biology-table1");
    let table2 = document.querySelector("#biology-table2");

    let biologyData = json.QuizBank.filter(s => s.Category == "Biology");
	
    let table1Length = 14;
    let table2Length = 100;


	initialzeTable(table1, biologyData, 0, table1Length);
	initialzeTable(table2, biologyData, table1Length, table2Length);

}

const initalizeMathTables = () => {
    let table1 = document.querySelector("#math-table1");
    let table2 = document.querySelector("#math-table2");

    let mathData = json.QuizBank.filter(s => s.Category == "Maths");
	
    let table1Length = 21;
	let table2Start = table1Length;
	let table2Length = 100;

	initialzeTable(table1, mathData, 0, table1Length);
	initialzeTable(table2, mathData, table2Start, table2Length);

}

const initalizeHistoryTables = () => {
    let table1 = document.querySelector("#history-table1");
    let table2 = document.querySelector("#history-table2");
    let table3 = document.querySelector("#history-table3");
    let table4 = document.querySelector("#history-table4");



    let historyData = json.QuizBank.filter(s => s.Category == "History");
	
    let table1Length = 16;
	let table2Start = table1Length;
	let table2Length = 19;
	let table3Start = table2Length + table2Start;
	let table3Length = 100;
	let table4Start = table3Length + table3Start;
	let table4Length = 100;
	
	initialzeTable(table1, historyData, 0, table1Length);
	initialzeTable(table2, historyData, table2Start, table2Length);
	initialzeTable(table3, historyData, table3Start, table3Length);
	//initialzeTable(table4, historyData, table4Start, table4Length);
}

const initalizeOtherTables = () => {
    let table1 = document.querySelector("#other-table1");
    let table2 = document.querySelector("#other-table2");
    let table3 = document.querySelector("#other-table3");
    let table4 = document.querySelector("#other-table4");



    let otherData = json.QuizBank.filter(s => s.Category == "Other");
	
    let table1Length = 17;
	let table2Start = table1Length;
	let table2Length = 15;
	let table3Start = table2Length + table2Start;
	let table3Length = 15;
	let table4Start = table3Length + table3Start;
	let table4Length = 100;
	initialzeTable(table1, otherData, 0, table1Length);
	initialzeTable(table2, otherData, table2Start, table2Length);
	initialzeTable(table3, otherData, table3Start, table3Length);
	initialzeTable(table4, otherData, table4Start, table4Length);
}

const initialzeTable = (tableElement, dataArr, startIndex=0, length=0) => 
{
    let html = "<tbody> <tr> <th>Question</th> <th>Accepted Answers </th> </tr>";
    let dataNum = dataArr.length;

    for(let i = startIndex; i < length + startIndex && i < dataNum; i++)
    {
        let q = dataArr[i];

        let question = q.Question;
        let answers = q.Answers.join(", ");

        html += `<tr><td>${question}</td><td>${answers}</td></tr>`;
    }

    html += "</tbody>";
    tableElement.innerHTML = html;
}

init();