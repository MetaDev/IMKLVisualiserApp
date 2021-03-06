﻿using System.Collections;
using System.Collections.Generic;
using IMKL_Logic;
using IO;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ExtraSettingsPanel : MonoBehaviour
{

    public Toggle GPSToggle;
    public Button ClearCache;

    public Button OKLoginButton;
    public InputField AuthCodeInputField;

    public Toggle BetaToggle;

    // Use this for initialization
    void Start()
    {
        //extra settings
        //GPS toggle
        GPSToggle.OnValueChangedAsObservable().Subscribe(isOn => OnlineMapsLocationService.instance.updatePosition = isOn);
        //clear maps
        ClearCache.OnClickAsObservable().Subscribe(_ =>
        {
            GUIFactory.instance.MyModalWindow
            .Show("Are you sure you want to deleted all cached maps?", ModalWindow.ModalType.OKCANCEL);
            GUIFactory.instance.MyModalWindow.GetModalButtonObservable().Where(button=>button==ModalWindow.ModalReturn.OK)
            .Subscribe(__=> MapHelper.DeleteCachedMaps());
        });
        //Login
        OKLoginButton.OnClickAsObservable().Subscribe(_ => Login());
        //map cache toggle
        //beta
        BetaToggle.OnValueChangedAsObservable().Subscribe(isOn => WebService.BetaWebservice = isOn);
    }
    void Login()
    {
        var authCode = AuthCodeInputField.text;
        WebService.LoginWithAuthCode(authCode).DoOnError(error => GUIFactory.instance.MyModalWindow.Show(error.Message,
                    ModalWindow.ModalType.OK))
            .Subscribe(webRequest => {
                AuthCodeInputField.text="";
                GUIFactory.instance.MyModalWindow.Show("Login succeeded", ModalWindow.ModalType.OK);
                });
    }



}
