//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

using System;

namespace RichFlyer
{
    [Serializable]
    public class RFSegment
    {
        public string Name;
        public string Value;

        public RFSegment(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
