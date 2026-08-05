using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class SampleAppUIFactory
{
    public static readonly Color BackgroundColor = FromHex("F5F7FA");
    public static readonly Color CardColor = Color.white;
    public static readonly Color PrimaryColor = FromHex("009E8F");
    public static readonly Color PrimaryDarkColor = FromHex("007D72");
    public static readonly Color TextColor = FromHex("1F2937");
    public static readonly Color SecondaryTextColor = FromHex("667085");
    public static readonly Color BorderColor = FromHex("D8DEE8");
    public static readonly Color SuccessColor = FromHex("087A55");
    public static readonly Color ErrorColor = FromHex("B42318");

    private static Font _font;

    public static Font Font
    {
        get
        {
            if (_font == null)
            {
                _font = Font.CreateDynamicFontFromOSFont(
                    new[]
                    {
                        "Hiragino Sans",
                        "Noto Sans CJK JP",
                        "Noto Sans JP",
                        "Roboto",
                        "Arial"
                    },
                    30);
                if (_font == null)
                {
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
            }

            return _font;
        }
    }

    public static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = 5;
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    public static Image CreatePanel(Transform parent, string name, Color color)
    {
        GameObject gameObject = CreateUIObject(name, parent);
        Image image = gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static Text CreateText(
        Transform parent,
        string name,
        string value,
        int fontSize = 30,
        TextAnchor alignment = TextAnchor.MiddleLeft,
        FontStyle fontStyle = FontStyle.Normal,
        Color? color = null)
    {
        GameObject gameObject = CreateUIObject(name, parent);
        Text text = gameObject.AddComponent<Text>();
        text.font = Font;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color ?? TextColor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = false;
        text.raycastTarget = false;
        return text;
    }

    public static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Action onClick,
        float height = 76f,
        Color? backgroundColor = null,
        Color? textColor = null)
    {
        GameObject gameObject = CreateUIObject(name, parent);
        Image image = gameObject.AddComponent<Image>();
        image.color = backgroundColor ?? PrimaryColor;

        Button button = gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.94f, 0.94f, 0.94f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.selectedColor = Color.white;
        colors.colorMultiplier = 1f;
        button.colors = colors;
        if (onClick != null)
        {
            button.onClick.AddListener(() => onClick());
        }

        Text text = CreateText(
            gameObject.transform,
            "Label",
            label,
            29,
            TextAnchor.MiddleCenter,
            FontStyle.Bold,
            textColor ?? Color.white);
        Stretch(text.rectTransform, 14f, 14f, 8f, 8f);
        SetPreferredHeight(gameObject, height);
        return button;
    }

    public static InputField CreateInputField(
        Transform parent,
        string name,
        string placeholder,
        string initialValue = "",
        InputField.ContentType contentType = InputField.ContentType.Standard,
        float height = 70f)
    {
        GameObject gameObject = CreateUIObject(name, parent);
        Image image = gameObject.AddComponent<Image>();
        image.color = Color.white;

        InputField inputField = gameObject.AddComponent<InputField>();
        inputField.targetGraphic = image;
        inputField.contentType = contentType;
        inputField.lineType = InputField.LineType.SingleLine;

        Text inputText = CreateText(gameObject.transform, "Text", initialValue, 27);
        inputText.verticalOverflow = VerticalWrapMode.Truncate;
        Stretch(inputText.rectTransform, 18f, 18f, 8f, 8f);

        Text placeholderText = CreateText(
            gameObject.transform,
            "Placeholder",
            placeholder,
            27,
            TextAnchor.MiddleLeft,
            FontStyle.Italic,
            SecondaryTextColor);
        placeholderText.color = new Color(
            placeholderText.color.r,
            placeholderText.color.g,
            placeholderText.color.b,
            0.65f);
        Stretch(placeholderText.rectTransform, 18f, 18f, 8f, 8f);

        inputField.textComponent = inputText;
        inputField.placeholder = placeholderText;
        inputField.text = initialValue;
        SetPreferredHeight(gameObject, height);
        return inputField;
    }

    public static Dropdown CreateDropdown(
        Transform parent,
        string name,
        IReadOnlyList<string> options,
        int selectedIndex = 0,
        float height = 70f)
    {
        GameObject gameObject = CreateUIObject(name, parent);
        Image image = gameObject.AddComponent<Image>();
        image.color = Color.white;

        Dropdown dropdown = gameObject.AddComponent<Dropdown>();
        dropdown.targetGraphic = image;

        Text caption = CreateText(gameObject.transform, "Label", string.Empty, 27);
        Stretch(caption.rectTransform, 18f, 58f, 6f, 6f);
        dropdown.captionText = caption;

        Text arrow = CreateText(
            gameObject.transform,
            "Arrow",
            "▼",
            22,
            TextAnchor.MiddleCenter,
            FontStyle.Normal,
            SecondaryTextColor);
        RectTransform arrowRect = arrow.rectTransform;
        arrowRect.anchorMin = new Vector2(1f, 0f);
        arrowRect.anchorMax = new Vector2(1f, 1f);
        arrowRect.pivot = new Vector2(1f, 0.5f);
        arrowRect.anchoredPosition = new Vector2(-8f, 0f);
        arrowRect.sizeDelta = new Vector2(48f, 0f);

        RectTransform template = CreateDropdownTemplate(gameObject.transform, out Text itemLabel);
        dropdown.template = template;
        dropdown.itemText = itemLabel;

        List<Dropdown.OptionData> optionData = new List<Dropdown.OptionData>();
        if (options != null)
        {
            foreach (string option in options)
            {
                optionData.Add(new Dropdown.OptionData(option));
            }
        }

        dropdown.options = optionData;
        dropdown.value = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, optionData.Count - 1));
        dropdown.RefreshShownValue();
        SetPreferredHeight(gameObject, height);
        return dropdown;
    }

    public static ScrollRect CreateScrollView(Transform parent, string name, out RectTransform content)
    {
        GameObject root = CreateUIObject(name, parent);
        Image rootImage = root.AddComponent<Image>();
        rootImage.color = Color.clear;
        rootImage.raycastTarget = true;

        ScrollRect scrollRect = root.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.scrollSensitivity = 36f;

        GameObject viewportObject = CreateUIObject("Viewport", root.transform);
        viewportObject.AddComponent<RectMask2D>();
        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        Stretch(viewport);

        GameObject contentObject = CreateUIObject("Content", viewportObject.transform);
        content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = contentObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 28, 32);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;
        Stretch(root.GetComponent<RectTransform>());
        return scrollRect;
    }

    public static VerticalLayoutGroup AddVerticalLayout(
        GameObject gameObject,
        int left = 22,
        int right = 22,
        int top = 18,
        int bottom = 18,
        float spacing = 12f)
    {
        VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(left, right, top, bottom);
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return layout;
    }

    public static HorizontalLayoutGroup AddHorizontalLayout(
        GameObject gameObject,
        int left = 0,
        int right = 0,
        int top = 0,
        int bottom = 0,
        float spacing = 12f)
    {
        HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(left, right, top, bottom);
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return layout;
    }

    public static LayoutElement SetPreferredHeight(GameObject gameObject, float height)
    {
        LayoutElement layout = GetOrAddLayoutElement(gameObject);
        layout.preferredHeight = height;
        layout.flexibleHeight = 0f;
        return layout;
    }

    public static LayoutElement SetPreferredWidth(GameObject gameObject, float width)
    {
        LayoutElement layout = GetOrAddLayoutElement(gameObject);
        layout.preferredWidth = width;
        layout.flexibleWidth = 0f;
        return layout;
    }

    public static LayoutElement SetFlexibleHeight(GameObject gameObject, float value = 1f)
    {
        LayoutElement layout = GetOrAddLayoutElement(gameObject);
        layout.flexibleHeight = value;
        return layout;
    }

    public static LayoutElement SetFlexibleWidth(GameObject gameObject, float value = 1f)
    {
        LayoutElement layout = GetOrAddLayoutElement(gameObject);
        layout.flexibleWidth = value;
        return layout;
    }

    public static void Stretch(
        RectTransform rectTransform,
        float left = 0f,
        float right = 0f,
        float top = 0f,
        float bottom = 0f)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(left, bottom);
        rectTransform.offsetMax = new Vector2(-right, -top);
    }

    public static void AnchorToTop(RectTransform rectTransform, float height)
    {
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(0f, height);
    }

    public static void AnchorToBottom(RectTransform rectTransform, float height)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = new Vector2(1f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(0f, height);
    }

    public static Color FromHex(string hex)
    {
        Color color;
        return ColorUtility.TryParseHtmlString("#" + hex, out color) ? color : Color.white;
    }

    public static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));
        eventSystem.transform.SetAsLastSibling();
    }

    private static LayoutElement GetOrAddLayoutElement(GameObject gameObject)
    {
        LayoutElement layout = gameObject.GetComponent<LayoutElement>();
        return layout != null ? layout : gameObject.AddComponent<LayoutElement>();
    }

    private static RectTransform CreateDropdownTemplate(Transform parent, out Text itemLabel)
    {
        GameObject templateObject = CreateUIObject("Template", parent);
        Image templateImage = templateObject.AddComponent<Image>();
        templateImage.color = Color.white;
        Canvas dropdownCanvas = templateObject.AddComponent<Canvas>();
        dropdownCanvas.overrideSorting = true;
        dropdownCanvas.sortingOrder = 100;
        templateObject.AddComponent<GraphicRaycaster>();
        templateObject.AddComponent<CanvasGroup>();

        RectTransform template = templateObject.GetComponent<RectTransform>();
        template.anchorMin = new Vector2(0f, 0f);
        template.anchorMax = new Vector2(1f, 0f);
        template.pivot = new Vector2(0.5f, 1f);
        template.anchoredPosition = new Vector2(0f, -4f);
        template.sizeDelta = new Vector2(0f, 260f);

        ScrollRect scrollRect = templateObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        GameObject viewportObject = CreateUIObject("Viewport", templateObject.transform);
        viewportObject.AddComponent<RectMask2D>();
        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        Stretch(viewport);

        GameObject contentObject = CreateUIObject("Content", viewportObject.transform);
        RectTransform content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = contentObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject itemObject = CreateUIObject("Item", contentObject.transform);
        Image itemImage = itemObject.AddComponent<Image>();
        itemImage.color = Color.white;
        Toggle toggle = itemObject.AddComponent<Toggle>();
        toggle.targetGraphic = itemImage;
        SetPreferredHeight(itemObject, 58f);

        Text checkmark = CreateText(
            itemObject.transform,
            "Item Checkmark",
            "✓",
            22,
            TextAnchor.MiddleCenter,
            FontStyle.Bold,
            PrimaryColor);
        RectTransform checkmarkRect = checkmark.rectTransform;
        checkmarkRect.anchorMin = new Vector2(0f, 0f);
        checkmarkRect.anchorMax = new Vector2(0f, 1f);
        checkmarkRect.pivot = new Vector2(0f, 0.5f);
        checkmarkRect.anchoredPosition = new Vector2(8f, 0f);
        checkmarkRect.sizeDelta = new Vector2(40f, 0f);
        toggle.graphic = checkmark;

        itemLabel = CreateText(itemObject.transform, "Item Label", "Option", 25);
        Stretch(itemLabel.rectTransform, 50f, 12f, 4f, 4f);

        scrollRect.viewport = viewport;
        scrollRect.content = content;
        templateObject.SetActive(false);
        return template;
    }
}

public sealed class SampleSafeAreaFitter : MonoBehaviour
{
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;

    private void Awake()
    {
        Apply();
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea ||
            _lastScreenSize.x != Screen.width ||
            _lastScreenSize.y != Screen.height)
        {
            Apply();
        }
    }

    private void Apply()
    {
        Rect safeArea = Screen.safeArea;
        RectTransform rectTransform = (RectTransform)transform;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Mathf.Max(1f, Screen.width);
        anchorMin.y /= Mathf.Max(1f, Screen.height);
        anchorMax.x /= Mathf.Max(1f, Screen.width);
        anchorMax.y /= Mathf.Max(1f, Screen.height);
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }
}
