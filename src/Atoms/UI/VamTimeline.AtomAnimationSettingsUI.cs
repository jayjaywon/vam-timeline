using System;
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
    public class AtomAnimationSettingsUI : AtomAnimationBaseUI
    {
        public const string ScreenName = "Animation Settings";
        public override string Name => ScreenName;

        public const string ChangeLengthModeLocked = "Locked";
        public const string ChangeLengthModeCropExtend = "Crop or Extend";
        public const string ChangeLengthModeStretch = "Stretch";

        private JSONStorableStringChooser _lengthModeJSON;
        private JSONStorableFloat _lengthJSON;
        private JSONStorableFloat _speedJSON;
        private JSONStorableFloat _blendDurationJSON;
        private JSONStorableBool _ensureQuaternionContinuity;
        private JSONStorableBool _loop;
        private JSONStorableStringChooser _addControllerListJSON;
        private JSONStorableAction _toggleControllerJSON;
        private UIDynamicPopup _addControllerUI;
        private JSONStorableStringChooser _linkedAnimationPatternJSON;
        private JSONStorableStringChooser _addStorableListJSON;
        private JSONStorableStringChooser _addParamListJSON;
        private UIDynamicPopup _addFloatParamListUI;
        private UIDynamicButton _toggleControllerUI;
        private UIDynamicButton _toggleFloatParamUI;
        private UIDynamicPopup _addParamListUI;
        private JSONStorableStringChooser _nextAnimationJSON;
        private JSONStorableFloat _nextAnimationTimeJSON;
        private JSONStorableString _nextAnimationPreviewJSON;
        private readonly List<JSONStorableBool> _removeToggles = new List<JSONStorableBool>();

        public AtomAnimationSettingsUI(IAtomPlugin plugin)
            : base(plugin)
        {

        }

        #region Init

        public override void Init()
        {
            base.Init();

            // Left side

            InitAnimationSelectorUI(false);

            InitAnimationSettingsUI(false);

            InitSequenceUI();

            // Right side

            InitControllersUI();

            CreateSpacer(true);

            InitAnimationPatternLinkUI();

            CreateSpacer(true);

            InitFloatParamsUI();

            CreateSpacer(true);

            GenerateRemoveToggles();
        }

        private void InitAnimationSettingsUI(bool rightSide)
        {
            var addAnimationUI = Plugin.CreateButton("Add New Animation", rightSide);
            addAnimationUI.button.onClick.AddListener(() => Plugin.AddAnimationJSON.actionCallback());
            _components.Add(addAnimationUI);

            _lengthModeJSON = new JSONStorableStringChooser("Change Length Mode", new List<string> {
                ChangeLengthModeLocked,
                ChangeLengthModeCropExtend,
                ChangeLengthModeStretch
             }, ChangeLengthModeLocked, "Change Length Mode");
            Plugin.CreateScrollablePopup(_lengthModeJSON);
            _linkedStorables.Add(_lengthModeJSON);

            _lengthJSON = new JSONStorableFloat("AnimationLength", AtomAnimationClip.DefaultAnimationLength, v => UpdateAnimationLength(v), 0.5f, 120f, false, true);

            var lengthUI = Plugin.CreateSlider(_lengthJSON, rightSide);
            lengthUI.valueFormat = "F3";
            _linkedStorables.Add(_lengthJSON);

            _speedJSON = new JSONStorableFloat("AnimationSpeed", 1f, v => UpdateAnimationSpeed(v), 0f, 5f, false);
            var speedUI = Plugin.CreateSlider(_speedJSON, rightSide);
            speedUI.valueFormat = "F3";
            _linkedStorables.Add(_speedJSON);

            _blendDurationJSON = new JSONStorableFloat("BlendDuration", AtomAnimationClip.DefaultBlendDuration, v => UpdateBlendDuration(v), 0f, 5f, false);
            var blendDurationUI = Plugin.CreateSlider(_blendDurationJSON, rightSide);
            blendDurationUI.valueFormat = "F3";
            _linkedStorables.Add(_blendDurationJSON);

            _loop = new JSONStorableBool("Loop", Plugin.Animation?.Current?.Loop ?? true, (bool val) => ChangeLoop(val));
            var loopingUI = Plugin.CreateToggle(_loop);
            _linkedStorables.Add(_loop);

            _ensureQuaternionContinuity = new JSONStorableBool("Ensure Quaternion Continuity", true, (bool val) => SetEnsureQuaternionContinuity(val));
            Plugin.CreateToggle(_ensureQuaternionContinuity);
            _linkedStorables.Add(_ensureQuaternionContinuity);
        }

        private void InitSequenceUI()
        {
            _nextAnimationJSON = new JSONStorableStringChooser("Next Animation", GetEligibleNextAnimations(), "", "Next Animation", (string val) => ChangeNextAnimation(val));
            var nextAnimationUI = Plugin.CreateScrollablePopup(_nextAnimationJSON);
            _linkedStorables.Add(_nextAnimationJSON);

            _nextAnimationTimeJSON = new JSONStorableFloat("Next Blend After Seconds", 1f, (float val) => SetNextAnimationTime(val), 0f, 60f, false)
            {
                valNoCallback = Plugin.Animation.Current.NextAnimationTime
            };
            var nextAnimationTimeUI = Plugin.CreateSlider(_nextAnimationTimeJSON);
            nextAnimationTimeUI.valueFormat = "F3";
            _linkedStorables.Add(_nextAnimationTimeJSON);

            _nextAnimationPreviewJSON = new JSONStorableString("Next Preview", "");
            var nextAnimationResultUI = Plugin.CreateTextField(_nextAnimationPreviewJSON);
            nextAnimationResultUI.height = 30f;
            _linkedStorables.Add(_nextAnimationPreviewJSON);

            UpdateNextAnimationPreview();
        }

        private void UpdateForcedNextAnimationTime()
        {
            if (Plugin.Animation.Current.Loop) return;
            if (Plugin.Animation.Current.NextAnimationId == null)
            {
                Plugin.Animation.Current.NextAnimationTime = 0;
                _nextAnimationTimeJSON.valNoCallback = 0;
            }
            Plugin.Animation.Current.NextAnimationTime = (Plugin.Animation.Current.AnimationLength - Plugin.Animation.Current.BlendDuration) * Plugin.Animation.Current.Speed;
            _nextAnimationTimeJSON.valNoCallback = Plugin.Animation.Current.NextAnimationTime;
        }

        private void UpdateNextAnimationPreview()
        {
            var current = Plugin.Animation.Current;

            if (current.NextAnimationId == null)
            {
                _nextAnimationPreviewJSON.val = "No next animation configured";
                return;
            }

            if (!current.Loop)
            {
                _nextAnimationPreviewJSON.val = $"Will loop once and blend at {Math.Round(current.AnimationLength - current.BlendDuration, 2)}s";
                return;
            }

            if (_nextAnimationTimeJSON.val == 0)
            {
                _nextAnimationPreviewJSON.val = "Will loop indefinitely";
            }
            else
            {
                _nextAnimationPreviewJSON.val = $"Will loop {Math.Round(current.NextAnimationTime + current.BlendDuration / current.AnimationLength, 2)} times including blending";
            }
        }

        private List<string> GetEligibleNextAnimations()
        {
            return new[] { "" }.Concat(Plugin.Animation.Clips.Where(n => n != Plugin.Animation.Current)).Select(c => c.AnimationLabel).ToList();
        }

        private void InitControllersUI()
        {
            _addControllerListJSON = new JSONStorableStringChooser("Animate Controller", GetEligibleFreeControllers().ToList(), GetEligibleFreeControllers().FirstOrDefault(), "Animate controller", (string name) => UIUpdated())
            {
                isStorable = false
            };

            _toggleControllerJSON = new JSONStorableAction("Toggle Controller", () => AddAnimatedController());

            _addControllerUI = Plugin.CreateScrollablePopup(_addControllerListJSON, true);
            _addControllerUI.popupPanelHeight = 900f;
            _linkedStorables.Add(_addControllerListJSON);

            _toggleControllerUI = Plugin.CreateButton("Add Controller", true);
            _toggleControllerUI.button.onClick.AddListener(() => _toggleControllerJSON.actionCallback());
            _components.Add(_toggleControllerUI);
        }

        private IEnumerable<string> GetEligibleFreeControllers()
        {
            foreach (var fc in Plugin.ContainingAtom.freeControllers)
            {
                if (fc.name == "control") yield return fc.name;
                if (!fc.name.EndsWith("Control")) continue;
                yield return fc.name;
            }
        }

        private void InitAnimationPatternLinkUI()
        {
            _linkedAnimationPatternJSON = new JSONStorableStringChooser("Linked Animation Pattern", new[] { "" }.Concat(SuperController.singleton.GetAtoms().Where(a => a.type == "AnimationPattern").Select(a => a.uid)).ToList(), "", "Linked Animation Pattern", (string uid) => LinkAnimationPattern(uid))
            {
                isStorable = false
            };

            var linkedAnimationPatternUI = Plugin.CreateScrollablePopup(_linkedAnimationPatternJSON, true);
            linkedAnimationPatternUI.popupPanelHeight = 800f;
            linkedAnimationPatternUI.popup.onOpenPopupHandlers += () => _linkedAnimationPatternJSON.choices = new[] { "" }.Concat(SuperController.singleton.GetAtoms().Where(a => a.type == "AnimationPattern").Select(a => a.uid)).ToList();
            _linkedStorables.Add(_linkedAnimationPatternJSON);
        }

        private void InitFloatParamsUI()
        {
            var storables = GetStorablesWithFloatParams().ToList();
            _addStorableListJSON = new JSONStorableStringChooser("Animate Storable", storables, storables.Contains("geometry") ? "geometry" : storables.FirstOrDefault(), "Animate Storable", (string name) => RefreshStorableFloatsList(true))
            {
                isStorable = false
            };

            _addParamListJSON = new JSONStorableStringChooser("Animate Param", new List<string>(), "", "Animate Param", (string name) => UIUpdated())
            {
                isStorable = false
            };

            _addFloatParamListUI = Plugin.CreateScrollablePopup(_addStorableListJSON, true);
            _addFloatParamListUI.popupPanelHeight = 700f;
            _addFloatParamListUI.popup.onOpenPopupHandlers += () => _addStorableListJSON.choices = GetStorablesWithFloatParams().ToList();
            _linkedStorables.Add(_addStorableListJSON);

            _addParamListUI = Plugin.CreateScrollablePopup(_addParamListJSON, true);
            _addParamListUI.popupPanelHeight = 600f;
            _addParamListUI.popup.onOpenPopupHandlers += () => RefreshStorableFloatsList(false);
            _linkedStorables.Add(_addParamListJSON);

            _toggleFloatParamUI = Plugin.CreateButton("Add Param", true);
            _toggleFloatParamUI.button.onClick.AddListener(() => AddAnimatedFloatParam());
            _components.Add(_toggleFloatParamUI);
        }

        private IEnumerable<string> GetStorablesWithFloatParams()
        {
            foreach (var storableId in Plugin.ContainingAtom.GetStorableIDs().OrderBy(s => s))
            {
                var storable = Plugin.ContainingAtom.GetStorableByID(storableId);
                if ((storable?.GetFloatParamNames()?.Count ?? 0) > 0)
                    yield return storableId;
            }
        }

        private void RefreshStorableFloatsList(bool autoSelect)
        {
            if (string.IsNullOrEmpty(_addStorableListJSON.val))
            {
                _addParamListJSON.choices = new List<string>();
                if (autoSelect)
                    _addParamListJSON.valNoCallback = "";
                return;
            }
            var values = Plugin.ContainingAtom.GetStorableByID(_addStorableListJSON.val)?.GetFloatParamNames() ?? new List<string>();
            _addParamListJSON.choices = values;
            if (autoSelect && !values.Contains(_addParamListJSON.val))
                _addParamListJSON.valNoCallback = values.FirstOrDefault();
        }

        private void GenerateRemoveToggles()
        {
            if (string.Join(",", Plugin.Animation.Current.AllTargets.Select(tc => tc.Name).OrderBy(n => n).ToArray()) == string.Join(",", _removeToggles.Select(ct => ct.name).OrderBy(n => n).ToArray()))
                return;

            ClearRemoveToggles();
            foreach (var target in Plugin.Animation.Current.TargetControllers)
            {
                var jsb = new JSONStorableBool(target.Name, true, (bool val) =>
                {
                    _addControllerListJSON.val = target.Name;
                    RemoveAnimatedController(target);
                });
                var jsbUI = Plugin.CreateToggle(jsb, true);
                _removeToggles.Add(jsb);
            }
            foreach (var target in Plugin.Animation.Current.TargetFloatParams)
            {
                var jsb = new JSONStorableBool(target.Name, true, (bool val) =>
                {
                    _addStorableListJSON.val = target.Storable.name;
                    _addParamListJSON.val = target.FloatParam.name;
                    RemoveFloatParam(target);
                });
                var jsbUI = Plugin.CreateToggle(jsb, true);
                _removeToggles.Add(jsb);
            }
            // Ensures shows on top
            _addControllerListJSON.popup.Toggle();
            _addControllerListJSON.popup.Toggle();
            _addStorableListJSON.popup.Toggle();
            _addStorableListJSON.popup.Toggle();
            _addParamListJSON.popup.Toggle();
            _addParamListJSON.popup.Toggle();
        }

        private void ClearRemoveToggles()
        {
            if (_removeToggles == null) return;
            foreach (var toggleJSON in _removeToggles)
            {
                // TODO: Take care of keeping track of those separately
                Plugin.RemoveToggle(toggleJSON);
            }
        }

        #endregion

        #region Callbacks

        private void UpdateAnimationLength(float v)
        {
            if (v <= 0.1f)
            {
                v = 0.1f;
                _lengthJSON.valNoCallback = v;
            }
            switch (_lengthModeJSON.val)
            {
                case ChangeLengthModeLocked:
                    {
                        _lengthJSON.valNoCallback = Plugin.Animation.Current.AnimationLength;
                        return;
                    }
                case ChangeLengthModeStretch:
                    Plugin.Animation.Current.StretchLength(v);
                    break;
                case ChangeLengthModeCropExtend:
                    Plugin.Animation.Current.CropOrExtendLength(v);
                    break;
            }
            Plugin.Animation.RebuildAnimation();
            UpdateForcedNextAnimationTime();
            Plugin.AnimationModified();
        }

        private void UpdateAnimationSpeed(float v)
        {
            if (v < 0)
                _speedJSON.valNoCallback = v = 0f;
            Plugin.Animation.Speed = v;
            UpdateForcedNextAnimationTime();
            Plugin.AnimationModified();
        }

        private void UpdateBlendDuration(float v)
        {
            if (v < 0)
                _blendDurationJSON.valNoCallback = v = 0f;
            if (!Plugin.Animation.Current.Loop && v > Plugin.Animation.Current.AnimationLength)
                _blendDurationJSON.valNoCallback = v = Plugin.Animation.Current.AnimationLength;
            Plugin.Animation.Current.BlendDuration = v;
            UpdateForcedNextAnimationTime();
            Plugin.AnimationModified();
        }

        private void ChangeLoop(bool val)
        {
            Plugin.Animation.Current.Loop = val;
            Plugin.Animation.RebuildAnimation();
            Plugin.AnimationModified();
        }

        private void SetEnsureQuaternionContinuity(bool val)
        {
            Plugin.Animation.Current.EnsureQuaternionContinuity = val;
            Plugin.AnimationModified();
        }

        private void ChangeNextAnimation(string val)
        {
            Plugin.Animation.Current.NextAnimationId = Plugin.Animation.Clips.First(c => c.AnimationLabel == val).AnimationId;
            Plugin.AnimationModified();
        }

        private void SetNextAnimationTime(float nextTime)
        {
            if (Plugin.Animation.Current.NextAnimationId == null)
            {
                _nextAnimationTimeJSON.valNoCallback = nextTime;
                Plugin.Animation.Current.NextAnimationTime = nextTime;
                return;
            }
            else if (!Plugin.Animation.Current.Loop)
            {
                nextTime = Plugin.Animation.Current.AnimationLength - Plugin.Animation.Current.BlendDuration;
            }

            if (nextTime < 0f)
                nextTime = 0f;

            _nextAnimationTimeJSON.valNoCallback = nextTime;
            Plugin.Animation.Current.NextAnimationTime = nextTime;
            Plugin.AnimationModified();
        }

        private void LinkAnimationPattern(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                Plugin.Animation.Current.AnimationPattern = null;
                return;
            }
            var animationPattern = SuperController.singleton.GetAtomByUid(uid)?.GetComponentInChildren<AnimationPattern>();
            if (animationPattern == null)
            {
                SuperController.LogError($"VamTimeline: Could not find Animation Pattern '{uid}'");
                return;
            }
            Plugin.Animation.Current.AnimationPattern = animationPattern;
            animationPattern.SetBoolParamValue("autoPlay", false);
            animationPattern.SetBoolParamValue("pause", false);
            animationPattern.SetBoolParamValue("loop", false);
            animationPattern.SetBoolParamValue("loopOnce", false);
            animationPattern.SetFloatParamValue("speed", Plugin.Animation.Speed);
            animationPattern.ResetAnimation();
            Plugin.AnimationModified();
        }

        private void AddAnimatedController()
        {
            try
            {
                var uid = _addControllerListJSON.val;
                var controller = Plugin.ContainingAtom.freeControllers.Where(x => x.name == uid).FirstOrDefault();
                if (controller == null)
                {
                    SuperController.LogError($"VamTimeline: Controller {uid} in atom {Plugin.ContainingAtom.uid} does not exist");
                    return;
                }
                if (Plugin.Animation.Current.TargetControllers.Any(c => c.Controller == controller))
                    return;

                controller.currentPositionState = FreeControllerV3.PositionState.On;
                controller.currentRotationState = FreeControllerV3.RotationState.On;
                var target = Plugin.Animation.Add(controller);
                Plugin.AnimationModified();
            }
            catch (Exception exc)
            {
                SuperController.LogError("VamTimeline.AtomAnimationSettingsUI.AddAnimatedController: " + exc);
            }
        }

        private void AddAnimatedFloatParam()
        {
            try
            {
                var storable = Plugin.ContainingAtom.GetStorableByID(_addStorableListJSON.val);
                if (storable == null)
                {
                    SuperController.LogError($"VamTimeline: Storable {_addStorableListJSON.val} in atom {Plugin.ContainingAtom.uid} does not exist");
                    return;
                }
                var sourceFloatParam = storable.GetFloatJSONParam(_addParamListJSON.val);
                if (sourceFloatParam == null)
                {
                    SuperController.LogError($"VamTimeline: Param {_addParamListJSON.val} in atom {Plugin.ContainingAtom.uid} does not exist");
                    return;
                }
                if (Plugin.Animation.Current.TargetFloatParams.Any(c => c.FloatParam == sourceFloatParam))
                {
                    return;
                }

                Plugin.Animation.Add(storable, sourceFloatParam);
                Plugin.AnimationModified();
            }
            catch (Exception exc)
            {
                SuperController.LogError("VamTimeline.AtomAnimationSettingsUI.ToggleAnimatedFloatParam: " + exc);
            }
        }

        private void RemoveAnimatedController(FreeControllerAnimationTarget target)
        {
            try
            {
                Plugin.Animation.Remove(target.Controller);
                Plugin.AnimationModified();
            }
            catch (Exception exc)
            {
                SuperController.LogError("VamTimeline.AtomAnimationSettingsUI.RemoveAnimatedController: " + exc);
            }
        }

        private void RemoveFloatParam(FloatParamAnimationTarget target)
        {
            try
            {
                Plugin.Animation.Current.TargetFloatParams.Remove(target);
                Plugin.AnimationModified();
            }
            catch (Exception exc)
            {
                SuperController.LogError("VamTimeline.AtomAnimationSettingsUI.RemoveAnimatedController: " + exc);
            }
        }

        #endregion

        #region Events

        public override void UIUpdated()
        {
            base.UIUpdated();
        }

        public override void AnimationModified()
        {
            base.AnimationModified();
            GenerateRemoveToggles();

            var current = Plugin.Animation.Current;
            _lengthJSON.valNoCallback = current.AnimationLength;
            _speedJSON.valNoCallback = current.Speed;
            _blendDurationJSON.valNoCallback = current.BlendDuration;
            _loop.valNoCallback = current.Loop;
            _ensureQuaternionContinuity.valNoCallback = current.EnsureQuaternionContinuity;
            _nextAnimationJSON.valNoCallback = current.NextAnimationLabel;
            _nextAnimationJSON.choices = GetEligibleNextAnimations();
            _nextAnimationTimeJSON.valNoCallback = current.NextAnimationTime;
            _linkedAnimationPatternJSON.valNoCallback = current.AnimationPattern?.containingAtom.uid ?? "";
            UpdateNextAnimationPreview();
        }

        public override void Remove()
        {
            ClearRemoveToggles();
            base.Remove();
        }

        #endregion
    }
}

