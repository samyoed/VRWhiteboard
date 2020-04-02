using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrushSize : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        slider = transform.GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        textMesh.text = "Brush Size: " + slider.value;
    }
}
