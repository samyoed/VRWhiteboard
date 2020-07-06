using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.Events;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class SpeechToText : MonoBehaviour
{
    public UnityEvent ActivateCustomStream, DeactivateCustomStream;
    public bool aggressivePunctuation = true;
    public GameObject dictationControls;
    public XRController right, left;
    public PostIt postItTemplate;
    public TextMeshPro tmp, debugTmp, translationInput;
    public bool writing = false, cursored = true, finalized = true;
    public string beginExp = "start", endExp = "stop", written = "", toDisplay = "";
    public float moveRate = 1.0f, timer = 0;
    public float cursorJoystickCutoff = 0.1f;
    public int cursorLocation = 0;
    public GameObject beginButton, endButton;


    // Start is called before the first frame update
    void Start()
    {


        dictationControls = GameObject.Find("Dictation Controls");
        right = (right!=null)?right: GameObject.Find("RightHand Controller").GetComponent<XRController>();
        left = (left!=null)?left: GameObject.Find("LeftHand Controller").GetComponent<XRController>();
        //postItTemplate = GameObject.Find("Post It").GetComponent<PostIt>();
        tmp = GameObject.Find("Dictation TMP").GetComponent<TextMeshPro>();
        debugTmp = GameObject.Find("Debug TMP").GetComponent<TextMeshPro>();

        postItTemplate.floatingControls = GameObject.Find("Floating Controls");
        postItTemplate.stuckToWallControls = GameObject.Find("Stuck To Wall Controls");

        postItTemplate.floatingControls.SetActive(false);
        postItTemplate.stuckToWallControls.SetActive(false);

        dictationControls.SetActive(false);
        //debugTmp.gameObject.SetActive(false);
        postItTemplate.gameObject.SetActive(false);

#if PLATFORM_ANDROID
        // actually requesting the microphone permissions.
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif

        tmp.text = "";
    }

    void CursorBlink()
    {
        if (writing)
        {
            dictationControls.SetActive(true);
            timer += Time.deltaTime;
            if (timer >= 1)
            {
                cursored = !cursored;
                timer = 0;
            }
            tmp.text = toDisplay.Substring(0, cursorLocation) + (cursored && finalized ? "|" : "") + toDisplay.Substring(cursorLocation, toDisplay.Length - cursorLocation);
        }
        else
        {
            dictationControls.SetActive(false);
        }
    }

    string CursorAppend(string str)
    {
        return written.Substring(0, cursorLocation) + str + written.Substring(cursorLocation, written.Length - cursorLocation);
    }


    public void MoveAway() // moving the TMP away from the XR rig.
    {
        tmp.gameObject.transform.localPosition += new Vector3(0, 0, Time.deltaTime * moveRate);
    }

    public void MoveCloser() // moving the TMP closer to thee XR rig.
    {
        if (tmp.gameObject.transform.localPosition.z >= 0)
        {
            tmp.gameObject.transform.localPosition += new Vector3(0, 0, -Time.deltaTime * moveRate);
        }
    }

    public void OnInterim(string str) // before final prediction is made for speech
    {

        debugTmp.text = "On Interim: " + str;
        if (writing) // only add to the text if the TMP is in edit mode (i.e. writing).
        {
            toDisplay = CursorAppend(" " + str); // preemptively insert what was heard into the text
            finalized = false;
            translationInput.text = toDisplay;
        }


    }

    public void beginPostItTranscription()
    {
        beginButton.SetActive(false);
        endButton.SetActive(true);
        ActivateCustomStream.Invoke();
        OnFinal(beginExp);
    }

    public void endPostItTranscription()
    {
        DeactivateCustomStream.Invoke();
        OnFinal(endExp);
    }

    public void OnFinal(string str) // when final prediction is made for speech
    {

        debugTmp.text = "On Final: " + str;
        if (aggressivePunctuation)
        {
            string cpy = str;

            cpy = cpy.Replace("full stop", ".");
            cpy = cpy.Replace("semi colon", ";");
            cpy = cpy.Replace("semicolon", ";");
            cpy = cpy.Replace("period", ".");
            cpy = cpy.Replace("perio.", ".");
            cpy = cpy.Replace("exclamation point", "!");
            cpy = cpy.Replace("exclamation mark", "!");
            cpy = cpy.Replace("exclamation mark!", "!");
            cpy = cpy.Replace("exclamation marks", "!");
            cpy = cpy.Replace("exclamation points", "!");
            cpy = cpy.Replace("exclamation poin!", "!");
            cpy = cpy.Replace("question mark", "?");
            cpy = cpy.Replace("question mar?", "?");
            cpy = cpy.Replace("colon", ":");
            cpy = cpy.Replace("colo:", ":");
            cpy = cpy.Replace("Colin", ":");
            cpy = cpy.Replace("Coli:", ":");
            cpy = cpy.Replace("comma", ",");
            
            str = cpy;
        }

        finalized = true;
        if (!writing) // if the tmp is not in edit mode
        {
            if (str.ToLower().Contains(beginExp.ToLower())) // checking to see if heard the beginning keyword
            {
                writing = true;
                int ind = str.IndexOf(beginExp, System.StringComparison.CurrentCultureIgnoreCase);
                if (ind + beginExp.Length <= str.Length)
                {
                    written = str.Substring(ind + beginExp.Length);
                }
                cursorLocation = written.Length;
                toDisplay = written;
            }
        }
        else
        {
            if (str.ToLower().Contains(endExp.ToLower())) // checking to see if heard the ending keyword
            {
                int ind = str.IndexOf(endExp, System.StringComparison.CurrentCultureIgnoreCase);
                tmp.text = written + " " + str.Substring(0, ind);



                /*TextMeshPro newTMP = Instantiate(tmp); // making a new TMP that will contain the message.
                newTMP.gameObject.transform.position = tmp.gameObject.transform.position;
                newTMP.gameObject.transform.eulerAngles = tmp.gameObject.transform.eulerAngles;
                newTMP.color = Color.white;*/


                GameObject newPostObj = Instantiate(postItTemplate.gameObject);
                PostIt newPost = newPostObj.GetComponent<PostIt>();
                newPostObj.SetActive(true);
                newPost.SetText(tmp.text);
                newPost.Select();


                tmp.text = ""; // clearing the old TMP.
                written = "";
                toDisplay = "";
                cursorLocation = 0;
                writing = false;
                timer = 0;
            }
            else
            {
                written = CursorAppend(" " + str);
                cursorLocation += (" " + str).Length;
                toDisplay = written;
            }
            translationInput.text = toDisplay;
        }

        /*if (aggressivePunctuation)
        {
            string cpy = written;

            print(cpy);

            cpy = cpy.Replace("period", ".");
            cpy = cpy.Replace("perio.", ".");
            cpy = cpy.Replace("exclamation point", "!");
            cpy = cpy.Replace("exclamation points", "!");
            cpy = cpy.Replace("exclamation poin!", "!");
            cpy = cpy.Replace("question mark", "?");
            cpy = cpy.Replace("question mar?", "?");
            cpy = cpy.Replace("colon", ":");
            cpy = cpy.Replace("colo:", ":");
            cpy = cpy.Replace("Colin", ":");
            cpy = cpy.Replace("Coli:", ":");
            cpy = cpy.Replace("comma", ",");

            print(cpy);
            written = cpy;
            toDisplay = written;
            if(cursorLocation >= toDisplay.Length)
            {
                cursorLocation = toDisplay.Length - 1;
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        if (debugTmp.text == "")
        {
            debugTmp.text = "empty";
        }
        CursorBlink();
        // temp test controls here; ideally we would have some sort of gesture or
        // VR controller input that prompts these responses.

        if(Input.GetKeyDown(KeyCode.Space))
        {
            beginPostItTranscription();
        }

        if(Input.GetKeyDown(KeyCode.Period))
        {
            endPostItTranscription();
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoveAway();
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveCloser();
        }

        Vector2 vector;

        if (right.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out vector))
        {
            if (vector.x < -cursorJoystickCutoff && cursorLocation > 0)
            {
                cursorLocation--;
                cursored = true;
                timer = 0;
            }
            else if (vector.x > cursorJoystickCutoff && cursorLocation < toDisplay.Length)
            {
                cursorLocation++;
                cursored = true;
                timer = 0;
            }
        }


        bool pressed;
        if (right.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out pressed))
        {
            if (pressed && cursorLocation > 0)
            {
                string first = written.Substring(0, cursorLocation - 1);
                string second = written.Substring(cursorLocation, (written.Length - cursorLocation));
                cursorLocation--;
                written = first + second;
                toDisplay = written;
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
        {
            if (cursorLocation > 0)
            {
                string first = written.Substring(0, cursorLocation - 1);
                string second = written.Substring(cursorLocation, (written.Length - cursorLocation));
                cursorLocation--;
                written = first + second;
                toDisplay = written;
            }
        }


        if ((Input.GetKeyDown(KeyCode.LeftArrow)) && cursorLocation > 0)
        {
            cursorLocation--;
            cursored = true;
            timer = 0;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) && cursorLocation < toDisplay.Length)
        {
            cursorLocation++;
            cursored = true;
            timer = 0;
        }
    }
}
