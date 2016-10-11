﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using IMKL_Logic;
using System.Linq;

public class DrawElementPanel : MonoBehaviour
{
    public GameObject DrawElementProperties;
    public Button DrawElementsSelectedOpen;
    public MultiSelectPanel DrawElementsSelected;
    public Button DrawElementPropertiesBack;

    public Text DrawElementPropertiesText;

    // Use this for initialization
    void Start()
    {
        DrawElementPropertiesBack.OnClickAsObservable().Subscribe(_ =>
        {
            DrawElementsSelected.gameObject.SetActive(true);
            DrawElementProperties.SetActive(false);
        });
        //Show selected drawn elements stuff
        DrawElementsSelected.OnSelectedItemsAsObservable().Subscribe(items =>
       {
           var properties = (Dictionary<string, string>)items.First().content;
           DrawElementPropertiesText.text = string.Join("\n", properties.ToList()
           .Select(pair => pair.Key + ": " + pair.Value).ToArray());
           DrawElementProperties.SetActive(true);
           DrawElementsSelected.gameObject.SetActive(false);
       });
        //TODO add back button to eltproperty UI
        //draw element properties
        DrawElementsSelected.gameObject.SetActive(true);
        DrawElementProperties.SetActive(false);
    }

    public void SubscribeToDrawnElements(List<DrawElement> elements)
    {
        //show the selected draw elements panel
        DrawElementsSelected.gameObject.SetActive(true);
        //subscribe property panel to all elements
        DrawElementsSelected.ClearItemUIs();
        Observable.Zip(elements.Select(elt => elt.OnClickPropertiesObservable()))
        .Select(elts => elts.Where(elt => elt != null))
        .Where(elts => elts.Count() > 0).Subscribe(DrawElements =>
          {
            //the observable returns the elements properties if the element has been clicked and null otherwise
            DrawElementsSelected.AddItems(DrawElements
        .Select(elts => Tuple.Create(elts.GetTextForPropertiesPanel(), (object)elts.Properties)));
          });
    }
}