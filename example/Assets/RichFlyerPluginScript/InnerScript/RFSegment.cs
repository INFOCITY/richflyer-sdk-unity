//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

using System;
using System.Globalization;
using UnityEngine;

namespace RichFlyer
{
    [Serializable]
    public class RFSegment
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public string Value;

        [NonSerialized]
        private SegmentValueType _valueType;

        [NonSerialized]
        private DateTime _dateValue;

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
            return this.Value;
        }

        public bool getBoolValue()
        {
            if (_valueType == SegmentValueType.String || _valueType == SegmentValueType.Date)
            {
                return false;
            }

            bool boolValue;
            if (bool.TryParse(this.Value, out boolValue))
            {
                return boolValue;
            }

            long numberValue;
            return long.TryParse(this.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out numberValue)
                && numberValue > 0;
        }

        public long getNumberValue()
        {
            if (_valueType == SegmentValueType.String)
            {
                return 0;
            }

            bool boolValue;
            if (bool.TryParse(this.Value, out boolValue))
            {
                return boolValue ? 1 : 0;
            }

            long numberValue;
            return long.TryParse(this.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out numberValue)
                ? numberValue
                : 0;
        }

        public DateTime getDateValue()
        {
            if (_valueType == SegmentValueType.Date)
            {
                return _dateValue;
            }

            if (_valueType != SegmentValueType.Unknown)
            {
                return default;
            }

            long unixTimestamp;
            return long.TryParse(this.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out unixTimestamp)
                ? DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).LocalDateTime
                : default;
        }
        

        private void setupValue(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Type type = value.GetType();
            if (type == typeof(string))
            {
                _valueType = SegmentValueType.String;
                this.Value = (string)value;
            }
            else if (type == typeof(long))
            {
                _valueType = SegmentValueType.Number;
                this.Value = ((long)value).ToString(CultureInfo.InvariantCulture);
            }
            else if (type == typeof(bool))
            {
                _valueType = SegmentValueType.Bool;
                this.Value = (bool)value ? "true" : "false";
            }
            else if (type == typeof(DateTime))
            {
                _valueType = SegmentValueType.Date;
                _dateValue = (DateTime)value;
                long unixTimestamp = new DateTimeOffset((DateTime)value).ToUnixTimeSeconds();
                this.Value = unixTimestamp.ToString(CultureInfo.InvariantCulture);
            }

        }

        private enum SegmentValueType
        {
            Unknown,
            String,
            Bool,
            Number,
            Date
        }

    }
}
