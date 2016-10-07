using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using MoreLinq;

namespace IMKL_Logic
{
    public abstract class DrawElement : MonoBehaviour
    {
        static OnlineMapsRange drawRange = new OnlineMapsRange(15, 20);
        public static OnlineMapsRange DrawRange
        {
            get{
                return drawRange;
            }
        }

       
        public abstract void Init();

    }
}

