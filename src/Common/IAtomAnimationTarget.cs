using System;
using UnityEngine.Events;

namespace VamTimeline
{
    /// <summary>
    /// VaM Timeline
    /// By Acidbubbles
    /// Animation timeline with keyframes
    /// Source: https://github.com/acidbubbles/vam-timeline
    /// </summary>
    public interface IAtomAnimationTarget : IDisposable
    {
        UnityEvent onAnimationKeyframesModified { get; }
        UnityEvent onSelectedChanged { get; }
        bool dirty { get; set; }
        bool selected { get; set; }
        string name { get; }

        void Validate(float animationLength);

        void StartBulkUpdates();
        void EndBulkUpdates();

        bool TargetsSameAs(IAtomAnimationTarget target);
        string GetShortName();

        float[] GetAllKeyframesTime();
        bool HasKeyframe(float time);
        void DeleteFrame(float time);
    }
}
