using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Category
{
    Geography,
    Language,
    Wildlife,
    Biology,
    Maths,
    KTANE,
    History,
    Other
}



public class Trivia {

    public string Question { get; private set; } //the question being asked
    public List<string> AcceptedAnswers { get; private set; } //strings that will pass the questions
    public List<string> WrongAnswers { get; private set; } //strings that the cpu will use the when they need to get the question wrong
    public Category Category { get; private set; } //They category of the question


    public Trivia(string question, List<string> acceptedAnswers, List<string> wrongAnswers, Category category)
    {
        this.Question = question;
        this.AcceptedAnswers = acceptedAnswers;
        this.WrongAnswers = wrongAnswers;
        this.Category = category;
    }
}
