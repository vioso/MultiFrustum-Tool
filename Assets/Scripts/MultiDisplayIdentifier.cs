using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MultiDisplayIdentifier : MonoBehaviour
{
    [Header("Toggle from your UI")]
    public bool showLabels = false;            // shows/hides all labels
    public void SetShowLabels(bool v) { showLabels = v; SetActive(v); }

    [Header("Look")]
    public int fontSize = 120;
    public Color textColor = Color.white;
    public Color shadowColor = new Color(0, 0, 0, 0.6f);
    public Vector2 shadowDistance = new Vector2(2f, -2f);
    public int sortingOrder = 32760;          // draw on top of other canvases

    [Header("Text")]
    public string prefix = "DISPLAY";         // e.g., "DISPLAY 0 - 1920x1080"

    private readonly Dictionary<int, GameObject> canvases = new Dictionary<int, GameObject>();

    void Start()
    {
        BuildOrRefresh();
        SetActive(showLabels);
    }

    void OnValidate()
    {
        if (!Application.isPlaying) return;
        BuildOrRefresh();
        SetActive(showLabels);
    }

    void BuildOrRefresh()
    {
        // Make (or reuse) one overlay canvas per display.
        for (int i = 0; i < Display.displays.Length; i++)
        {
            if (!canvases.ContainsKey(i) || canvases[i] == null)
                canvases[i] = CreateCanvasForDisplay(i);

            // Update the text with the current resolution.
            var txt = canvases[i].GetComponentInChildren<Text>(true);
            var d = Display.displays[i];
            txt.fontSize = fontSize;
            txt.color = textColor;
            txt.text = $"{prefix} {i} - {d.systemWidth}x{d.systemHeight}";

            // Shadow & order
            var sh = txt.GetComponent<Shadow>();
            sh.effectColor = shadowColor;
            sh.effectDistance = shadowDistance;
            canvases[i].GetComponent<Canvas>().sortingOrder = sortingOrder;
        }
    }

    GameObject CreateCanvasForDisplay(int displayIndex)
    {
        var go = new GameObject($"DisplayLabel_Canvas_{displayIndex}");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.targetDisplay = displayIndex;
        canvas.sortingOrder = sortingOrder;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>().enabled = false;

        // Big centered Text
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var txt = textGO.AddComponent<Text>();
        txt.raycastTarget = false;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        var rect = txt.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        textGO.AddComponent<Shadow>();

        return go;
    }

    void SetActive(bool active)
    {
        foreach (var kv in canvases)
            if (kv.Value) kv.Value.SetActive(active);
    }
}
