﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UniRx;
using UnityEngine;

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
        public enum MapRequestStatus
        {
            AVAILABLE, NONAVAILABLE
        }
        private MapRequestStatus Status;
     
        public IEnumerable<Vector2d> MapRequestZone
        {
            get;
            private set;
        }
        public bool DownloadIMKL{
            get{return (Status == IMKLPackage.MapRequestStatus.AVAILABLE);}
        }
        public IEnumerable<XDocument> KLBResponses
        {
            get;
            set;
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
            this.Status = status.EndsWith("available") ? MapRequestStatus.AVAILABLE : MapRequestStatus.NONAVAILABLE;
            this.MapRequestZone = mapRequestZone;
        }

    }

}