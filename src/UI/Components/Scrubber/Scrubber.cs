using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VamTimeline
{
    public class Scrubber : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private readonly ScrubberStyle _style = new ScrubberStyle();
        private readonly RectTransform _scrubberRect;
        private readonly Text _timeText;

        private Vector2 _lastScrubberUpdate = new Vector2(-1, -1);
        private Vector2 _lastTextUpdate = new Vector2(-1, -1);
        private ScrubberMarkers _markers;

        public AtomAnimation animation;

        public Scrubber()
        {
            var image = gameObject.AddComponent<Image>();
            image.raycastTarget = false;

            var mask = gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            CreateBackground(gameObject, _style.BackgroundColor);
            CreateMarkers();
            _scrubberRect = CreateLine(gameObject, _style.ScrubberColor).GetComponent<RectTransform>();
            _timeText = CreateTime();
        }

        private GameObject CreateBackground(GameObject parent, Color color)
        {
            var go = new GameObject();
            go.transform.SetParent(parent.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.StretchParent();

            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            return go;
        }

        private void CreateMarkers()
        {
            var go = new GameObject();
            go.transform.SetParent(transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.StretchParent();

            _markers = go.AddComponent<ScrubberMarkers>();
            _markers.raycastTarget = false;
            _markers.style = _style;
        }

        private GameObject CreateLine(GameObject parent, Color color)
        {
            var go = new GameObject("Scrubber");
            go.transform.SetParent(parent.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.StretchParent();

            var line = new GameObject("Scrubber Line");
            line.transform.SetParent(go.transform, false);

            var lineRect = line.AddComponent<RectTransform>();
            lineRect.StretchCenter();
            lineRect.sizeDelta = new Vector2(_style.ScrubberSize, 0);

            var image = line.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            return line;
        }

        private Text CreateTime()
        {
            var go = new GameObject();
            go.transform.SetParent(transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.StretchParent();
            rect.anchoredPosition = new Vector2(0, -5f);

            var text = go.AddComponent<Text>();
            text.text = "0.000s / 0.000s";
            text.font = _style.Font;
            text.fontSize = 24;
            text.color = _style.FontColor;
            text.alignment = TextAnchor.UpperCenter;
            text.raycastTarget = false;

            return text;
        }

        public void Update()
        {
            if (UIPerformance.ShouldSkip()) return;

            var currentUpdate = new Vector2(animation.clipTime, animation.current.animationLength);

            if (_lastScrubberUpdate != currentUpdate)
            {
                if (_lastScrubberUpdate.y != currentUpdate.y)
                    _markers.length = currentUpdate.y;

                _lastScrubberUpdate = currentUpdate;
                var ratio = Mathf.Clamp01(currentUpdate.x / currentUpdate.y);
                _scrubberRect.anchorMin = new Vector2(ratio, 0);
                _scrubberRect.anchorMax = new Vector2(ratio, 1);
            }

            if (_lastTextUpdate != currentUpdate && UIPerformance.ShouldRun(UIPerformance.LowFPSUIRate))
            {
                _lastTextUpdate = currentUpdate;
                _timeText.text = $"{animation.clipTime:0.000}s / {animation.current.animationLength:0.000}s";
            }
        }

        public void OnDisable()
        {
            _scrubberRect.anchorMin = new Vector2(0, 0);
            _scrubberRect.anchorMax = new Vector2(0, 1);
            _timeText.text = $"Locked";
        }

        public void OnEnable()
        {
            if (animation == null) return;

            var ratio = Mathf.Clamp01(animation.clipTime / animation.current.animationLength);
            _scrubberRect.anchorMin = new Vector2(ratio, 0);
            _scrubberRect.anchorMax = new Vector2(ratio, 1);
            _timeText.text = $"{animation.clipTime:0.000}s / {animation.current.animationLength:0.000}s";
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
            UpdateScrubberFromView(eventData, true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            UpdateScrubberFromView(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateScrubberFromView(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            UpdateScrubberFromView(eventData, true);
        }

        private void UpdateScrubberFromView(PointerEventData eventData, bool final = false)
        {
            if (animation == null) return;
            Vector2 localPosition;
            var rect = GetComponent<RectTransform>();
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out localPosition))
                return;
            var ratio = Mathf.Clamp01((localPosition.x + rect.sizeDelta.x / 2f) / rect.sizeDelta.x);
            var time = (animation.current.animationLength * ratio).Snap(final ? animation.snap : 0);
            if (time >= animation.current.animationLength - 0.001f)
            {
                if (animation.current.loop)
                    time = 0f;
                else
                    time = animation.current.animationLength;
            }
            animation.clipTime = time;
        }
    }
}
