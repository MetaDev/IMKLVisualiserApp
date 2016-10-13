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
    [System.Serializable]
    public class MenuTabAndPanel
    {
        public Button TabButton;
        public GameObject Panel;
    }
    public Button FooterURLButton;
    public DrawElementPanel ElementPanel;
    public MenuTabAndPanel[] MenuTabAndPanels;
    public MultiSelectPanel OnlinePackagesPanel;
    public MultiSelectPanel LocalPackagesPanel;


    public Button RefreshOnlinePackages;

    public ModalWindow MyModalWindow;



    void LoadPacketInfo()
    {
        MyModalWindow.Show("Please wait while downloading all available packages information"
                                                                    , ModalWindow.ModalType.MESSAGE);
        WebService.GetAllIMKLPackage()
        .DoOnCompleted(() => MyModalWindow.Close())
            .DoOnError(error => MyModalWindow.Show(error.Message, ModalWindow.ModalType.OK))
            .SelectMany(packages =>
          {
              OnlinePackagesPanel.ClearItemUIs();
              OnlinePackagesPanel.AddItems(packages.Select(package => Tuple.Create(package.Reference, (object)package, package.DownloadIMKL)));
              //every time ok is clicked the selected items are streamed
              return OnlinePackagesPanel.OnSelectedItemsAsObservable();
          }).Subscribe(items =>
          {
              int i = 0;
              MyModalWindow.Show("Please wait while downloading selected package data", ModalWindow.ModalType.MESSAGE);
              items.Select(item => item.content).Cast<IMKLPackage>()
                            .Select(package => WebService.DownloadXMLForIMKLPackage(package).Select(xmls => Tuple.Create(package, xmls)))
                            .Merge()
                            .Do(_ =>
                            {
                                i++;
                                MyModalWindow.Show("Please wait while downloading selected package data.\n" +
                                "Currently downloading package " + i + " out of " + items.Count(), ModalWindow.ModalType.MESSAGE);
                            })
                            .ToList()
                            .DoOnError(error => MyModalWindow.Show(error.Message, ModalWindow.ModalType.OK))
                            .DoOnCompleted(() => { MyModalWindow.Close(); ClickTab(1); })
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
    List<DrawElement> ActiveDrawElements;

    void InitDrawPanel()
    {
        //init draw panel with saved packages
        Serializer.LoadAllIMKLPackages();
        //refresh the draw panel on the content of the imkl packages
        Serializer.PackagesChanged().Subscribe(packages => AddDrawPackages(packages));

        LocalPackagesPanel.OnSelectedItemsAsObservable()
        .Do(_ =>
        {
            MyModalWindow.Show("Please wait while elements from the package are being drawn.", ModalWindow.ModalType.MESSAGE);
            //delete previous drawing on map
            if (ActiveDrawElements != null)
            {
                foreach (DrawElement drawElement in ActiveDrawElements)
                {
                    Destroy(drawElement.GO);
                }
                ActiveDrawElements = null;
            }
        })
        //Add a small delay to show message before draing starts
        .Delay(TimeSpan.FromSeconds(0.2f))
        .Subscribe(
            items =>
            {
                var progressNotifier = new ScheduledNotifier<float>();
                progressNotifier.Subscribe(prog => MyModalWindow.Show(
                    "Please wait while elements from the package are being drawn.\n " + prog.ToString("0%"), ModalWindow.ModalType.MESSAGE));
                Observable.FromCoroutine(() => DrawPackages(items.Select(item => item.content).Cast<IMKLPackage>(), progressNotifier))
                .DoOnError(error => MyModalWindow.Show(error.Message, ModalWindow.ModalType.OK))
                .DoOnCompleted(() => MyModalWindow.Close()).Subscribe();
            });
    }

    void Start()
    {

        //Singleton
        instance = this;
        //zoom and center map on flanders
        MapHelper.ZoomAndCenter(new Vector2(4.2159f, 51.0236f), 10);
        //Draw Panel
        InitDrawPanel();
        //refresh of load packet information
        RefreshOnlinePackages.OnClickAsObservable().Subscribe(_ => LoadPacketInfo());
        //online packages panel
        RefreshOnlinePackages.OnClickAsObservable().Subscribe(_ => LoadPacketInfo());

        //Tab menu
        Enumerable.Range(0, MenuTabAndPanels.Length)
        .ForEach(i => MenuTabAndPanels[i].TabButton.OnClickAsObservable()
        .Subscribe(_ => ClickTab(i)));
        //tab menu init
        //first tab starts open
        ClickTab(0);

        FooterURLButton.OnClickAsObservable().Subscribe(_ => Application.OpenURL("http://www.vianova-systems.be/"));

    }
    void ClickTab(int i)
    {

        Enumerable.Range(0, MenuTabAndPanels.Length).ForEach(j =>
        {
            MenuTabAndPanels[j].TabButton.interactable = j != i;
            MenuTabAndPanels[j].Panel.SetActive(j == i);
        });
    }

    void AddDrawPackages(IEnumerable<IMKLPackage> packages)
    {
        LocalPackagesPanel.ClearItemUIs();
        LocalPackagesPanel.AddItems(packages.Where(package => package.KLBResponses != null)
                    .Select(package =>
                    {
                        return Tuple.Create(package.Reference, (object)package);
                    }));
    }


    IEnumerator DrawPackages(IEnumerable<IMKLPackage> packages, IProgress<float> progressNotifier)
    {
        if (packages != null)
        {
            List<DrawElement> elements = new List<DrawElement>();
            foreach (IMKLPackage package in packages)
            {
                var drawElements = IMKLParser.ParseDrawElements(package.GetKLBXML())
                             .Where(elts => elts != null);
                MapHelper.ZoomAndCenterOnElements(package.MapRequestZone);
                int i = 0;
                elements.AddRange(drawElements);
                foreach (DrawElement elt in drawElements)
                {
                    elt.Init();
                    //draw X elements per frame
                    if (i % 5 == 0)
                    {
                        progressNotifier.Report((float)i / drawElements.Count());
                        yield return null;
                    }
                    i++;
                }
            }
            ActiveDrawElements = elements;
            ElementPanel.SubscribeToDrawnElements(elements);

        }
        else
        {
            Debug.Log("package are null");
        }

    }






}
