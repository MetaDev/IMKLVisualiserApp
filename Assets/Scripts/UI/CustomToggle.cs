using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
public class CustomToggle : MonoBehaviour
{

    public Toggle MyToggle;
    public Image SelectedImage;
    public Image UnSelectedImage;


    // Use this for initialization
    void Start()
    {
        MyToggle.toggleTransition = Toggle.ToggleTransition.None;
        MyToggle.OnValueChangedAsObservable().Subscribe(isOn =>
        {
            SelectedImage.gameObject.SetActive(isOn);
            UnSelectedImage.gameObject.SetActive(!isOn);

        });
    }
}
