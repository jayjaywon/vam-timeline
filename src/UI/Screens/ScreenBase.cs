using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VamTimeline
{
    public abstract class ScreenBase : MonoBehaviour
    {
        public class ScreenChangeRequestedEvent : UnityEvent<string> { }

        protected static readonly Color navButtonColor = new Color(0.8f, 0.7f, 0.8f);
        private static readonly Font _font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        public ScreenChangeRequestedEvent onScreenChangeRequested = new ScreenChangeRequestedEvent();
        public Transform popupParent;
        public abstract string screenId { get; }

        protected AtomAnimation animation => plugin.animation;
        protected OperationsFactory operations => new OperationsFactory(animation, current);

        protected IAtomPlugin plugin;
        protected VamPrefabFactory prefabFactory;
        protected AtomAnimationClip current;
        protected bool _disposing;

        protected ScreenBase()
        {
        }

        public virtual void Init(IAtomPlugin plugin)
        {
            this.plugin = plugin;
            prefabFactory = gameObject.AddComponent<VamPrefabFactory>();
            prefabFactory.plugin = plugin;
            plugin.animation.onCurrentAnimationChanged.AddListener(OnCurrentAnimationChanged);
            current = plugin.animation?.current;
        }

        protected virtual void OnCurrentAnimationChanged(AtomAnimation.CurrentAnimationChangedEventArgs args)
        {
            current = plugin.animation?.current;
        }

        protected Text CreateHeader(string val, int level)
        {
            var headerUI = prefabFactory.CreateSpacer();
            headerUI.height = 40f;

            var text = headerUI.gameObject.AddComponent<Text>();
            text.text = val;
            text.font = _font;
            switch (level)
            {
                case 1:
                    text.fontSize = 30;
                    text.fontStyle = FontStyle.Bold;
                    text.color = new Color(0.95f, 0.9f, 0.92f);
                    break;
                case 2:
                    text.fontSize = 28;
                    text.fontStyle = FontStyle.Bold;
                    text.color = new Color(0.85f, 0.8f, 0.82f);
                    break;
            }

            return text;
        }

        protected UIDynamicButton CreateChangeScreenButton(string label, string screenName)
        {
            var ui = prefabFactory.CreateButton(label);
            ui.button.onClick.AddListener(() => onScreenChangeRequested.Invoke(screenName));
            ui.buttonColor = navButtonColor;
            return ui;
        }

        protected string GetNewLayerName()
        {
            var layers = new HashSet<string>(animation.clips.Select(c => c.animationLayer));
            for (var i = 1; i < 999; i++)
            {
                var layerName = "Layer " + i;
                if (!layers.Contains(layerName)) return layerName;
            }
            return Guid.NewGuid().ToString();
        }

        public virtual void OnDestroy()
        {
            _disposing = true;
            onScreenChangeRequested.RemoveAllListeners();
            plugin.animation.onCurrentAnimationChanged.RemoveListener(OnCurrentAnimationChanged);
        }
    }
}

