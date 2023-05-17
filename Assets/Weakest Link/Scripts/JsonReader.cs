using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonReader : MonoBehaviour {

    private void Start()
    {
        LoadData();
    }

	//object used in order to read data from json

	public TextAsset textJSON;

    private JsonData json;

    //holds the data read from the json
    [System.Serializable]
    public class JsonData
    {
        List<string> name; //list of all the names of the contestants
        public List<JsonTrivia> allQuestions; //list of all the questions and its respective data
    }

    public class JsonTrivia
    {
        public string Question; //the question being asked
        public List<string> AcceptedAnswers;  //strings that will pass the questions
        public List<string> WrongAnswers;  //strings that the cpu will use the when they need to get the question wrong
        public string Category;  //They category of the question

        public JsonTrivia(string Question, List<string> AcceptedAnswers, List<string> WrongAnswers, string Category)
        {
            this.Question = Question;
            this.AcceptedAnswers = AcceptedAnswers;
            this.WrongAnswers = WrongAnswers;
            this.Category = Category;
        }
    }

    //Loads the data from the json
    public void LoadData()
    {
        Debug.Log("Loading data...\n" + textJSON.text);
        json = JsonUtility.FromJson<JsonData>(textJSON.text);


    }
}
