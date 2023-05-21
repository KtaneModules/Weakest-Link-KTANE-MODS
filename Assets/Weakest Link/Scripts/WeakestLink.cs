using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using static UnityEngine.Debug;


public class WeakestLink : MonoBehaviour {

	static int ModuleIdCounter = 1;
	int ModuleId;
	private bool ModuleSolved;

	public KMBombInfo Bomb;
	public KMAudio Audio;

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
	#endregion



	void SetUpModule()
	{
		//get json data
		jsonData = gameObject.GetComponent<JsonReader>();

		//load json data if loaded alreay
		if(jsonData.json == null)
        {
			gameObject.GetComponent<JsonReader>().LoadData();
		}

		//create constestants
		int categoryCount = 7;
		int nameCount = jsonData.ContestantNames.Count;

		int randomFont = Rnd.Range(0, handWritingMaterials.Count);
		int randomFont2 = Rnd.Range(0, handWritingMaterials.Count);

		//initalize all varables
		stage1Objects = transform.Find("Skill Check Phase").gameObject;
		contestant1GameObject = stage1Objects.transform.GetChild(0).gameObject;
		contestant2GameObject = stage1Objects.transform.GetChild(1).gameObject;
		stage1NextStageButton = stage1Objects.transform.GetChild(2).gameObject.GetComponent<KMSelectable>();
		stage1NextStageButton.OnInteract += delegate () { Debug.Log("next stage pressed");  GoToNextStage(1); StartQuestionPhase(); return false;  };

		stage2Objects = transform.Find("Question Phase").gameObject;

		inQuestionPhase = false;
		timerTextMesh = stage2Objects.transform.GetChild(0).gameObject.GetComponent<TextMesh>();

		c1 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant1GameObject, handWritingMaterials[randomFont], handWritingFonts[randomFont], nameDisplayMaterial, nameDisplayFont);
		c2 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount), contestant2GameObject, handWritingMaterials[randomFont2], handWritingFonts[randomFont2], nameDisplayMaterial, nameDisplayFont);

		//make sure the right game objects are visible
		GoToNextStage(0);
	}


	void Start () {
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

	void Update () {
		if (inQuestionPhase)
		{
			currentTime -= Time.deltaTime;
			timerTextMesh.text = $"{(int)currentTime / 60}:{(int)currentTime % 60}";
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
	}
}
