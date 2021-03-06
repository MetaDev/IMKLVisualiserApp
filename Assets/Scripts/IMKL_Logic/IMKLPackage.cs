﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniRx;
using UnityEngine;
using System.Linq;
using System.Xml;

namespace IMKL_Logic
{
    [Serializable]
    public class IMKLPackage
    {
        public string ZIPUrl
        {
            get;
            private set;
        }
        public string ID
        {
            get;
            private set;
        }
        public string Reference
        {
            get;
            private set;
        }
        [JsonIgnore]
        public IReactiveProperty<bool> HasAllMaps
        {
            get;
            private set;
        }

        public enum MapRequestStatus
        {
            AVAILABLE, NONAVAILABLE
        }
        [JsonIgnore]
        private MapRequestStatus Status = MapRequestStatus.AVAILABLE;

        public IEnumerable<Vector2d> MapRequestZone
        {
            get;
            private set;
        }
        [JsonIgnore]
        public bool DownloadIMKL
        {
            get { return (Status == IMKLPackage.MapRequestStatus.AVAILABLE); }
        }
        public IEnumerable<String> KLBResponses
        {
            get;
            set;
        }
        public IEnumerable<XDocument> GetKLBXML()
        {
            try
            {
                return KLBResponses.Select(resp => XDocument.Parse(resp));

            }
            catch (XmlException e)
            {
                GUIFactory.instance.MyModalWindow.Show("Something whent wrong when parsing xml from stored: " + e.Message,
                        ModalWindow.ModalType.OK);
                Debug.Log("Something whent wrong when parsing xml from stored: " + e.Message);
                return null;
            }
        }
        public override string ToString()
        {
            return "ID: " + ID + ", Reference: " + ", Status: " + Status + "MaprequestZone" + Utility.StringParser.EnumerableString(MapRequestZone);
        }

        public IMKLPackage(string id, string reference, string status, IEnumerable<Vector2d> mapRequestZone, string zipURL)
        {
            this.ZIPUrl = zipURL;
            this.ID = id;
            this.Reference = reference;
            //throws nullpointer becuase status is not serialised
            if (status != null)
            {
                this.Status = status.EndsWith("available") ? MapRequestStatus.AVAILABLE : MapRequestStatus.NONAVAILABLE;
            }
            this.MapRequestZone = mapRequestZone;

            this.HasAllMaps = new ReactiveProperty<bool>(MapHelper.GetRequiredTilesForPackage(this).Count() == 0);
        }

    }

}
