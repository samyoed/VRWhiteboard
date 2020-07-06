using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrostweepGames.Plugins.GoogleCloud.Translation;
using TMPro;
using UnityEngine.UI;



public class Translate : MonoBehaviour
{
    string prev;
    public GCTranslation gCT;
    public TextMeshPro input, output;
    public string inputLang = "en", outputLang = "es";
    public Dropdown sourceLang, targetLang;
    // Start is called before the first frame update
    void Start()
    {
        prev = input.text;

        gCT.TranslateSuccessEvent += SuccessTranslate;
        //gCT.DetectLanguageSuccessEvent += SuccessTranslate;
        //_gcTranslation.GetLanguagesSuccessEvent += GetLanguagesSuccessEventHandler;

        gCT.TranslateFailedEvent += FailTranslate;
       // gCT.DetectLanguageFailedEvent += DetectLanguageFailedEventHandler;
        //gCT.GetLanguagesFailedEvent += GetLanguagesFailedEventHandler;

        gCT.ContentOutOfLengthEvent += ContentOutOfLength;
    }

    public string languageCode(string lang)
    {
        if (lang == "English (United States)")
        {
            return "en";
        }

        if (lang == "Spanish (Mexico)")
        {
            return "es";
        }

        if (lang == "French (France)")
        {
            return "fr";
        }

        return "en_US";
    }

    public void ChangeSrcLang(int i)
    {
        inputLang = languageCode(sourceLang.options[sourceLang.value].text);
    }

    public void changeTargetLang(int i)
    {
        outputLang = languageCode(targetLang.options[targetLang.value].text);
    }

    public void ContentOutOfLength()
    {
        output.text = "Translation Error: Content out of Length.";
    }

    public void FailTranslate(string value)
    {
        output.text = "Translation Error: " + value;
    }

    public void SuccessTranslate(TranslationResponse response)
    {
        output.text = response.data.translations[0].translatedText;
    }

    void StartTranslation(string t, string targetLanguage, string sourceLanguage = "en")
    {
        gCT.Translate(new TranslationRequest()
        {
            q = t,
            source = sourceLanguage,
            target = targetLanguage,
            format = "text",
            model = "nmt"
        });
    }

    // Update is called once per frame
    void Update()
    {
        if(prev != input.text)
        {
            prev = input.text;
            StartTranslation(input.text, outputLang, inputLang);
        }
    }
}
