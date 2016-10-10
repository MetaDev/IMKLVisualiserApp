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
    public CustomToggle MenuToggle;
    public MultiSelectPanel OnlinePackagesPanel;
    public MultiSelectPanel LocalPackagesPanel;

    public Button RefreshOnlinePackages;

    public ModalWindow MyModalWindow;

    public GameObject FixedMenuSlidePart;
    public Button HiddenMenuBtn;
    public GameObject HiddenMenu;
    public Toggle GPSToggle;
    public Button ClearLocalPackages;

    public Button OKLoginButton;
    public InputField AuthCodeInputField;

    public MultiSelectPanel SelectedDrawElements;
    public Text DrawElementPropertiesUI;


    void LoadPacketInfo()
    {
        MyModalWindow.Show("Please wait while downloading all available packages information", false);
        WebService.GetAllIMKLPackage()
        .DoOnCompleted(() => MyModalWindow.Close())
            .DoOnError(error => MyModalWindow.Show(error.Message, true))
            .SelectMany(packages =>
          {
              OnlinePackagesPanel.AddItems(packages.Select(package => Tuple.Create(package.Reference, (object)package, package.DownloadIMKL)));
              //every time ok is clicked the selected items are streamed
              return OnlinePackagesPanel.OnSelectedItemsAsObservable();
          }).Subscribe(items =>
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
          });
    }
    public static GUIFactory instance;


    void InitDrawPanel()
    {
        //init draw panel with saved packages
        AddDrawPackages(Serializer.LoadAllIMKLPackages());
        //refresh the draw panel on the content of the imkl packages
        Serializer.PackagesChanged().Subscribe(packages => AddDrawPackages(packages));
        //TODO delete previous drawing on map
        LocalPackagesPanel.OnSelectedItemsAsObservable()
        .Do(_ => MyModalWindow.Show("Please wait while elements from the package are being drawn.", false))
        //Add a small delay to show message before draing starts
        .Delay(TimeSpan.FromSeconds(0.2f))
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
        RefreshOnlinePackages.OnClickAsObservable().Subscribe(_ => LoadPacketInfo());
        //toggle
        MenuToggle.MyToggle.OnValueChangedAsObservable().Subscribe(isOn => FixedMenuSlidePart.SetActive(isOn));
        //HiddenMenuBtn
        HiddenMenuBtn.OnClickAsObservable().Subscribe(_ => HiddenMenu.SetActive(true));
        //GPS toggle
        GPSToggle.OnValueChangedAsObservable().Subscribe(isOn => OnlineMapsLocationService.instance.updatePosition = isOn);
        //clear packages
        ClearLocalPackages.OnClickAsObservable().Subscribe(_ => Serializer.DeleteStoredPackages());
        //Login
        OKLoginButton.OnClickAsObservable().Subscribe(_ => Login());
        //line stuff

    }
    void AddDrawPackages(IEnumerable<IMKLPackage> packages)
    {
        LocalPackagesPanel.AddItems(packages.Where(package => package.KLBResponses != null)
                    .Select(package =>
                    {
                        return Tuple.Create(package.Reference, (object)package);
                    }));
    }

    public void InitDrawElementProperties()
    {
        SelectedDrawElements.OnSelectedItemsAsObservable().Subscribe(items =>
        {
            var properties = (Dictionary<string, string>)items.First().content;
            DrawElementPropertiesUI.text = string.Join("\n", properties.ToList()
            .Select(pair => pair.Key + ": " + pair.Value).ToArray());
        });
        //TODO add back button to eltproperty UI

    }
    public void AddDrawElementToPropertiesObservable(string text, IObservable<Dictionary<string, string>> obsClick)
    {
        obsClick = obsClick.Merge(obsClick).Delay(TimeSpan.FromSeconds(0.5f))
        .Do(properties => SelectedDrawElements.AddItem(Tuple.Create(text, (object)properties)));
    }
    //TODO Add progressbar

    IEnumerator DrawPackages(IEnumerable<IMKLPackage> packages)
    {
        if (packages != null)
        {
            List<DrawElement> elements = new List<DrawElement>();
            foreach (IMKLPackage package in packages)
            {
                var drawElements = IMKLParser.ParseDrawElements(package.GetKLBXML())
                             .Where(elts => elts != null);
                MapHelper.ZoomAndCenterOnElements(drawElements);
                int i = 0;
                elements.AddRange(drawElements);
                foreach (DrawElement elt in drawElements)
                {
                    elt.Init();
                    //draw X elements per frame
                    if (i % 5 == 0)
                    {
                        yield return null;
                    }
                    i++;
                }
            }

            //subscribe property panel to all elements
            SelectedDrawElements.ClearItemUIs();
            Debug.Log(elements.Count());
            Observable.Zip(elements.Select(elt => elt.OnClickPropertiesObservable())).Subscribe(DrawElements =>
            {
                Debug.Log("all clicked elements"+DrawElements.Count());
                SelectedDrawElements.AddItems(DrawElements
                    .Select(elts => Tuple.Create(elts.GetTextForPropertiesPanel(), (object)elts.Properties)));
            });
        }
        else
        {
            Debug.Log("package are null");
        }

    }


    void Login()
    {
        var authCode = AuthCodeInputField.text;
        WebService.LoginWithAuthCode(authCode).DoOnError(error => GUIFactory.instance.MyModalWindow.Show(error.Message, true))
            .Subscribe(webRequest => GUIFactory.instance.MyModalWindow.Show("Login succeeded", true));
    }



}
