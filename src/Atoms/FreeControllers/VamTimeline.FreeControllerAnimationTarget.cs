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
    public class FreeControllerAnimationTarget : IAnimationTarget
    {
        public FreeControllerV3 Controller;
        public SortedDictionary<int, KeyframeSettings> Settings = new SortedDictionary<int, KeyframeSettings>();
        public AnimationCurve X = new AnimationCurve();
        public AnimationCurve Y = new AnimationCurve();
        public AnimationCurve Z = new AnimationCurve();
        public AnimationCurve RotX = new AnimationCurve();
        public AnimationCurve RotY = new AnimationCurve();
        public AnimationCurve RotZ = new AnimationCurve();
        public AnimationCurve RotW = new AnimationCurve();
        public List<AnimationCurve> Curves;

        public string Name => Controller.name;

        public FreeControllerAnimationTarget(FreeControllerV3 controller)
        {
            Curves = new List<AnimationCurve> {
                X, Y, Z, RotX, RotY, RotZ, RotW
            };
            Controller = controller;
        }

        #region Control

        public AnimationCurve GetLeadCurve()
        {
            return X;
        }

        public IEnumerable<AnimationCurve> GetCurves()
        {
            return Curves;
        }

        public void ReapplyCurveTypes()
        {
            if (X.length < 2) return;

            foreach (var setting in Settings)
            {
                if (setting.Value.CurveType == CurveTypeValues.LeaveAsIs)
                    continue;

                var time = (setting.Key / 1000f).Snap();
                foreach (var curve in Curves)
                {
                    curve.ApplyCurve(time, setting.Value.CurveType);
                }
            }
        }

        public void ReapplyCurvesToClip(AnimationClip clip)
        {
            var path = GetRelativePath();
            clip.SetCurve(path, typeof(Transform), "localPosition.x", X);
            clip.SetCurve(path, typeof(Transform), "localPosition.y", Y);
            clip.SetCurve(path, typeof(Transform), "localPosition.z", Z);
            clip.SetCurve(path, typeof(Transform), "localRotation.x", RotX);
            clip.SetCurve(path, typeof(Transform), "localRotation.y", RotY);
            clip.SetCurve(path, typeof(Transform), "localRotation.z", RotZ);
            clip.SetCurve(path, typeof(Transform), "localRotation.w", RotW);
        }

        public void SmoothLoop()
        {
            foreach (var curve in Curves)
            {
                curve.SmoothLoop();
            }
        }

        private string GetRelativePath()
        {
            var root = Controller.containingAtom.transform;
            var target = Controller.transform;
            var parts = new List<string>();
            Transform t = target;
            while (t != root && t != t.root)
            {
                parts.Add(t.name);
                t = t.parent;
            }
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        #endregion

        #region Quaternions

        public void EnsureQuaternionContinuity()
        {
            return;
            // Test 3
            // for (var key = 1; key < RotX.length - 1; key++)
            // {
            //     Keyframe x = RotX[key];
            //     Keyframe y = RotY[key];
            //     Keyframe z = RotZ[key];
            //     Keyframe w = RotW[key];
            //     var quaternion = new Quaternion(
            //         x.value,
            //         y.value,
            //         z.value,
            //         w.value
            //     );
            //     var i = Quaternion.Inverse(quaternion);
            //     x.value = i.x;
            //     RotX.MoveKey(key, x);
            //     y.value = i.y;
            //     RotY.MoveKey(key, y);
            //     z.value = i.z;
            //     RotZ.MoveKey(key, z);
            //     w.value = i.w;
            //     RotW.MoveKey(key, w);
            // }

            // Test 1
            // Keyframe x = RotX[1];
            // Keyframe y = RotY[1];
            // Keyframe z = RotZ[1];
            // Keyframe w = RotW[1];
            // var quaternion = new Quaternion(
            //     x.value,
            //     y.value,
            //     z.value,
            //     w.value
            // );
            // var i = Quaternion.Inverse(quaternion);

            // x.value = i.x;
            // RotX.MoveKey(1, x);
            // y.value = i.y;
            // RotY.MoveKey(1, y);
            // z.value = i.z;
            // RotZ.MoveKey(1, z);
            // w.value = i.w;
            // RotW.MoveKey(1, w);

            // Attempt 0
            Quaternion? previous = null;
            for (var key = 0; key < RotX.length - 1; key++)
            {
                Keyframe x = RotX[key];
                Keyframe y = RotY[key];
                Keyframe z = RotZ[key];
                Keyframe w = RotW[key];
                var quaternion = new Quaternion(
                    x.value,
                    y.value,
                    z.value,
                    w.value
                );

                if (previous == null)
                {
                    previous = quaternion;
                    continue;
                }

                var inversed = Quaternion.Inverse(quaternion);
                var midQ = Quaternion.Slerp(previous.Value, quaternion, 0.5f);
                var midI = Quaternion.Slerp(previous.Value, inversed, 0.5f);
                var deltaQ = Quaternion.Angle(previous.Value, midQ);
                var deltaI = Quaternion.Angle(previous.Value, midI);
                SuperController.LogMessage($"{deltaQ} v.s. {deltaI}");
                if (deltaI == deltaQ)
                {
                    x.value = inversed.x;
                    RotX.MoveKey(key, x);
                    y.value = inversed.y;
                    RotY.MoveKey(key, y);
                    z.value = inversed.z;
                    RotZ.MoveKey(key, z);
                    w.value = inversed.w;
                    RotW.MoveKey(key, w);

                    previous = inversed;
                }
                else
                {
                    previous = quaternion;
                }
            }
        }

        #endregion

        #region Keyframes control

        public void SetKeyframeToCurrentTransform(float time)
        {
            SetKeyframe(time, Controller.transform.localPosition, Controller.transform.localRotation);
        }

        public void SetKeyframe(float time, Vector3 localPosition, Quaternion localRotation)
        {
            X.SetKeyframe(time, localPosition.x);
            Y.SetKeyframe(time, localPosition.y);
            Z.SetKeyframe(time, localPosition.z);
            RotX.SetKeyframe(time, localRotation.x);
            RotY.SetKeyframe(time, localRotation.y);
            RotZ.SetKeyframe(time, localRotation.z);
            RotW.SetKeyframe(time, localRotation.w);
            var ms = time.ToMilliseconds();
            if (!Settings.ContainsKey(ms))
                Settings[ms] = new KeyframeSettings { CurveType = CurveTypeValues.Smooth };
        }

        public void DeleteFrame(float time)
        {
            var key = GetLeadCurve().KeyframeBinarySearch(time);
            if (key != -1) DeleteFrameByKey(key);
        }

        public void DeleteFrameByKey(int key)
        {
            var settingIndex = Settings.Remove(GetLeadCurve()[key].time.ToMilliseconds());
            foreach (var curve in GetCurves())
            {
                curve.RemoveKey(key);
            }
        }

        public IEnumerable<float> GetAllKeyframesTime()
        {
            var set = new SortedDictionary<float, int>();
            foreach (var curve in Curves)
            {
                for (var i = 0; i < curve.length; i++)
                    set[curve[i].time] = 0;
            }
            return set.Keys;
        }

        #endregion

        #region Curves

        public void ChangeCurve(float time, string curveType)
        {
            if (string.IsNullOrEmpty(curveType)) return;

            UpdateSetting(time, curveType);
        }

        #endregion

        #region Snapshots

        public FreeControllerV3Snapshot GetCurveSnapshot(float time)
        {
            if (X.KeyframeBinarySearch(time) == -1) return null;
            KeyframeSettings setting;
            return new FreeControllerV3Snapshot
            {
                X = X[X.KeyframeBinarySearch(time)],
                Y = Y[Y.KeyframeBinarySearch(time)],
                Z = Z[Z.KeyframeBinarySearch(time)],
                RotX = RotX[RotX.KeyframeBinarySearch(time)],
                RotY = RotY[RotY.KeyframeBinarySearch(time)],
                RotZ = RotZ[RotZ.KeyframeBinarySearch(time)],
                RotW = RotW[RotW.KeyframeBinarySearch(time)],
                CurveType = Settings.TryGetValue(time.ToMilliseconds(), out setting) ? setting.CurveType : CurveTypeValues.LeaveAsIs
            };
        }

        public void SetCurveSnapshot(float time, FreeControllerV3Snapshot snapshot)
        {
            X.SetKeySnapshot(time, snapshot.X);
            Y.SetKeySnapshot(time, snapshot.Y);
            Z.SetKeySnapshot(time, snapshot.Z);
            RotX.SetKeySnapshot(time, snapshot.RotX);
            RotY.SetKeySnapshot(time, snapshot.RotY);
            RotZ.SetKeySnapshot(time, snapshot.RotZ);
            RotW.SetKeySnapshot(time, snapshot.RotW);
            UpdateSetting(time, snapshot.CurveType);
        }

        private void UpdateSetting(float time, string curveType)
        {
            var ms = time.ToMilliseconds();
            if (Settings.ContainsKey(ms))
                Settings[ms].CurveType = curveType;
            else
                Settings.Add(ms, new KeyframeSettings { CurveType = curveType });
        }

        #endregion

        #region Interpolation


        public bool Interpolate(float playTime, float maxDistanceDelta, float maxRadiansDelta)
        {
            // TODO: We should calculate this once, and start a coroutine and just wait.

            var targetLocalPosition = new Vector3
            {
                x = X.Evaluate(playTime),
                y = Y.Evaluate(playTime),
                z = Z.Evaluate(playTime)
            };

            var targetLocalRotation = new Quaternion
            {
                x = RotX.Evaluate(playTime),
                y = RotY.Evaluate(playTime),
                z = RotZ.Evaluate(playTime),
                w = RotW.Evaluate(playTime)
            };

            Controller.transform.localPosition = Vector3.MoveTowards(Controller.transform.localPosition, targetLocalPosition, maxDistanceDelta);
            Controller.transform.localRotation = Quaternion.RotateTowards(Controller.transform.localRotation, targetLocalRotation, maxRadiansDelta);

            var posDistance = Vector3.Distance(Controller.transform.localPosition, targetLocalPosition);
            // NOTE: We skip checking for rotation reached because in some cases we just never get even near the target rotation.
            // var rotDistance = Quaternion.Dot(Controller.transform.localRotation, targetLocalRotation);
            return posDistance < 0.01f;
        }

        #endregion

        #region  Rendering

        public void RenderDebugInfo(StringBuilder display, float time)
        {
            RenderStateController(time, display, "X", X);
            /*
            RenderStateController(time, display, "Y", Y);
            RenderStateController(time, display, "Z", Z);
            RenderStateController(time, display, "RotX", RotX);
            RenderStateController(time, display, "RotY", RotY);
            RenderStateController(time, display, "RotZ", RotZ);
            RenderStateController(time, display, "RotW", RotW);
            */
        }

        private static void RenderStateController(float time, StringBuilder display, string name, AnimationCurve curve)
        {
            display.AppendLine($"{name}");
            foreach (var keyframe in curve.keys)
            {
                display.AppendLine($"  {(keyframe.time.IsSameFrame(time) ? "+" : "-")} {keyframe.time:0.00}s: {keyframe.value:0.00}");
                display.AppendLine($"    Tngt in: {keyframe.inTangent:0.00} out: {keyframe.outTangent:0.00}");
                display.AppendLine($"    Wght in: {keyframe.inWeight:0.00} out: {keyframe.outWeight:0.00} {keyframe.weightedMode}");
            }
        }

        #endregion

        public class Comparer : IComparer<FreeControllerAnimationTarget>
        {
            public int Compare(FreeControllerAnimationTarget t1, FreeControllerAnimationTarget t2)
            {
                return t1.Controller.name.CompareTo(t2.Controller.name);

            }
        }
    }
}
