using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    RectTransform[] tempButtonList;
    public List<RectTransform> buttonList;
    public List<RectTransform> buttonHideList;
    public RectTransform canvas;
    void Start()
    {
        tempButtonList = GetComponentsInChildren<RectTransform>();
        foreach(RectTransform trans in tempButtonList)
            buttonList.Add(trans);
        buttonList.Remove(canvas);
        foreach(RectTransform trans in buttonHideList)
            buttonList.Remove(trans);
        
        foreach(RectTransform trans in buttonList)
            trans.gameObject.SetActive(false);
        
    }
    void Update()
    {
        if(Input.GetKeyDown("space"))
        {
            showMenu();
            print("showmenu");
        }
        if(Input.GetKeyUp("m"))
        {
            hideMenu();
            print("hideMenu");
        }
    }
    public void showMenu()
    {
        foreach(RectTransform trans in buttonHideList)
            trans.gameObject.SetActive(false);
        foreach(RectTransform trans in buttonList)
            trans.gameObject.SetActive(true);
    }
    public void hideMenu()
    {
        foreach(RectTransform trans in buttonHideList)
            trans.gameObject.SetActive(true);
        foreach(RectTransform trans in buttonList)
            trans.gameObject.SetActive(false);
    }

}
