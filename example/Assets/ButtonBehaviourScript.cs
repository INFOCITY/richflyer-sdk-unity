using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RichFlyer;

public class ButtonBehaviourScript : MonoBehaviour
{
    private static string eventPostId;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick(int number)
    {
        switch (number)
        {
            case 0:
                // Initialize
                Initialize();
                break;

            case 1:
                // Register Segment
                TMP_InputField segmentNameField = GameObject.Find("SegmentName").GetComponent<TMP_InputField>();
                TMP_InputField segmentValueField = GameObject.Find("SegmentValue").GetComponent<TMP_InputField>();

                RegisterSegment(segmentNameField.text, segmentValueField.text);
                break;

            case 2:
                // Get Segments
                GetSegments();
                break;

            case 3:
                // Show Latest Notification
                ShowLatestNotification();
                break;

            case 4:
                // Get Histories
                GetHistories();
                break;

            case 5:
                // Send Message
                TMP_InputField EventNameField = GameObject.Find("EventName").GetComponent<TMP_InputField>();
                TMP_InputField StandbyTimeField = GameObject.Find("StandbyTime").GetComponent<TMP_InputField>();

                TMP_InputField VariableName1Field = GameObject.Find("VariableName1").GetComponent<TMP_InputField>();
                TMP_InputField VariableValue1Field = GameObject.Find("VariableValue1").GetComponent<TMP_InputField>();
                TMP_InputField VariableName2Field = GameObject.Find("VariableName2").GetComponent<TMP_InputField>();
                TMP_InputField VariableValue2Field = GameObject.Find("VariableValue2").GetComponent<TMP_InputField>();

                int? standbyTime = null;
                if (StandbyTimeField.text.Length > 0)
                {
                    standbyTime =  System.Int32.Parse(StandbyTimeField.text);
                }

                Dictionary<string, string> variables = new Dictionary<string, string>();
                variables.Add(VariableName1Field.text, VariableValue1Field.text);
                variables.Add(VariableName2Field.text, VariableValue2Field.text);

                SendMessage(EventNameField.text, standbyTime, variables);

                break;

            case 6:
                // Cancel Send Message
                CancelSendMessage();
                break;
        }
    }

    private void Initialize()
    {
        Debug.Log("RF-Initialize called.");
        RFPluginScript.Initialize("RichFlyer", RFNotificationReceiver, (bool result, long code, string message) =>
        {
            if (result)
            {
                Debug.Log("RF-initialize succeeded");
            }
            else
            {
                Debug.Log($"RF-initialize failed. code:{code} message:{message}");
            }
        });
    }

    private void RegisterSegment(string name, string value)
    {
        Debug.Log($"RF-RegisterSegment called.{name}:{value}");
        // セグメント値を取得する
        RFSegment seg = new RFSegment(name, value);

        // セグメントを登録する
        RFPluginScript.RegistSegments(new RFSegment[] { seg }, (bool result, long code, string message) =>
        {
            if (result)
            {
                Debug.Log("RF-Register Segment succeeded");
            }
            else
            {
                Debug.Log($"RF-Register Segment failed. code:{code} message:{message}");
            }
        });
    }

    private void GetSegments()
    {
        Debug.Log("RF-GetSegments called.");
        RFSegment[] segments = RFPluginScript.GetSegments();
        foreach (RFSegment segment in segments)
        {
            Debug.Log($"RF-Segment => [{segment.getName()}:{segment.getStringValue()}]");
        }
    }

    private void ShowLatestNotification()
    {
        Debug.Log("RF-ShowLatestNotification called.");
        RFContent content = RFPluginScript.GetLatestReceivedData();
        if (content != null)
        {
            RFPluginScript.DisplayContent(content.NotificationId, (string buttonTitle, string buttonValue, string buttonValueType, ulong buttonIndex) =>
            {
                Debug.Log($"RF---index: {buttonIndex}");
                Debug.Log($"RF---title: {buttonTitle}");
                Debug.Log($"RF---type: {buttonValueType}");
                Debug.Log($"RF---value: {buttonValue}");
            });
        }
    }

    private void GetHistories()
    {
        Debug.Log("RF-GetHistories called.");
        foreach (RFContent content in RFPluginScript.GetReceivedData())
        {
            Debug.Log($"RF-title: {content.Title}");
            Debug.Log($"RF-body: {content.Body}");
            Debug.Log($"RF-type: {content.Type}");
            Debug.Log($"RF-recvdate: {content.getReceivedDate().ToString()}");
            Debug.Log($"RF-notifDate: {content.getNotificationDate().ToString()}");
            Debug.Log($"RF-imagePath: {content.ImagePath}");

            if (content.ActionButtons == null) continue;

            foreach (RFAction action in content.ActionButtons)
            {
                Debug.Log($"RF---index: {action.Index}");
                Debug.Log($"RF---title: {action.Title}");
                Debug.Log($"RF---type: {action.Type}");
                Debug.Log($"RF---value: {action.Value}");
            }
        }
    }

    private void SendMessage(string eventName, int? standbyTime, Dictionary<string,string>variables)
    {
        Debug.Log("RF-SendMessage called.");

        if (eventName == null || eventName.Length == 0)
        {
            Debug.Log("RF-Event Name not specified.");
            return;
        }

        RFPluginScript.PostMessage(new string[] { eventName }, variables, standbyTime,
            (bool result, long code, string message, string[] eventPostIds) => {
                Debug.Log($"RF-Post Message. result:{result}, code:{code}, message:{message}");
                if (eventPostIds != null)
                {
                    foreach (string eventPostId in eventPostIds)
                    {
                        Debug.Log($"RF-EventPostId: {eventPostId}");
                        ButtonBehaviourScript.eventPostId = eventPostId;
                    }
                }
            });
    }

    private void CancelSendMessage()
    {
        Debug.Log("RF-CancelSendMessage called.");
        if (ButtonBehaviourScript.eventPostId == null || ButtonBehaviourScript.eventPostId.Length == 0)
        {
            Debug.Log("RF-EventPostId is not exists.");
            return;
        }
        RFPluginScript.CancelPosting(ButtonBehaviourScript.eventPostId, (bool result, long code, string message, string[] eventPostIds) =>
        {
            Debug.Log($"RF-cancel Message. result:{result}, code:{code}, message:{message}");
        });
    }

    public static void RFNotificationReceiver(string buttonTitle, string buttonValue, string buttonValueType, ulong buttonIndex, string extendedProperty)
    {
        // カスタムアクションボタンのラベル
        Debug.Log($"RF-ButtonTitle:{buttonTitle}");
        // カスタムアクションに設定されている値
        Debug.Log($"RF-ButtonValue:{buttonValue}");
        // カスタムアクションの種類(url,scheme)
        Debug.Log($"RF-ButtonValueType:{buttonValueType}");
        // カスタムアクションボタンのインデックス
        Debug.Log($"RF-ButtonIndex:{buttonIndex}");
        // 拡張プロパティ
        Debug.Log($"RF-ExtendedProperty:{extendedProperty}");
    }

}
