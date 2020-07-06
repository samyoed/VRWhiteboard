using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using FrostweepGames.Plugins.GoogleCloud.Translation;

public class Translation : MonoBehaviour
{
    public GCTranslation gc;
    public TextMeshPro input, output;
    string prev;

    // Start is called before the first frame update
    void Start()
	{
       
    }
   
    // Update is called once per frame
    void Update()
    {
       /* if(prev != input.text)
        {
            prev = input.text;
            TranslationResult result = client.TranslateText(prev, LanguageCodes.Spanish);
            output.text = result.TranslatedText;
            //output.text = prev;
        }*/
    }
}
