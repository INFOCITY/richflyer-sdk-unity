using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RichFlyer;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ButtonBehavior : MonoBehaviour
{
    private const string UnsupportedMessage = "Run this operation on an iOS or Android device.";
    private const string UnixTimeExample = "1753782566";
    private const string FallbackMessageBridgeObjectName = "RFCallback";

    private static readonly ConcurrentQueue<Action> MainThreadActions =
        new ConcurrentQueue<Action>();

    private static ButtonBehavior _instance;

    private readonly List<GameObject> _pages = new List<GameObject>();
    private readonly List<Button> _navigationButtons = new List<Button>();
    private readonly List<Texture2D> _loadedTextures = new List<Texture2D>();

    private bool _isController;
    private Text _headerTitle;
    private Text _homeStatus;
    private Text _launchActionStatus;
    private Text _debugLogText;
    private Text _segmentStatus;
    private Text _savedSegmentsText;
    private Text _receivedStatus;
    private Text _eventStatus;
    private RectTransform _historyContent;

    private Dropdown _genreDropdown;
    private Dropdown _dayDropdown;
    private Dropdown _ageDropdown;
    private Dropdown _registeredDropdown;
    private InputField _birthdayInput;
    private InputField _installedDateInput;

    private readonly List<InputField> _eventInputs = new List<InputField>();
    private readonly List<InputField> _variableNameInputs = new List<InputField>();
    private readonly List<InputField> _variableValueInputs = new List<InputField>();
    private InputField _standbyTimeInput;

    private static bool IsSupportedRuntime
    {
        get
        {
            return Application.platform == RuntimePlatform.IPhonePlayer ||
                   Application.platform == RuntimePlatform.Android;
        }
    }

    // Persistent, file-based debug log for diagnosing cold-launch notification handling.
    // Survives app termination and does not depend on catching a live console stream at
    // the exact right moment. Retrieve via Xcode > Window > Devices and Simulators >
    // select device/app > "..." > Download Container > AppData/Documents/rf_debug.log
    // (on Android: /storage/emulated/0/Android/data/<package>/files/rf_debug.log, or
    // `adb exec-out run-as <package> cat files/rf_debug.log`).
    private static readonly object DebugLogFileLock = new object();

    private static string DebugLogFilePath
    {
        get { return Path.Combine(Application.persistentDataPath, "rf_debug.log"); }
    }

    private static void AppendDebugLog(string message)
    {
        string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}";
        Debug.Log(line);
        try
        {
            lock (DebugLogFileLock)
            {
                File.AppendAllText(DebugLogFilePath, line + Environment.NewLine);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    // Fires before any scene loads, ahead of every MonoBehaviour's Awake/Start.
    // If this line is missing from rf_debug.log after a cold launch, the managed
    // runtime itself never ran and the issue is native-side, before Unity boots.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnEngineLoaded()
    {
        AppendDebugLog("RF-engine loaded (BeforeSceneLoad)");
    }

    private void Awake()
    {
        if (gameObject.name != "Initialize")
        {
            enabled = false;
            return;
        }

        if (_instance != null && _instance != this)
        {
            enabled = false;
            return;
        }

        _instance = this;
        _isController = true;
        AppendDebugLog("RF-awake controller elected");

        // The original scene has one ButtonBehavior on every legacy button.
        // Keep a single controller alive outside the Canvas while the old UI is replaced.
        transform.SetParent(null, false);
        gameObject.name = "RichFlyer Sample Controller";
        foreach (Graphic graphic in GetComponentsInChildren<Graphic>(true))
        {
            graphic.enabled = false;
        }

        Button legacyButton = GetComponent<Button>();
        if (legacyButton != null)
        {
            legacyButton.enabled = false;
        }
    }

    private void Start()
    {
        if (!_isController)
        {
            return;
        }

        AppendDebugLog("RF-start scene");
        BuildUI();
        // The SDK manual calls Initialize from Start() so the notification receiver is
        // registered before the OS delivers a launch/action tap that occurred while the
        // app was fully terminated. Keep the on-screen "Initialize" button for manual retry.
        InitializeRichFlyer();
    }

    private void Update()
    {
        Action action;
        while (MainThreadActions.TryDequeue(out action))
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        ClearLoadedTextures();
    }

    // Kept for the existing scene's serialized button callbacks.
    public void OnClick(int number)
    {
        switch (number)
        {
            case 0:
                InitializeRichFlyer();
                break;
            case 1:
                RegisterSegments();
                break;
            case 2:
                ShowLatestContent();
                break;
            case 3:
                RefreshHistory();
                break;
            case 4:
                RefreshSegments();
                break;
            case 5:
                PostMessage();
                break;
        }
    }

    public static void RFNotificationReceiver(
        string buttonTitle,
        string buttonValue,
        string buttonValueType,
        ulong buttonIndex,
        string extendedProperty)
    {
        AppendDebugLog(
            $"RF-NotificationReceiver title:{buttonTitle} value:{buttonValue} " +
            $"type:{buttonValueType} index:{buttonIndex} extendedProperty:{extendedProperty}");

        EnqueueOnMainThread(() =>
        {
            if (_instance == null)
            {
                return;
            }

            _instance.SetContentActionStatus(
                BuildActionStatus(buttonTitle, buttonValue, buttonValueType, buttonIndex));
            _instance.OpenBrowserUrlIfPresent(buttonValue);
        });
    }

    private void BuildUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Sample Canvas was not found.");
            return;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        for (int index = canvas.transform.childCount - 1; index >= 0; index--)
        {
            GameObject oldChild = canvas.transform.GetChild(index).gameObject;
            oldChild.SetActive(false);
            Destroy(oldChild);
        }

        SampleAppUIFactory.EnsureEventSystem();

        Image background = SampleAppUIFactory.CreatePanel(
            canvas.transform,
            "Sample Background",
            SampleAppUIFactory.BackgroundColor);
        SampleAppUIFactory.Stretch(background.rectTransform);

        GameObject safeArea = SampleAppUIFactory.CreateUIObject("Safe Area", background.transform);
        SampleAppUIFactory.Stretch(safeArea.GetComponent<RectTransform>());
        safeArea.AddComponent<SampleSafeAreaFitter>();

        BuildHeader(safeArea.transform);

        GameObject pagesRoot = SampleAppUIFactory.CreateUIObject("Pages", safeArea.transform);
        SampleAppUIFactory.Stretch(
            pagesRoot.GetComponent<RectTransform>(),
            0f,
            0f,
            112f,
            116f);

        BuildHomePage(pagesRoot.transform);
        BuildSegmentPage(pagesRoot.transform);
        BuildReceivedPage(pagesRoot.transform);
        BuildEventPage(pagesRoot.transform);

        foreach (GameObject page in _pages)
        {
            SampleAppUIFactory.Stretch(page.GetComponent<RectTransform>());
        }

        BuildNavigation(safeArea.transform);
        ShowPage(0, "RichFlyer Sample");
    }

    private void BuildHeader(Transform parent)
    {
        Image header = SampleAppUIFactory.CreatePanel(
            parent,
            "Header",
            SampleAppUIFactory.CardColor);
        SampleAppUIFactory.AnchorToTop(header.rectTransform, 112f);
        SampleAppUIFactory.AddHorizontalLayout(header.gameObject, 34, 34, 12, 12);

        _headerTitle = SampleAppUIFactory.CreateText(
            header.transform,
            "Title",
            "RichFlyer Sample",
            36,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        SampleAppUIFactory.SetFlexibleWidth(_headerTitle.gameObject);
    }

    private void BuildHomePage(Transform parent)
    {
        GameObject page = CreatePage(parent, "Home Page");
        RectTransform content;
        SampleAppUIFactory.CreateScrollView(page.transform, "Home Scroll", out content);

        GameObject statusCard = CreateCard(content, "SDK Status Card");
        SampleAppUIFactory.CreateText(
            statusCard.transform,
            "Heading",
            "SDK 初期化",
            31,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        _homeStatus = SampleAppUIFactory.CreateText(
            statusCard.transform,
            "Status",
            "未初期化",
            25,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        SampleAppUIFactory.SetPreferredHeight(_homeStatus.gameObject, 105f);
        SampleAppUIFactory.CreateButton(
            statusCard.transform,
            "Initialize",
            "Initialize",
            InitializeRichFlyer);

        GameObject launchActionCard = CreateCard(content, "Launch Action Card");
        SampleAppUIFactory.CreateText(
            launchActionCard.transform,
            "Heading",
            "受信通知アクション（起動・タップ時）",
            29,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        _launchActionStatus = SampleAppUIFactory.CreateText(
            launchActionCard.transform,
            "Status",
            "まだ受信していません。\n完全終了状態から通知（本文/アクションボタン）をタップして起動し、Initializeをタップすると、ここに結果が表示されます。",
            23,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        SampleAppUIFactory.SetPreferredHeight(_launchActionStatus.gameObject, 145f);

        GameObject debugLogCard = CreateCard(content, "Debug Log Card");
        SampleAppUIFactory.CreateText(
            debugLogCard.transform,
            "Heading",
            "デバッグログ（端末に保存・再起動しても残る）",
            27,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        GameObject debugLogButtonsRow = SampleAppUIFactory.CreateUIObject(
            "Debug Log Buttons",
            debugLogCard.transform);
        SampleAppUIFactory.AddHorizontalLayout(debugLogButtonsRow, 0, 0, 0, 0, 12f);
        SampleAppUIFactory.SetPreferredHeight(debugLogButtonsRow, 68f);
        SampleAppUIFactory.CreateButton(
            debugLogButtonsRow.transform,
            "Show Debug Log",
            "Show Log",
            ShowDebugLog,
            68f,
            SampleAppUIFactory.PrimaryDarkColor);
        SampleAppUIFactory.CreateButton(
            debugLogButtonsRow.transform,
            "Clear Debug Log",
            "Clear Log",
            ClearDebugLog,
            68f,
            SampleAppUIFactory.PrimaryDarkColor);
        _debugLogText = SampleAppUIFactory.CreateText(
            debugLogCard.transform,
            "Log",
            "「Show Log」をタップするとrf_debug.logの内容を表示します。",
            19,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        _debugLogText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _debugLogText.verticalOverflow = VerticalWrapMode.Overflow;
        SampleAppUIFactory.SetPreferredHeight(_debugLogText.gameObject, 480f);

        GameObject contentCard = CreateCard(content, "Latest Content Card");
        SampleAppUIFactory.CreateText(
            contentCard.transform,
            "Heading",
            "最新の通知",
            31,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        Text description = SampleAppUIFactory.CreateText(
            contentCard.transform,
            "Description",
            "受信した最新の通知を、画像・タイトル・本文・アクションボタン付きで表示します。",
            24,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        SampleAppUIFactory.SetPreferredHeight(description.gameObject, 96f);
        SampleAppUIFactory.CreateButton(
            contentCard.transform,
            "Show Latest Content",
            "Show Content",
            ShowLatestContent);
    }

    private void BuildSegmentPage(Transform parent)
    {
        GameObject page = CreatePage(parent, "Segment Page");
        RectTransform content;
        SampleAppUIFactory.CreateScrollView(page.transform, "Segment Scroll", out content);

        _genreDropdown = CreateLabeledDropdown(
            content,
            "ジャンル",
            "genre",
            new[] { "comic", "magazine", "novel" });
        _dayDropdown = CreateLabeledDropdown(
            content,
            "曜日",
            "day",
            new[] { "月", "火", "水", "木", "金", "土", "日" });
        _ageDropdown = CreateLabeledDropdown(
            content,
            "年齢",
            "age",
            new[] { "0", "10", "20", "30", "40", "50", "60", "70", "80", "90" });
        _registeredDropdown = CreateLabeledDropdown(
            content,
            "登録状況",
            "registered",
            new[] { "true", "false" });

        _birthdayInput = CreateLabeledInput(
            content,
            "誕生日",
            "birthday",
            "Unix time",
            UnixTimeExample,
            InputField.ContentType.IntegerNumber);
        _installedDateInput = CreateLabeledInput(
            content,
            "インストール日",
            "installedDate",
            "Unix time",
            UnixTimeExample,
            InputField.ContentType.IntegerNumber);

        SampleAppUIFactory.CreateButton(
            content,
            "Register Segments",
            "Regist Segment",
            RegisterSegments,
            84f);

        SampleAppUIFactory.CreateText(
            content,
            "Segment Sync Notice",
            "※ ここで登録した内容は端末内に保存され、アプリがバックグラウンドに移行したタイミングでサーバーへ送信されます。"
                + "登録直後にGet Segmentsを押しても、サーバー反映前の値が表示されることがあります。",
            22,
            TextAnchor.UpperLeft,
            FontStyle.Italic,
            SampleAppUIFactory.SecondaryTextColor);

        GameObject statusCard = CreateCard(content, "Segment Result Card");
        SampleAppUIFactory.CreateText(
            statusCard.transform,
            "Heading",
            "登録結果",
            29,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        _segmentStatus = SampleAppUIFactory.CreateText(
            statusCard.transform,
            "Status",
            "登録結果はまだありません",
            24,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        SampleAppUIFactory.SetPreferredHeight(_segmentStatus.gameObject, 88f);

        GameObject savedCard = CreateCard(content, "Saved Segments Card");
        SampleAppUIFactory.CreateText(
            savedCard.transform,
            "Heading",
            "保存されているセグメント",
            29,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        SampleAppUIFactory.CreateButton(
            savedCard.transform,
            "Get Segments",
            "Get Segments",
            RefreshSegments,
            68f,
            SampleAppUIFactory.PrimaryDarkColor);
        _savedSegmentsText = SampleAppUIFactory.CreateText(
            savedCard.transform,
            "Segments",
            "Get Segmentsをタップするとキーと値を表示します。",
            24,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        SampleAppUIFactory.SetPreferredHeight(_savedSegmentsText.gameObject, 170f);
    }

    private void BuildReceivedPage(Transform parent)
    {
        GameObject page = CreatePage(parent, "Received Page");
        VerticalLayoutGroup layout = SampleAppUIFactory.AddVerticalLayout(
            page,
            26,
            26,
            22,
            18,
            12f);
        layout.childForceExpandHeight = false;

        SampleAppUIFactory.CreateButton(
            page.transform,
            "Get History",
            "Get History",
            RefreshHistory,
            72f);
        _receivedStatus = SampleAppUIFactory.CreateText(
            page.transform,
            "History Status",
            "Get Historyをタップすると受信履歴を表示します。",
            23,
            TextAnchor.MiddleLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        SampleAppUIFactory.SetPreferredHeight(_receivedStatus.gameObject, 56f);

        ScrollRect historyScroll = SampleAppUIFactory.CreateScrollView(
            page.transform,
            "History Scroll",
            out _historyContent);
        SampleAppUIFactory.SetFlexibleHeight(historyScroll.gameObject);
        Image scrollBackground = historyScroll.GetComponent<Image>();
        scrollBackground.color = SampleAppUIFactory.BackgroundColor;
    }

    private void BuildEventPage(Transform parent)
    {
        GameObject page = CreatePage(parent, "Event Page");
        RectTransform content;
        SampleAppUIFactory.CreateScrollView(page.transform, "Event Scroll", out content);

        GameObject eventsCard = CreateCard(content, "Event IDs Card");
        SampleAppUIFactory.CreateText(
            eventsCard.transform,
            "Heading",
            "イベントID",
            31,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        for (int index = 0; index < 3; index++)
        {
            InputField input = SampleAppUIFactory.CreateInputField(
                eventsCard.transform,
                $"Event {index + 1}",
                $"event{index + 1}");
            _eventInputs.Add(input);
        }

        GameObject variablesCard = CreateCard(content, "Variables Card");
        SampleAppUIFactory.CreateText(
            variablesCard.transform,
            "Heading",
            "変数",
            31,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        Text variableDescription = SampleAppUIFactory.CreateText(
            variablesCard.transform,
            "Description",
            "名前は文字列、値は文字列または数値を指定できます。",
            23,
            TextAnchor.MiddleLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        SampleAppUIFactory.SetPreferredHeight(variableDescription.gameObject, 44f);
        for (int index = 0; index < 3; index++)
        {
            GameObject row = SampleAppUIFactory.CreateUIObject(
                $"Variable Row {index + 1}",
                variablesCard.transform);
            SampleAppUIFactory.AddHorizontalLayout(row, 0, 0, 0, 0, 12f);
            SampleAppUIFactory.SetPreferredHeight(row, 70f);

            InputField nameInput = SampleAppUIFactory.CreateInputField(
                row.transform,
                "Name",
                "Name");
            InputField valueInput = SampleAppUIFactory.CreateInputField(
                row.transform,
                "Value",
                "Value");
            SampleAppUIFactory.SetFlexibleWidth(nameInput.gameObject);
            SampleAppUIFactory.SetFlexibleWidth(valueInput.gameObject);
            _variableNameInputs.Add(nameInput);
            _variableValueInputs.Add(valueInput);
        }

        GameObject standbyCard = CreateCard(content, "Standby Time Card");
        SampleAppUIFactory.CreateText(
            standbyCard.transform,
            "Heading",
            "待機時間（分）",
            29,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        _standbyTimeInput = SampleAppUIFactory.CreateInputField(
            standbyCard.transform,
            "Standby Time",
            "0 or greater",
            "1",
            InputField.ContentType.IntegerNumber);

        SampleAppUIFactory.CreateButton(
            content,
            "Post Message",
            "Post Message",
            PostMessage,
            84f);

        GameObject resultCard = CreateCard(content, "Post Result Card");
        SampleAppUIFactory.CreateText(
            resultCard.transform,
            "Heading",
            "送信結果",
            29,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        _eventStatus = SampleAppUIFactory.CreateText(
            resultCard.transform,
            "Status",
            "送信結果はまだありません",
            24,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        SampleAppUIFactory.SetPreferredHeight(_eventStatus.gameObject, 145f);
    }

    private void BuildNavigation(Transform parent)
    {
        Image navigation = SampleAppUIFactory.CreatePanel(
            parent,
            "Navigation",
            SampleAppUIFactory.CardColor);
        SampleAppUIFactory.AnchorToBottom(navigation.rectTransform, 116f);
        HorizontalLayoutGroup layout = SampleAppUIFactory.AddHorizontalLayout(
            navigation.gameObject,
            18,
            18,
            12,
            12,
            10f);
        layout.childForceExpandHeight = false;

        string[] labels = { "ホーム", "セグメント", "受信履歴", "イベント" };
        string[] titles = { "RichFlyer Sample", "Segment", "Received", "Event" };
        for (int index = 0; index < labels.Length; index++)
        {
            int pageIndex = index;
            Button button = SampleAppUIFactory.CreateButton(
                navigation.transform,
                labels[index],
                labels[index],
                () =>
                {
                    ShowPage(pageIndex, titles[pageIndex]);
                    if (pageIndex == 2)
                    {
                        RefreshHistory();
                    }
                },
                86f,
                index == 0
                    ? SampleAppUIFactory.PrimaryColor
                    : SampleAppUIFactory.PrimaryDarkColor);
            SampleAppUIFactory.SetFlexibleWidth(button.gameObject);
            _navigationButtons.Add(button);
        }
    }

    private GameObject CreatePage(Transform parent, string name)
    {
        Image image = SampleAppUIFactory.CreatePanel(
            parent,
            name,
            SampleAppUIFactory.BackgroundColor);
        GameObject page = image.gameObject;
        _pages.Add(page);
        return page;
    }

    private GameObject CreateCard(Transform parent, string name)
    {
        Image card = SampleAppUIFactory.CreatePanel(
            parent,
            name,
            SampleAppUIFactory.CardColor);
        SampleAppUIFactory.AddVerticalLayout(card.gameObject, 24, 24, 22, 22, 12f);
        return card.gameObject;
    }

    private Dropdown CreateLabeledDropdown(
        Transform parent,
        string label,
        string key,
        string[] options)
    {
        GameObject card = CreateCard(parent, $"{label} Card");
        SampleAppUIFactory.CreateText(
            card.transform,
            "Label",
            $"{label}  (key: {key})",
            26,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        return SampleAppUIFactory.CreateDropdown(card.transform, "Value", options);
    }

    private InputField CreateLabeledInput(
        Transform parent,
        string label,
        string key,
        string placeholder,
        string initialValue,
        InputField.ContentType contentType)
    {
        GameObject card = CreateCard(parent, $"{label} Card");
        SampleAppUIFactory.CreateText(
            card.transform,
            "Label",
            $"{label}  (key: {key})",
            26,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
        return SampleAppUIFactory.CreateInputField(
            card.transform,
            "Value",
            placeholder,
            initialValue,
            contentType);
    }

    private void ShowPage(int index, string title)
    {
        for (int pageIndex = 0; pageIndex < _pages.Count; pageIndex++)
        {
            _pages[pageIndex].SetActive(pageIndex == index);
        }

        if (_headerTitle != null)
        {
            _headerTitle.text = title;
        }

        for (int buttonIndex = 0; buttonIndex < _navigationButtons.Count; buttonIndex++)
        {
            Image image = _navigationButtons[buttonIndex].targetGraphic as Image;
            if (image != null)
            {
                image.color = buttonIndex == index
                    ? SampleAppUIFactory.PrimaryColor
                    : SampleAppUIFactory.PrimaryDarkColor;
            }
        }
    }

    private void InitializeRichFlyer()
    {
        if (!IsSupportedRuntime)
        {
            SetStatus(_homeStatus, UnsupportedMessage, false);
            return;
        }

        SetStatus(_homeStatus, "Initializing...", null);
        string bridgeObjectName = ResolveMessageBridgeObjectName();
        AppendDebugLog($"RF-Initialize begin bridgeObjectName:{bridgeObjectName}");
        try
        {
            RFPluginScript.Initialize(
                bridgeObjectName,
                RFNotificationReceiver,
                (result, code, message) =>
                {
                    EnqueueOnMainThread(() =>
                    {
                        AppendDebugLog(
                            $"RF-Initialize onResult result:{result} code:{code} message:{message}");
                        string status = BuildResultText(
                            "Initialize",
                            result,
                            code,
                            message);
                        SetStatus(_homeStatus, status, result);
                    });
                });
        }
        catch (Exception exception)
        {
            AppendDebugLog($"RF-Initialize threw: {exception}");
            Debug.LogException(exception);
            SetStatus(_homeStatus, $"Initialize failed\n{exception.Message}", false);
        }
    }

    // Android delivers native notification/action taps via UnityPlayer.UnitySendMessage
    // targeted at the GameObject holding RFMessageBridge, not at this controller.
    // Prefer the bridge's own cached instance (set in its Awake, which Unity guarantees
    // runs before any Start) over a scene scan, and fall back to the scan only if the
    // bridge hasn't registered itself yet for some reason.
    private static string ResolveMessageBridgeObjectName()
    {
        RFMessageBridge bridge = RFMessageBridge.Instance ?? FindObjectOfType<RFMessageBridge>();
        if (bridge == null)
        {
            AppendDebugLog(
                $"RF-warning RFMessageBridge not found in scene; falling back to \"{FallbackMessageBridgeObjectName}\". " +
                "Notification/action callbacks will silently fail to reach C# if no GameObject has that name and an RFMessageBridge component.");
            return FallbackMessageBridgeObjectName;
        }
        return bridge.gameObject.name;
    }

    private void ShowDebugLog()
    {
        try
        {
            string content = File.Exists(DebugLogFilePath)
                ? File.ReadAllText(DebugLogFilePath)
                : "(rf_debug.log is empty. Nothing has been logged yet.)";
            SetStatus(_debugLogText, content, null);
        }
        catch (Exception exception)
        {
            SetStatus(_debugLogText, $"Failed to read log: {exception.Message}", false);
        }
    }

    private void ClearDebugLog()
    {
        try
        {
            if (File.Exists(DebugLogFilePath))
            {
                File.Delete(DebugLogFilePath);
            }
            SetStatus(_debugLogText, "(cleared)", null);
        }
        catch (Exception exception)
        {
            SetStatus(_debugLogText, $"Failed to clear log: {exception.Message}", false);
        }
    }

    private void RegisterSegments()
    {
        if (!IsSupportedRuntime)
        {
            SetStatus(_segmentStatus, UnsupportedMessage, false);
            return;
        }

        long birthday;
        string validationError;
        if (!TryParseUnixTime(_birthdayInput.text, "birthday", out birthday, out validationError))
        {
            SetStatus(_segmentStatus, validationError, false);
            return;
        }

        long installedDate;
        if (!TryParseUnixTime(
                _installedDateInput.text,
                "installedDate",
                out installedDate,
                out validationError))
        {
            SetStatus(_segmentStatus, validationError, false);
            return;
        }

        long age;
        if (!long.TryParse(
                SelectedText(_ageDropdown),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out age))
        {
            SetStatus(_segmentStatus, "age must be a number.", false);
            return;
        }

        bool registered;
        if (!bool.TryParse(SelectedText(_registeredDropdown), out registered))
        {
            SetStatus(_segmentStatus, "registered must be true or false.", false);
            return;
        }

        RFSegment[] segments =
        {
            new RFSegment("genre", SelectedText(_genreDropdown)),
            new RFSegment("day", SelectedText(_dayDropdown)),
            new RFSegment("age", age),
            new RFSegment("registered", registered),
            new RFSegment("birthday", birthday),
            new RFSegment("installedDate", installedDate)
        };

        SetStatus(_segmentStatus, "Registering segments...", null);
        try
        {
            RFPluginScript.RegistSegments(
                segments,
                (result, code, message) =>
                {
                    EnqueueOnMainThread(() =>
                    {
                        string resultText = BuildResultText("Regist Segment", result, code, message);
                        if (result)
                        {
                            resultText += "\n※ サーバーへの反映はアプリがバックグラウンドに移行したタイミングで行われます。"
                                + "直後にGet Segmentsを押しても、この変更はまだ表示されません。";
                        }
                        SetStatus(_segmentStatus, resultText, result);
                        if (result)
                        {
                            RefreshSegments();
                        }
                    });
                });
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus(_segmentStatus, $"Registration failed\n{exception.Message}", false);
        }
    }

    private void RefreshSegments()
    {
        if (!IsSupportedRuntime)
        {
            SetStatus(_savedSegmentsText, UnsupportedMessage, false);
            return;
        }

        try
        {
            RFSegment[] segments = RFPluginScript.GetSegments() ?? Array.Empty<RFSegment>();
            Array.Sort(
                segments,
                (left, right) => string.Compare(
                    left?.getName(),
                    right?.getName(),
                    StringComparison.Ordinal));

            if (segments.Length == 0)
            {
                SetStatus(_savedSegmentsText, "No saved segments.", null);
                return;
            }

            List<string> values = new List<string>();
            foreach (RFSegment segment in segments)
            {
                if (segment != null)
                {
                    values.Add($"{segment.getName()}    {segment.getStringValue()}");
                }
            }

            SetStatus(_savedSegmentsText, string.Join("\n", values), true);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus(_savedSegmentsText, $"Get Segments failed\n{exception.Message}", false);
        }
    }

    private void ShowLatestContent()
    {
        if (!IsSupportedRuntime)
        {
            SetStatus(_homeStatus, UnsupportedMessage, false);
            return;
        }

        try
        {
            RFContent content = RFPluginScript.GetLatestReceivedData();
            if (content == null)
            {
                SetStatus(_homeStatus, "No received notifications.", null);
                return;
            }

            DisplayContent(content);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus(_homeStatus, $"Show Content failed\n{exception.Message}", false);
        }
    }

    private void RefreshHistory()
    {
        if (_historyContent == null)
        {
            return;
        }

        ClearHistory();
        if (!IsSupportedRuntime)
        {
            SetStatus(_receivedStatus, UnsupportedMessage, false);
            AddEmptyHistoryMessage(UnsupportedMessage);
            return;
        }

        try
        {
            RFContent[] contents = RFPluginScript.GetReceivedData() ?? Array.Empty<RFContent>();
            if (contents.Length == 0)
            {
                SetStatus(_receivedStatus, "No received notifications.", null);
                AddEmptyHistoryMessage("No received notifications.");
                return;
            }

            SetStatus(_receivedStatus, $"{contents.Length} notification(s)", true);
            foreach (RFContent content in contents)
            {
                if (content != null)
                {
                    AddHistoryCell(content);
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus(_receivedStatus, $"Get History failed: {exception.Message}", false);
            AddEmptyHistoryMessage("Unable to load history.");
        }
    }

    private void ClearHistory()
    {
        StopAllCoroutines();
        ClearLoadedTextures();
        for (int index = _historyContent.childCount - 1; index >= 0; index--)
        {
            GameObject child = _historyContent.GetChild(index).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private void ClearLoadedTextures()
    {
        foreach (Texture2D texture in _loadedTextures)
        {
            if (texture != null)
            {
                Destroy(texture);
            }
        }

        _loadedTextures.Clear();
    }

    private void AddEmptyHistoryMessage(string message)
    {
        GameObject card = CreateCard(_historyContent, "Empty History");
        Text text = SampleAppUIFactory.CreateText(
            card.transform,
            "Message",
            message,
            26,
            TextAnchor.MiddleCenter,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        SampleAppUIFactory.SetPreferredHeight(text.gameObject, 130f);
    }

    private void AddHistoryCell(RFContent content)
    {
        Image card = SampleAppUIFactory.CreatePanel(
            _historyContent,
            $"History {content.NotificationId}",
            SampleAppUIFactory.CardColor);
        card.raycastTarget = true;
        SampleAppUIFactory.SetPreferredHeight(card.gameObject, 190f);
        HorizontalLayoutGroup cardLayout = SampleAppUIFactory.AddHorizontalLayout(
            card.gameObject,
            18,
            18,
            18,
            18,
            18f);
        cardLayout.childForceExpandWidth = false;

        Button button = card.gameObject.AddComponent<Button>();
        button.targetGraphic = card;
        button.onClick.AddListener(() => DisplayContent(content));

        GameObject imageObject = SampleAppUIFactory.CreateUIObject(
            "Image Container",
            card.transform);
        Image imageBackground = imageObject.AddComponent<Image>();
        imageBackground.color = SampleAppUIFactory.BorderColor;
        imageBackground.raycastTarget = false;
        imageObject.AddComponent<RectMask2D>();
        LayoutElement imageLayout = SampleAppUIFactory.SetPreferredWidth(imageObject, 190f);
        imageLayout.preferredHeight = 154f;

        GameObject rawImageObject = SampleAppUIFactory.CreateUIObject(
            "Image",
            imageObject.transform);
        RawImage rawImage = rawImageObject.AddComponent<RawImage>();
        rawImage.texture = Texture2D.whiteTexture;
        rawImage.color = Color.clear;
        rawImage.raycastTarget = false;
        SampleAppUIFactory.Stretch(rawImage.rectTransform);
        AspectRatioFitter aspectFitter = rawImageObject.AddComponent<AspectRatioFitter>();
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        aspectFitter.aspectRatio = 1f;

        GameObject summary = SampleAppUIFactory.CreateUIObject("Summary", card.transform);
        SampleAppUIFactory.SetFlexibleWidth(summary);
        SampleAppUIFactory.AddVerticalLayout(summary, 0, 0, 0, 0, 5f);

        Text title = SampleAppUIFactory.CreateText(
            summary.transform,
            "Title",
            string.IsNullOrEmpty(content.Title) ? "(No title)" : content.Title,
            27,
            TextAnchor.UpperLeft,
            FontStyle.Bold);
        title.verticalOverflow = VerticalWrapMode.Truncate;
        SampleAppUIFactory.SetPreferredHeight(title.gameObject, 42f);

        Text body = SampleAppUIFactory.CreateText(
            summary.transform,
            "Body",
            string.IsNullOrEmpty(content.Body) ? "(No body)" : content.Body,
            23,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        body.verticalOverflow = VerticalWrapMode.Truncate;
        SampleAppUIFactory.SetPreferredHeight(body.gameObject, 66f);

        Text date = SampleAppUIFactory.CreateText(
            summary.transform,
            "Notification Date",
            FormatUnixTime(content.NotificationDateUnixTime),
            21,
            TextAnchor.LowerLeft,
            FontStyle.Normal,
            SampleAppUIFactory.SecondaryTextColor);
        SampleAppUIFactory.SetPreferredHeight(date.gameObject, 30f);

        if (!string.IsNullOrWhiteSpace(content.ImagePath))
        {
            StartCoroutine(LoadHistoryImage(content.ImagePath, rawImage, aspectFitter));
        }
    }

    private IEnumerator LoadHistoryImage(
        string imagePath,
        RawImage target,
        AspectRatioFitter aspectFitter)
    {
        string uri;
        if (!TryBuildImageUri(imagePath, out uri))
        {
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"RF-History image load failed: {request.error}");
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (texture == null)
            {
                yield break;
            }

            _loadedTextures.Add(texture);
            if (target != null)
            {
                target.texture = texture;
                target.color = Color.white;
                if (aspectFitter != null && texture.height > 0)
                {
                    aspectFitter.aspectRatio = (float)texture.width / texture.height;
                }
            }
        }
    }

    private void DisplayContent(RFContent content)
    {
        if (content == null || string.IsNullOrEmpty(content.NotificationId))
        {
            SetContentActionStatus("The notification ID is empty.");
            return;
        }

        try
        {
            RFPluginScript.DisplayContent(
                content.NotificationId,
                (buttonTitle, buttonValue, buttonValueType, buttonIndex) =>
                {
                    EnqueueOnMainThread(() =>
                    {
                        SetContentActionStatus(
                            BuildActionStatus(
                                buttonTitle,
                                buttonValue,
                                buttonValueType,
                                buttonIndex));
                        OpenBrowserUrlIfPresent(buttonValue);
                    });
                });
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetContentActionStatus($"Show Content failed: {exception.Message}");
        }
    }

    private void PostMessage()
    {
        if (!IsSupportedRuntime)
        {
            SetStatus(_eventStatus, UnsupportedMessage, false);
            return;
        }

        List<string> eventIds = new List<string>();
        foreach (InputField eventInput in _eventInputs)
        {
            string eventId = eventInput.text.Trim();
            if (!string.IsNullOrEmpty(eventId))
            {
                eventIds.Add(eventId);
            }
        }

        if (eventIds.Count == 0)
        {
            SetStatus(_eventStatus, "Enter at least one event ID.", false);
            return;
        }

        Dictionary<string, string> variables = new Dictionary<string, string>();
        for (int index = 0; index < _variableNameInputs.Count; index++)
        {
            string name = _variableNameInputs[index].text.Trim();
            string value = _variableValueInputs[index].text;
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (string.IsNullOrEmpty(name))
            {
                SetStatus(_eventStatus, $"Variable {index + 1} requires a name.", false);
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                SetStatus(_eventStatus, $"Variable '{name}' requires a value.", false);
                return;
            }

            if (variables.ContainsKey(name))
            {
                SetStatus(_eventStatus, $"Variable name '{name}' is duplicated.", false);
                return;
            }

            variables.Add(name, value);
        }

        int standbyTime;
        if (!int.TryParse(
                _standbyTimeInput.text.Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out standbyTime) ||
            standbyTime < 0)
        {
            SetStatus(_eventStatus, "Standby time must be an integer of 0 or greater.", false);
            return;
        }

        SetStatus(_eventStatus, "Posting message...", null);
        try
        {
            RFPluginScript.PostMessage(
                eventIds.ToArray(),
                variables.Count == 0 ? null : variables,
                standbyTime,
                (result, code, message, eventPostIds) =>
                {
                    EnqueueOnMainThread(() =>
                    {
                        string text = BuildResultText("Post Message", result, code, message);
                        if (eventPostIds != null && eventPostIds.Length > 0)
                        {
                            text += "\neventPostId: " + string.Join(", ", eventPostIds);
                        }

                        SetStatus(_eventStatus, text, result);
                    });
                });
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus(_eventStatus, $"Post Message failed\n{exception.Message}", false);
        }
    }

    private void SetContentActionStatus(string status)
    {
        SetStatus(_homeStatus, status, null);
        SetStatus(_receivedStatus, status, null);
        SetStatus(_launchActionStatus, status, null);
    }

    private void OpenBrowserUrlIfPresent(string value)
    {
        Uri uri;
        if (!Uri.TryCreate(value, UriKind.Absolute, out uri))
        {
            return;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Application.OpenURL(uri.AbsoluteUri);
    }

    private static string SelectedText(Dropdown dropdown)
    {
        if (dropdown == null ||
            dropdown.options == null ||
            dropdown.options.Count == 0)
        {
            return string.Empty;
        }

        int index = Mathf.Clamp(dropdown.value, 0, dropdown.options.Count - 1);
        return dropdown.options[index].text;
    }

    private static bool TryParseUnixTime(
        string value,
        string fieldName,
        out long unixTime,
        out string error)
    {
        if (!long.TryParse(
                value.Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out unixTime))
        {
            error = $"{fieldName} must be a Unix time integer.";
            return false;
        }

        try
        {
            DateTimeOffset.FromUnixTimeSeconds(unixTime);
            error = null;
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            error = $"{fieldName} is outside the supported Unix time range.";
            return false;
        }
    }

    private static string FormatUnixTime(long unixTime)
    {
        try
        {
            return DateTimeOffset
                .FromUnixTimeSeconds(unixTime)
                .ToLocalTime()
                .ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
        }
        catch (ArgumentOutOfRangeException)
        {
            return unixTime.ToString(CultureInfo.InvariantCulture);
        }
    }

    private static bool TryBuildImageUri(string source, out string uri)
    {
        uri = null;
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        Uri parsed;
        if (Uri.TryCreate(source, UriKind.Absolute, out parsed) &&
            !string.IsNullOrEmpty(parsed.Scheme))
        {
            uri = parsed.AbsoluteUri;
            return true;
        }

        try
        {
            string path = Path.IsPathRooted(source)
                ? source
                : Path.Combine(Application.persistentDataPath, source);
            uri = new Uri(Path.GetFullPath(path)).AbsoluteUri;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"RF-Invalid image path: {exception.Message}");
            return false;
        }
    }

    private static string BuildResultText(
        string operation,
        bool result,
        long code,
        string message)
    {
        string state = result ? "Succeeded" : "Failed";
        string resultText = $"{operation}: {state}\ncode: {code}";
        if (!string.IsNullOrEmpty(message))
        {
            resultText += $"\nmessage: {message}";
        }

        return resultText;
    }

    private static string BuildActionStatus(
        string buttonTitle,
        string buttonValue,
        string buttonValueType,
        ulong buttonIndex)
    {
        if (string.IsNullOrEmpty(buttonTitle) &&
            string.IsNullOrEmpty(buttonValue) &&
            string.IsNullOrEmpty(buttonValueType))
        {
            return "Content closed without selecting an action.";
        }

        return
            $"Action selected: {buttonTitle}\n" +
            $"type: {buttonValueType}, index: {buttonIndex}\n" +
            $"value: {buttonValue}";
    }

    private static void SetStatus(Text target, string value, bool? succeeded)
    {
        if (target == null)
        {
            return;
        }

        target.text = value;
        target.color = !succeeded.HasValue
            ? SampleAppUIFactory.SecondaryTextColor
            : succeeded.Value
                ? SampleAppUIFactory.SuccessColor
                : SampleAppUIFactory.ErrorColor;
    }

    private static void EnqueueOnMainThread(Action action)
    {
        if (action != null)
        {
            MainThreadActions.Enqueue(action);
        }
    }
}
