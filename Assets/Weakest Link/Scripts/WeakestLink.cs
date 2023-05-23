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

public class WeakestLink : MonoBehaviour 
{

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

	enum QuestionPhaseTurn
	{
		Player,
		C1,
		C2
	};

	enum MoneyPhaseTurn
	{ 
		Player,
		Conestant
	}


	//variables that will be used in all stages
	#region Global Variables

	QuestionPhaseTurn questionPhaseCurrentTurn = QuestionPhaseTurn.Player;

	[SerializeField]
	Material nameDisplayMaterial;

	[SerializeField]
	Font nameDisplayFont;

	//fonts for the questions and the answers
	[SerializeField]
	List<Material> handWritingMaterials;

	[SerializeField]
	List<Font> handWritingFonts;

	[SerializeField]
	List<Material> questionMaterials;

	[SerializeField]
	List<Font> questionFonts;

	JsonReader jsonData;

	Contestant c1;
	Contestant c2;
	Contestant playerContestant;

	string day; // the day of the week


	bool focused; //if the module is selected

	const int TIME_READ = 5; //the amount of time it would take the contestants to read the question 
	float currentTime;


	Color inactiveColor = new Color(.55f, .55f, .55f); //the color the player's names will be when it's not their turn
	Color correctColor = new Color(.38f, .97f, .43f); //the color the qustion will be if the answer is correct
	Color incorrectColor = new Color(1, 0, 0); //the color the qustion will be if the answer is wrong
	Trivia currentTrivia;

	#endregion

	#region Stage 1 Objects 
	GameObject stage1Objects;

	GameObject contestant1GameObject;
	GameObject contestant2GameObject;

	KMSelectable stage1NextStageButton;
	#endregion

	#region Stage 2 Objects
	GameObject stage2Objects;

	
	const int questionPhaseTimerMax = 1; // the amount of time the user has to answers qustions in the first stage 

	//const int questionPhaseTimerMax = 120; // the amount of time the user has to answers qustions in the first stage 


	bool inQuestionPhase;
	TextMesh questionPhaseTimerTextMesh;
	Text questionPhaseQuestionText;
	Text questionPhaseAnswerText;

	Text questionPhasePlayerText;
	Text questionPhaseContestant1Text;
	Text questionPhaseContestant2Text;



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

	GameObject stage3Objects;

	Contestant personToEliminate;
	bool inEliminationPhase;

	Text eliminationText;

	#endregion

	#region Stage 4
	GameObject stage4Objects;
	GameObject moneyCanvas;
	List<GameObject> moneyGameObjects;
	Text playerDisplay;
	Text contestantDisplay;
	GameObject bankGameObject;
	Button bankButton;
	bool inMoneyPhase;

	MoneyPhaseTurn moneyPhaseCurrentTurn;

	Text moneyPhaseQuestionText;
	Text moneyPhaseAnswerText;

	const int moneyPhaseTimerMax = 180; //the starting time for the money phase

	List<Category> categoryList;
	int categoryCurrentIndex;

	Contestant aliveConestant;
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

		//initalize all varables

		day = DateTime.Now.DayOfWeek.ToString().ToUpper();

		#region stage1
		stage1Objects = transform.Find("Skill Check Phase").gameObject;
		contestant1GameObject = stage1Objects.transform.Find("Contestant 1").gameObject;
		contestant2GameObject = stage1Objects.transform.Find("Contestant 2").gameObject;
		stage1NextStageButton = stage1Objects.transform.Find("Next Stage Button").gameObject.GetComponent<KMSelectable>();
		stage1NextStageButton.OnInteract += delegate () { GoToNextStage(1); UpdateTurn(true, 2); UpdateQuestion(true, 2); return false; };
		#endregion

		GetNewContestants(false);

		#region stage2
		stage2Objects = transform.Find("Question Phase").gameObject;
		inQuestionPhase = false;
		GameObject timerGameObject = stage2Objects.transform.Find("Timer").gameObject;
		questionPhaseTimerTextMesh = timerGameObject.GetComponent<TextMesh>();

		GameObject canvas = stage2Objects.transform.Find("Canvas").gameObject;
		questionPhaseQuestionText = canvas.transform.Find("Question").gameObject.GetComponent<Text>();
		questionPhaseAnswerText = canvas.transform.Find("Answer").gameObject.GetComponent<Text>();

		questionPhasePlayerText = canvas.transform.Find("Player").transform.Find("Player Name").GetComponent<Text>();
		questionPhaseContestant1Text = canvas.transform.Find("Contestant 1").transform.Find("Contestant 1 Name").GetComponent<Text>();
		questionPhaseContestant2Text = canvas.transform.Find("Contestant 2").transform.Find("Contestant 2 Name").GetComponent<Text>();

		questionPhasePlayerText.text = "PLAYER";
		questionPhaseContestant1Text.text = c1.Name.ToUpper();
		questionPhaseContestant2Text.text = c2.Name.ToUpper();
		#endregion

		#region stage3
		inEliminationPhase = false;
		stage3Objects = transform.Find("Elimination Phase").gameObject;
		eliminationText = stage3Objects.transform.Find("Canvas").transform.Find("Elimination Name").GetComponent<Text>();
		#endregion

		#region stage4
		inMoneyPhase = false;

		stage4Objects = transform.Find("Money Phase").gameObject;

		moneyCanvas = stage4Objects.transform.Find("Money Canvas").gameObject;

		GameObject money = moneyCanvas.transform.Find("Money").gameObject;

		bankGameObject = money.transform.Find("Button").gameObject;

		bankButton = bankGameObject.GetComponent<Button>();

		moneyGameObjects = new List<GameObject>()
		{
			money.transform.Find("20 Image").gameObject,
			money.transform.Find("50 Image").gameObject,
			money.transform.Find("100 Image").gameObject,
			money.transform.Find("200 Image").gameObject,
			money.transform.Find("300 Image").gameObject,
			money.transform.Find("450 Image").gameObject,
			money.transform.Find("600 Image").gameObject,
			money.transform.Find("800 Image").gameObject,
			money.transform.Find("1000 Image").gameObject,

		};

		GameObject c = stage4Objects.transform.Find("Canvas").gameObject;

		moneyPhaseQuestionText = c.transform.Find("Question").GetComponent<Text>();
		moneyPhaseAnswerText = c.transform.Find("Answer").GetComponent<Text>();

		playerDisplay = c.transform.Find("Player").Find("Player Name").gameObject.GetComponent<Text>();
		contestantDisplay = c.transform.Find("Contestant").Find("Contestant Name").gameObject.GetComponent<Text>();


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
			questionPhaseTimerTextMesh.text = string.Format("{0:0}:{1:00}", (int)(currentTime / 60), (int)currentTime % 60);

			if (currentTime <= 0f)
			{
				inQuestionPhase = false;
				GoToNextStage(2);


				if (CalculatePersonToEliminate())
				{
					inEliminationPhase = true;
				}

				else
				{
					GetComponent<KMBombModule>().HandleStrike();
					GetNewContestants(true);
					GoToNextStage(0);
				}
			}

			if (focused && questionPhaseCurrentTurn == QuestionPhaseTurn.Player) //keyboard input
			{
				GetKeyboardInput(2);
			}
		}

		else if (focused)
		{
			if (inEliminationPhase)
			{ 
				GetKeyboardInput(3);
			}
			else if (inMoneyPhase)
			{
				GetKeyboardInput(4);
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
				stage3Objects.SetActive(false);
				stage4Objects.SetActive(false);

				break;
			case 1:
				stage1Objects.SetActive(false);
				stage2Objects.SetActive(true);
				stage3Objects.SetActive(false);
				stage4Objects.SetActive(false);

				Logging("Starting question phase");
				break;

			case 2:
				stage1Objects.SetActive(false);
				stage2Objects.SetActive(false);
				stage3Objects.SetActive(true);
				stage4Objects.SetActive(false);

				eliminationText.text = "";

				Logging("Starting elimination phase");
				break;

			case 3:
				stage1Objects.SetActive(false);
				stage2Objects.SetActive(false);
				stage3Objects.SetActive(false);
				stage4Objects.SetActive(true);
				UpdateQuestion(true, 4);
				UpdateTurn(true, 4);

				Logging("Starting money phase");
				break;
		}
	}

	void UpdateQuestion(bool init, int stage)
	{
		if (stage == 2)
		{
			if (init)
			{
				currentTime = questionPhaseTimerMax;
				inQuestionPhase = true;
			}

			currentTrivia = GetQuestion();

			questionPhaseQuestionText.color = Color.white;

			questionPhaseQuestionText.text = currentTrivia.Question;

			questionPhaseAnswerText.text = "";
		}

		else if (stage == 4)
		{
			if (init)
			{
				currentTime = moneyPhaseTimerMax;
				inMoneyPhase = true;

				categoryCurrentIndex = 0;
				categoryList = GetCategoryList();
			}

			else
			{
				categoryCurrentIndex = (categoryCurrentIndex + 1) % categoryList.Count;
			}

			currentTrivia = GetQuestion(categoryList[categoryCurrentIndex]);

			moneyPhaseQuestionText.color = Color.white;

			moneyPhaseQuestionText.text = currentTrivia.Question;

			moneyPhaseAnswerText.text = "";

		}

		Debug.Log("Answers: " + string.Join(", ", currentTrivia.AcceptedAnswers.ToArray()));
	}

	void UpdateTurn(bool init, int stage)
	{
		if (stage == 2)
		{
			if (init)
			{
				questionPhaseCurrentTurn = QuestionPhaseTurn.Player;
			}

			else
			{
				questionPhaseCurrentTurn = (QuestionPhaseTurn)(((int)questionPhaseCurrentTurn + 1) % 3);
			}

		}

		else if (stage == 4)
		{
			if (init)
			{
				moneyPhaseCurrentTurn = MoneyPhaseTurn.Player;
				aliveConestant = c1.Eliminated ? c2 : c1;

				contestantDisplay.text = aliveConestant.Name.ToUpper();
			}

			else
			{
				moneyPhaseCurrentTurn = (MoneyPhaseTurn)(((int)questionPhaseCurrentTurn + 1) % 2);
			}
		}

		UpdateNameColors(stage);
	}

	void UpdateNameColors(int stage)
	{
		if (stage == 2)
		{
			questionPhasePlayerText.color = questionPhaseContestant1Text.color = questionPhaseContestant2Text.color = inactiveColor;

			Text[] names = new Text[] { questionPhasePlayerText, questionPhaseContestant1Text, questionPhaseContestant2Text };

			names[(int)questionPhaseCurrentTurn].color = Color.white;
		}

		else if (stage == 4)
		{
			playerDisplay.color = contestantDisplay.color = inactiveColor;

			Text[] names = new Text[] { playerDisplay, contestantDisplay};

			names[(int)moneyPhaseCurrentTurn].color = Color.white;
		}
	}

	//returns true if the player got enough questions right
	bool CalculatePersonToEliminate()
	{
		//Not answering at least 5 questions or not answering over 50% correctly will lead to a strike

		//debug code to move on to the next stage
		bool lessThanThresholdAsked = playerContestant.QuestionsAsked < 0;
		bool lessThanThresholdCorrect = (float)playerContestant.CorrectAnswer / playerContestant.QuestionsAsked < 0f;

		//bool lessThanThresholdAsked = playerContestant.QuestionsAsked < 5;
		//bool lessThanThresholdCorrect = (float)playerContestant.CorrectAnswer / playerContestant.QuestionsAsked < .5f;

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
				personToEliminate = c1.Category == playerContestant.Category ? c1 : c2;

				Logging($"{personToEliminate.Name} shares the same category as you. Eliminate them");
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

					personToEliminate = a[0];

					Logging($"Elimation values are the same. {personToEliminate.Name} comes first alphabetically. Eliminate them");
				}

				else
				{
					if (contestant1Percentage > contestant2Percentage)
					{
						personToEliminate = c2;
					}

					else
					{
						personToEliminate = c1;
					}

					Logging($"{personToEliminate.Name} has the lower elimination value. Eliminate them");
				}
			}

			inEliminationPhase = true;
			return true;
		}
	}

	int GetEliminationValue(Contestant contestant, int baseContestantValue)
	{
		Logging($"{contestant.Name}'s base elimination value: {baseContestantValue}");

		int boost = 0;

		string serialNumber = Bomb.GetSerialNumber().ToUpper();
		foreach (char c in contestant.Name.ToUpper())
		{
			if (serialNumber.Contains(c))
			{
				boost++;
				Logging($"Serial Number contains a {c}. Boost is now {boost}");
			}

			else if (numberToLettter.ContainsKey(c) && serialNumber.Contains(numberToLettter[c]))
			{
				boost++;
				Logging($"Serial Number contains a {numberToLettter[c]} which counts as a {c}. Boost is now {boost}");
			}
		}

		baseContestantValue += boost;

		Logging($"{contestant.Name}'s elimination value: {baseContestantValue}");

		return baseContestantValue;
	}

	IEnumerator Submit(int stage)
	{
		if (stage == 2)
		{
			bool turnChanged = false;

			string response = questionPhaseAnswerText.text;

			string[] answers = currentTrivia.AcceptedAnswers.Select(x => x.ToUpper()).ToArray();

			Contestant currentContestant = questionPhaseCurrentTurn == QuestionPhaseTurn.Player ? playerContestant : questionPhaseCurrentTurn ==
														  QuestionPhaseTurn.C1 ? c1 : c2;

			if (questionPhaseCurrentTurn != QuestionPhaseTurn.C2)
			{
				UpdateTurn(false, 2);
				turnChanged = true;
			}

			string log = $"Question: \"{currentTrivia.Question}\". {(currentContestant.Name == "" ? "You" : currentContestant.Name)} answered \"{response}\", ";

			currentContestant.QuestionsAsked++;

			if (answers.Contains(response))
			{
				questionPhaseQuestionText.color = correctColor;
				currentContestant.CorrectAnswer++;
				log += "which is correct";
			}

			else
			{
				questionPhaseQuestionText.color = incorrectColor;
				log += "which is incorrect";
			}


			log += $". Ratio is now {currentContestant.CorrectAnswer}/{currentContestant.QuestionsAsked}";

			Logging(log);

			questionPhaseAnswerText.text = "";
			yield return new WaitForSeconds(2f);

			if (currentTime <= 0)
				yield break;

			UpdateQuestion(false, 2);

			if (!turnChanged && questionPhaseCurrentTurn == QuestionPhaseTurn.C2)
			{
				UpdateTurn(false, 2);
			}

			if (questionPhaseCurrentTurn != QuestionPhaseTurn.Player)
			{
				yield return new WaitForSeconds(TIME_READ);

				Contestant c = questionPhaseCurrentTurn == QuestionPhaseTurn.C1 ? c1 : c2;

				float percentage = currentTrivia.Category == c.Category ? Contestant.GOOD_RIGHT_CHOICE : Contestant.REGULAR_RIGHT_CHOICE;

				bool correctAnswer = Rnd.Range(0f, 1f) < percentage;

				string input = correctAnswer ? currentTrivia.AcceptedAnswers[Rnd.Range(0, currentTrivia.AcceptedAnswers.Count)].ToUpper() :
											   currentTrivia.WrongAnswers[Rnd.Range(0, currentTrivia.WrongAnswers.Count)].ToUpper();
				foreach (char ch in input)
				{
					if (currentTime <= 0)
						yield break;

					questionPhaseAnswerText.text += "" + ch;
					yield return new WaitForSeconds(0.1f);
				}

				yield return new WaitForSeconds(1f);

				if (currentTime <= 0)
					yield break;

				StartCoroutine(Submit(2));
			}
		}

		else if (stage == 3)
		{
			string log;
			if (eliminationText.text == personToEliminate.Name.ToUpper())
			{
				personToEliminate.Eliminated = true;
				log = $"You entered \"{eliminationText.text}\". Which is correct.";
				GoToNextStage(3);
			}

			else
			{
				log = $"Strike! You entered \"{eliminationText.text}\".";
				GetComponent<KMBombModule>().HandleStrike();
				GoToNextStage(0);
				GetNewContestants(true);
			}

			Logging(log);
		}
	}

	Trivia GetQuestion()
	{
		return jsonData.TriviaList[Rnd.Range(0, jsonData.TriviaList.Count)];
	}

	Trivia GetQuestion(Category category)
	{
		List<Trivia> a = jsonData.TriviaList.Where(s => s.Category == category).ToList();

		return a[Rnd.Range(0, a.Count)];
	}

	List<Category> GetCategoryList()
	{
		switch (day)
		{
			case "SUNDAY":
				return new List<Category>() { Category.Maths, Category.KTANE, Category.Wildlife, Category.Biology, Category.History, Category.Geography, Category.Language };
			case "MONDAY":
				return new List<Category>() { Category.Geography, Category.Wildlife, Category.KTANE, Category.History, Category.Language, Category.Biology, Category.Maths };
			case "TUESDAY":
				return new List<Category>() { Category.Biology, Category.History, Category.Maths, Category.KTANE, Category.Geography, Category.Language, Category.Wildlife };
			case "WEDNESDAY":
				return new List<Category>() { Category.Wildlife, Category.Maths, Category.History, Category.Geography, Category.Language, Category.KTANE, Category.Biology };
			case "THURSDAY":
				return new List<Category>() { Category.Maths, Category.KTANE, Category.Wildlife, Category.Biology, Category.Language, Category.History, Category.Geography };
			case "FRIDAY":
				return new List<Category>() { Category.KTANE, Category.Wildlife, Category.Maths, Category.Geography, Category.Language, Category.Biology, Category.History };
			default:
				return new List<Category>() { Category.Maths, Category.Wildlife, Category.Biology, Category.History, Category.Language, Category.KTANE, Category.Geography };
		}
	}

	//debug function used to make sure font was big enough for answer
	string GetLongestAnswer()
	{
		List<string> a = jsonData.TriviaList.Select(x => string.Join(",", x.AcceptedAnswers.ToArray())).ToList();

		List<string> b = new List<string>();

		a.ForEach(x => b.AddRange(x.Split(',')));

		return b.OrderByDescending(x => x.Length).First();
	}

	void GetNewContestants(bool updatePlayer)
	{
		int categoryCount = 7;
		int nameCount = jsonData.ContestantNames.Count;

		int randomFont = Rnd.Range(0, handWritingMaterials.Count);
		int randomFont2 = Rnd.Range(0, handWritingMaterials.Count);

		c1 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant1GameObject, handWritingMaterials[randomFont], handWritingFonts[randomFont], nameDisplayMaterial, nameDisplayFont, true);

		c2 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant2GameObject, handWritingMaterials[randomFont2], handWritingFonts[randomFont2], nameDisplayMaterial, nameDisplayFont, true);

		if (updatePlayer)
		{
			playerContestant = new Contestant("", GetPlayerSkill(), null, null, null, null, null, false);
		}
	}

	void Logging(string s)
	{
		LogFormat($"[The Weakest Link #{ModuleId}] {s}");
	}

	void GetKeyboardInput(int stage)
	{
		string currentText = stage == 2 ? questionPhaseAnswerText.text : stage == 3 ? eliminationText.text : moneyPhaseAnswerText.text;

		foreach (KeyCode keyCode in TypableKeys)
		{
			if (keyCode == KeyCode.Backspace && Input.GetKeyDown(keyCode))
			{
				if (currentText != "")
				{
					string newText = currentText.Substring(0, currentText.Length - 1);

					if (stage == 2)
					{
						questionPhaseAnswerText.text = newText;
					}

					else if (stage == 3)
					{
						eliminationText.text = newText;
					}

					else
					{
						moneyPhaseAnswerText.text = newText;
					}
				}
			}

			else if (keyCode == KeyCode.Return && Input.GetKeyDown(keyCode))
			{
					
				StartCoroutine(Submit(stage));
			}

			else if ((int)keyCode >= 48 && (int)keyCode <= 57 && Input.GetKeyDown(keyCode))
			{
				string newText = keyCode.ToString().Substring(5, 1);

				if (stage == 2)
				{
					questionPhaseAnswerText.text += newText;
				}
				else if (stage == 3)
				{
					eliminationText.text = newText;
				}

				else
				{ 
					moneyPhaseAnswerText.text = newText;
				}
			}

			else if (keyCode == KeyCode.Space && Input.GetKeyDown(keyCode))
			{
				if (stage == 2)
				{
					questionPhaseAnswerText.text += " ";
				}
				else if (stage == 3)
				{
					eliminationText.text += " ";
				}

				else
				{ 
					moneyPhaseAnswerText.text += " ";
				}
			}

			else if (keyCode == KeyCode.Minus && Input.GetKeyDown(keyCode))
			{
				if (stage == 2)
				{
					questionPhaseAnswerText.text += "-";
				}
				else if (stage == 3)
				{
					eliminationText.text += "-";
				}

				else
				{ 
					moneyPhaseAnswerText.text += "-";
				}
			}

			else if (Input.GetKeyDown(keyCode))
			{
				string newString = keyCode.ToString().ToUpper();

				if (stage == 2)
				{
					questionPhaseAnswerText.text += newString;
				}
				else if (stage == 3)
				{
					eliminationText.text += newString;
				}

				else
				{ 
					moneyPhaseAnswerText.text += newString;
				}
			}
		}
	}
}
