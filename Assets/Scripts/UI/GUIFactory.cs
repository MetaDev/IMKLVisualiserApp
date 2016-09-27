using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IMKL_Logic;
using IO;
using MoreLinq;
using UniRx;
using UnityEngine;

public class GUIFactory 
{
   
    public static MultiSelectPanel CreateMultiSelectPanel(Vector2 screenPos)
    {
        var prefab = Resources.Load("GUI/MultiSelectPanel") as GameObject;
        var go = GameObject.Instantiate(prefab);
        go.GetComponent<RectTransform>().position = screenPos;
        return go.GetComponent<MultiSelectPanel>();
    }
    public static void ShowAllIMKLPanel()
    {
        var panel = GUIFactory.CreateMultiSelectPanel(new Vector2(50, 50));

        panel.AddItems(IMKLParser.GetAllXMLFiles().Select(f => Tuple.Create(f.Name, f.FullName)));

        var drawElementsObs = panel.OnSelectedItemsAsObservable().ObserveOn(Scheduler.ThreadPool)
                    .Select(items => IMKLParser.Parse(items.Select(i => i.GetText().Item2))).Publish();
        drawElementsObs.Subscribe(elts => IMKL_Geometry.Draw(elts));
        drawElementsObs.ObserveOnMainThread().Do(elts => elts.ForEach(elt => elt.Init()));
        drawElementsObs.Connect();
    }
	public static void ShowSettingsPanel(){
		//TODO panel with login flow
		
	}
	
    

}
