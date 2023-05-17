using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class JsonReader : MonoBehaviour {

    public List<Trivia> TriviaList { get; private set; }
    public List<string> ContestantNames { get; private set; }

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
        public List<string> CharacterNames; //list of all the names of the contestants
        public List<JsonTrivia> QuizBank; //list of all the questions and its respective data
    }

    [System.Serializable]
    public class JsonTrivia
    {
        public string Question; //the question being asked
        public List<string> Answers;  //strings that will pass the questions
        public List<string> WrongAnswers;  //strings that the cpu will use the when they need to get the question wrong
        public string Category;  //They category of the question

        public JsonTrivia(string Question, List<string> Answers, List<string> WrongAnswers, string Category)
        {
            this.Question = Question;
            this.Answers = Answers;
            this.WrongAnswers = WrongAnswers;
            this.Category = Category;
        }
    }

    //Loads the data from the json
    public void LoadData()
    {
        //Debug.Log("Loading data...\n" + textJSON.text);
        json = JsonUtility.FromJson<JsonData>(textJSON.text);

        //debug line used to make sure data is getting extracted correctly

        //JsonTrivia firstData = json.QuizBank[0];

        //Debug.Log($"First Question detail: question: {firstData.Question}, Accepted Answer: {ListToString(firstData.Answers)}, " +
        //    $"Wrong Answers: {ListToString(firstData.WrongAnswers)}, Category: {firstData.Category}");


        //convert data into Trivia Class
        TriviaList = json.QuizBank.Select(q => ConvertJsonToTrivia(q)).ToList();

        ContestantNames = json.CharacterNames;
    }

    private Trivia ConvertJsonToTrivia(JsonTrivia j)
    {
        Category category;

        switch (j.Category.ToUpper())
        {
            case "KTANE":
                category = Category.KTANE;
            break;

            case "GEOGRAPHY":
                category = Category.Geography;
                break;

            case "LANGUAGE":
                category = Category.Language;
                break;

            case "WILDLIFE":
                category = Category.Wildlife;
                break;

            case "BIOLOGY":
                category = Category.Biology;
                break;

            default:
                category = Category.Maths;
                break;
        }

        return new Trivia(j.Question, j.Answers, j.WrongAnswers, category);
    }
}
