
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

    let ktaneData = json.QuizBank.filter(s => s.Category == "KTANE");


    initialzeTable(table1, ktaneData, 0, 1000);
}

const initalizeGeographyTables = () => {
    let table1 = document.querySelector("#geography-table1");

    let geographyData = json.QuizBank.filter(s => s.Category == "Geography");

	initialzeTable(table1, geographyData, 0, 1000);
}

const initalizeLanguageTables = () => {
    let table1 = document.querySelector("#language-table1");

    let languageData = json.QuizBank.filter(s => s.Category == "Language");

	initialzeTable(table1, languageData, 0, 1000);
}

const initalizeWildlifeTables = () => {
    let table1 = document.querySelector("#wildlife-table1");

    let wildlifeData = json.QuizBank.filter(s => s.Category == "Wildlife");
	

	initialzeTable(table1, wildlifeData, 0, 1000);
}

const initalizeBiologyTables = () => {
    let table1 = document.querySelector("#biology-table1");
    let biologyData = json.QuizBank.filter(s => s.Category == "Biology");
	
	initialzeTable(table1, biologyData, 0, 1000);

}

const initalizeMathTables = () => {
    let table1 = document.querySelector("#math-table1");

    let mathData = json.QuizBank.filter(s => s.Category == "Maths");

	initialzeTable(table1, mathData, 0, 1000);

}

const initalizeHistoryTables = () => {
    let table1 = document.querySelector("#history-table1");

    let historyData = json.QuizBank.filter(s => s.Category == "History");
	
	initialzeTable(table1, historyData, 0, 1000);
}

const initalizeOtherTables = () => {
    let table1 = document.querySelector("#other-table1");

    let otherData = json.QuizBank.filter(s => s.Category == "Other");
	
	initialzeTable(table1, otherData, 0, 1000);
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

window.onload = () => {
    init();
};
