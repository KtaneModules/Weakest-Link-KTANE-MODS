using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fake3dText : MonoBehaviour {

    [SerializeField] private TextMesh _mainText;
    [SerializeField] private TextMesh _shadowText;

    private readonly Color _mainTextLit = new Color(0.82f, 0.82f, 0.82f);
    private readonly Color _mainTextUnlit = new Color(0.55f, 0.55f, 0.55f);
    private readonly Color _shadowTextLit = new Color(0.71f, 0.71f, 0.71f);
    private readonly Color _shadowTextUnlit = new Color(0.34f, 0.34f, 0.34f);

    public void SetText(string text) {
        _mainText.text = text;
        _shadowText.text = text;
    }

    public void SetLit(bool setLit) {
        _mainText.color = setLit ? _mainTextLit : _mainTextUnlit;
        _shadowText.color = setLit ? _shadowTextLit : _shadowTextUnlit;
    }

}
