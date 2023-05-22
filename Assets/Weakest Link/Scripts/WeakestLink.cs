using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using static UnityEngine.Debug;
using UnityEngine.UI;

public class WeakestLink : MonoBehaviour {

	static int ModuleIdCounter = 1;
	int ModuleId;
	private bool ModuleSolved;

	public KMBombInfo Bomb;
	public KMAudio Audio;

	private KeyCode[] TypableKeys =
	{
		KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M,
		KeyCode.Period, KeyCode.Return, KeyCode.Minus, 
		KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.Backspace, KeyCode.Space
	};

	//variables that will be used in all stages
	#region Global Variables
	JsonReader jsonData;
	Contestant c1;
	Contestant c2;

	[SerializeField]
	Material nameDisplayMaterial;

	[SerializeField]
	Font nameDisplayFont;

	Category playerCategory; //the skill the player is good at

	string day; // the day of the week


	bool focused; //if the module is selected

	//fonts for the questions and the answers
	[SerializeField]
	List<Material> handWritingMaterials;

	[SerializeField]
	List<Font> handWritingFonts;

	[SerializeField]
	List<Material> questionMaterials;

	[SerializeField]
	List<Font> questionFonts;
	#endregion

	//objects for the first stage
	#region Stage 1 Objects 
	GameObject stage1Objects;

	GameObject contestant1GameObject;
	GameObject contestant2GameObject;

	KMSelectable stage1NextStageButton;
	#endregion

	#region Stage 2 Objects
	GameObject stage2Objects;

	const int timerMax = 60 * 2; //the amount of time the user has to answers qustions in the first stage 
	float currentTime;
	bool inQuestionPhase;
	TextMesh timerTextMesh;
	Text questionText;
	Text answerText;
	#endregion



	void SetUpModule()
	{
		GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; };
		GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };

		//get json data
		jsonData = gameObject.GetComponent<JsonReader>();

		//load json data if loaded alreay
		if (jsonData.json == null)
		{
			gameObject.GetComponent<JsonReader>().LoadData();
		}

		//create constestants
		int categoryCount = 7;
		int nameCount = jsonData.ContestantNames.Count;

		int randomFont = Rnd.Range(0, handWritingMaterials.Count);
		int randomFont2 = Rnd.Range(0, handWritingMaterials.Count);

		//initalize all varables
		#region stage1
		stage1Objects = transform.Find("Skill Check Phase").gameObject;
		contestant1GameObject = stage1Objects.transform.GetChild(0).gameObject;
		contestant2GameObject = stage1Objects.transform.GetChild(1).gameObject;
		stage1NextStageButton = stage1Objects.transform.GetChild(2).gameObject.GetComponent<KMSelectable>();
		stage1NextStageButton.OnInteract += delegate () { GoToNextStage(1); StartQuestionPhase(); return false; };
		#endregion

		#region stage2
		stage2Objects = transform.Find("Question Phase").gameObject;
		inQuestionPhase = false;
		GameObject timerGameObject = stage2Objects.transform.GetChild(0).gameObject;
		timerTextMesh = timerGameObject.GetComponent<TextMesh>();

		GameObject questionCanvas = stage2Objects.transform.GetChild(1).gameObject;
		questionText = questionCanvas.transform.GetChild(0).gameObject.GetComponent<Text>();
		
		GameObject answerCanvas = stage2Objects.transform.GetChild(2).gameObject;
		answerText = answerCanvas.transform.GetChild(0).gameObject.GetComponent<Text>();
		#endregion

		c1 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant1GameObject, handWritingMaterials[randomFont], handWritingFonts[randomFont], nameDisplayMaterial, nameDisplayFont);
		c2 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant2GameObject, handWritingMaterials[randomFont2], handWritingFonts[randomFont2], nameDisplayMaterial, nameDisplayFont);

		//make sure the right game objects are visible
		GoToNextStage(0);
	}


	void Start() {
		ModuleId = ModuleIdCounter++;

		SetUpModule();

		//get the player's skill
		day = DateTime.Now.DayOfWeek.ToString().ToUpper();

		switch (day)
		{
			case "SUNDAY":
				playerCategory = Category.History;
				break;
			case "MONDAY":
				playerCategory = Category.KTANE;
				break;
			case "TUESDAY":
				playerCategory = Category.Geography;
				break;
			case "WEDNESDAY":
				playerCategory = Category.Language;
				break;
			case "THURSDAY":
				playerCategory = Category.Wildlife;
				break;
			case "FRIDAY":
				playerCategory = Category.Biology;
				break;
			default:
				playerCategory = Category.Maths;
				break;
		}

	}

	void Update() {
		if (inQuestionPhase)
		{
			currentTime -= Time.deltaTime;
			timerTextMesh.text = $"{(int)currentTime / 60}:{(int)currentTime % 60}";


			if (focused) //keyboard input
			{
				string currentText = answerText.text;

				for (int i = 0; i < TypableKeys.Count(); i++)
				{
					if (TypableKeys[i] == KeyCode.Backspace && Input.GetKeyDown(TypableKeys[i]))
					{
						if (answerText.text != "")
						{ 
							answerText.text = currentText.Substring(0, currentText.Length - 1);
						}
					}

					else if (TypableKeys[i] == KeyCode.Return && Input.GetKeyDown(TypableKeys[i]))
                    {
						Submit();
					}

					else if (Input.GetKeyDown(TypableKeys[i]))
					{ 
						answerText.text += TypableKeys[i].ToString().ToUpper();
					}
				}
			}
		}
	}

	void GoToNextStage(int currentStage)
	{
		switch (currentStage)
		{
			case 0:
				stage1Objects.SetActive(true);
				stage2Objects.SetActive(false);

				break;
			case 1:
				stage1Objects.SetActive(false);
				stage2Objects.SetActive(true);
				break;
		}
	}

	void StartQuestionPhase()
	{
		currentTime = timerMax;
		inQuestionPhase = true;

		Trivia currentTrivia = GetQuestion();

		questionText.text = currentTrivia.Question;

		answerText.text = "";

		Debug.Log("Question:" + currentTrivia.Question);

		Debug.Log("Answers: " + string.Join(", ", currentTrivia.AcceptedAnswers.ToArray()));
	}

	void Submit()
	{
		answerText.text = "";
	}

	Trivia GetQuestion()
	{
		return jsonData.TriviaList[Rnd.Range(0, jsonData.TriviaList.Count)];
	}

	Trivia GetQuestion(Category category)
	{
		List<Trivia> a = jsonData.TriviaList.Where(s => s.Category == category).ToList();

		return a[Rnd.Range(0, jsonData.TriviaList.Count)];
	}

	//debug function used to make sure font was big enough for answer
	string GetLongestAnswer()
	{
		List<string> a = jsonData.TriviaList.Select(x => string.Join(",", x.AcceptedAnswers.ToArray())).ToList();

		List<string> b = new List<string>();

		a.ForEach(x => b.AddRange(x.Split(',')));

		return b.OrderByDescending(x => x.Length).First();
	}
}
