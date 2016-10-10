using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using MoreLinq;

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

        public Dictionary<string, string> Properties{
            protected set; get;
        }

        public abstract void Init();
        public DrawElement(Dictionary<string, string> properties)
        {
            this.Properties = properties;
        }
    }
}

