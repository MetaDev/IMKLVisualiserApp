﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UniRx;
using MoreLinq;
public class MultiSelectPanel : MonoBehaviour
{
    private List<MultiSelectItem> itemUIs = new List<MultiSelectItem>();
    // Use this for initialization
    public Button[] confirm;
    public ToggleGroup Group;
    public ScrollRect Scroll;

    public RectTransform contentView
    {
        get { return Scroll.content.GetComponent<RectTransform>(); }
    }


    public IObservable<IEnumerable<MultiSelectItem>> OnSelectedItemsAsObservable(int buttonIndex)
    {
        if (confirm[buttonIndex] != null)
        {
            return confirm[buttonIndex].OnClickAsObservable().Select(_ => itemUIs.Where(i => i.IsSelected()));
        }else{
            throw new MissingComponentException("Not possible to observe multiselect panel selected items without OK button.");
        }
    }
    MultiSelectItem _InitItemUI()
    {
        var go = Instantiate(Resources.Load("GUI/MultiSelectItemPrefab")) as GameObject;
        go.SetActive(true);
        var itemUI = go.GetComponent<MultiSelectItem>();
        itemUI.transform.SetParent(contentView, false);
        return itemUI;
    }
    MultiSelectItem InitItemUI(string label, object content, bool interactable)
    {
        var itemUI = _InitItemUI();
        itemUI.Init(label, content, interactable, Group);
        return itemUI;
    }

    public void AddItems(IEnumerable<Tuple<string, System.Object, bool>> items)
    {
        ClearItemUIs();
        //add new ones
        //the togglegroup has to be created in superclass because Start() cannot be overridden
        itemUIs = items.Select(item => InitItemUI(item.Item1, item.Item2, item.Item3)).ToList();

    }
    public void ClearItemUIs()
    {
        //remove items, and delete them
        contentView.DetachChildren();
        itemUIs.ForEach(iUI => iUI.Destroy());
    }
    public void AddItems(IEnumerable<Tuple<string, System.Object>> items)
    {
        //add new ones
        //the togglegroup has to be created in superclass because Start() cannot be overridden
        itemUIs = items.Select(item => InitItemUI(item.Item1, item.Item2, true)).ToList();

    }



}
