﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    RectTransform[] tempButtonList;
    List<RectTransform> buttonList;
    public RectTransform showMenuButton;
    void Start()
    {
        tempButtonList = GetComponentsInChildren<RectTransform>();
        foreach(RectTransform trans in tempButtonList)
            buttonList.Add(trans);
        buttonList.Remove(showMenuButton);
    }
    public void showMenu()
    {
        showMenuButton.gameObject.SetActive(false);
        foreach(RectTransform trans in buttonList)
            trans.gameObject.SetActive(true);
    }
    public void hideMenu()
    {
        showMenuButton.gameObject.SetActive(true);
        foreach(RectTransform trans in buttonList)
            trans.gameObject.SetActive(false);
    }

}
