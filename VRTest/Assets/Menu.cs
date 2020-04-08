using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    RectTransform[] tempButtonList;
    public List<GameObject> buttonList;
    public GameObject whiteboardMenu;
    public GameObject mainMenu;
    public RectTransform canvas;
    public Camera mainCamera;
    void Start()
    {
        // tempButtonList = GetComponentsInChildren<RectTransform>();
        // foreach(RectTransform trans in tempButtonList)
        //     buttonList.Add(trans);
        // buttonList.Remove(canvas);
        // foreach(RectTransform trans in buttonHideList)
        //     buttonList.Remove(trans);
        
        // foreach(RectTransform trans in buttonList)
        //     trans.gameObject.SetActive(false);
        
    }
    void Update()
    {

    }
    public void WhiteboardMenu()
    {
        whiteboardMenu.SetActive(true);
        foreach (GameObject button in buttonList)
            if(button != whiteboardMenu)
                button.SetActive(false);
        // foreach(RectTransform trans in buttonHideList)
        //     trans.gameObject.SetActive(false);
        // foreach(RectTransform trans in buttonList)
        //     trans.gameObject.SetActive(true);
    }
    public void hideMenu()
    {
        mainMenu.SetActive(true);
        foreach (GameObject button in buttonList)
            if(button != mainMenu)
                button.SetActive(false);
    }

}
