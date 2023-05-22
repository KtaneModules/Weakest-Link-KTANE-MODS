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

	KeyCode[] TypableKeys =
	{
		KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M,
		KeyCode.Period, KeyCode.Return, KeyCode.Minus,
		KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.Backspace, KeyCode.Space
	};

	enum Turn
	{
		Player,
		C1,
		C2
	};


	//variables that will be used in all stages
	#region Global Variables
	JsonReader jsonData;
	Contestant c1;
	Contestant c2;

	Turn currentTurn = Turn.Player;


	[SerializeField]
	Material nameDisplayMaterial;

	[SerializeField]
	Font nameDisplayFont;


	Contestant playerContestant;

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

	//const int timerMax = 60 * 3; // the amount of time the user has to answers qustions in the first stage 
	const int timerMax = 60 * 1; // the amount of time the user has to answers qustions in the first stage 


	float currentTime;
	bool inQuestionPhase;
	const int TIME_READ = 5; //the amount of time it would take the contestants to read the question 

	Color inactiveColor = new Color(.55f, .55f, .55f); //the color the player's names will be when it's not their turn
	Color correctColor = new Color(.38f, .97f, .43f); //the color the qustion will be if the answer is correct
	Color incorrectColor = new Color(1, 0, 0); //the color the qustion will be if the answer is wrong


	TextMesh timerTextMesh;
	Text questionText;
	Text answerText;

	TextMesh playerTextMesh;
	TextMesh contestant1TextMesh;
	TextMesh contestant2TextMesh;

	Trivia currentTrivia;


	Dictionary<char, char> numberToLettter = new Dictionary<char, char>()
	{
		{ '1', 'A' },
		{ '2', 'B' },
		{ '3', 'C' },
		{ '4', 'D' },
		{ '5', 'E' },
		{ '6', 'F' },
		{ '7', 'G' },
		{ '8', 'H' },
		{ '9', 'I' },
		{ '0', 'J' }
	};
	#endregion

	#region Stage 3
	string personToEliminate;
	bool inEliminationPhase;

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

		day = DateTime.Now.DayOfWeek.ToString().ToUpper();

		#region stage1
		stage1Objects = transform.Find("Skill Check Phase").gameObject;
		contestant1GameObject = stage1Objects.transform.GetChild(0).gameObject;
		contestant2GameObject = stage1Objects.transform.GetChild(1).gameObject;
		stage1NextStageButton = stage1Objects.transform.GetChild(2).gameObject.GetComponent<KMSelectable>();
		stage1NextStageButton.OnInteract += delegate () { GoToNextStage(1); UpdateTurn(true); UpdateQuestion(true); return false; };
		#endregion

		c1 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant1GameObject, handWritingMaterials[randomFont], handWritingFonts[randomFont], nameDisplayMaterial, nameDisplayFont, true);
		c2 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant2GameObject, handWritingMaterials[randomFont2], handWritingFonts[randomFont2], nameDisplayMaterial, nameDisplayFont, true);

		#region stage2
		stage2Objects = transform.Find("Question Phase").gameObject;
		inQuestionPhase = false;
		GameObject timerGameObject = stage2Objects.transform.GetChild(0).gameObject;
		timerTextMesh = timerGameObject.GetComponent<TextMesh>();

		GameObject questionCanvas = stage2Objects.transform.GetChild(1).gameObject;
		questionText = questionCanvas.transform.GetChild(0).gameObject.GetComponent<Text>();
		
		GameObject answerCanvas = stage2Objects.transform.GetChild(2).gameObject;
		answerText = answerCanvas.transform.GetChild(0).gameObject.GetComponent<Text>();

		playerTextMesh = stage2Objects.transform.GetChild(3).gameObject.GetComponent<TextMesh>();
		contestant1TextMesh = stage2Objects.transform.GetChild(4).gameObject.GetComponent<TextMesh>();
		contestant2TextMesh = stage2Objects.transform.GetChild(5).gameObject.GetComponent<TextMesh>();

		playerTextMesh.text = "PLAYER";
		contestant1TextMesh.text = c1.Name.ToUpper();
		contestant2TextMesh.text = c2.Name.ToUpper();
		#endregion

		#region stage3
		inEliminationPhase = false;
		#endregion

		//create player
		playerContestant = new Contestant("", GetPlayerSkill(), null, null, null, null, null, false);
		//make sure the right game objects are visible
		GoToNextStage(0);

		Logging($"First contestant is {c1.Name} who specializese in {c1.Category}");
		Logging($"Second contestant is {c2.Name} who specializese in {c2.Category}");
		Logging($"You specialize in {playerContestant.Category}");
	}


	void Start() {
		ModuleId = ModuleIdCounter++;
		SetUpModule();
	}

	void Update() {
		if (inQuestionPhase)
		{
			currentTime -= Time.deltaTime;
			timerTextMesh.text = string.Format("{0:00}:{1:00}", (int)(currentTime / 60), (int)currentTime % 60);

			if (currentTime <= 0f)
			{
				inQuestionPhase = false;
				Logging($"Player Stats: {playerContestant.CorrectAnswer}/{playerContestant.QuestionsAsked}");
				Logging($"{c1.Name} Stats: {c1.CorrectAnswer}/{c1.QuestionsAsked}");
				Logging($"{c2.Name} Stats: {c2.CorrectAnswer}/{c2.QuestionsAsked}");
				GoToNextStage(2);


				if (CalculatePersonToEliminate())
				{
					inEliminationPhase = true;
				}

				else
				{
					GetComponent<KMBombModule>().HandleStrike();
					GoToNextStage(0);
				}
			}

			if (focused && currentTurn == Turn.Player) //keyboard input
			{
				string currentText = answerText.text;

				foreach(KeyCode keyCode in TypableKeys)
				{
					if (keyCode == KeyCode.Backspace && Input.GetKeyDown(keyCode))
					{
						if (answerText.text != "")
						{
							answerText.text = currentText.Substring(0, currentText.Length - 1);
						}
					}

					else if (keyCode == KeyCode.Return && Input.GetKeyDown(keyCode))
					{
						StartCoroutine(Submit());
					}

					else if ((int)keyCode >= 48 && (int)keyCode <= 57 && Input.GetKeyDown(keyCode))
					{
						answerText.text += keyCode.ToString().Substring(5, 1);
					}

					else if (keyCode == KeyCode.Space && Input.GetKeyDown(keyCode))
					{
						answerText.text += " ";
					}

					else if (keyCode == KeyCode.Minus && Input.GetKeyDown(keyCode))
					{
						answerText.text += "-";
					}

					else if (Input.GetKeyDown(keyCode))
					{
						answerText.text += keyCode.ToString().ToUpper();
					}
				}
			}

		}
	}

	Category GetPlayerSkill()
	{
		switch (day)
		{
			case "SUNDAY":
				return Category.History;
			case "MONDAY":
				return Category.KTANE;
			case "TUESDAY":
				return Category.Geography;
			case "WEDNESDAY":
				return Category.Language;
			case "THURSDAY":
				return Category.Wildlife;
			case "FRIDAY":
				return Category.Biology;
			default:
				return Category.Maths;
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

			case 2:
				stage1Objects.SetActive(false);
				stage2Objects.SetActive(false);
				break;
		}
	}

	void UpdateQuestion(bool init)
	{
		if (init)
		{
			currentTime = timerMax;
			inQuestionPhase = true;
		}

		currentTrivia = GetQuestion();

		questionText.color = Color.white;


		questionText.text = currentTrivia.Question;

		answerText.text = "";

		Debug.Log("Answers: " + string.Join(", ", currentTrivia.AcceptedAnswers.ToArray()));
	}

	void UpdateTurn(bool init)
	{
		if (init)
		{
			currentTurn = Turn.Player;
		}

		else
		{ 
			currentTurn = (Turn)(((int)currentTurn + 1) % 3);
		}

		UpdateNameColors();
	}

void UpdateNameColors()
	{
		playerTextMesh.color = contestant1TextMesh.color = contestant2TextMesh.color = inactiveColor;

		TextMesh[] names = new TextMesh[] { playerTextMesh, contestant1TextMesh, contestant2TextMesh };

		names[(int)currentTurn].color = Color.white;
	}

	//returns true if the player got enough questions right
	bool CalculatePersonToEliminate()
	{
		//Not answering at least 5 questions or not answering over 50% correctly will lead to a strike

		bool lessThanThresholdAsked = playerContestant.QuestionsAsked < 5;
		bool lessThanThresholdCorrect = (float)playerContestant.CorrectAnswer  / playerContestant.QuestionsAsked < .5f;

		int playerPercentage = (int)((float)playerContestant.CorrectAnswer / playerContestant.QuestionsAsked * 100);
		int contestant1Percentage = (int)((float)c1.CorrectAnswer / c1.QuestionsAsked * 100);
		int contestant2Percentage = (int)((float)c2.CorrectAnswer / c2.QuestionsAsked * 100);

		if (lessThanThresholdAsked)
		{
			Logging($"Strike! You only answered {playerContestant.QuestionsAsked} questions");
			return false;
		}

		else if (lessThanThresholdCorrect)
		{
			Logging($"Strike! Your ratio was only {playerPercentage}%");
			return false;
		}

		else
		{
			//If only one contestant has the same skill as you, type in their name to eliminate them.
			if (c1.Category != c2.Category && (c1.Category == playerContestant.Category || c2.Category == playerContestant.Category))
			{
				Contestant c = c1.Category == playerContestant.Category ? c1 : c2;

				Logging($"{c.Name} shares the same category as you. Eliminate them");
			}

			else
			{
				//if the character is in the serial number, add 1 to the percentage. Convert numbers into letters (1=A, 2=B, etc) with 0 meaning J.
				contestant1Percentage = GetEliminationValue(c1, contestant1Percentage);
				contestant2Percentage = GetEliminationValue(c2, contestant2Percentage);

				if (contestant1Percentage == contestant2Percentage)
				{
					Contestant[] a = new Contestant[] { c1, c2 };

					Array.Sort(a, (x, y) => String.Compare(x.Name, y.Name));

					personToEliminate = a[0].Name;

					Logging($"Elimation values are the same. {personToEliminate} comes first alphabetically. Eliminate them");
				}

				else
				{
					if (contestant1Percentage > contestant2Percentage)
					{
						personToEliminate = c2.Name;
					}

					else
					{
						personToEliminate = c1.Name;
					}

					Logging($"{personToEliminate} has the lower elimination value. Eliminate them");
				}
			}
			inEliminationPhase = true;
			return true;
		}
	}

	int GetEliminationValue(Contestant contestant, int baseContestantValue)
	{
		Logging($"{contestant}'s base elimination value: {baseContestantValue}");

		int boost = 0;

		string serialNumber = Bomb.GetSerialNumber().ToUpper();
		foreach (char c in contestant.Name.ToUpper())
		{
			if (serialNumber.Contains(c))
			{
				Logging($"Serial Number contains a {c}. Boost is now {boost++}");
			}

			else if (numberToLettter.ContainsKey(c) && serialNumber.Contains(numberToLettter[c]))
			{
				Logging($"Serial Number contains a {numberToLettter[c]} which counts as a {c}. Boost is now {boost++}");
			}
		}

		baseContestantValue += boost;

		Logging($"{contestant.Name}'s elimination value: {baseContestantValue}");

		return baseContestantValue;
	}

	IEnumerator Submit()
	{
		bool turnChanged = false;

		string response = answerText.text;

		string[] answers = currentTrivia.AcceptedAnswers.Select(x => x.ToUpper()).ToArray();

		Contestant currentContestant = currentTurn == Turn.Player ? playerContestant : currentTurn ==
													  Turn.C1 ? c1 : c2;

		if (currentTurn != Turn.C2)
		{ 
			UpdateTurn(false);
			turnChanged = true;
		}

		string log = $"Question: \"{currentTrivia.Question}\". {(currentContestant.Name == "" ? "You" : currentContestant.Name)} answered \"{response}\", ";

		currentContestant.QuestionsAsked++;

		if (answers.Contains(response))
		{
			questionText.color = correctColor;
			currentContestant.CorrectAnswer++;
			log += "which is correct";
		}

		else
		{
			questionText.color = incorrectColor;
			log += "which is incorrect";
		}


		log += $". Ratio is now ({currentContestant.CorrectAnswer}/{currentContestant.QuestionsAsked})";

		Logging(log);

		answerText.text = "";
		yield return new WaitForSeconds(2f);

		if (currentTime <= 0)
			yield break;

		UpdateQuestion(false);

		if (!turnChanged && currentTurn == Turn.C2)
		{
			UpdateTurn(false);
		}

		if (currentTurn != Turn.Player)
		{
			yield return new WaitForSeconds(TIME_READ);

			Contestant c =  currentTurn == Turn.C1 ? c1 : c2;

			float percentage = currentTrivia.Category == c.Category ? Contestant.GOOD_RIGHT_CHOICE : Contestant.REGULAR_RIGHT_CHOICE;

			bool correctAnswer = Rnd.Range(0f, 1f) < percentage;

			string input = correctAnswer ? currentTrivia.AcceptedAnswers[Rnd.Range(0, currentTrivia.AcceptedAnswers.Count)].ToUpper() : 
				                           currentTrivia.WrongAnswers[Rnd.Range(0, currentTrivia.WrongAnswers.Count)].ToUpper();
			foreach (char ch in input)
			{
				if (currentTime <= 0)
					yield break;

				answerText.text += "" + ch;
				yield return new WaitForSeconds(0.1f);
			}

			yield return new WaitForSeconds(1f);

			if (currentTime <= 0)
				yield break;

			StartCoroutine(Submit());
		}
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

	void Logging(string s)
	{
		LogFormat($"[The WeakestLink #{ModuleId}] {s}");
	}
}
