using System.Collections;
using System.Collections.Generic;
using IO;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ExtraSettingsPanel : MonoBehaviour {
	
	public Toggle GPSToggle;
    public Button ClearLocalPackages;

    public Button OKLoginButton;
    public InputField AuthCodeInputField;
	// Use this for initialization
	void Start () {
		//extra settings
        //GPS toggle
        GPSToggle.OnValueChangedAsObservable().Subscribe(isOn => OnlineMapsLocationService.instance.updatePosition = isOn);
        //clear packages
        ClearLocalPackages.OnClickAsObservable().Subscribe(_ => Serializer.DeleteStoredPackages());
        //Login
        OKLoginButton.OnClickAsObservable().Subscribe(_ => Login());
	}
	 void Login()
    {
        var authCode = AuthCodeInputField.text;
        WebService.LoginWithAuthCode(authCode).DoOnError(error => GUIFactory.instance.MyModalWindow.Show(error.Message, true))
            .Subscribe(webRequest => GUIFactory.instance.MyModalWindow.Show("Login succeeded", true));
    }
	


}
