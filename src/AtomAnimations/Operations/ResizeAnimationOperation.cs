﻿using System;
using System.Linq;
using UnityEngine;

namespace VamTimeline
{
    public class ResizeAnimationOperation
    {
        private readonly AtomAnimationClip _clip;

        public ResizeAnimationOperation(AtomAnimationClip clip)
        {
            _clip = clip;
        }

        private class SnapshotAt
        {
            public float time;
            public ISnapshot snapshot;
        }

        #region Stretch

        public void Stretch(float newAnimationLength)
        {
            var keyframeOps = new KeyframesOperation(_clip);
            var originalAnimationLength = _clip.animationLength;
            _clip.animationLength = newAnimationLength;
            var ratio = newAnimationLength / originalAnimationLength;
            foreach (var target in _clip.GetAllTargets())
            {
                var snapshots = target
                    .GetAllKeyframesTime()
                    .Select(t => new SnapshotAt { time = t, snapshot = target.GetSnapshot(t) })
                    .ToList();
                keyframeOps.RemoveAll(target, true);

                foreach (var s in snapshots)
                {
                    target.SetSnapshot((s.time * ratio).Snap(), s.snapshot);
                }
            }
        }

        #endregion

        #region CropOrExtendEnd

        public void CropOrExtendEnd(float newAnimationLength)
        {
            var originalAnimationLength = _clip.animationLength;
            _clip.animationLength = newAnimationLength;

            if (newAnimationLength < originalAnimationLength)
            {
                CropEnd(newAnimationLength);
            }
            else if (newAnimationLength > originalAnimationLength)
            {
                ExtendEnd(newAnimationLength);
            }
        }

        private void CropEnd(float newAnimationLength)
        {
            foreach (var target in _clip.GetAllCurveTargets())
            {
                foreach (var curve in target.GetCurves())
                {
                    var key = curve.AddKey(newAnimationLength, curve.Evaluate(newAnimationLength));
                }
                target.EnsureKeyframeSettings(newAnimationLength, target.settings.Last().Value.curveType);
                target.dirty = true;
                var keyframesToDelete = target.GetAllKeyframesTime().Where(t => t > newAnimationLength);
                foreach (var t in keyframesToDelete)
                    target.DeleteFrame(t);
            }
            foreach (var target in _clip.targetTriggers)
            {
                while (target.triggersMap.Count > 0)
                {
                    var lastTrigger = target.triggersMap.Keys.Last();
                    if (lastTrigger * 1000f > newAnimationLength)
                    {
                        target.triggersMap.Remove(lastTrigger);
                        continue;
                    }
                    break;
                }
                target.AddEdgeFramesIfMissing(newAnimationLength);
            }
        }

        private void ExtendEnd(float newAnimationLength)
        {
            foreach (var target in _clip.GetAllTargets())
            {
                target.AddEdgeFramesIfMissing(newAnimationLength);
            }
        }

        #endregion

        #region CropOrExtendAt

        public void CropOrExtendAt(float newAnimationLength, float time)
        {
            var originalAnimationLength = _clip.animationLength;
            _clip.animationLength = newAnimationLength;
            var delta = newAnimationLength - originalAnimationLength;

            if (newAnimationLength < originalAnimationLength)
            {
                CropAt(delta, time);
            }
            else if (newAnimationLength > originalAnimationLength)
            {
                ExtendAt(delta, time);
            }
        }

        private void CropAt(float delta, float time)
        {
            var keyframeOps = new KeyframesOperation(_clip);
            foreach (var target in _clip.GetAllTargets())
            {
                // TODO: Create new keyframe if missing from evaluate curve
                var snapshots = target
                    .GetAllKeyframesTime()
                    .Where(t => t < time || t >= time - delta)
                    .Select(t =>
                    {
                        var newTime = t < time ? t : t + delta;
                        return new SnapshotAt { time = newTime, snapshot = target.GetSnapshot(t) };
                    })
                    .ToList();
                keyframeOps.RemoveAll(target, true);

                foreach (var s in snapshots)
                {
                    target.SetSnapshot(s.time, s.snapshot);
                }

                target.AddEdgeFramesIfMissing(_clip.animationLength);
            }
        }

        private void ExtendAt(float delta, float time)
        {
            var keyframeOps = new KeyframesOperation(_clip);
            var originalAnimationLength = _clip.animationLength;
            foreach (var target in _clip.GetAllTargets())
            {
                // TODO: Create new keyframe if missing from evaluate curve
                var snapshots = target
                    .GetAllKeyframesTime()
                    .Select(t => new SnapshotAt { time = t < time ? t : t + delta, snapshot = target.GetSnapshot(t) })
                    .ToList();
                keyframeOps.RemoveAll(target, true);

                foreach (var s in snapshots)
                {
                    target.SetSnapshot(s.time, s.snapshot);
                }

                target.AddEdgeFramesIfMissing(_clip.animationLength);
            }
        }

        #endregion

        public void Loop(float newAnimationLength)
        {
            var keyframeOps = new KeyframesOperation(_clip);
            var originalAnimationLength = _clip.animationLength;
            _clip.animationLength = newAnimationLength;
            foreach (var target in _clip.GetAllTargets())
            {
                var snapshots = target
                    .GetAllKeyframesTime()
                    .Select(t => new SnapshotAt { time = t, snapshot = target.GetSnapshot(t) })
                    .ToList();
                snapshots.RemoveAt(snapshots.Count - 1);
                keyframeOps.RemoveAll(target, true);

                float time = 0f;
                var iteration = 0;
                var i = 0;
                while (true)
                {
                    var snapshot = snapshots[i++];
                    time = snapshot.time + (iteration * originalAnimationLength);
                    if (time > newAnimationLength) break;
                    target.SetSnapshot(time, snapshot.snapshot);
                    if (i >= snapshots.Count) { i = 0; iteration++; }
                }

                target.AddEdgeFramesIfMissing(_clip.animationLength);
            }
        }
    }
}
