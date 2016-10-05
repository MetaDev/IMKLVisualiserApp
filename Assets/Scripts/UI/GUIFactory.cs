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
    public MultiSelectPanel IMKLPackageInfoPanel;
    public MultiSelectPanel IMKLPackageDrawPanel;


    public void LoadPacketInfo()
    {
        WebService.GetAllIMKLPackage().Select(packages =>
          {
              IMKLPackageInfoPanel.AddItems(packages.Select(package => Tuple.Create(package.Reference, (object)package, package.DownloadIMKL)));
              //every time ok is clicked the selected items are streamed
              return IMKLPackageInfoPanel.OnSelectedItemsAsObservable();
          }).Subscribe(itemsObs => itemsObs.Subscribe(items =>
          {
              //   List<IObservable<Tuple<IMKLPackage,List<XDocument>>>> packagesObs = new List<IObservable<Tuple<IMKLPackage,List<XDocument>>>>();
              items.Select(item => item.content).Cast<IMKLPackage>()
                            .Select(package => WebService.DownloadXMLForIMKLPackage(package)
                            .Select(xmls => Tuple.Create(package, xmls)))
                            .Merge().ToList().Subscribe(packagesAndXmls =>
                            {
                                Debug.Log(packagesAndXmls.Count());
                                //save xml docs in package instance
                                packagesAndXmls.ForEach(pAndXs => pAndXs.Item1.KLBResponses = pAndXs.Item2);
                                //serialise package instances
                                Serializer.SaveIMKLPackages(packagesAndXmls.Select(pAndX => pAndX.Item1));
                            });
          }));
    }

    void Start()
    {
        //init draw panel with saved packages
        AddDrawPackages(Serializer.LoadAllIMKLPackages());
                //refresh the draw panel on the content of the imkl packages
        Serializer.PackagesChanged().Subscribe(packages => AddDrawPackages(packages));
        //TODO delete previous drawing on map
        IMKLPackageDrawPanel.OnSelectedItemsAsObservable().Subscribe(
            items => items.Select(item => item.content).Cast<IMKLPackage>()
            .ForEach(package => DrawPackages(package)));
    }
    void AddDrawPackages(IEnumerable<IMKLPackage> packages)
    {
        IMKLPackageDrawPanel.AddItems(packages.Where(package => package.KLBResponses != null)
                    .Select(package => Tuple.Create(package.Reference, (object)package, true)));
    }

    //TODO Add loading screen 

    void DrawPackages(IMKLPackage package)
    {
        if (package != null)
        {
            var drawElements = IMKLParser.ParseDrawElements(package.GetKLBXML())
                             .Where(elts => elts != null);
            drawElements.ForEach(elt => elt.Init());
            MapHelper.ZoomAndCenterOnElements(drawElements);
        }
        else
        {
            Debug.Log("package are null");
        }

    }


    public void LoginPress()
    {
        var authCode = menuPanel.AuthCodeInputField.text;
        WebService.LoginWithAuthCode(authCode).Subscribe(webRequest => Debug.Log("login response code: " + webRequest.responseCode));
    }



}
