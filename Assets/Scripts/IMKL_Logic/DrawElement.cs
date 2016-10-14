using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using MoreLinq;
using UniRx;
namespace IMKL_Logic
{
    public abstract class DrawElement
    {
        static OnlineMapsRange drawRange = new OnlineMapsRange(15, 20);
        public static OnlineMapsRange DrawRange
        {
            get
            {
                return drawRange;
            }
        }
        public GameObject GO{
            protected set; get;
        }
        public List<string[]> Properties
        {
            protected set; get;
        }
        protected bool IsDestroyed(){
            return GO==null;
        }
        protected static float clickLineSensitivity = 150.0f;
        public abstract string GetTextForPropertiesPanel();
        protected abstract bool ClickWithinDistance(Vector3 worldMousePos, float maxDist);
        public IObservable<DrawElement> OnClickPropertiesObservable()
        {
            //check if not clicked
            //works for mobile devices as well
            //TODO distinguish between drag and click, only work on click see UNirx drag and drop
            var obs = Observable.EveryUpdate()
             .Where(_ => Input.GetMouseButtonDown(0))
             .Select(_ => ClickWithinDistance(InputMousePositionToWorld(Input.mousePosition), clickLineSensitivity / OnlineMaps.instance.zoom)
             ? this : null);
            //destroy observable when element is destroyed
            obs.Subscribe().AddTo(GO);
            return obs;
        }
        Vector3 InputMousePositionToWorld(Vector3 inputMousePos)
        {
            //works for mobile devices as well
            //find closest point to line
            var mousScreenPos = inputMousePos;
            mousScreenPos.z = 0;
            var mousePos = Camera.main.ScreenToWorldPoint(mousScreenPos);
            mousePos.z = 0;
            return mousePos;
        }
        public abstract void Init();
        public DrawElement(List<string[]> properties)
        {
            this.Properties = properties;
        }
    }
}

