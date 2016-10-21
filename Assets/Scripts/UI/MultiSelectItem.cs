using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
[RequireComponent(typeof(Button), typeof(Image))]
public class MultiSelectItem : MonoBehaviour
{
    public object content
    {
        get;
        private set;
    }
    public Color UnSelected;
    public Color Selected;
    public Color Disabled;
    public string label
    {
        get;
        private set;
    }
    Button _Button;
    public Button GetButton()
    {
        if (_Button == null)
        {
            _Button = GetComponent<Button>();
        }
        return _Button;
    }
    Image image;
    bool isOn;
    bool Interactable;
    bool Toggle;
    public void Init(string label, object content, bool interactable = true, bool toggle = true)
    {
        base.transform.FindChild("Label").GetComponent<Text>().text = label;
        this.label = label;
        this.content = content;
        this.Interactable = interactable;
        this.Toggle = toggle;
    }
    public IObservable<MultiSelectItem> OnClickSelf()
    {
        return GetButton().OnPointerDownAsObservable().Select(_ => this);
    }
    public void Destroy()
    {
        if (this != null && this.gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    public bool IsSelected()
    {
        return isOn;
    }
    public void SetTextColor(Color col)
    {
        base.transform.FindChild("Label").GetComponent<Text>().color = col;
    }
    void SetColor(bool interactable, bool isOn)
    {
        if (!interactable)
        {
            this.image.color = Disabled;
        }
        else if (isOn)
        {
            this.image.color = Selected;
        }
        else
        {
            this.image.color = UnSelected;
        }
    }
    void Start()
    {
        GetButton().interactable = Interactable;

        image = GetComponent<Image>();

        SetColor(Interactable, isOn);
        if (Toggle)
        {
            OnClickSelf().Subscribe(_ =>
                            {
                                this.isOn = !isOn;
                                SetColor(Interactable, isOn);
                            });
        }
       
    }
}
