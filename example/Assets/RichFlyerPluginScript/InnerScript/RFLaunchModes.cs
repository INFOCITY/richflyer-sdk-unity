//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

namespace RichFlyer
{
    public enum RFLaunchModes
    {
        RFLaunchModeNone  = 0,
        RFLaunchModeText  = (1 << 0), //Text
        RFLaunchModeImage = (1 << 1), //Image
        RFLaunchModeGif   = (1 << 2), //Gif
        RFLaunchModeMovie = (1 << 3)  //Movie
    }

}

