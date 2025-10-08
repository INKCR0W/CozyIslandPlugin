using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyIsland.UI
{
    public static class EasyuGUI
    {
        private static Transform _currentParent;        // 当前控件挂载点
        private static VerticalLayoutGroup _vlg;        // ScrollView 里的 Content
        private static HorizontalLayoutGroup _hlg;      // Horizontal 区域

        public static void UILabel(Transform parent, string text, bool box = false)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.color = Color.white;
            txt.fontSize = 18;
            if (box)
            {
                var img = go.AddComponent<Image>();
                img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                var padding = go.AddComponent<LayoutElement>();
                padding.minHeight = 30;
                txt.alignment = TextAlignmentOptions.Center;
            }
        }

        public static bool UIButton(string text)
        {
            var go = new GameObject("Button");
            go.transform.SetParent(GetCurrentGroup(), false);
            var btn = go.AddComponent<Button>();
            var img = go.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            btn.targetGraphic = img;

            var txt = NewText(go, text);
            bool clicked = false;
            btn.onClick.AddListener(() => clicked = true);
            return clicked;
        }

        public static bool UIToggle(bool value, string text, bool buttonStyle = false)
        {
            return UIButton(text);
        }

        public static void UISpace(float height)
        {
            var go = new GameObject("Space");
            go.transform.SetParent(GetCurrentGroup(), false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
        }

        /// <summary>
        /// 开始一个 ScrollView，返回传入的 scroll 值（未做双向回写，够用即可）。
        /// 结束后必须调用 UIScrollViewEnd()。
        /// </summary>
        public static Vector2 UIScrollViewBegin(Transform parent, Vector2 scroll, float height)
        {
            var go = new GameObject("ScrollView");
            go.transform.SetParent(parent, false);
            var scrollRt = go.GetComponent<RectTransform>();
            scrollRt.sizeDelta = new Vector2(0, height);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = height;

            var scrollRect = go.AddComponent<ScrollRect>();
            var viewport = new GameObject("Viewport").AddComponent<RectTransform>();
            viewport.SetParent(scrollRt, false);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.sizeDelta = Vector2.zero;
            viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.3f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content").AddComponent<RectTransform>();
            content.SetParent(viewport, false);
            scrollRect.content = content;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 5;
            vlg.padding = new RectOffset(5, 5, 5, 5);
            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _vlg = vlg;
            _currentParent = content;
            return scroll;
        }

        public static void UIScrollViewEnd()
        {
            _vlg = null;
            _currentParent = null;
        }

        public static void UIHorizontalBegin(Transform parent)
        {
            var go = new GameObject("Horizontal");
            go.transform.SetParent(parent, false);
            _hlg = go.AddComponent<HorizontalLayoutGroup>();
            _hlg.childControlWidth = true;
            _hlg.childForceExpandWidth = true;
            _hlg.spacing = 5;
            _currentParent = go.transform;
        }

        public static void UIHorizontalEnd()
        {
            _hlg = null;
            _currentParent = null;
        }

        private static Transform GetCurrentGroup()
        {
            if (_hlg != null) return _hlg.transform;
            if (_vlg != null) return _vlg.transform;
            return _currentParent;
        }

        private static TextMeshProUGUI NewText(GameObject parent, string text)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent.transform, false);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.color = Color.white;
            txt.fontSize = 18;
            txt.alignment = TextAlignmentOptions.Center;
            var rt = txt.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            return txt;
        }
    }
}
