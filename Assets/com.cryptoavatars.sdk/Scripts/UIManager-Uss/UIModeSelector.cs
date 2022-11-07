using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LoginUIModes { Window, Popup };
public class UIModeSelector : MonoBehaviour
{

    private  LoginUIModes loginUIMode;
    public event Action OnUIModeChanged;

    public GameObject[] popupUIGameObjects;
    public GameObject[] windowUIGameObjects;

    private void Awake()
    {
        OnUIModeChanged += LoadUI;
    }
    private void Start()
    {
        LoadUI();
    }
    public LoginUIModes LoginUIMode
    {
        get
        {
            return loginUIMode;
        }
        set
        {
            if (value != loginUIMode)
            {
                Debug.Log("UI Mode Changed to: " + value);
                loginUIMode = value;
                OnUIModeChanged?.Invoke();
            }
        }
    }

    private void LoadUI()
    {
        switch (loginUIMode)
        {
            case LoginUIModes.Popup:
                foreach (GameObject popupGO in popupUIGameObjects)
                {
                    popupGO.SetActive(true);
                }
                foreach (GameObject windowGO in windowUIGameObjects)
                {
                    windowGO.SetActive(false);
                }
                break;
            case LoginUIModes.Window:
                foreach (GameObject windowGO in windowUIGameObjects)
                {
                    windowGO.SetActive(true);
                }
                foreach (GameObject popupGO in popupUIGameObjects)
                {
                    popupGO.SetActive(false);
                }
                break;
        }
    }

}
