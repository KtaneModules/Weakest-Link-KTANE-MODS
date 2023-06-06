using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameDisplay : MonoBehaviour {

    private TextMesh mainText;
    private TextMesh shadowText;

    private readonly Color _mainTextLit = new Color(0.82f, 0.82f, 0.82f);
    private readonly Color _mainTextUnlit = new Color(0.55f, 0.55f, 0.55f);
    private readonly Color _shadowTextLit = new Color(0.71f, 0.71f, 0.71f);
    private readonly Color _shadowTextUnlit = new Color(0.34f, 0.34f, 0.34f);

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

    public void SetLit(bool setLit)
    {
        mainText.color = setLit ? _mainTextLit : _mainTextUnlit;
        shadowText.color = setLit ? _shadowTextLit : _shadowTextUnlit;
    }
}
