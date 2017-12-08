using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputMapper : MonoBehaviour
{
	static protected InputMapping KeyboardMapping;
	static protected InputMapping PadMapping;

	public KeyCode keyboardActivatingKey = KeyCode.Return;
	public KeyCode keyboardCancelKey = KeyCode.Escape;

	public Transform listParent;
	
	public InputMapperEntry entryPrefab;

	private InputMapperEntry[] _mapperEntries;
	
	private int _currentActive = 0;
	private bool _inMapping = false;

	private float _axisThresold = 0;
	private bool _axisBackToZero = true;
	
	
	//This is to call BEFORE opening the scene containing the inputmapper
	public static void SetMappingData(InputMapping keyboard, InputMapping pad = null)
	{
		KeyboardMapping = keyboard;
		PadMapping = pad;
	}


	private void Start()
	{
		//TODO : make that WAYYY more generic and robust
		Canvas canvas = GetComponent<Canvas>();
		canvas.worldCamera = Camera.main;
		
		if (KeyboardMapping == null)
		{
			Debug.LogError("You need to setup at least a keyboard mapping before opening the InputMapper");
			return;
		}

		_mapperEntries = new InputMapperEntry[KeyboardMapping.buttons.Length];

		for (int i = 0; i < KeyboardMapping.buttons.Length; ++i)
		{
			_mapperEntries[i] = Instantiate(entryPrefab, listParent);
			_mapperEntries[i].padEntry.SetActive(PadMapping != null);

			_mapperEntries[i].nameText.text = KeyboardMapping.buttons[i].name;
			_mapperEntries[i].keyboardKey.text = KeyboardMapping.GetKeyCode(KeyboardMapping.buttons[i].name).ToString();
			if (PadMapping != null)
				_mapperEntries[i].padKey.text = PadMapping.GetKeyCode(PadMapping.buttons[i].name).ToString().Replace("Joystick", "");
		}

		_currentActive = 0;
        _mapperEntries[_currentActive].SetSelected(true);
	}

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    private void Update()
	{
		if (KeyboardMapping == null)
			return;
		
		if (!_inMapping)
		{
			float yAxis = Input.GetAxis("Vertical");

			if (!_axisBackToZero && _axisThresold > 0 && yAxis < _axisThresold)
				_axisBackToZero = true;
			else if (!_axisBackToZero && _axisThresold < 0 && yAxis > _axisThresold)
				_axisBackToZero = true;

			if (yAxis > 0.2f && _axisBackToZero)
			{
                _mapperEntries[_currentActive].SetSelected(false);
                _currentActive = mod(_currentActive - 1, KeyboardMapping.buttons.Length);
                _mapperEntries[_currentActive].SetSelected(true);
                _axisBackToZero = false;
				_axisThresold = 0.2f;
			}
			else if (yAxis < -0.2f && _axisBackToZero)
			{
                _mapperEntries[_currentActive].SetSelected(false);
                _currentActive = mod(_currentActive + 1, KeyboardMapping.buttons.Length);
                _mapperEntries[_currentActive].SetSelected(true);
                _axisBackToZero = false;
				_axisThresold = -0.2f;
			}

			if (Input.GetKeyDown(keyboardActivatingKey))
			{
				_inMapping = true;
				_mapperEntries[_currentActive].keyboardKey.text = "Press Any";
				if(PadMapping != null) _mapperEntries[_currentActive].padKey.text = "Press Any";
			}

			if (Input.GetKeyDown(keyboardCancelKey))
			{
				SceneManager.UnloadSceneAsync("inputMapper");
				Time.timeScale = 1.0f;
			}
		}
		else
		{
			if (Input.anyKeyDown)
			{
				bool keyValid = false;
				foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
				{
					if (Input.GetKeyDown(key))
					{
						//TODO: handle multiple joystick by finding the number of the joystick 
						string name = key.ToString();
						if (name.Contains("Joystick"))
						{
							if (PadMapping != null)
							{
								keyValid = true;
								PadMapping.ChangeKeycode(PadMapping.buttons[_currentActive].name, key);
								break;
							}
						}
						else
						{
							keyValid = true;
							KeyboardMapping.ChangeKeycode(KeyboardMapping.buttons[_currentActive].name, key);
							break;
						}
					}
				}

				if (keyValid)
				{
					_inMapping = false;
					_mapperEntries[_currentActive].keyboardKey.text = KeyboardMapping.GetKeyCode(KeyboardMapping.buttons[_currentActive].name).ToString();
					if(PadMapping != null) _mapperEntries[_currentActive].padKey.text = PadMapping.GetKeyCode(PadMapping.buttons[_currentActive].name).ToString().Replace("Joystick", "");
				}
			}
		}
	}
}
