//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

using System;
using UnityEngine;

namespace RichFlyer
{
    public enum RFContentType
    {
        RFContentTypeText,  //テキストのみ
        RFContentTypeImage, //静止画像
        RFContentTypeGif,   //GIF画像
        RFContentTypeMovie  //動画
    }

    [Serializable]
    public class RFAction
    {
        public string Title;
        public string Type;
        public string Value;
        public int Index;

        public RFAction(string title, string type, string value, int index)
        {
            this.Title = title;
            this.Type = type;
            this.Value = value;
            this.Index = index;
        }
    }

    [Serializable]
    public class RFContent
    {
        public string Title;    
        public string Body;
        public string NotificationId;
        public string ImagePath;
        public long ReceivedDateUnixTime;
        public long NotificationDateUnixTime;
        public RFContentType Type;

        [SerializeField] public RFAction[] ActionButtons;
        public string ExtendedProperty;

        public DateTime getReceivedDate()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(this.ReceivedDateUnixTime);
            return dateTimeOffset.DateTime;
        }

        public DateTime getNotificationDate()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(this.NotificationDateUnixTime);
            return dateTimeOffset.DateTime;
        }
    }
}