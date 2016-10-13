using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
[RequireComponent(typeof(Canvas))]
public class ModalWindow : MonoBehaviour
{
    public enum ModalType
    {
        MESSAGE, OK, OKCANCEL
    }
    public Button OK;
    public Button Cancel;
    public Text Message;
    // Use this for initialization
    void Start()
    {
        OK.OnClickAsObservable().Subscribe(_ => Close());
        Cancel.OnClickAsObservable().Subscribe(_ => Close());
        var okObs = OK.OnClickAsObservable().Select(_ => 1);
        var cancelObs = OK.OnClickAsObservable().Select(_ => 0); ;
        ModalButtonObservable = okObs.Merge(cancelObs);
        
    }
    IObservable<int> ModalButtonObservable;
    public IObservable<int> GetModalButtonObservable(){
        //if the modal window has never been activated before, than immediately retrieving observable after show will throw a null pointer
        if (ModalButtonObservable==null){
            Start();
        }
        return ModalButtonObservable.First();
    }

    public void Show(string message, ModalType type)
    {
        Message.text = message;
        switch (type)
        {
            case ModalType.MESSAGE:
                OK.gameObject.SetActive(false);
                Cancel.gameObject.SetActive(false);
                break;
            case ModalType.OKCANCEL:
                OK.gameObject.SetActive(true);
                Cancel.gameObject.SetActive(true);
                break;
            case ModalType.OK:
                OK.gameObject.SetActive(true);
                Cancel.gameObject.SetActive(false);
                break;
        }
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }


}
