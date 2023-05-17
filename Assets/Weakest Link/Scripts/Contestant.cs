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

    const float REGULAR_RIGHT_CHOICE = .50f; //the percentage the Contestant will answer correctly 
    const float GOOD_RIGHT_CHOICE = .80f;

    public Contestant(string name, Category category)
    {
        Name = name;
        Category = category;
        WrongNum = 0;
        CorrectAnswer = 0;
        QuestionsAsked = 0;
    }
}
