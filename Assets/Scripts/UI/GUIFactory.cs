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
    public MultiSelectPanel IMKLPackagePanel;


    public void ShowAllMRPanel()
    {
        //TODO something goes wrong 

        WebService.GetAllIMKLPackage().Select(packages =>
          {

              IMKLPackagePanel.AddItems(packages.Select(package => Tuple.Create(package.Reference, (object)package)));
              IMKLPackagePanel.Show();
              //every time ok is clicked the selected items are streamed
              return IMKLPackagePanel.OnSelectedItemsAsObservable();
          }).Subscribe(itemsObs => itemsObs.Subscribe(items =>
          {
              items.Select(item => item.content).Cast<IMKLPackage>().ForEach(package =>
              {
                  //after downloading the packages draw their elements
                  Debug.Log("download started");

                  WebService.DownloadXMLForIMKLPackage(package).Where(xmls => xmls != null).Do(xmls => Debug.Log("nr of xmls"+xmls.Count()))
                  .Subscribe(xmls =>
                         {
                             //cache xdocs 
                             package.KLBResponses=xmls;
                             DrawPackages(package);
                         });
              });
          }));
        //download XMLs in packages
    }

    //TODO Add loading screen 
    //TODO serialize IMKLPackages

    void DrawPackages(IMKLPackage package)
    {
        if (package != null)
        {
            // packages.ForEach(package => IMKLParser.ParseDrawElements(package.KLBResponses).ForEach(elt => elt.Init()));
            var drawElements = IMKLParser.ParseDrawElements(package.KLBResponses)
                             .Where(elts => elts != null);
            drawElements.ForEach(elt => elt.Init());
            MapHelper.ZoomAndCenterOnElements(drawElements);
        }
        else
        {
            Debug.Log("package are null");
        }

    }
    public void ShowAllIMKLPanel()
    {
        if (Serializer.LoadAllIMKLPackages() == null)
            return;
        IMKLPackagePanel.AddItems(Serializer.LoadAllIMKLPackages()
        .Select(package => Tuple.Create(package.ID, (object)package)));
        IMKLPackagePanel.Show();
        //TODO if something went when reading xml output message in where null
        var drawElementsObs = IMKLPackagePanel.OnSelectedItemsAsObservable().ObserveOn(Scheduler.ThreadPool)
                    .Select(items => IMKLParser.ParseDrawElements(items
                    .Select(item => item.content).Cast<XDocument>()))
                    .Where(elts => elts != null).ObserveOnMainThread().Publish();
        drawElementsObs.Subscribe(elts => elts.ForEach(elt => elt.Init()));
        drawElementsObs.Subscribe(elts => MapHelper.ZoomAndCenterOnElements(elts));

        drawElementsObs.Connect();
    }

    public void LoginPress()
    {
        var authCode = menuPanel.authCodeInputField.text;
        WebService.LoginWithAuthCode(authCode).Subscribe(webRequest => Debug.Log("login response code: " + webRequest.responseCode));
    }



}
