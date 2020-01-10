using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VamTimeline
{

    /// <summary>
    /// VaM Timeline
    /// By Acidbubbles
    /// Animation timeline with keyframes
    /// Source: https://github.com/acidbubbles/vam-timeline
    /// </summary>
    public class AtomAnimationClip
    {
        public const float DefaultAnimationLength = 2f;
        public const float DefaultBlendDuration = 0.75f;

        private float _animationLength = DefaultAnimationLength;
        private IAnimationTarget _selected;
        private bool _loop = true;
        private string _nextAnimationId;

        public AnimationClip Clip { get; }
        public AnimationPattern AnimationPattern { get; set; }
        public readonly List<FloatParamAnimationTarget> TargetFloatParams = new List<FloatParamAnimationTarget>();
        public readonly List<FreeControllerAnimationTarget> TargetControllers = new List<FreeControllerAnimationTarget>();
        public IEnumerable<IAnimationTarget> AllTargets => TargetControllers.Cast<IAnimationTarget>().Concat(TargetFloatParams.Cast<IAnimationTarget>());
        public bool EnsureQuaternionContinuity { get; set; } = true;
        public string AnimationId { get; }
        public string AnimationLabel { get; set; }
        public float Speed { get; set; } = 1f;
        public float AnimationLength
        {
            get
            {
                return _animationLength;
            }
            private set
            {
                _animationLength = value;
            }
        }
        public bool Loop
        {
            get
            {
                return _loop;
            }
            set
            {
                _loop = value;
                Clip.wrapMode = value ? WrapMode.Loop : WrapMode.Once;
            }
        }
        public float BlendDuration { get; set; } = DefaultBlendDuration;
        public string NextAnimationId
        {
            get
            {
                return _nextAnimationId
            }
            set
            {
                _nextAnimationId = value == "" ? null : value;
            }
        }

        public float NextAnimationTime { get; set; }

        public AtomAnimationClip(string id, string label)
        {
            AnimationId = id;
            AnimationLabel = label;
            Clip = new AnimationClip
            {
                legacy = true
            };
        }

        public bool IsEmpty()
        {
            return AllTargets.Count() == 0;
        }

        public void SelectTargetByName(string val)
        {
            _selected = string.IsNullOrEmpty(val)
                ? null
                : AllTargets.FirstOrDefault(c => c.Name == val);
        }

        public FreeControllerAnimationTarget Add(FreeControllerV3 controller)
        {
            if (TargetControllers.Any(c => c.Controller == controller)) return null;
            var target = new FreeControllerAnimationTarget(controller, AnimationLength);
            TargetControllers.Add(target);
            return target;
        }

        public FloatParamAnimationTarget Add(JSONStorable storable, JSONStorableFloat jsf)
        {
            if (TargetFloatParams.Any(s => s.Storable.name == storable.name && s.Name == jsf.name)) return null;
            var target = new FloatParamAnimationTarget(storable, jsf);
            Add(target);
            return target;
        }

        public void Add(FloatParamAnimationTarget target)
        {
            if (target == null) throw new NullReferenceException(nameof(target));
            TargetFloatParams.Add(target);
        }

        public void Remove(FreeControllerV3 controller)
        {
            var existing = TargetControllers.FirstOrDefault(c => c.Controller == controller);
            if (existing == null) return;
            TargetControllers.Remove(existing);
        }

        public void RebuildAnimation()
        {
        }

        public void ChangeCurve(float time, string curveType)
        {
            if (time == 0 || time == AnimationLength) return;

            foreach (var controller in GetAllOrSelectedControllerTargets())
            {
                controller.ChangeCurve(time, curveType);
            }
        }

        public void SmoothAllFrames()
        {
            foreach (var controller in TargetControllers)
            {
                controller.SmoothAllFrames();
            }
        }

        public float GetNextFrame(float time)
        {
            if (time == AnimationLength)
                return 0f;
            var nextTime = AnimationLength;
            foreach (var controller in GetAllOrSelectedTargets())
            {
                var targetNextTime = controller.GetCurves().First().keys.FirstOrDefault(k => k.time > time).time;
                if (targetNextTime != 0 && targetNextTime < nextTime) nextTime = targetNextTime;
            }
            if (nextTime == AnimationLength && Loop)
                return 0f;
            else
                return nextTime;
        }

        public float GetPreviousFrame(float time)
        {
            if (time == 0f)
                return GetAllOrSelectedTargets().SelectMany(c => c.GetCurves()).SelectMany(c => c.keys).Select(c => c.time).Where(t => !Loop || t != AnimationLength).Max();
            var previousTime = 0f;
            foreach (var controller in GetAllOrSelectedTargets())
            {
                var targetPreviousTime = controller.GetCurves().First().keys.LastOrDefault(k => k.time < time).time;
                if (targetPreviousTime != 0 && targetPreviousTime > previousTime) previousTime = targetPreviousTime;
            }
            return previousTime;
        }

        public void DeleteFrame(float time)
        {
            foreach (var target in GetAllOrSelectedTargets())
            {
                target.DeleteFrame(time);
            }
        }

        public IEnumerable<IAnimationTarget> GetAllOrSelectedTargets()
        {
            if (_selected != null) return new IAnimationTarget[] { _selected };
            return AllTargets.Cast<IAnimationTarget>();
        }

        public IEnumerable<FreeControllerAnimationTarget> GetAllOrSelectedControllerTargets()
        {
            if (_selected as FreeControllerAnimationTarget != null) return new[] { (FreeControllerAnimationTarget)_selected };
            return TargetControllers;
        }

        public IEnumerable<FloatParamAnimationTarget> GetAllOrSelectedFloatParamTargets()
        {
            if (_selected as FloatParamAnimationTarget != null) return new[] { (FloatParamAnimationTarget)_selected };
            return TargetFloatParams;
        }

        public void StretchLength(float value)
        {
            if (value == _animationLength)
                return;
            _animationLength = value;
            foreach (var target in AllTargets)
            {
                foreach (var curve in target.GetCurves())
                    curve.StretchLength(value);
            }
        }

        public void CropOrExtendLength(float animationLength)
        {
            if (animationLength == _animationLength)
                return;
            _animationLength = animationLength;
            foreach (var target in AllTargets)
            {
                foreach (var curve in target.GetCurves())
                    curve.CropOrExtendLength(animationLength);
            }
        }
    }
}
