using System.Collections.Generic;
using System.Linq;

namespace VamTimeline
{
    /// <summary>
    /// VaM Timeline
    /// By Acidbubbles
    /// Animation timeline with keyframes
    /// Source: https://github.com/acidbubbles/vam-timeline
    /// </summary>
    public class AtomAnimationControllersUI : AtomAnimationBaseUI
    {
        public const string ScreenName = "Controllers";
        public override string Name => ScreenName;

        private class TargetRef
        {
            internal JSONStorableBool KeyframeJSON;
            internal FreeControllerAnimationTarget Target;
        }

        private List<TargetRef> _targets;
        private JSONStorableStringChooser _curveTypeJSON;
        private JSONStorableAction _smoothAllFramesJSON;

        public AtomAnimationControllersUI(IAtomPlugin plugin)
            : base(plugin)
        {

        }
        public override void Init()
        {
            base.Init();

            // Left side

            InitAnimationSelectorUI(false);

            InitPlaybackUI(false);

            InitFrameNavUI(false);

            InitCurvesUI();

            InitClipboardUI(false);

            // Right side

            InitDisplayUI(true);
        }

        private void InitCurvesUI()
        {
            _curveTypeJSON = new JSONStorableStringChooser(StorableNames.ChangeCurve, CurveTypeValues.CurveTypes, "", "Change Curve", ChangeCurve);
            var curveTypeUI = Plugin.CreateScrollablePopup(_curveTypeJSON, false);
            curveTypeUI.popupPanelHeight = 340f;
            _linkedStorables.Add(_curveTypeJSON);

            _smoothAllFramesJSON = new JSONStorableAction(StorableNames.SmoothAllFrames, () => SmoothAllFrames());

            var smoothAllFramesUI = Plugin.CreateButton("Smooth All Frames", false);
            smoothAllFramesUI.button.onClick.AddListener(() => _smoothAllFramesJSON.actionCallback());
            _components.Add(smoothAllFramesUI);
        }

        public override void UpdatePlaying()
        {
            base.UpdatePlaying();
            UpdateValues();
        }

        public override void AnimationFrameUpdated()
        {
            base.AnimationFrameUpdated();
            UpdateValues();
            UpdateCurrentCurveType();
        }

        public override void AnimationModified()
        {
            base.AnimationModified();
            RefreshTargetsList();
            UpdateCurrentCurveType();
        }

        private void UpdateCurrentCurveType()
        {
            var time = Plugin.Animation.Time.Snap();
            if (Plugin.Animation.Current.Loop && (time.IsSameFrame(0) || time.IsSameFrame(Plugin.Animation.Current.AnimationLength)))
            {
                _curveTypeJSON.valNoCallback = "(Loop)";
                return;
            }
            var curveTypes = Plugin.Animation.Current.GetAllOrSelectedControllerTargets()
                .Select(c => c.Settings.ContainsKey(time) ? c.Settings[time] : null)
                .Where(s => s != null)
                .Select(s => s.CurveType)
                .Distinct()
                .ToArray();
            if (curveTypes.Length == 0)
                _curveTypeJSON.valNoCallback = "(No Keyframe)";
            else if (curveTypes.Length == 1)
                _curveTypeJSON.valNoCallback = curveTypes[0].ToString();
            else
                _curveTypeJSON.valNoCallback = "(" + string.Join("/", curveTypes) + ")";
        }

        private void UpdateValues()
        {
            if (_targets != null)
            {
                var time = Plugin.Animation.Time;
                foreach (var targetRef in _targets)
                {
                    targetRef.KeyframeJSON.valNoCallback = targetRef.Target.X.keys.Any(k => k.time.IsSameFrame(time));
                }
            }
        }

        private void RefreshTargetsList()
        {
            if (Plugin.Animation == null) return;
            if (_targets != null && Enumerable.SequenceEqual(Plugin.Animation.Current.TargetControllers, _targets.Select(t => t.Target)))
                return;
            RemoveTargets();
            var time = Plugin.Animation.Time;
            _targets = new List<TargetRef>();
            foreach (var target in Plugin.Animation.Current.TargetControllers)
            {
                var keyframeJSON = new JSONStorableBool($"{target.Name} Keyframe", target.X.keys.Any(k => k.time.IsSameFrame(time)), (bool val) => ToggleKeyframe(target, val));
                var keyframeUI = Plugin.CreateToggle(keyframeJSON, true);
                _targets.Add(new TargetRef
                {
                    Target = target,
                    KeyframeJSON = keyframeJSON
                });
            }
        }

        public override void Remove()
        {
            RemoveTargets();
            base.Remove();
        }

        private void RemoveTargets()
        {
            if (_targets == null) return;
            foreach (var targetRef in _targets)
            {
                // TODO: Take care of keeping track of those separately
                Plugin.RemoveToggle(targetRef.KeyframeJSON);
            }
        }

        private void ChangeCurve(string curveType)
        {
            if (string.IsNullOrEmpty(curveType) || curveType.StartsWith("("))
            {
                UpdateCurrentCurveType();
                return;
            }
            if (Plugin.Animation.IsPlaying()) return;
            if (Plugin.Animation.Current.Loop && (Plugin.Animation.Time.IsSameFrame(0) || Plugin.Animation.Time.IsSameFrame(Plugin.Animation.Current.AnimationLength)))
            {
                UpdateCurrentCurveType();
                return;
            }
            Plugin.Animation.ChangeCurve(curveType);
            Plugin.AnimationModified();
        }

        private void SmoothAllFrames()
        {
            if (Plugin.Animation.IsPlaying()) return;
            Plugin.Animation.SmoothAllFrames();
            Plugin.AnimationModified();
        }

        private void ToggleKeyframe(FreeControllerAnimationTarget target, bool val)
        {
            if (Plugin.Animation.IsPlaying()) return;
            var time = Plugin.Animation.Time.Snap();
            if (time.IsSameFrame(0f))
            {
                _targets.First(t => t.Target == target).KeyframeJSON.valNoCallback = true;
                return;
            }
            if (val)
            {
                Plugin.Animation.SetKeyframeToCurrentTransform(target, time);
            }
            else
            {
                target.DeleteFrame(time);
            }
            Plugin.Animation.RebuildAnimation();
            Plugin.AnimationModified();
        }
    }
}

