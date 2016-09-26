using System.Collections;
using System.Collections.Generic;
using IO;
using UnityEngine;
using System.Linq;
using UniRx;
using IMKL_Logic;
using Utility;
using MoreLinq;

public class Test : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        //TODO
        //unreadable and dangerous code convert to OO and coroutines-> requires IMKLParser.Parse to return a list and IEnumerable
        var panel = GUIFactory.CreateMultiSelectPanel(new Vector2(50, 50));

        panel.AddItems(IMKLParser.GetAllXMLFiles().Select(f => Tuple.Create(f.Name, f.FullName)));

        var drawElementsObs = panel.OnSelectedItemsAsObservable().Select(items => IMKLParser.Parse(items.Select(i => i.GetText().Item2))).Publish();
        drawElementsObs.Subscribe(elts => IMKL_Geometry.Draw(elts));
        drawElementsObs.ObserveOnMainThread().Subscribe(elts => elts.ForEach(elt => elt.Init()));

        drawElementsObs.Connect();


    }


    // Update is called once per frame
    void Update()
    {

    }
}
