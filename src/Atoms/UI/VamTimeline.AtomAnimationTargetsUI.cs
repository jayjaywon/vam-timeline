using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetBundles;
using UnityEngine;

namespace VamTimeline
{
    /// <summary>
    /// VaM Timeline
    /// By Acidbubbles
    /// Animation timeline with keyframes
    /// Source: https://github.com/acidbubbles/vam-timeline
    /// </summary>
    public class AtomAnimationTargetsUI : AtomAnimationBaseUI
    {
        public const string ScreenName = "Add/Remove Targets";
        public override string Name => ScreenName;

        private JSONStorableStringChooser _addControllerListJSON;
        private UIDynamicPopup _addControllerUI;
        private JSONStorableStringChooser _addStorableListJSON;
        private JSONStorableStringChooser _addParamListJSON;
        private UIDynamicPopup _addFloatParamListUI;
        private UIDynamicButton _toggleControllerUI;
        private UIDynamicButton _toggleFloatParamUI;
        private UIDynamicPopup _addParamListUI;
        private AnimationStep animStep;
        private readonly List<JSONStorableBool> _removeToggles = new List<JSONStorableBool>();

        public AtomAnimationTargetsUI(IAtomPlugin plugin)
            : base(plugin)
        {

        }

        #region Init

        public class TestHandler : TriggerActionHandler
        {
            public RectTransform CreateTriggerActionDiscreteUI()
            {
                var apAsset = SuperController.singleton.atomAssets.FirstOrDefault(a => a.assetName == "AnimationPattern");
                var prefab = SuperController.singleton.GetCachedPrefab(apAsset.assetBundleName, apAsset.assetName);
                var animPattern = prefab.GetComponentInChildren<AnimationPattern>();
                var animStep = animPattern.animationStepPrefab.GetComponentInChildren<AnimationStep>();
                var triggerUI = GameObject.Instantiate(animStep.triggerActionDiscretePrefab);
                return triggerUI;
            }

            public RectTransform CreateTriggerActionTransitionUI()
            {
                throw new NotSupportedException();
            }

            public void DuplicateTriggerAction(TriggerAction ta)
            {
                throw new NotSupportedException();
            }

            public void RemoveTriggerAction(TriggerAction ta)
            {
                throw new NotSupportedException();
            }
        }
        public override void Init()
        {
            base.Init();

            // Left side

            InitAnimationSelectorUI(false);

            InitPlaybackUI(false);

            // Right side

            InitControllersUI();

            CreateSpacer(true);

            InitFloatParamsUI();

            CreateSpacer(true);

            GenerateRemoveToggles();

            // var handler = new TestHandler();
            // var triggerActionDiscrete = new TriggerActionDiscrete();
            // triggerActionDiscrete.handler = handler;
            // var rectTransform = handler.CreateTriggerActionDiscreteUI();
            // SuperController.LogMessage($"{_components[0].transform.parent.parent.parent.name}");
            // rectTransform.SetParent(_components[0].transform.parent.parent);
            // rectTransform.gameObject.SetActive(true);

            Plugin.StartCoroutine(LoadAnimationStepUI());
        }

        private IEnumerator LoadAnimationStepUI()
        {
            var apAsset = SuperController.singleton.atomAssets.FirstOrDefault(a => a.assetName == "AnimationPattern");
            if (apAsset == null) throw new NullReferenceException(nameof(apAsset));
            var prefab = SuperController.singleton.GetCachedPrefab(apAsset.assetBundleName, apAsset.assetName);
            if (prefab == null)
            {
                // We should use SuperController.LoadAtomFromBundleAsync but it is protected...
                var request = AssetBundleManager.LoadAssetAsync(apAsset.assetBundleName, apAsset.assetName, typeof(GameObject));
                yield return Plugin.StartCoroutine(request);
                var go = request.GetAsset<GameObject>();
                SuperController.singleton.RegisterPrefab(apAsset.assetBundleName, apAsset.assetName, go);
            }
            var animPattern = prefab.GetComponentInChildren<AnimationPattern>();
            if (animPattern == null) throw new NullReferenceException(nameof(animPattern));
            var animStepAtom = GameObject.Instantiate(animPattern.animationStepPrefab);
            if (animStepAtom == null) throw new NullReferenceException(nameof(animStepAtom));
            animStep = animStepAtom.GetComponentInChildren<AnimationStep>();
            if (animStep == null) throw new NullReferenceException(nameof(animStep));
            animStep.InitUI();
            // PrintTree(animStepAtom.gameObject, true);
            // foreach(var uiConnect in animStepAtom.gameObject.GetComponentsInChildren<UIConnector>(true))
            // {
            //     SuperController.LogMessage(uiConnect.storeid);
            // }
            var animationConnector = animStepAtom.gameObject.GetComponentsInChildren<UIConnector>(true).FirstOrDefault(c => c.storeid == "animation");
            animationConnector.Connect();
            if (animStepAtom == null) throw new NullReferenceException(nameof(animationConnector));
            foreach(var p in animStepAtom.GetAllParamAndActionNames())
            {
                SuperController.LogMessage(p);
            }
            // yield break;
            SuperController.LogError("1");
            if (animStep.UITransform == null) throw new NullReferenceException(nameof(animStep.UITransform));
            var animStepTriggerUI = animStep.UITransform.GetComponentInChildren<AnimationStepTriggerUI>();
            SuperController.LogError("2");
            if (animStepTriggerUI == null) throw new NullReferenceException(nameof(animStepTriggerUI));
            SuperController.LogError("3");
            animStep.trigger = new Trigger();
            SuperController.LogError("4");
            animStep.trigger.handler = animStep;
            SuperController.LogError("5");
            animStep.trigger.triggerActionsParent = animStepTriggerUI.transform;
            animStep.trigger.triggerPanel = animStepTriggerUI.transform;
            animStep.trigger.triggerActionsPanel = animStepTriggerUI.transform;
            SuperController.LogError("6");
            animStep.trigger.InitTriggerUI();
            animStep.trigger.InitTriggerActionsUI();
            animStep.trigger.OpenTriggerActionsPanel();
        }

    public static void PrintTree(GameObject o, bool showScripts, params string[] exclude)
    {
        PrintTree(0, o, showScripts, exclude, new HashSet<GameObject>());
    }

    public static void PrintTree(int indent, GameObject o, bool showScripts, string[] exclude, HashSet<GameObject> found)
    {
        if (found.Contains(o))
        {
            SuperController.LogMessage("|" + new String(' ', indent) + " [" + o.tag + "] " + o.name + " {RECURSIVE}");
            return;
        }
        if (o == null)
        {
            SuperController.LogMessage("|" + new String(' ', indent) + "{null}");
            return;
        }
        if (exclude.Any(x => o.gameObject.name.Contains(x)))
        {
            return;
        }
        found.Add(o);
        SuperController.LogMessage(
            "|" +
            new String(' ', indent) +
            " [" + o.tag + "] " +
            o.name +
            " -> " +
            (showScripts ? string.Join(", ", o.GetComponents<MonoBehaviour>().Select(b => b.ToString()).ToArray()) : "")
            );
        for (int i = 0; i < o.transform.childCount; i++)
        {
            var under = o.transform.GetChild(i).gameObject;
            PrintTree(indent + 4, under, showScripts, exclude, found);
        }
    }

        private void InitControllersUI()
        {
            _addControllerListJSON = new JSONStorableStringChooser("Animate Controller", GetEligibleFreeControllers().ToList(), GetEligibleFreeControllers().FirstOrDefault(), "Animate controller", (string name) => UIUpdated())
            {
                isStorable = false
            };

            _addControllerUI = Plugin.CreateScrollablePopup(_addControllerListJSON, true);
            _addControllerUI.popupPanelHeight = 900f;
            _linkedStorables.Add(_addControllerListJSON);

            _toggleControllerUI = Plugin.CreateButton("Add Controller", true);
            _toggleControllerUI.button.onClick.AddListener(() => AddAnimatedController());
            _components.Add(_toggleControllerUI);
        }

        private IEnumerable<string> GetEligibleFreeControllers()
        {
            yield return "";
            foreach (var fc in Plugin.ContainingAtom.freeControllers)
            {
                if (fc.name == "control") yield return fc.name;
                if (!fc.name.EndsWith("Control")) continue;
                yield return fc.name;
            }
        }

        private void InitFloatParamsUI()
        {
            var storables = GetStorablesWithFloatParams().ToList();
            _addStorableListJSON = new JSONStorableStringChooser("Animate Storable", storables, storables.Contains("geometry") ? "geometry" : storables.FirstOrDefault(), "Animate Storable", (string name) => RefreshStorableFloatsList(true))
            {
                isStorable = false
            };

            _addParamListJSON = new JSONStorableStringChooser("Animate Param", new List<string> { "" }, "", "Animate Param", (string name) => UIUpdated())
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
            yield return "";
            foreach (var storableId in Plugin.ContainingAtom.GetStorableIDs().OrderBy(s => s))
            {
                if (storableId.StartsWith("hairTool")) continue;
                var storable = Plugin.ContainingAtom.GetStorableByID(storableId);
                if (storable == null) continue;
                if ((storable.GetFloatParamNames()?.Count ?? 0) > 0)
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
            _addParamListJSON.choices = values.OrderBy(v => v).ToList();
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

        private void AddAnimatedController()
        {
            try
            {
                var uid = _addControllerListJSON.val;
                if (string.IsNullOrEmpty(uid)) return;
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
                Plugin.Animation.RebuildAnimation();
                Plugin.AnimationModified();
            }
            catch (Exception exc)
            {
                SuperController.LogError($"VamTimeline.{nameof(AtomAnimationSettingsUI)}.{nameof(AddAnimatedController)}: " + exc);
            }
        }

        private void AddAnimatedFloatParam()
        {
            try
            {
                if (string.IsNullOrEmpty(_addStorableListJSON.val)) return;

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
                Plugin.Animation.RebuildAnimation();
                Plugin.AnimationModified();
            }
            catch (Exception exc)
            {
                SuperController.LogError($"VamTimeline.{nameof(AtomAnimationSettingsUI)}.{nameof(AddAnimatedFloatParam)}: " + exc);
            }
        }

        private void RemoveAnimatedController(FreeControllerAnimationTarget target)
        {
            try
            {
                Plugin.Animation.Current.Remove(target.Controller);
                Plugin.Animation.RebuildAnimation();
                Plugin.AnimationModified();
            }
            catch (Exception exc)
            {
                SuperController.LogError($"VamTimeline.{nameof(AtomAnimationSettingsUI)}.{nameof(RemoveAnimatedController)}: " + exc);
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
                SuperController.LogError($"VamTimeline.{nameof(AtomAnimationSettingsUI)}.{nameof(RemoveAnimatedController)}: " + exc);
            }
        }

        #endregion

        #region Events

        public override void AnimationModified()
        {
            base.AnimationModified();
            GenerateRemoveToggles();
        }

        public override void Dispose()
        {
            ClearRemoveToggles();
            base.Dispose();
        }

        #endregion
    }
}

