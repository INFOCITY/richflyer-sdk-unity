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

}


