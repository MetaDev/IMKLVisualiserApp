using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using IMKL_Logic;
using IO;
using MoreLinq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class GUIFactory : MonoBehaviour
{
    public MenuPanel menuPanel;
    public CustomToggle MenuToggle;
    public MultiSelectPanel IMKLPackageInfoPanel;
    public MultiSelectPanel IMKLPackageDrawPanel;

    public ModalWindow MyModalWindow;

    public GameObject HideAblePanel;



    public void LoadPacketInfo()
    {
        MyModalWindow.Show("Please wait while downloading all available packages information", false);
        WebService.GetAllIMKLPackage()
        .DoOnCompleted(() => MyModalWindow.Close())
            .DoOnError(error => MyModalWindow.Show(error.Message, true))
            .Select(packages =>
          {
              IMKLPackageInfoPanel.AddItems(packages.Select(package => Tuple.Create(package.Reference, (object)package, package.DownloadIMKL)));
              //every time ok is clicked the selected items are streamed
              return IMKLPackageInfoPanel.OnSelectedItemsAsObservable();
          }).Subscribe(itemsObs => itemsObs.Subscribe(items =>
          {
              MyModalWindow.Show("Please wait while downloading selected package data", false);
              //   List<IObservable<Tuple<IMKLPackage,List<XDocument>>>> packagesObs = new List<IObservable<Tuple<IMKLPackage,List<XDocument>>>>();
              items.Select(item => item.content).Cast<IMKLPackage>()
                            .Select(package => WebService.DownloadXMLForIMKLPackage(package)
                            .Select(xmls => Tuple.Create(package, xmls)))
                            .Merge().ToList()
                            .DoOnError(error => MyModalWindow.Show(error.Message, true))
                            .DoOnCompleted(() => MyModalWindow.Close())
                            .Subscribe(packagesAndXmls =>
                            {
                                //save xml docs in package instance
                                packagesAndXmls.ForEach(pAndXs => pAndXs.Item1.KLBResponses = pAndXs.Item2);
                                //serialise package instances
                                Serializer.SaveIMKLPackages(packagesAndXmls.Select(pAndX => pAndX.Item1));
                            });
          }));
    }
    public static GUIFactory instance;


    void InitDrawPanel()
    {
        //init draw panel with saved packages
        AddDrawPackages(Serializer.LoadAllIMKLPackages());
        //refresh the draw panel on the content of the imkl packages
        Serializer.PackagesChanged().Subscribe(packages => AddDrawPackages(packages));
        //TODO delete previous drawing on map
        IMKLPackageDrawPanel.OnSelectedItemsAsObservable()
        .Do(_ => MyModalWindow.Show("Please wait while elements from the package are being drawn.", false))
        //HACK wait to first show dialog box and later execute drawing
        .Delay(TimeSpan.FromSeconds(0.5))
        .Subscribe(
            items =>
            {
                Observable.FromCoroutine(() => DrawPackages(items.Select(item => item.content).Cast<IMKLPackage>()))
                .DoOnError(error => MyModalWindow.Show(error.Message, true))
                .DoOnCompleted(() => MyModalWindow.Close()).Subscribe();
            });
    }
    void Start()
    {
        //Singleton
        instance = this;
        //Draw Panel
        InitDrawPanel();
        //Info Panel
        //TODO load packet info is assigned here
        //Login
        //toggle
        MenuToggle.MyToggle.OnValueChangedAsObservable().Subscribe(isOn =>
        {
            HideAblePanel.SetActive(isOn);
        });

    }
    void AddDrawPackages(IEnumerable<IMKLPackage> packages)
    {
        IMKLPackageDrawPanel.AddItems(packages.Where(package => package.KLBResponses != null)
                    .Select(package => Tuple.Create(package.Reference, (object)package, true)));
    }

    //TODO Add loading screen 

    IEnumerator DrawPackages(IEnumerable<IMKLPackage> packages)
    {
        if (packages != null)
        {
            foreach (IMKLPackage package in packages)
            {
                var drawElements = IMKLParser.ParseDrawElements(package.GetKLBXML())
                             .Where(elts => elts != null);
                MapHelper.ZoomAndCenterOnElements(drawElements);
                foreach (DrawElement elt in drawElements)
                {
                    elt.Init();
                    yield return null;
                }
            }
        }
        else
        {
            Debug.Log("package are null");
        }

    }


    public void LoginPress()
    {
        var authCode = menuPanel.AuthCodeInputField.text;
        WebService.LoginWithAuthCode(authCode).DoOnError(error => GUIFactory.instance.MyModalWindow.Show(error.Message, true))
            .Subscribe(webRequest => GUIFactory.instance.MyModalWindow.Show("Login succeeded", true));
    }



}
