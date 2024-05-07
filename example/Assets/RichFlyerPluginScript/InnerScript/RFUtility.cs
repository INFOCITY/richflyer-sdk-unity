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
    public class RFSegmentsJson
    {

        [SerializeField] public RFSegment[] Segments = default;


        public RFSegmentsJson(RFSegment[] segments)
        {
            this.Segments = segments;
        }
    }

    [Serializable]
    public class RFContentArrayJson
    {
        [SerializeField] public RFContent[] Contents = default;

        public RFContentArrayJson(RFContent[] contents)
        {
            this.Contents = contents;
        }
    }

    [Serializable]
    public class RFEventArrayJson
    {
        [SerializeField] public string[] Events = default;

        public RFEventArrayJson(string[] events)
        {
            this.Events = events;
        }
    }

    [Serializable]
    public class RFVariable
    {
        [SerializeField] public string Name;
        [SerializeField] public string Value;

        public RFVariable(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
        
    }

    [Serializable]
    public class RFVariablesJson
    {
        [SerializeField] public RFVariable[] Variables = default;

        public RFVariablesJson(RFVariable[] variables)
        {
            this.Variables = variables;
        }
    }
}


