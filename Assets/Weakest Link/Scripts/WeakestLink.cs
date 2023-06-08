using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KModkit;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Debug;
using Rnd = UnityEngine.Random;

public class WeakestLink : MonoBehaviour 
{

	//todo get rid of uncessary code
	//todo get rid of uncessary files


	const string url = "https://ktane-mods.github.io/Weakest-Link-Data/data.json";

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
	AudioClip musicClip1;

	[SerializeField]
	AudioClip musicClip2;

	[SerializeField]
	List<AudioClip> playAudioList;
	
	[SerializeField]
	List<AudioClip> goodbyeAudioList;
	
	[SerializeField]
	List<AudioClip> startClockAudioList;

	[SerializeField]
	List<AudioClip> friendPlayAudioList;

	[SerializeField]
	List<AudioClip> friendStartClockAudioList;


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

	QuestionPhaseTurn questionPhaseCurrentTurn = QuestionPhaseTurn.Player;

	JsonReader jsonData;


	Contestant[] contestants;
	Contestant c1;
	Contestant c2;
	Contestant playerContestant;


	string day; // the day of the week

	bool focused; //if the module is selected

	int longestQuestionLength;

	const int TIME_READ = 5; //the amount of time it would take the contestants to read the longest question 
	float currentTime;

	private readonly Color correctColor = new Color(.38f, .97f, .43f); //the color the qustion will be if the answer is correct
	private readonly Color incorrectColor = new Color(1, 0, 0); //the color the qustion will be if the answer is wrong
	private readonly Color mainTextLit = new Color(0.82f, 0.82f, 0.82f);
	private readonly Color mainTextUnlit = new Color(0.55f, 0.55f, 0.55f);
	private readonly Color shadowTextLit = new Color(0.71f, 0.71f, 0.71f);
	private readonly Color shadowTextUnlit = new Color(0.34f, 0.34f, 0.34f);

	Trivia currentTrivia;

	bool colorBlindOn;

	#endregion

	#region Stage 1 Objects 
	GameObject stage1Objects;

	GameObject contestant1GameObject;
	GameObject contestant2GameObject;

	KMSelectable stage1NextStageButton;
	#endregion

	#region Stage 2 Objects
	GameObject stage2Objects;

	const int stage2TimerMax = 120; // the amount of time the user has to answers qustions in the first stage 

	bool inQuestionPhase;

	TextMesh stage2TimerText;
	Text stage2QuestionText;
	Text stage2AnswerText;

	NameDisplay[] stage2NameDisplays;
	
	Text stage2ColorBlindText;
	#endregion

	#region Stage 3

	GameObject stage3Objects;

	NameDisplay contestant1Display;

	NameDisplay contestant1EliminationText;

	NameDisplay contestant2Display;

	NameDisplay contestant2EliminationText;

	NameDisplay eliminationText;

	Contestant personToEliminate;
	bool inEliminationPhase;

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

	#region Stage 4
	GameObject stage4Objects;

	KMSelectable stage4NextStageButton;

	#endregion

	#region Stage 5
	GameObject stage5Objects;

	NameDisplay[] stage5NameDisplays;

	GameObject moneyCanvas;
	List<Money> moneyObjects;
	int currentMoneyIndex;
	GameObject bankGameObject;
	KMSelectable bankButton;
	TextMesh bankMoneyAmountTextMesh;
	bool inMoneyPhase;

	MoneyPhaseTurn moneyPhaseCurrentTurn;

	Text stage5QuestionText;
	Text stage5AnswerText;

	const int moneyPhaseTimerMax = 180; //the starting time for the money phase

	List<Category> categoryList;
	int categoryCurrentIndex;

	Contestant aliveConestant;

	TextMesh stage5TimerText;

	int moneyStored;

	Text stage5ColorBlindText;
	#endregion


	#region Stage 6
	GameObject stage6Canvas;

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

	#region Stage 7
	GameObject stage7Objects;
    #endregion

    Text failText;

	List<GameObject> stageObjectsList;

	IEnumerator Start() {
		ModuleId = ModuleIdCounter++;

		//hide other stages
		GetGameCoponents();

		ShowSpecifcStage(1);

		//get json data
		jsonData = gameObject.GetComponent<JsonReader>();

		yield return jsonData.LoadData(url);

		SetUpModule();
	}

	void Update() {
		if (!ModuleSolved && !audioPlaying)
		{
			if (inQuestionPhase)
			{
				currentTime -= Time.deltaTime;
				stage2TimerText.text = string.Format("{0:0}:{1:00}", (int)(currentTime / 60), (int)currentTime % 60);

				if (currentTime <= 0f)
				{
					inQuestionPhase = false;

					Logging($"You: {playerContestant.CorrectAnswer}/{playerContestant.QuestionsAsked}");
					Logging($"{c1.Name}: {c1.CorrectAnswer}/{c1.QuestionsAsked}");
					Logging($"{c2.Name}: {c2.CorrectAnswer}/{c2.QuestionsAsked}");

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

				else if (focused && questionPhaseCurrentTurn == QuestionPhaseTurn.Player)
				{
					GetKeyboardInput(2);
				}
			}

			else if (inMoneyPhase)
			{
				currentTime -= Time.deltaTime;
				stage5TimerText.text = string.Format("{0:0}:{1:00}", (int)(currentTime / 60), (int)currentTime % 60);

				if (currentTime <= 0f)
				{
					EndMoneyPhase(false, $"Strike! Time ran out and you only banked {bankMoneyAmountTextMesh.text}");
				}

				else if (focused && moneyPhaseCurrentTurn == MoneyPhaseTurn.Player)
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

	IEnumerator StageButton(int stage)
	{
		bool friend = Rnd.Range(0, 9) == 0;

		AudioClip playClip;
		int friendIndex;
		AudioClip clockClip;

		if (friend)
		{
			friendIndex = Rnd.Range(0, friendPlayAudioList.Count);
			playClip = friendPlayAudioList[friendIndex];
			clockClip = friendStartClockAudioList[friendIndex];
		}

		else
		{ 
			playClip = playAudioList[Rnd.Range(0, playAudioList.Count)];
			clockClip = startClockAudioList[Rnd.Range(0, startClockAudioList.Count)];
		}


		//let's play the weakest link
		audioPlaying = true;
		Audio.PlaySoundAtTransform(playClip.name, transform);
		yield return new WaitForSeconds(playClip.length);

		//do do do do
		Audio.PlaySoundAtTransform(musicClip1.name, transform);
		yield return new WaitForSeconds(musicClip1.length);

		//start the clock
		Audio.PlaySoundAtTransform(clockClip.name, transform);
		yield return new WaitForSeconds(clockClip.length);

		//dun dun
		Audio.PlaySoundAtTransform(musicClip2.name, transform);
		yield return new WaitForSeconds(musicClip2.length);

		audioPlaying = false;

		if (stage == 1)
		{
			GoToNextStage(1);
			UpdateTurn(true, 2);
			UpdateQuestion(true, 2);
		}


		else
		{
			moneyStored = 0;
			BreakMoneyChain();
			GoToNextStage(4);
		}
	}

	//gets all object/coponents for the module
	void GetGameCoponents()
	{
		colorBlindOn = GetComponent<KMColorblindMode>().ColorblindModeActive;

		#region Stage 1
		stage1Objects = transform.Find("Skill Check Phase").gameObject;
		GameObject stage1Canvas = stage1Objects.transform.Find("Canvas").gameObject;
		contestant1GameObject = stage1Canvas.transform.Find("Contestant 1").gameObject;
		contestant2GameObject = stage1Canvas.transform.Find("Contestant 2").gameObject;
		stage1NextStageButton = stage1Objects.transform.Find("Next Stage Button").GetComponent<KMSelectable>();

		failText = stage1Canvas.transform.Find("Fail Text").GetComponent<Text>();
		#endregion

		#region stage2
		stage2Objects = transform.Find("Question Phase").gameObject;

		stage2NameDisplays = new NameDisplay[] { stage2Objects.transform.Find("Player").GetComponent<NameDisplay>(),
												 stage2Objects.transform.Find("Contestant 1").GetComponent<NameDisplay>(),
												 stage2Objects.transform.Find("Contestant 2").GetComponent<NameDisplay>()};

		stage2NameDisplays.ToList().ForEach(g => { g.InitializeVariables(); g.SetLit(true); });

		stage2TimerText = stage2Objects.transform.Find("TimeBox").transform.Find("Time Text").GetComponent<TextMesh>();

		GameObject canvas = stage2Objects.transform.Find("Canvas").gameObject;
		stage2QuestionText = canvas.transform.Find("Question").GetComponent<Text>();
		stage2AnswerText = canvas.transform.Find("Answer").GetComponent<Text>();

		stage2ColorBlindText = canvas.transform.Find("Color Blind Text").transform.GetComponent<Text>();
		#endregion

		#region stage3

		stage3Objects = transform.Find("Elimination Phase").gameObject;

		contestant1Display = stage3Objects.transform.Find("Contestant 1 Name").GetComponent<NameDisplay>();
		contestant1Display.InitializeVariables();
		contestant1Display.SetLit(true);
		contestant1EliminationText = stage3Objects.transform.Find("Contestant 1 Elimination").GetComponent<NameDisplay>();
		contestant1EliminationText.InitializeVariables();
		contestant1EliminationText.SetLit(true);
		contestant2Display = stage3Objects.transform.Find("Contestant 2 Name").GetComponent<NameDisplay>();
		contestant2Display.InitializeVariables();
		contestant2Display.SetLit(true);
		contestant2EliminationText = stage3Objects.transform.Find("Contestant 2 Elimination").GetComponent<NameDisplay>();
		contestant2EliminationText.InitializeVariables();
		contestant2EliminationText.SetLit(true);
		eliminationText = stage3Objects.transform.Find("Player Elimination").GetComponent<NameDisplay>();
		eliminationText.InitializeVariables();
		eliminationText.SetLit(true);
		#endregion

		#region stage 4

		stage4Objects = transform.Find("Intermission Phase").gameObject;

		stage4NextStageButton = stage4Objects.transform.Find("Next Stage Button").GetComponent<KMSelectable>();
		#endregion

		#region Stage 5
		stage5Objects = transform.Find("Money Phase").gameObject;

		moneyCanvas = stage5Objects.transform.Find("Canvas").gameObject;

		bankGameObject = stage5Objects.transform.Find("Bank Button").gameObject;

		bankMoneyAmountTextMesh = bankGameObject.transform.Find("Money Amount").GetComponent<TextMesh>();

		bankButton = bankGameObject.transform.GetComponent<KMSelectable>();

		bankMoneyAmountTextMesh = bankGameObject.transform.Find("Money Amount").GetComponent<TextMesh>();


		stage5NameDisplays = new NameDisplay[] { stage5Objects.transform.Find("Player").GetComponent<NameDisplay>(),
												 stage5Objects.transform.Find("Contestant").GetComponent<NameDisplay>()};

		stage5NameDisplays.ToList().ForEach(g => g.InitializeVariables());
		stage5NameDisplays[0].Text = "PLAYER";

		stage5TimerText = stage5Objects.transform.Find("TimeBox").transform.Find("Time Text").GetComponent<TextMesh>();

		GameObject c = stage5Objects.transform.Find("Canvas").gameObject;
		stage5QuestionText = c.transform.Find("Question").GetComponent<Text>();
		stage5AnswerText = c.transform.Find("Answer").GetComponent<Text>();
		stage5ColorBlindText = c.transform.Find("Color Blind Text").transform.GetComponent<Text>();
		#endregion

		#region Stage 6
		stage6Objects = transform.Find("Face Off Phase").gameObject;

		stage6Canvas = stage6Objects.transform.Find("Canvas").gameObject;

		stage6QuestionText = stage6Canvas.transform.Find("Question").GetComponent<Text>();
		stage6AnswerText = stage6Canvas.transform.Find("Answer").GetComponent<Text>();
		#endregion

		#region Stage 7
		stage7Objects = transform.Find("Solve Phase").gameObject;
		#endregion

		stageObjectsList = new List<GameObject>() { stage1Objects, stage2Objects, stage3Objects, stage4Objects, stage5Objects, stage6Objects, stage7Objects };
	}

	void ShowSpecifcStage(int stage)
	{
		stageObjectsList.ForEach(s => s.SetActive(false));
		stageObjectsList[stage - 1].SetActive(true);
	}

    void SetUpModule()
	{
		if (!jsonData.Success)
		{
			Logging("Unable to load data, press the button to solve the module");

			//set button to solve module
			stage1NextStageButton.OnInteract += delegate () { stage1NextStageButton.AddInteractionPunch(1f); Solve(); return false; };

			//set other text as blank
			contestant1GameObject.SetActive(false);
			contestant2GameObject.SetActive(false);

			return;
		}

		failText.text = "";

		audioPlaying = false;
		GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; };
		GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };

		day = DateTime.Now.DayOfWeek.ToString().ToUpper();

		do
		{
			GetNewContestants(false);
		} while (c1.Name == c2.Name);

		longestQuestionLength = GetLongestQuestionLength();

		#region stage1
		stage1NextStageButton.OnInteract += delegate () { if (!audioPlaying) { stage1NextStageButton.AddInteractionPunch(1f); StartCoroutine(StageButton(1)); }; return false; };
		#endregion

		#region stage2
		inQuestionPhase = false;

		stage2NameDisplays[0].Text = "PLAYER";
		stage2NameDisplays[1].Text = c1.Name.ToUpper();
		stage2NameDisplays[2].Text = c2.Name.ToUpper();
		#endregion

		#region stage3
		inEliminationPhase = false;
		#endregion

		#region stage 4
		stage4NextStageButton.OnInteract += delegate () { if (!audioPlaying) { stage4NextStageButton.AddInteractionPunch(1f); StartCoroutine(StageButton(4)); } return false; };
		#endregion

		#region stage5
		inMoneyPhase = false;

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

		bankButton.OnInteract += delegate () { bankButton.AddInteractionPunch(1f); BankButtonPressed(); return false; };

		#endregion

		#region Stage 6
		inFaceOffPhase = false;
		correctAnswers = 0;
		quesetionsAsked = 0;

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

		contestants = new Contestant[] { playerContestant, c1, c2 };

		//make sure the right game objects are visible


		GoToNextStage(0);

		Logging($"First contestant is {c1.Name} who specializes in {c1.Category}");
		Logging($"Second contestant is {c2.Name} who specializes in {c2.Category}");
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
			case 1:
				stage2ColorBlindText.text = "";

				Logging("Question Phase");
				break;

			case 2:
				eliminationText.SetFont(playerContestant.HandWritingFont, playerContestant.HandWritingMaterial);
				eliminationText.Text = "";

				contestant1Display.SetFont(c1.NameDisplayFont, c1.NameDisplayMaterial);
				contestant1Display.Text = c1.Name.ToUpper();

				contestant1EliminationText.SetFont(c1.HandWritingFont, c1.HandWritingMaterial);
				contestant1EliminationText.Text = "";

				contestant2Display.SetFont(c2.NameDisplayFont, c2.NameDisplayMaterial);
				contestant2Display.Text = c2.Name.ToUpper();

				contestant2EliminationText.SetFont(c2.HandWritingFont, c2.HandWritingMaterial);
				contestant2EliminationText.Text = "";

				Logging("Elimination Phase");
				break;

			case 4:
				Logging("Money Phase");
				UpdateQuestion(true, 5);
				UpdateTurn(true, 5);
				break;

			case 5:
				inFaceOffPhase = true;

				foreach (CorrectIndicator i in moduleIndicators)
				{
					i.SetUnused();
				}

				correctAnswers = 0;
				quesetionsAsked = 0;

				UpdateQuestion(true, 6);

				Logging("Face Off Phase");
				break;
		}

		ShowSpecifcStage(currentStage + 1);
	}

	void UpdateQuestion(bool init, int stage)
	{
		if (stage == 2)
		{
			if (init)
			{
				currentTime = stage2TimerMax;
				inQuestionPhase = true;
			}

			currentTrivia = GetQuestion();

			stage2QuestionText.font = GetQuestionFont();

			stage2QuestionText.color = Color.white;

			stage2QuestionText.text = currentTrivia.Question;

			stage2AnswerText.text = "";
			stage2ColorBlindText.text = "";


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

			stage5QuestionText.color = Color.white;

			stage5QuestionText.text = currentTrivia.Question;

			stage5QuestionText.font = GetQuestionFont();

			stage5AnswerText.text = "";
			
			stage5ColorBlindText.text = "";
		}

		else
		{
			currentTrivia = GetQuestion();

			stage6QuestionText.color = Color.white;

			stage6QuestionText.text = currentTrivia.Question;

			stage6QuestionText.font = GetQuestionFont();

			stage6AnswerText.font = playerContestant.HandWritingFont;

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



				stage5NameDisplays[1].Text = aliveConestant.Name.ToUpper();
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
			stage2NameDisplays.ToList().ForEach(g => g.GetComponent<NameDisplay>().SetLit(false));
			stage2NameDisplays[(int)questionPhaseCurrentTurn].GetComponent<NameDisplay>().SetLit(true);
			stage2AnswerText.font = contestants[(int)questionPhaseCurrentTurn].HandWritingFont;
		}

		else if (stage == 5)
		{
			stage5NameDisplays.ToList().ForEach(g => g.SetLit(false));
			stage5NameDisplays[(int)moneyPhaseCurrentTurn].GetComponent<NameDisplay>().SetLit(true);
			stage5AnswerText.font = contestants[(int)moneyPhaseCurrentTurn].HandWritingFont;
		}
	}

	//returns true if the player got enough questions right
	bool CalculatePersonToEliminate()
	{
		//Not answering at least 5 questions or not answering over 50% correctly will lead to a strike

		bool lessThanThresholdAsked = playerContestant.QuestionsAsked < 5;
		bool lessThanThresholdCorrect = (float)playerContestant.CorrectAnswer / playerContestant.QuestionsAsked < .5f;

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
		string serialNumber = Bomb.GetSerialNumber().ToUpper();

		string convertedSerial = new string(serialNumber.ToList().Select(c => char.IsDigit(c) ? numberToLettter[c] : c).ToArray());

		if (contestant == c1)
		{
			Logging("New Serial Number: " + convertedSerial);
		}
		
		Logging($"{contestant.Name}'s base elimination value: {baseContestantValue}");


		foreach (char c in contestant.Name.ToUpper())
		{
			if (convertedSerial.Contains(c))
			{
				baseContestantValue++;
				Logging($"Serial Number contains a {c}. Eimination value is now {baseContestantValue}");
			}
		}

		Logging($"{contestant.Name}'s elimination value: {baseContestantValue}");

		return baseContestantValue;
	}

	IEnumerator Submit(int stage)
	{
		if (stage == 2)
		{
			bool turnChanged = false;

			string response = stage2AnswerText.text;

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
				if(colorBlindOn)
                {
					stage2ColorBlindText.text = "Correct";
                }

				stage2QuestionText.color = correctColor;
				currentContestant.CorrectAnswer++;
				log += "which is correct";
			}

			else
			{
				if (colorBlindOn)
				{
					stage2ColorBlindText.text = "Incorrect";
				}

				stage2QuestionText.color = incorrectColor;
				log += "which is incorrect";
			}


			log += $". Ratio is now {currentContestant.CorrectAnswer}/{currentContestant.QuestionsAsked}";

			Logging(log);

			stage2AnswerText.text = "";
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

					stage2AnswerText.text += "" + ch;
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
			inEliminationPhase = false;
			string log;

			if (eliminationText.Text == personToEliminate.Name.ToUpper())
			{

				if (personToEliminate == c1) //first person picks player
				{
					string input = Rnd.Range(0, 2) == 0 ? "PLAYER" : c2.Name.ToUpper();

					foreach (char ch in input)
					{
						contestant1EliminationText.Text += "" + ch;
						yield return new WaitForSeconds(0.1f);
					}

					yield return new WaitForSeconds(1f);

					foreach (char ch in c1.Name.ToUpper())
					{
						contestant2EliminationText.Text += "" + ch;
						yield return new WaitForSeconds(0.1f);
					}

					yield return new WaitForSeconds(1f);
				}

				else
				{
					string input = Rnd.Range(0, 2) == 0 ? "PLAYER" : c1.Name.ToUpper();

					foreach (char ch in c2.Name.ToUpper())
					{
						contestant1EliminationText.Text += "" + ch;
						yield return new WaitForSeconds(0.1f);
					}

					yield return new WaitForSeconds(1f);

					foreach (char ch in input)
					{
						contestant2EliminationText.Text += "" + ch;
						yield return new WaitForSeconds(0.1f);
					}

					yield return new WaitForSeconds(1f);
				}

				inEliminationPhase = false;
				personToEliminate.Eliminated = true;
				log = $"You entered \"{eliminationText.Text}\". Which is correct.";
				GoToNextStage(3);
			}

			else
			{
				foreach (char ch in "PLAYER")
				{
					contestant1EliminationText.Text += "" + ch;
					yield return new WaitForSeconds(0.1f);
				}

				yield return new WaitForSeconds(1f);

				foreach (char ch in "PLAYER")
				{
					contestant2EliminationText.Text += "" + ch;
					yield return new WaitForSeconds(0.1f);
				}

				yield return new WaitForSeconds(1f);

				log = $"Strike! You entered \"{eliminationText.Text}\".";
				StartCoroutine(Strike(3));
				
			}

			Logging(log);
		}

		else if (stage == 5)
		{
			bool turnChanged = false;

			string response = stage5AnswerText.text;

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
				if (colorBlindOn)
				{
					stage5ColorBlindText.text = "Correct";
				}

				stage5QuestionText.color = correctColor;
				log += "which is correct";
			}

			else
			{
				if (colorBlindOn)
				{
					stage5ColorBlindText.text = "Incorrect";
				}

				stage5QuestionText.color = incorrectColor;
				log += "which is incorrect";
			}

			if (!turnChanged && !correct)
			{
				log += $". This is their {(aliveConestant.WrongNum == 1 ? "1st" : aliveConestant.WrongNum == 2 ? "2nd" : "3rd")} wrong question";
			}

			Logging(log);

			UpdateMoney(correct);

			stage5AnswerText.text = "";
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

				if (!correctAnswer)
				{
					aliveConestant.WrongNum++;
				}


				string input = correctAnswer ? currentTrivia.AcceptedAnswers[Rnd.Range(0, currentTrivia.AcceptedAnswers.Count)].ToUpper() :
											   currentTrivia.WrongAnswers[Rnd.Range(0, currentTrivia.WrongAnswers.Count)].ToUpper();
				foreach (char ch in input)
				{
					if (!inMoneyPhase || currentTime <= 0)
						yield break;

					stage5AnswerText.text += "" + ch;
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

			moneyObject.ToggleCorrect(true);

				for (int i = 0; i < currentMoneyIndex; i++)
				{
					moneyObjects[i].ToggleColor(false);
				}

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
		int categoryCount = Enum.GetNames(typeof(Category)).Length;
		int nameCount = jsonData.ContestantNames.Count;

		int randomFont = Rnd.Range(0, handWritingMaterials.Count);
		int randomFont2 = Rnd.Range(0, handWritingMaterials.Count);

		c1 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant1GameObject, handWritingMaterials[randomFont], handWritingFonts[randomFont], nameDisplayMaterial, nameDisplayFont, true);

		c2 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant2GameObject, handWritingMaterials[randomFont2], handWritingFonts[randomFont2], nameDisplayMaterial, nameDisplayFont, true);

		if (updatePlayer)
		{
			int randomFont3 = Rnd.Range(0, handWritingMaterials.Count);
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
			case 5:
				GoToNextStage(4);
				break;
			case 6: //face off phase
				GoToNextStage(5);
				break;
		}
	}

	void Solve()
	{
		GoToNextStage(6);
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
			StartCoroutine(Strike(5));
		}
	}

	void BreakMoneyChain()
	{
		currentMoneyIndex = -1;

		bankMoneyAmountTextMesh.text = "£0";

		foreach (Money m in moneyObjects)
		{
			m.ToggleCorrect(false);
			m.ToggleColor(false);
		}
	}

	void GetKeyboardInput(int stage)
	{
		string currentText = stage == 2 ? stage2AnswerText.text : stage == 3 ? eliminationText.Text : stage == 5 ? stage5AnswerText.text : stage6AnswerText.text;

		foreach (KeyCode keyCode in TypableKeys)
		{
			if (keyCode == KeyCode.Backspace && Input.GetKeyDown(keyCode))
			{
				if (currentText != "")
				{
					string newText = currentText.Substring(0, currentText.Length - 1);

					if (stage == 2)
					{
						stage2AnswerText.text = newText;
					}

					else if (stage == 3)
					{
						eliminationText.Text = newText;
					}

					else if (stage == 5)
					{
						stage5AnswerText.text = newText;
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
					stage2AnswerText.text += newText;
				}
				else if (stage == 3 && eliminationText.Text.Length < 9)
				{
					eliminationText.Text += newText;
				}

				else if (stage == 5)
				{ 
					stage5AnswerText.text += newText;
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
					stage2AnswerText.text += " ";
				}
				else if (stage == 3 && eliminationText.Text.Length < 9)
				{
					eliminationText.Text += " ";
				}

				else if (stage == 5)
				{
					stage5AnswerText.text += " ";
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
					stage2AnswerText.text += "-";
				}
				else if (stage == 3 && eliminationText.Text.Length < 9)
				{
					eliminationText.Text += "-";
				}

				else if (stage == 5)
				{
					stage5AnswerText.text += "-";
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
					stage2AnswerText.text += newString;
				}

				else if (stage == 3 && eliminationText.Text.Length < 9)
				{
					eliminationText.Text += newString;
				}

				else if (stage == 5)
				{ 
					stage5AnswerText.text += newString;
				}

				else
				{
					stage6AnswerText.text += newString;
				}
			}
		}
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"To start question/money phase, use `!{0} start`. To type in an answer/name, use `!{0} [answer]`. To bank, use `!{0} bank`.";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string Command)
	{
		Command = Command.ToUpper();
		yield return null;

		//answering question / eliminating someone
		if (inQuestionPhase || inEliminationPhase || inMoneyPhase || inFaceOffPhase)
		{
			if (inQuestionPhase)
			{
				foreach (char c in Command)
				{
					stage2AnswerText.text += c;
					yield return new WaitForSeconds(0.1f);
				}
				StartCoroutine(Submit(2));
			}

			else if (inEliminationPhase)
			{
				foreach (char c in Command)
				{
					eliminationText.Text += c;
					yield return new WaitForSeconds(0.1f);
				}
				StartCoroutine(Submit(3));
			}

			else if (inMoneyPhase)
			{
				if (Command == "BANK")
				{
					bankButton.OnInteract();
				}

				else
				{
					foreach (char c in Command)
					{
						stage5AnswerText.text += c;
						yield return new WaitForSeconds(0.1f);
					}

					StartCoroutine(Submit(5));
				}
			}

			else if (inFaceOffPhase)
			{
				foreach (char c in Command)
				{
					stage6AnswerText.text += c;
					yield return new WaitForSeconds(0.1f);
				}
				StartCoroutine(Submit(6));
			}
		}

		else if (Command == "START")
		{
			if (stage1Objects.activeInHierarchy)
			{
				stage1NextStageButton.OnInteract();
			}

			else
			{
				stage4NextStageButton.OnInteract();
			}
		}
	}
}
