using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameDisplay : MonoBehaviour {

    private TextMesh mainText;
    private TextMesh shadowText;

    private readonly Color _mainTextLit = new Color(0.74f, 0.74f, 0.74f);
    private readonly Color _mainTextUnlit = new Color(0.27f, 0.27f, 0.27f);
    private readonly Color _shadowTextLit = new Color(0.5f, 0.5f, 0.5f);
    private readonly Color _shadowTextUnlit = new Color(0.45f, 0.45f, 0.45f);

    private readonly Color _eliminationTextLit = new Color(0.9f, 0.9f, 0.9f);
    private readonly Color _shadowEliminationTextUnlit = new Color(0.5f, 0.5f, 0.5f);

    public string Text { get { return mainText.text; } set { mainText.text = value; shadowText.text = value;  } }

    public void InitializeVariables()
    {
        mainText = transform.Find("Main").GetComponent<TextMesh>();
        shadowText = transform.Find("Shadow").GetComponent<TextMesh>();

    }

    public void SetFont(Font font, Material material)
    {
        mainText.font = shadowText.font = font;
        mainText.GetComponent<MeshRenderer>().material = shadowText.GetComponent<MeshRenderer>().material = material;
        Text = "";
    }

    public void SetLit(bool setLit, bool elimination)
    {
        if (elimination)
        {
            mainText.color = _eliminationTextLit;
            shadowText.color = _shadowEliminationTextUnlit;
        }

        else
        {
            mainText.color = setLit ? _mainTextLit : _mainTextUnlit;
            shadowText.color = setLit ? _shadowTextLit : _shadowTextUnlit;
        }
    }
}
