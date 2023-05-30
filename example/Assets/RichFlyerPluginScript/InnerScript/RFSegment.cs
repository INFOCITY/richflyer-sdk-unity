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
        [SerializeField]
        private string Name;

        [SerializeField]
        private string Value;

        [NonSerialized]
        private string   StringValue;
        [NonSerialized]
        private bool     BoolValue;
        [NonSerialized]
        private long      NumberValue;
        [NonSerialized]
        private DateTime DateValue;

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

        public RFSegment(string name, long value)
        {
            this.Name = name;
            setupValue(value);
        }

        public RFSegment(string name, DateTime value)
        {
            this.Name = name;
            setupValue(value);
        }

        public string getName()
        {
            return this.Name;
        }

        public string getStringValue()
        {
            return this.StringValue;
        }

        public bool getBoolValue()
        {
            return this.BoolValue;
        }

        public long getNumberValue()
        {
            return this.NumberValue;
        }

        public DateTime getDateValue()
        {
            return this.DateValue;
        }
        

        private void setupValue(object value)
        {
            System.Type type = value.GetType();
            if (type == typeof(string))
            {
                this.Value = (string)value;
                this.StringValue = (string)value;
            }
            else if (type == typeof(long))
            {
                this.Value = ((long)value).ToString();
                this.StringValue = this.Value;
                this.NumberValue = (long)value;
                this.BoolValue = ((long)value > 0);
            }
            else if (type == typeof(bool))
            {
                this.Value = (bool)value ? "true" : "false";
                this.StringValue = this.Value;
                this.NumberValue = (bool)value ? 1 : 0;
                this.BoolValue = (bool)value;            
            }
            else if (type == typeof(DateTime))
            {
                DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime dateTime = TimeZoneInfo.ConvertTimeToUtc((DateTime)value);
                TimeSpan elapsedTime = dateTime - unixEpoch;

                long unixTimestamp = (long)elapsedTime.TotalSeconds;
                this.Value = unixTimestamp.ToString();
                this.NumberValue = unixTimestamp;
                this.StringValue = this.Value;
                this.DateValue = (DateTime)value;
            }

        }

    }
}
