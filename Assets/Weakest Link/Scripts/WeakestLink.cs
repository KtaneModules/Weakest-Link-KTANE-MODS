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

	//variables that will be used in all stages
	#region Global Variables
	JsonReader jsonData;
	Contestant c1;
	Contestant c2;
	#endregion

	//objects for the first stage
	#region Stage 1 Objects 
	GameObject stage1Objects;

	GameObject contestant1GameObject;
	GameObject contestant2GameObject;
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
		int categoryCount = 5;
		int nameCount = jsonData.ContestantNames.Count;


		c1 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount));
		c2 = new Contestant(jsonData.ContestantNames[Rnd.Range(0, nameCount)], (Category)Rnd.Range(0, categoryCount));

		Debug.Log($"C1: {c1.Name}, {c1.Category}");
		Debug.Log($"C2: {c2.Name}, {c2.Category}");


		//initalize all varables
		stage1Objects = transform.Find("Skill Check Phase").gameObject;

		contestant1GameObject = stage1Objects.transform.GetChild(0).gameObject;
		contestant2GameObject = stage1Objects.transform.GetChild(1).gameObject;

		//make sure the right game objects are visible
		stage1Objects.SetActive(true);

		//set the names for the conestants on the module
		contestant1GameObject.transform.GetChild(0).GetComponent<TextMesh>().text = c1.Name;
		contestant1GameObject.transform.GetChild(1).GetComponent<TextMesh>().text = c1.Category.ToString();

		contestant2GameObject.transform.GetChild(0).GetComponent<TextMesh>().text = c2.Name;
		contestant2GameObject.transform.GetChild(1).GetComponent<TextMesh>().text = c2.Category.ToString();
	}

	void Start () {
        ModuleId = ModuleIdCounter++;

		SetUpModule();
	}

	void Update () {
		
	}
}
