using System.Collections;
using System.Collections.Generic;
using IO;
using UnityEngine;
using System.Linq;
using UniRx;
using IMKL_Logic;
using MoreLinq;

public class Test : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        var panel = GUIFactory.CreateMultiSelectPanel(new Vector2(50, 50));

        panel.AddItems(IMKLParser.GetAllXMLFiles().Select(f => Tuple.Create(f.Name, f.FullName)));

        var drawElementsObs = panel.OnSelectedItemsAsObservable().ObserveOn(Scheduler.ThreadPool)
                    .Select(items => IMKLParser.Parse(items.Select(i => i.GetText().Item2))).Publish();
        drawElementsObs.Subscribe(elts => IMKL_Geometry.Draw(elts));
        drawElementsObs.ObserveOnMainThread().Do(elts => elts.ForEach(elt => elt.Init()));
        drawElementsObs.Connect();


    }
}
