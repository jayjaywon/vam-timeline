using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VamTimeline
{
    /// <summary>
    /// VaM Timeline
    /// By Acidbubbles
    /// Animation timeline with keyframes
    /// Source: https://github.com/acidbubbles/vam-timeline
    /// </summary>
    public class FloatParamAnimationTarget : IAnimationTarget
    {
        public JSONStorable Storable { get; }
        public JSONStorableFloat FloatParam{get;}
        public AnimationCurve Value { get; } = new AnimationCurve();

        public string Name => Storable != null ? $"{Storable.name}/{FloatParam.name}" : FloatParam.name;

        public FloatParamAnimationTarget(JSONStorable storable, JSONStorableFloat jsf)
        {
            Storable = storable;
            FloatParam = jsf;
        }

        public IEnumerable<AnimationCurve> GetCurves()
        {
            return new[] { Value };
        }

        public void SetKeyframe(float time, float value)
        {
            Value.SetKeyframe(time, value);
        }

        public void DeleteFrame(float time)
        {
            var key = Array.FindIndex(Value.keys, k => k.time.IsSameFrame(time));
            if (key != -1) Value.RemoveKey(key);
        }

        public IEnumerable<float> GetAllKeyframesTime()
        {
            return Value.keys.Select(k => k.time);
        }

        public class Comparer : IComparer<FloatParamAnimationTarget>
        {
            public int Compare(FloatParamAnimationTarget t1, FloatParamAnimationTarget t2)
            {
                return t1.FloatParam.name.CompareTo(t2.FloatParam.name);

            }
        }
    }
}
