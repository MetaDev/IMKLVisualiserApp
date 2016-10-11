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

    public SideMenuRight right;
    public MenuTabAndPanel[] MenuTabAndPanels;
    public MultiSelectPanel OnlinePackagesPanel;
    public MultiSelectPanel LocalPackagesPanel;
    

    public Button RefreshOnlinePackages;

    public ModalWindow MyModalWindow;

    
   
    

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
        //online packages panel
        RefreshOnlinePackages.OnClickAsObservable().Subscribe(_ => LoadPacketInfo());

        //Tab menu
        Enumerable.Range(0, MenuTabAndPanels.Length)
        .ForEach(i =>MenuTabAndPanels[i].TabButton.OnClickAsObservable()
        .Subscribe(_=>ClickTab(i)));
        //tab menu init
        //first tab starts open
        ClickTab(0);

       

    }
    void ClickTab(int i)
    {
    
        Enumerable.Range(0, MenuTabAndPanels.Length).ForEach(j =>
        {
            MenuTabAndPanels[j].TabButton.interactable = j!=i;
            MenuTabAndPanels[j].Panel.SetActive(j==i);
        });
    }

    void AddDrawPackages(IEnumerable<IMKLPackage> packages)
    {
        LocalPackagesPanel.AddItems(packages.Where(package => package.KLBResponses != null)
                    .Select(package =>
                    {
                        return Tuple.Create(package.Reference, (object)package);
                    }));
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
            right.ElementPanel.SubscribeToDrawnElements(elements);
            
        }
        else
        {
            Debug.Log("package are null");
        }

    }


   



}
