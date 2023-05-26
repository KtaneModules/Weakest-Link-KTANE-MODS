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

	bool audioPlaying;

	KeyCode[] TypableKeys =
	{
		KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M, KeyCode.Period, KeyCode.Return, KeyCode.Minus, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.Backspace, KeyCode.Space
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

	[SerializeField]
	List<AudioClip> playAudioList;
	
	[SerializeField]
	List<AudioClip> goodbyeAudioList;
	
	[SerializeField]
	List<AudioClip> startClockAudioList;

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

	[SerializeField]
	Sprite redBackground;

	JsonReader jsonData;

	Contestant c1;
	Contestant c2;
	Contestant playerContestant;

	string day; // the day of the week


	bool focused; //if the module is selected

	int longestQuestionLength;

	const int TIME_READ = 5; //the amount of time it would take the contestants to read the longest question 
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

	const int questionPhaseTimerMax = 120; // the amount of time the user has to answers qustions in the first stage 


	bool inQuestionPhase;
	Text questionPhaseTimerText;
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

	Text contestant1Display;

	Text contestant1EliminationText;

	Text contestant2Display;

	Text contestant2EliminationText;

	Contestant personToEliminate;
	bool inEliminationPhase;

	Text eliminationText;

	#endregion

	#region Stage 4
	GameObject stage4Objects;

	KMSelectable stage4NextStageButton;

	#endregion

	#region Stage 5
	GameObject stage5Objects;
	GameObject moneyCanvas;
	List<Money> moneyObjects;
	int currentMoneyIndex;
	Text playerDisplay;
	Text contestantDisplay;
	GameObject bankGameObject;
	KMSelectable bankButton;
	TextMesh bankMoneyAmountTextMesh;
	bool inMoneyPhase;

	MoneyPhaseTurn moneyPhaseCurrentTurn;

	Text moneyPhaseQuestionText;
	Text moneyPhaseAnswerText;

	const int moneyPhaseTimerMax = 180; //the starting time for the money phase

	List<Category> categoryList;
	int categoryCurrentIndex;

	Contestant aliveConestant;

	Text moneyPhaseTimerText;

	int moneyStored;

	#endregion


	#region Stage 6
	[SerializeField]
	Sprite checkmarkSprite;

	[SerializeField]
	Sprite xSprite;

	bool inFaceOffPhase;

	GameObject stage6Objects;

	List<CorrectIndicator> moduleIndicators;

	Text stage6QuestionText;

	Text stage6AnswerText;

	int correctAnswers;
	int quesetionsAsked;
	#endregion




	void Start() {
		ModuleId = ModuleIdCounter++;
		SetUpModule();
	}

	void Update() {
		if (!ModuleSolved && !audioPlaying)
		{
			if (inQuestionPhase)
			{
				currentTime -= Time.deltaTime;
				questionPhaseTimerText.text = string.Format("{0:0}:{1:00}", (int)(currentTime / 60), (int)currentTime % 60);

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
						StartCoroutine(Strike(2));
					}
				}

				if (focused && questionPhaseCurrentTurn == QuestionPhaseTurn.Player)
				{
					GetKeyboardInput(2);
				}
			}

			else if (inMoneyPhase)
			{
				currentTime -= Time.deltaTime;
				moneyPhaseTimerText.text = string.Format("{0:0}:{1:00}", (int)(currentTime / 60), (int)currentTime % 60);

				if (currentTime <= 0f)
				{
					EndMoneyPhase(false, $"Strike! Time ran out and you only banked {bankMoneyAmountTextMesh.text}");
				}

				if (focused && moneyPhaseCurrentTurn == MoneyPhaseTurn.Player)
				{
					GetKeyboardInput(5);
				}
			}

			else if (focused)
			{
				if (inEliminationPhase)
				{
					GetKeyboardInput(3);
				}

				else if (inFaceOffPhase)
				{
					GetKeyboardInput(6);
				}
			}
		}
	}

	IEnumerator Stage1Button()
	{
		//let's play the weakest link
		AudioClip playClip = playAudioList[Rnd.Range(0, playAudioList.Count)];

		audioPlaying = true;
		Audio.PlaySoundAtTransform(playClip.name, transform);
		yield return new WaitForSeconds(playClip.length + 1);


		//start the clock
		AudioClip clockClip = playAudioList[Rnd.Range(0, playAudioList.Count)];

		Audio.PlaySoundAtTransform(clockClip.name, transform);
		yield return new WaitForSeconds(clockClip.length + 1);

		audioPlaying = false;

		GoToNextStage(1); 
		UpdateTurn(true, 2);
		UpdateQuestion(true, 2); 
	}

	void SetUpModule()
	{
		audioPlaying = false;
		GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; };
		GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };

		//get json data
		jsonData = gameObject.GetComponent<JsonReader>();



		//load json data if not loaded alreay
		if (jsonData.json == null)
		{
			gameObject.GetComponent<JsonReader>().LoadData();
		}

		//initalize all varables
		day = DateTime.Now.DayOfWeek.ToString().ToUpper();

		longestQuestionLength = GetLongestQuestionLength();

		#region stage1
		stage1Objects = transform.Find("Skill Check Phase").gameObject;
		GameObject stage1Canvas = stage1Objects.transform.Find("Canvas").gameObject;
		contestant1GameObject = stage1Canvas.transform.Find("Contestant 1").gameObject;
		contestant2GameObject = stage1Canvas.transform.Find("Contestant 2").gameObject;
		stage1NextStageButton = stage1Objects.transform.Find("Next Stage Button").GetComponent<KMSelectable>();
		stage1NextStageButton.OnInteract += delegate () { if (!audioPlaying) { StartCoroutine(Stage1Button());  } return false; };
		#endregion

		do
		{
			GetNewContestants(false);
		} while (c1.Name == c2.Name);

		#region stage2
		stage2Objects = transform.Find("Question Phase").gameObject;
		inQuestionPhase = false;

		GameObject canvas = stage2Objects.transform.Find("Canvas").gameObject;
		questionPhaseQuestionText = canvas.transform.Find("Question").GetComponent<Text>();
		questionPhaseAnswerText = canvas.transform.Find("Answer").GetComponent<Text>();

		questionPhaseTimerText = canvas.transform.Find("Timer").transform.Find("Text").GetComponent<Text>();

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

		GameObject stage3Canvas = stage3Objects.transform.Find("Canvas").gameObject;

		eliminationText = stage3Canvas.transform.Find("Image").transform.Find("Elimination Name").GetComponent<Text>();

		contestant1Display = stage3Canvas.transform.Find("Contestant 1 Name").transform.Find("Name").GetComponent<Text>();
		contestant1EliminationText = stage3Canvas.transform.Find("Contestant 1 Elimination").transform.Find("Elimination Text").GetComponent<Text>();


		contestant2Display = stage3Canvas.transform.Find("Contestant 2 Name").transform.Find("Name").GetComponent<Text>();
		contestant2EliminationText = stage3Canvas.transform.Find("Contestant 2 Elimination").transform.Find("Elimination Text").GetComponent<Text>();
		#endregion

		#region stage 4

		stage4Objects = transform.Find("Intermission Phase").gameObject;

		stage4NextStageButton = stage4Objects.transform.Find("Next Stage Button").GetComponent<KMSelectable>();

		stage4NextStageButton.OnInteract += delegate () { if (!audioPlaying) { GoToNextStage(4); } return false; };
		#endregion

		#region stage5
		inMoneyPhase = false;

		stage5Objects = transform.Find("Money Phase").gameObject;

		moneyCanvas = stage5Objects.transform.Find("Canvas").gameObject;

		bankGameObject = moneyCanvas.transform.Find("Bank Button").gameObject;

		bankMoneyAmountTextMesh = bankGameObject.transform.Find("Money Amount").GetComponent<TextMesh>();

		bankButton = bankGameObject.transform.GetComponent<KMSelectable>();

		moneyObjects = new List<Money>()
		{
			new Money(20,   moneyCanvas.transform.Find("20 Image").gameObject,   redBackground),
			new Money(50,   moneyCanvas.transform.Find("50 Image").gameObject,   redBackground),
			new Money(100,  moneyCanvas.transform.Find("100 Image").gameObject,  redBackground),
			new Money(200,  moneyCanvas.transform.Find("200 Image").gameObject,  redBackground),
			new Money(300,  moneyCanvas.transform.Find("300 Image").gameObject,  redBackground),
			new Money(450,  moneyCanvas.transform.Find("450 Image").gameObject,  redBackground),
			new Money(600,  moneyCanvas.transform.Find("600 Image").gameObject,  redBackground),
			new Money(800,  moneyCanvas.transform.Find("800 Image").gameObject,  redBackground),
			new Money(1000, moneyCanvas.transform.Find("1000 Image").gameObject, redBackground),
		};

		currentMoneyIndex = -1;

		GameObject c = stage5Objects.transform.Find("Canvas").gameObject;

		moneyPhaseQuestionText = c.transform.Find("Question").GetComponent<Text>();
		moneyPhaseAnswerText = c.transform.Find("Answer").GetComponent<Text>();

		playerDisplay = c.transform.Find("Player").Find("Player Name").GetComponent<Text>();
		contestantDisplay = c.transform.Find("Contestant").Find("Contestant Name").GetComponent<Text>();

		moneyPhaseTimerText = c.transform.Find("Timer").transform.Find("Text").GetComponent<Text>();

		bankMoneyAmountTextMesh = bankGameObject.transform.Find("Money Amount").GetComponent<TextMesh>();
		bankButton.OnInteract += delegate () { BankButtonPressed(); return false; };

		#endregion

		#region Stage 6
		inFaceOffPhase = false;
		correctAnswers = 0;
		quesetionsAsked = 0;

		stage6Objects = transform.Find("Face Off Phase").gameObject;

		GameObject stage6Canvas = stage6Objects.transform.Find("Canvas").gameObject;

		stage6QuestionText = stage6Canvas.transform.Find("Question").GetComponent<Text>();
		stage6AnswerText = stage6Canvas.transform.Find("Answer").GetComponent<Text>();

		moduleIndicators = new List<CorrectIndicator>()
		{
			new CorrectIndicator(1, stage6Canvas.transform.Find("First Image").gameObject, redBackground, checkmarkSprite, xSprite),
			new CorrectIndicator(2, stage6Canvas.transform.Find("Second Image").gameObject, redBackground, checkmarkSprite, xSprite),
			new CorrectIndicator(3, stage6Canvas.transform.Find("Third Image").gameObject, redBackground, checkmarkSprite, xSprite),
			new CorrectIndicator(4, stage6Canvas.transform.Find("Fourth Image").gameObject, redBackground, checkmarkSprite, xSprite),
			new CorrectIndicator(5, stage6Canvas.transform.Find("Fifth Image").gameObject, redBackground, checkmarkSprite, xSprite),
		};
		#endregion

		//create player
		int playerFont = Rnd.Range(0, handWritingFonts.Count);
		playerContestant = new Contestant("", GetPlayerSkill(), null, handWritingMaterials[playerFont], handWritingFonts[playerFont], null, null, false);

		//make sure the right game objects are visible
		GoToNextStage(0);

		Logging($"First contestant is {c1.Name} who specializese in {c1.Category}");
		Logging($"Second contestant is {c2.Name} who specializese in {c2.Category}");
		Logging($"You specialize in {playerContestant.Category}");
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
				stage5Objects.SetActive(false);
				stage6Objects.SetActive(false);
				break;
			case 1:
				stage1Objects.SetActive(false);
				stage2Objects.SetActive(true);
				stage3Objects.SetActive(false);
				stage4Objects.SetActive(false);
				stage5Objects.SetActive(false);
				stage6Objects.SetActive(false);

				Logging("===========Question Phase===========");
				break;

			case 2:
				stage1Objects.SetActive(false);
				stage2Objects.SetActive(false);
				stage3Objects.SetActive(true);
				stage4Objects.SetActive(false);
				stage5Objects.SetActive(false);
				stage6Objects.SetActive(false);
				
				eliminationText.font = playerContestant.HandWritingFont;
				eliminationText.text = "";

				contestant1Display.text = c1.Name.ToUpper();
				contestant2Display.text = c2.Name.ToUpper();

				contestant1EliminationText.font = c1.HandWritingFont;
				contestant1EliminationText.text = "";

				contestant2EliminationText.font = c2.HandWritingFont;
				contestant2EliminationText.text = "";

				Logging("===========Elimination Phase===========");
				break;

			case 3:
				stage1Objects.SetActive(false);
				stage2Objects.SetActive(false);
				stage3Objects.SetActive(false);
				stage4Objects.SetActive(true);
				stage5Objects.SetActive(false);
				stage6Objects.SetActive(false);

				break;

			case 4:
				stage1Objects.SetActive(false);
				stage2Objects.SetActive(false);
				stage3Objects.SetActive(false);
				stage4Objects.SetActive(false);
				stage5Objects.SetActive(true);
				stage6Objects.SetActive(false);

				moneyStored = 0;
				BreakMoneyChain();
				Logging("===========Money Phase===========");
				UpdateQuestion(true, 5);
				UpdateTurn(true, 5);
				break;

			case 5:
				stage1Objects.SetActive(false);
				stage2Objects.SetActive(false);
				stage3Objects.SetActive(false);
				stage4Objects.SetActive(false);
				stage5Objects.SetActive(false);
				stage6Objects.SetActive(true);

				inFaceOffPhase = true;

				foreach (CorrectIndicator i in moduleIndicators)
				{
					i.SetUnused();
				}

				correctAnswers = 0;
				quesetionsAsked = 0;


				UpdateQuestion(true, 6);

				Logging("===========Face Off Phase===========");
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

			questionPhaseQuestionText.font = GetQuestionFont();

			questionPhaseQuestionText.color = Color.white;

			questionPhaseQuestionText.text = currentTrivia.Question;

			questionPhaseAnswerText.text = "";
		}

		else if (stage == 5)
		{
			if (init)
			{
				currentTime = moneyPhaseTimerMax;
				inMoneyPhase = true;
				categoryCurrentIndex = 0;
				categoryList = GetCategoryList();
				Logging("Category list: " + string.Join(", ", categoryList.Select(x => x.ToString()).ToArray()));
			}

			else
			{
				categoryCurrentIndex = (categoryCurrentIndex + 1) % categoryList.Count;
			}

			currentTrivia = GetQuestion(categoryList[categoryCurrentIndex]);

			moneyPhaseQuestionText.color = Color.white;

			moneyPhaseQuestionText.text = currentTrivia.Question;

			moneyPhaseQuestionText.font = GetQuestionFont();

			moneyPhaseAnswerText.text = "";

		}

		else
		{
			currentTrivia = GetQuestion();

			stage6QuestionText.color = Color.white;

			stage6QuestionText.text = currentTrivia.Question;

			stage6QuestionText.font = GetQuestionFont();

			stage6AnswerText.text = "";
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

		else if (stage == 5)
		{
			if (init)
			{
				moneyPhaseCurrentTurn = MoneyPhaseTurn.Player;
				aliveConestant = c1.Eliminated ? c2 : c1;

				contestantDisplay.text = aliveConestant.Name.ToUpper();
			}

			else
			{
				moneyPhaseCurrentTurn = moneyPhaseCurrentTurn == MoneyPhaseTurn.Player ? MoneyPhaseTurn.Conestant : MoneyPhaseTurn.Player;
			}
		}

		UpdateNameColors(stage);
	}

	Font GetQuestionFont()
	{ 
		return Rnd.Range(0, 100) > 0 ? questionFonts[1] : questionFonts[0];
	}

	void UpdateNameColors(int stage)
	{
		if (stage == 2)
		{
			questionPhasePlayerText.color = questionPhaseContestant1Text.color = questionPhaseContestant2Text.color = inactiveColor;

			switch (questionPhaseCurrentTurn)
			{
				case QuestionPhaseTurn.Player:
					questionPhasePlayerText.color = Color.white;
					questionPhaseAnswerText.font = playerContestant.HandWritingFont;
					break;
				case QuestionPhaseTurn.C1:
					questionPhaseContestant1Text.color = Color.white;
					questionPhaseAnswerText.font = c1.HandWritingFont;

					break;
				case QuestionPhaseTurn.C2:
					questionPhaseContestant2Text.color = Color.white;
					questionPhaseAnswerText.font = c2.HandWritingFont;
					break;
			}
		}

		else if (stage == 5)
		{
			playerDisplay.color = contestantDisplay.color = inactiveColor;

			bool playerTurn = moneyPhaseCurrentTurn == MoneyPhaseTurn.Player;

			if (playerTurn)
			{
				playerDisplay.color = Color.white;
				moneyPhaseAnswerText.font = playerContestant.HandWritingFont;
			}

			else
			{
				contestantDisplay.color = Color.white;
				moneyPhaseAnswerText.font = aliveConestant.HandWritingFont;
			}
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
				yield return new WaitForSeconds(CalculateReadingTime());

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

				if (personToEliminate == c1) //first person picks player
				{
					string input = Rnd.Range(0, 2) == 0 ? "PLAYER" : c2.Name.ToUpper();

					foreach (char ch in input)
					{
						contestant1EliminationText.text += "" + ch;
						yield return new WaitForSeconds(0.1f);
					}

					yield return new WaitForSeconds(1f);

					foreach (char ch in c1.Name.ToUpper())
					{
						contestant2EliminationText.text += "" + ch;
						yield return new WaitForSeconds(0.1f);
					}

					yield return new WaitForSeconds(1f);
				}

				else
				{
					string input = Rnd.Range(0, 2) == 0 ? "PLAYER" : c1.Name.ToUpper();

					foreach (char ch in c2.Name.ToUpper())
					{
						contestant1EliminationText.text += "" + ch;
						yield return new WaitForSeconds(0.1f);
					}

					yield return new WaitForSeconds(1f);

					foreach (char ch in input)
					{
						contestant2EliminationText.text += "" + ch;
						yield return new WaitForSeconds(0.1f);
					}

					yield return new WaitForSeconds(1f);
				}

				inEliminationPhase = false;
				personToEliminate.Eliminated = true;
				log = $"You entered \"{eliminationText.text}\". Which is correct.";
				GoToNextStage(3);
			}

			else
			{
				foreach (char ch in "PLAYER")
				{
					contestant1EliminationText.text += "" + ch;
					yield return new WaitForSeconds(0.1f);
				}

				yield return new WaitForSeconds(1f);

				foreach (char ch in "PLAYER")
				{
					contestant2EliminationText.text += "" + ch;
					yield return new WaitForSeconds(0.1f);
				}

				yield return new WaitForSeconds(1f);

				log = $"Strike! You entered \"{eliminationText.text}\".";
				StartCoroutine(Strike(3));
				
			}

			Logging(log);
		}

		else if (stage == 5 && inMoneyPhase)
		{
			bool turnChanged = false;

			string response = moneyPhaseAnswerText.text;

			string[] answers = currentTrivia.AcceptedAnswers.Select(x => x.ToUpper()).ToArray();

			Contestant currentContestant = moneyPhaseCurrentTurn == MoneyPhaseTurn.Player ? playerContestant : aliveConestant;

			if (moneyPhaseCurrentTurn == MoneyPhaseTurn.Player)
			{
				UpdateTurn(false, 5);
				turnChanged = true;
			}

			string log = $"Question: \"{currentTrivia.Question}\". {(currentContestant.Name == "" ? "You" : currentContestant.Name)} answered \"{response}\", ";

			bool correct = answers.Contains(response);
			if (correct)
			{
				moneyPhaseQuestionText.color = correctColor;
				log += "which is correct";
			}

			else
			{
				moneyPhaseQuestionText.color = incorrectColor;
				log += "which is incorrect";
			}

			if (!turnChanged)
			{
				log += $". This is there {(aliveConestant.WrongNum == 1 ? "1st" : aliveConestant.WrongNum == 2 ? "2nd" : "3rd")} wrong question";
			}

			Logging(log);

			UpdateMoney(correct);

			moneyPhaseAnswerText.text = "";
			yield return new WaitForSeconds(2f);

			if (!inMoneyPhase || currentTime <= 0)
				yield break;

			UpdateQuestion(false, 5);

			if (!turnChanged)
			{
				UpdateTurn(false, 5);
			}

			if (moneyPhaseCurrentTurn != MoneyPhaseTurn.Player)
			{
				yield return new WaitForSeconds(CalculateReadingTime());

				float percentage = currentTrivia.Category == aliveConestant.Category ? Contestant.GOOD_RIGHT_CHOICE : Contestant.REGULAR_RIGHT_CHOICE;

				bool correctAnswer;

				if (aliveConestant.WrongNum < Contestant.MAX_WRONG)
				{
					correctAnswer = Rnd.Range(0f, 1f) < percentage;
				}

				else
				{
					correctAnswer = true;
				}

				if (correct)
				{
					aliveConestant.WrongNum++;
				}


				string input = correctAnswer ? currentTrivia.AcceptedAnswers[Rnd.Range(0, currentTrivia.AcceptedAnswers.Count)].ToUpper() :
											   currentTrivia.WrongAnswers[Rnd.Range(0, currentTrivia.WrongAnswers.Count)].ToUpper();
				foreach (char ch in input)
				{
					if (!inMoneyPhase || currentTime <= 0)
						yield break;

					moneyPhaseAnswerText.text += "" + ch;
					yield return new WaitForSeconds(0.1f);
				}

				yield return new WaitForSeconds(1f);

				if (!inMoneyPhase || currentTime <= 0)
					yield break;

				StartCoroutine(Submit(5));
			}
		}

		else if (stage == 6)
		{
			string response = stage6AnswerText.text;

			string[] answers = currentTrivia.AcceptedAnswers.Select(x => x.ToUpper()).ToArray();

			string log = $"Question: \"{currentTrivia.Question}\". You answered \"{response}\", ";

			quesetionsAsked++;

			CorrectIndicator indicator = moduleIndicators[quesetionsAsked - 1];


			bool correct = answers.Contains(response);

			if (correct)
			{
				correctAnswers++;
				log += "which is correct";

			}

			else
			{
				log += "which is incorrect";
			}

			indicator.SetUsed(correct);

			log += $". Answered {correctAnswers}/{quesetionsAsked}";

			Logging(log);

			if (correctAnswers == 3)
			{
				Logging("You have answered 3 question. Solving module...");
				Solve();
			}

			else if ((correctAnswers == 0 && quesetionsAsked == 3) || (correctAnswers == 1 && quesetionsAsked == 4) || quesetionsAsked == 5)
			{
				Logging($"Strike! You have correctly answered {correctAnswers} out of {correctAnswers} questions. Unable to get 3/5.");
				StartCoroutine(Strike(6));
			}

			else
			{ 
				UpdateQuestion(false, 6);
			}
		}
	}

	void UpdateMoney(bool correctAnswer)
	{
		string log;

		if (correctAnswer)
		{
			currentMoneyIndex++;

			Money moneyObject = moneyObjects[currentMoneyIndex];

			moneyObject.ToggleColor(true);

			int money = moneyObject.MoneyAmount;

			log = $"Streak is now at {money}"; 

			if (money == 1000)
			{
				EndMoneyPhase(true, "");
			}
		}

		else
		{
			log = $"Resetting streak to £0";
			BreakMoneyChain();
		}


		Logging(log);
	}


	Trivia GetQuestion()
	{
		List<Trivia> a = jsonData.TriviaList.Where(x => !x.Asked).ToList();


		if (a.Count == 0)
		{
			ResetQuestions();
			a = new List<Trivia>();
			jsonData.TriviaList.ForEach(x => a.Add(x));
		}

		Trivia t = a[Rnd.Range(0, a.Count)];
		t.Asked = true;
		return t;
	}

	Trivia GetQuestion(Category category)
	{
		List<Trivia> a = jsonData.TriviaList.Where(s => s.Category == category && !s.Asked).ToList();

		if (a.Count == 0)
		{
			ResetQuestions(category);
			a = jsonData.TriviaList.Where(s => s.Category == category).ToList();
		}

		Trivia t = a[Rnd.Range(0, a.Count)];
		t.Asked = true;
		return t;
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

	int GetLongestQuestionLength()
	{
		return jsonData.TriviaList.OrderByDescending(x => x.Question.Length).First().Question.Length;
	}

	float CalculateReadingTime()
	{
		//the longest question should take 7 seconds to read. Make other questions reading time relative to this

		return (TIME_READ * currentTrivia.Question.Length / longestQuestionLength) + 2;
	}

	void GetNewContestants(bool updatePlayer)
	{
		int categoryCount = 7;
		int nameCount = jsonData.ContestantNames.Count;

		int randomFont = Rnd.Range(0, handWritingMaterials.Count);
		int randomFont2 = Rnd.Range(0, handWritingMaterials.Count);
		int randomFont3 = Rnd.Range(0, handWritingMaterials.Count);


		c1 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant1GameObject, handWritingMaterials[randomFont], handWritingFonts[randomFont], nameDisplayMaterial, nameDisplayFont, true);

		c2 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant2GameObject, handWritingMaterials[randomFont2], handWritingFonts[randomFont2], nameDisplayMaterial, nameDisplayFont, true);

		if (updatePlayer)
		{
			playerContestant = new Contestant("", GetPlayerSkill(), null, handWritingMaterials[randomFont3], handWritingFonts[randomFont3], null, null, false);
		}
	}

	public void ResetQuestions()
	{
		jsonData.TriviaList.ForEach(x => x.Asked = false);
	}

	public void ResetQuestions(Category c)
	{
		jsonData.TriviaList.Where(x => x.Category == c).ToList().ForEach(x => x.Asked = false);
	}

	void Logging(string s)
	{
		LogFormat($"[The Weakest Link #{ModuleId}] {s}");
	}

	IEnumerator Strike(int stage)
	{
		//you are the weakest link, goodbye
		AudioClip goodbye = goodbyeAudioList[Rnd.Range(0, goodbyeAudioList.Count)];

		audioPlaying = true;
		Audio.PlaySoundAtTransform(goodbye.name, transform);
		yield return new WaitForSeconds(goodbye.length + 1);
		audioPlaying = false;


		GetComponent<KMBombModule>().HandleStrike();


		switch (stage)
		{
			case 2: //question phase
			case 3: //elimination phase
				GetNewContestants(true);
				GoToNextStage(0);
				break;

			case 6: //face off phase
				GoToNextStage(5);
				break;
		}

	}

	void Solve()
	{
		GetComponent<KMBombModule>().HandlePass();
		ModuleSolved = true;
	}
	void BankButtonPressed()
	{
		if (moneyPhaseCurrentTurn == MoneyPhaseTurn.Player && currentMoneyIndex != -1)
		{
			int moneyAdd = moneyObjects[currentMoneyIndex].MoneyAmount;
			moneyStored += moneyAdd;
			bankMoneyAmountTextMesh.text = $"£{moneyStored}";

			Logging($"You banked £{moneyAdd} leaving you with a total of {bankMoneyAmountTextMesh.text}");

			BreakMoneyChain();

			if (moneyStored >= 1000)
			{
				EndMoneyPhase(true, $"");
			}
		}
	}

	void EndMoneyPhase(bool passedPhase, string log)
	{
		inMoneyPhase = false;

		if (log != "")
		{ 
			Logging(log);
		}

		if (passedPhase)
		{
			GoToNextStage(5);
		}

		else
		{
			Strike(5);
		}
	}

	void BreakMoneyChain()
	{
		currentMoneyIndex = -1;

		bankMoneyAmountTextMesh.text = "£0";

		foreach (Money m in moneyObjects)
		{
			m.ToggleColor(false);
		}
	}

	void GetKeyboardInput(int stage)
	{
		string currentText = stage == 2 ? questionPhaseAnswerText.text : stage == 3 ? eliminationText.text : stage == 5 ? moneyPhaseAnswerText.text : stage6AnswerText.text;

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

					else if (stage == 5)
					{
						moneyPhaseAnswerText.text = newText;
					}

					else
					{
						stage6AnswerText.text = newText;
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
					eliminationText.text += newText;
				}

				else if (stage == 5)
				{ 
					moneyPhaseAnswerText.text += newText;
				}

				else
                {
					stage6AnswerText.text += newText;
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

				else if (stage == 5)
				{
					moneyPhaseAnswerText.text += " ";
				}

				else
				{ 
					stage6AnswerText.text += " ";
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

				else if (stage == 5)
				{
					moneyPhaseAnswerText.text += "-";
				}

				else
				{
					stage6AnswerText.text += "-";
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

				else if (stage == 5)
				{ 
					moneyPhaseAnswerText.text += newString;
				}

				else
				{
					stage6AnswerText.text += newString;
				}
			}
		}
	}
}
