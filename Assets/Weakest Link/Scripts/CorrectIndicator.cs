using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CorrectIndicator {

    public int Index { get; private set; }
    public GameObject GameObject { get; private set; }
    public Sprite BlueBackground { get; private set; }
    public Sprite RedBackground { get; private set; }

    private Image backgroundImage;
    private Image checkImage;
    private RectTransform rectTransform;
    private Text text;

    private Sprite correctSprite;
    private Sprite wrongSprite;

    GameObject textGameObject;
    GameObject backgroundGameObject;



    public CorrectIndicator(int index, GameObject gameObject, Sprite redBackground, Sprite correctSprite, Sprite wrongSprite)
    {
        Index = index;
        GameObject = gameObject;
        RedBackground = redBackground;

        backgroundImage = GameObject.GetComponent<Image>();
        backgroundGameObject = GameObject.transform.Find("Image").gameObject;

        checkImage = backgroundGameObject.GetComponent<Image>();
        rectTransform = checkImage.GetComponent<RectTransform>();

        textGameObject = GameObject.transform.Find("Text").gameObject;
        text = textGameObject.GetComponent<Text>();

        BlueBackground = backgroundImage.sprite;

        this.correctSprite = correctSprite;
        this.wrongSprite = wrongSprite;
    }

    public void SetUsed(bool correct)
    {
        rectTransform.anchorMin = new Vector2(.4f, .2f);
        rectTransform.anchorMax = new Vector2(.6f, .8f);

        backgroundImage.sprite = RedBackground;

        if (correct)
        {
            checkImage.sprite = correctSprite;
        }

        else
        { 
            checkImage.sprite = wrongSprite;
        }

        textGameObject.SetActive(false);
        backgroundGameObject.SetActive(true);
    }

    public void SetUnused()
    {
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 1f);

        backgroundImage.sprite = BlueBackground;


        checkImage.sprite = null;

        text.text = "" + Index;

        textGameObject.SetActive(true);
        backgroundGameObject.SetActive(false);

    }
}
