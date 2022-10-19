//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

namespace RichFlyer
{
    /// <summary>
    /// callback result definition.
    /// </summary>
    /// <param name="result">true is success, false is failure.</param>
    /// <param name="code">result code</param>
    /// <param name="message">error message</param>
    public delegate void RFCompleted(bool result, long code, string message);

    /// <summary>
    /// callback close notification dialog.
    /// </summary>
    /// <param name="buttonTitle">selected button title</param>
    /// <param name="buttonValue">selected button value</param>
    /// <param name="buttonValueType">selected button type. http or scheme</param>
    /// <param name="buttonIndex">selected button index</param>
    public delegate void RFContentDisplayCallback(string buttonTitle, string buttonValue, string buttonValueType, ulong buttonIndex);

}
