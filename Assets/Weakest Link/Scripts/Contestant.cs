using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Contestant {

    public string Name { get; private set; } // name of the contestant
    
    public Category Category { get; private set; } //what are they knowledgble in

    public int CorrectAnswer { get; set; } //the number of questions answered correctly in the first stage
    public int QuestionsAsked { get; set; } //the number of questions answered in the first stage

    public int WrongNum { get;  set; }  //the amount of questions they have gotton wrong for the money stage
    public const int MAX_WRONG = 3; // the max amount of questions the contestant can get wrong in the money stage

    //used to hold the handwriting of this contestant
    public Material HandWritingMaterial { get; private set; }
    public Font HandWritingFont { get; private set; }

    public GameObject GameObject;

    public Material NameDisplayMaterial { get; set; }
    public Font NameDisplayFont { get; set; }

    public const float REGULAR_RIGHT_CHOICE = .50f; //the percentage the Contestant will answer correctly if they are not skilled in that category
    public const float GOOD_RIGHT_CHOICE = .80f; //the percentage the Contestant will answer correctly if they are skilled in that category

    public bool Eliminated { get; set; }



    public Contestant(string name, Category category, GameObject gameObject, Material handWritingMaterial, Font handWritingFont, Material nameDisplayMaterial, Font nameDisplayFont, bool setFonts)
    {
        Name = name;
        Category = category;
        GameObject = gameObject;

        WrongNum = 0;
        CorrectAnswer = 0;
        QuestionsAsked = 0;

        Eliminated = false;

        HandWritingMaterial = handWritingMaterial;
        HandWritingFont = handWritingFont;
        
        NameDisplayMaterial = nameDisplayMaterial;
        NameDisplayFont = nameDisplayFont;

        if (setFonts)
        { 
            InitalizeContestant();
        }
    }

    public void InitalizeContestant()
    {
        Text name = GameObject.transform.Find("Name").GetComponent<Text>();
        name.text = Name.ToUpper();
        name.font = NameDisplayFont;
        name.fontSize = 45;

        Text skill = GameObject.transform.Find("Skill").GetComponent<Text>();

        skill.text = Category.ToString().ToUpper();
        skill.font = HandWritingFont;
        skill.fontSize = 45;
    }
}
