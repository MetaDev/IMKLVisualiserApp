using System.Collections;
using System.Collections.Generic;
using IO;
using UnityEngine;
using System.Linq;
using UniRx;
using IMKL_Logic;
using Utility;
public class Test : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

        var panel = GUIFactory.CreateMultiSelectPanel(new Vector2(50, 50));

        panel.AddItems(IMKLParser.GetAllXMLFiles().Select(f => Tuple.Create(f.Name, f.FullName)));

        var drawElementsObs = panel.OnSelectedItemsAsObservable().Select(items =>
         IMKLParser.Parse(items.Select(i => i.GetText().Item2)));

        drawElementsObs.ObserveOnMainThread().Subscribe(elts => elts.Subscribe(elt => elt.Draw()));

        drawElementsObs.Subscribe(
                    eltbuttonstream => eltbuttonstream.ToList().Subscribe(
                        elts => IMKL_Geometry.Draw(elts)));

    }

    // Update is called once per frame
    void Update()
    {

    }
}
