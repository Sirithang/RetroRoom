using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputMapperEntry : MonoBehaviour
{
    public GameObject padEntry;
    public Text nameText;
    public Text keyboardKey;
    public Text padKey;
    public Image entryBackground;

    Color _entryBackgroundDefault;

    private void Awake()
    {
        _entryBackgroundDefault = entryBackground.color;
    }

    public void SetSelected(bool selected)
    {
        entryBackground.color = selected ? Color.blue : _entryBackgroundDefault;
    }
}
