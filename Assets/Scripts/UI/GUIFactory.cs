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
              return OnlinePackagesPanel.OnSelectedItemsAsObservable(0);
          }).Subscribe(items =>
          {
              int i = 0;
              MyModalWindow.Show("Please wait while downloading selected package data", ModalWindow.ModalType.MESSAGE);
              items.Select(item => item.content).Cast<IMKLPackage>()
                            .Select(package => WebService.DownloadXMLForIMKLPackage(package).Select(xmls => Tuple.Create(package, xmls)))
                            .Merge()
                            .Select((_,idx) =>
                            {
                                MyModalWindow.Show("Please wait while downloading selected package data.\n" +
                                "Currently downloading package " + idx + " out of " + items.Count(), ModalWindow.ModalType.MESSAGE);
                                return _;
                            })
                            .ToList()
                            .DoOnError(error => MyModalWindow.Show(error.Message, ModalWindow.ModalType.OK))
                            .DoOnCompleted(() => { MyModalWindow.Close(); ClickTab(1); })
                            .Subscribe(packagesAndXmls =>
                            {
                                //save xml docs in package instance
                                packagesAndXmls.ForEach(pAndXs => pAndXs.Item1.KLBResponses = pAndXs.Item2);
                                //serialise package instances
                                Serializer.SaveAdditionalIMKLPackages(packagesAndXmls.Select(pAndX => pAndX.Item1));
                            });
          });
    }
    public static GUIFactory instance;
    List<DrawElement> ActiveDrawElements;

    void InitLocalPackagePanel()
    {
        //init draw panel with saved packages
        Serializer.LoadAllIMKLPackages();
        //refresh the draw panel on the content of the imkl packages
        Serializer.PackagesChanged().Subscribe(packages => AddLocalPackages(packages));
        //download maps button
        // LocalPackagesPanel.OnSelectedItemsAsObservable(1)
        // .Do(_ => MyModalWindow.Show("Please wait while maps for the package are being downloaded.", ModalWindow.ModalType.MESSAGE))
        // .Subscribe(items => items.Select(item => (IMKLPackage)item.content).Cast<IMKLPackage>()
        //     .ForEach(package =>
        //     {
        //         var progressNotifier = new ScheduledNotifier<float>();
        //         progressNotifier.Subscribe(prog => MyModalWindow.Show(
        //             "Please wait while maps for the package are being downloaded.\nPackage :" + package.ID +
        //                                                     "progress: " + prog.ToString("0%"), ModalWindow.ModalType.MESSAGE));
        //         Observable.FromCoroutine(() =>MapHelper.DownloadTilesForPackage(package, progressNotifier).ToYieldInstruction())
        //         .DoOnCompleted(() => MyModalWindow.Close())
        //         .Subscribe();
        //         package.HasAllMaps.Value=true;
        //     }));
        //delete package button
        LocalPackagesPanel.OnSelectedItemsAsObservable(1).Subscribe(items =>
        {
            GUIFactory.instance.MyModalWindow
            .Show("Are you sure you want to deleted " + items.Count() + " packages?", ModalWindow.ModalType.OKCANCEL);
            GUIFactory.instance.MyModalWindow.GetModalButtonObservable().Where(button => button == ModalWindow.ModalReturn.OK)
            .ObserveOn(Scheduler.ThreadPool).Subscribe(_ =>
                Serializer.DeletePackages(items.Select(item => item.content).Cast<IMKLPackage>()));
        });



        //draw button
        LocalPackagesPanel.OnSelectedItemsAsObservable(0).Where(items => items.Count() > 1)
        .Subscribe(_ => MyModalWindow.Show("Only one package at a time can be drawn.", ModalWindow.ModalType.OK));
        LocalPackagesPanel.OnSelectedItemsAsObservable(0).Where(items => items.Count() == 1)
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
                Observable.FromCoroutine(() => DrawPackage(items.Select(item => item.content).Cast<IMKLPackage>().First(), progressNotifier))
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
        InitLocalPackagePanel();
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

    void AddLocalPackages(IEnumerable<IMKLPackage> packages)
    {
        LocalPackagesPanel.ClearItemUIs();
        LocalPackagesPanel.AddItems(packages.Where(package => package.KLBResponses != null)
                    .Select(package =>
                    {
                        return Tuple.Create(package.Reference, (object)package,true);
                    }))
                    .Zip(packages,(itemUI,package)=>package.HasAllMaps.AsObservable()
                    .Do(_=>Debug.Log("bazaar"+_))
                    .Where(hasMaps=>hasMaps)
                    .Subscribe(_=>itemUI.SetTextColor(Color.green)));
        
    }


    IEnumerator DrawPackage(IMKLPackage package, IProgress<float> progressNotifier)
    {
        if (package != null)
        {
            List<DrawElement> elements = new List<DrawElement>();

            var drawElements = IMKLParser.ParseDrawElements(package.GetKLBXML())
                         .Where(elts => elts != null);
            MapHelper.ZoomAndCenterOnElements(package.MapRequestZone, drawElements);
            int i = 0;
            elements.AddRange(drawElements);
            foreach (DrawElement elt in drawElements)
            {
                elt.Init();
                //draw X elements per frame
                if (i % 10 == 0)
                {
                    progressNotifier.Report((float)i / drawElements.Count());
                    yield return null;
                }
                i++;
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
