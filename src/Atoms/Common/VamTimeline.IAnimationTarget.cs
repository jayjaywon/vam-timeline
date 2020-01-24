using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VamTimeline
{
    public interface IAnimationTarget
    {
        string Name { get; }

        IEnumerable<AnimationCurve> GetCurves();
        IEnumerable<float> GetAllKeyframesTime();
        void DeleteFrame(float time);
    }
}
