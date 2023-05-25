using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Money {

    public int MoneyAmount { get; private set; }
    public GameObject GameObject { get; private set; }
    public Sprite BlueBackground { get; private set; }
    public Sprite RedBackground { get; private set; }
    public Vector2 blueAnchorPoints { get; private set; }
    public Vector2 redAnchorPoints { get; private set; }

    private Image image;
    private RectTransform rectTransform;

    public Money(int money, GameObject gameObject, Sprite redBackground)
    {
        MoneyAmount = money;
        GameObject = gameObject;
        image = GameObject.GetComponent<Image>();

        BlueBackground = image.sprite;
        RedBackground = redBackground;

        rectTransform = gameObject.GetComponent<RectTransform>();


    blueAnchorPoints = new Vector2(rectTransform.anchorMin.y, rectTransform.anchorMax.y);

        if (GameObject.name == "20 Image")
        {
            redAnchorPoints = new Vector2(blueAnchorPoints.x, blueAnchorPoints.y);
        }

        else
        {
            float offset = -.04f;


            switch (GameObject.name.Replace(" Image", ""))
            {
                case "100":
                    offset *= 2;
                    break;
                case "200":
                    offset *= 3;
                    break;
                case "300":
                    offset *= 4;
                    break;
                case "450":
                    offset *= 5;
                    break;
                case "600":
                    offset *= 6;
                    break;
                case "800":
                    offset *= 7;
                    break;
                case "1000":
                    offset *= 8;
                    break;
            }

            redAnchorPoints = new Vector2(blueAnchorPoints.x + offset, blueAnchorPoints.y + offset);
        }
    }

    public void ToggleColor(bool collected)
    {
        if (collected)
        { 
            image.sprite = RedBackground;
            rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, redAnchorPoints.x);
            rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, redAnchorPoints.y);
        }

        else
        {
            image.sprite = BlueBackground;
            rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, blueAnchorPoints.x);
            rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, blueAnchorPoints.y);
        }
    }
}
