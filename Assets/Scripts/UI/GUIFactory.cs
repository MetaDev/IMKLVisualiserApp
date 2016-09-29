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

        var packageObs = WebService.GetAllIMKLPackage().Select(packages =>
          {

              IMKLPackagePanel.AddItems(packages.Select(package => Tuple.Create(package.ID, (object)package)));
              IMKLPackagePanel.Show();
              //every time ok is clicked the selected items are streamed
              return IMKLPackagePanel.OnSelectedItemsAsObservable();
          }).Select(itemsObs=> itemsObs.Select(items => items.Select(item => item.content).Cast<IMKLPackage>()));

        var packageXMLObs = packageObs.Select(packageObser =>
            {
                return packageObser.Select(packages => WebService.DownloadXMLForIMKLPackage(packages));
                //download XMLs in packages
            }).Do(_ => Debug.Log("packages loaded"));
        packageXMLObs.Zip(packageObs, (_, packages) => packages).Do(_ => Debug.Log("zipstart loaded")).Subscribe(ackageObser =>
          {
              Debug.Log("drawing starts");

              //Serializer.SaveAllIMKLPackages(mrs);
              ackageObser.Subscribe(packages=>{
                  Debug.Log(packages.Count());
                  DrawPackages(packages);
              });
          });
        //TODO Add loading screen 
        //TODO serialize IMKLPackages

    }
    void DrawPackages(IEnumerable<IMKLPackage> packages)
    {
        if (packages != null)
        {
            packages.ForEach(package => IMKLParser.ParseDrawElements(package.KLBResponses).ForEach(elt => elt.Init()));
            // var parseElemenstObs = packages.ToObservable().ObserveOn(Scheduler.ThreadPool)
            //                  .Select(package => IMKLParser.ParseDrawElements(package.KLBResponses))
            //                  .Where(elts => elts != null).ObserveOnMainThread().Publish();
            // parseElemenstObs.Subscribe(_ => Debug.Log(_));
            // parseElemenstObs.Subscribe(elts => elts.ForEach(elt => elt.Init()));
            // parseElemenstObs.Subscribe(elts => MapHelper.ZoomAndCenterOnElements(elts));
            // parseElemenstObs.Connect();
            Debug.Log("drawing done"+packages.Count());
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
