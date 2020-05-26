using System.Collections.Generic;
using UnityEngine;

namespace VamTimeline
{
    /// <summary>
    /// VaM Timeline
    /// By Acidbubbles
    /// Animation timeline with keyframes
    /// Source: https://github.com/acidbubbles/vam-timeline
    /// </summary>
    public interface IAnimationTargetWithCurves : IAtomAnimationTarget
    {
        bool Dirty { get; set; }
        string Name { get; }

        AnimationCurve GetLeadCurve();
        IEnumerable<AnimationCurve> GetCurves();
        void DeleteFrame(float time);
        void DeleteFrameByKey(int key);
    }
}