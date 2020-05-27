using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;


public class InputManager : MonoBehaviour
{
    public XRNode xrNode = XRNode.LeftHand;

    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;

    
    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(xrNode, devices);
        device = devices.FirstOrDefault();

    }

    void OnEnable()
    {
        if(!device.isValid)
        {
            GetDevice();
        }
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.UpArrow))
            SnapshotCamera.TakeSnapshot_Static(1600,900);


        if(!device.isValid)
        {
            GetDevice();
        }

        List<InputFeatureUsage> features = new List<InputFeatureUsage>();
        device.TryGetFeatureUsages(features);

        foreach(var feature in features)
        {
            if(feature.type == typeof(bool))
            {
                //print($"Feature {feature.name} type {feature.type}");
            }
        }
        bool triggerButtonAction = false;
        bool primaryButtonAction = false;
        bool primaryButtonActionPrevFrame = false;
        bool secondaryButtonAction = false;

        if(device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonAction) && triggerButtonAction)
            print($"TriggerButton active {triggerButtonAction}");
        
        //Take snapshot
        if(device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonAction) && primaryButtonAction && !primaryButtonActionPrevFrame )
        {
            primaryButtonActionPrevFrame = true;
            print($"PrimaryButton active {primaryButtonAction}");
            SnapshotCamera.TakeSnapshot_Static(1600,900);
        }
        else
            primaryButtonActionPrevFrame = false;
        


    }
}
