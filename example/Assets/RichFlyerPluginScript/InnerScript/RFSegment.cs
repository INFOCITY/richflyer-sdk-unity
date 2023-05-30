//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

using System;
using UnityEngine;

namespace RichFlyer
{
    [Serializable]
    public class RFSegment
    {
        public string Name;

        [SerializeField]
        private string Value;

        [NonSerialized]
        public string   StringValue;
        [NonSerialized]
        public bool     BoolValue;
        [NonSerialized]
        public int      IntValue;
        [NonSerialized]
        public DateTime DateValue;

        public RFSegment(string name, string value)
        {
            this.Name = name;
            setupValue(value);
        }

        public RFSegment(string name,  bool value)
        {
            this.Name = name;
            setupValue(value);
        }

        public RFSegment(string name, int value)
        {
            this.Name = name;
            setupValue(value);
        }

        public RFSegment(string name, DateTime value)
        {
            this.Name = name;
            setupValue(value);
        }

        private void setupValue(object value)
        {
            System.Type type = value.GetType();
            if (type == typeof(string))
            {
                this.Value = (string)value;
                this.StringValue = (string)value;
            }
            else if (type == typeof(int))
            {
                this.Value = ((int)value).ToString();
                this.StringValue = this.Value;
                this.IntValue = (int)value;
                this.BoolValue = ((int)value > 0);
            }
            else if (type == typeof(bool))
            {
                this.Value = (bool)value ? "true" : "false";
                this.StringValue = this.Value;
                this.IntValue = (bool)value ? 1 : 0;
                this.BoolValue = (bool)value;            
            }
            else if (type == typeof(DateTime))
            {
                DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime dateTime = TimeZoneInfo.ConvertTimeToUtc((DateTime)value);
                TimeSpan elapsedTime = dateTime - unixEpoch;

                long unixTimestamp = (long)elapsedTime.TotalSeconds;
                this.Value = unixTimestamp.ToString();
                this.StringValue = this.Value;
                this.DateValue = (DateTime)value;
            }

        }

    }
}
