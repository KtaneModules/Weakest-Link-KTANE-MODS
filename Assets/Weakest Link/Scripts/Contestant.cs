using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contestant {

    public string Name { get; private set; } // name of the contestant
    
    public Category Category { get; private set; } //what are they knowledgble in

    public int CorrectAnswer { get; set; } //the number of questions answered correctly in the first stage
    public int QuestionsAsked { get; set; } //the number of questions answered in the first stage

    public int WrongNum { get;  set; }  //the amount of questions they have gotton wrong for the money stage
    const int MAX_WRONG = 3; // the max amount of questions the contestant can get wrong in the money stage


    //used to hold the handwriting of this contestant
    public Material HandWritingMaterial { get; set; }
    public Font HandWritingFont { get; set; }

    public GameObject GameObject; 


    const float REGULAR_RIGHT_CHOICE = .50f; //the percentage the Contestant will answer correctly if they are not skilled in that category
    const float GOOD_RIGHT_CHOICE = .80f; //the percentage the Contestant will answer correctly if they are skilled in that category

    public Contestant(string name, Category category, GameObject gameObject, Material handWritingMaterial, Font handWritingFont)
    {
        Name = name;
        Category = category;
        WrongNum = 0;
        CorrectAnswer = 0;
        QuestionsAsked = 0;
        HandWritingMaterial = handWritingMaterial;
        HandWritingFont = handWritingFont;
        GameObject = gameObject;


        InitalizeContestant();
    }

    public void InitalizeContestant()
    {
        TextMesh nameTextMesh = GameObject.transform.GetChild(0).GetComponent<TextMesh>();
        TextMesh skillTextMesh = GameObject.transform.GetChild(1).GetComponent<TextMesh>();

        nameTextMesh.text = Name;
        skillTextMesh.text = Category.ToString();

        nameTextMesh.font = skillTextMesh.font = HandWritingFont;

        GameObject.transform.GetChild(0).GetComponent<MeshRenderer>().materials =
        GameObject.transform.GetChild(1).GetComponent<MeshRenderer>().materials = new Material[] { HandWritingMaterial };
    }
}
