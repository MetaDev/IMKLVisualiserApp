using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
public class CustomToggle : MonoBehaviour
{

    public Toggle MyToggle;
    public Image Icon;
    public Sprite SelectedSprite;
    public Sprite UnSelectedSprite;


    // Use this for initialization
    void Start()
    {
        MyToggle.toggleTransition = Toggle.ToggleTransition.None;
        MyToggle.OnValueChangedAsObservable().Subscribe(isOn =>
        {
            Image targetImage = MyToggle.targetGraphic as Image;
            if (targetImage != null)
            {
				if(isOn){
					Icon.sprite = SelectedSprite;
				}else{
					Icon.sprite = UnSelectedSprite;
				}
            }
        });
    }
}
