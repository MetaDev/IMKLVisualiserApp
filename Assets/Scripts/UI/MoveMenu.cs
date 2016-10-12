using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;

namespace UI
{
    public class MoveMenu : MonoBehaviour
    {
        public Vector2 OpenPosition;
        public Vector2 ClosedPosition;
        public bool open;
        public void SetPosition()
        {
            if (open)
            {
                this.GetComponent<RectTransform>().anchoredPosition = OpenPosition;
            }
            else
            {
                this.GetComponent<RectTransform>().anchoredPosition = ClosedPosition;
            }
            open = !open;
        }



    }

}
