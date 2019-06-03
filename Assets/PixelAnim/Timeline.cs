using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.Linq;

namespace PixelEditor
{
    public class Timeline : ScriptableObject
    {
        public List<int> Frames;
        public int GetFrame(int index)
        {
            if (index < 0 || index >= Frames.Count) return -1;
            return Frames[index];
        }
        public void SetFrame(int index, int newVal) { Frames[index] = newVal; }

        public List<bool> IsFrameExitPoint = new List<bool>();
        public bool GetIsFrameExitPoint(int index) { return IsFrameExitPoint[index]; }
        public void SetIsFrameExitPoint(int index, bool newVal) { IsFrameExitPoint[index] = newVal; }

        private bool loopTimeline = false;
        public bool GetLoopTimeline() { return loopTimeline; }
        public void SetLoopTimeline(bool b) { loopTimeline = b; }

        public float framesPerSecond = 10f;

        private List<Parameter> TransitionParameters = new List<Parameter>();
        //get set and add to list
        public Parameter GetParameter(int t) { if (TransitionParameters.Count > t) return TransitionParameters[t]; return null; }
        public void SetParameter(Parameter p, int index) { TransitionParameters[index] = p; }
        public void AddParameter(Parameter p) { if (p != null) TransitionParameters.Add(p); }
        public int GetParameterCount() { return TransitionParameters.Count; }
        //public string GetParameterName(int index) { return TransitionParameters[index].GetName(); }
        public string GetParameterTypeAsString(int index) { return TransitionParameters[index].GetTypeAsString(); }

        public List<Parameter> Parameters = new List<Parameter>();

        public List<Transition> Transitions = new List<Transition>();

        public Timeline()
        {
        }

        public bool IsPrime(int n)
        {
            bool isPrime = false;

            int i, m = 0, check = 0;
            m = n / 2;
            for (i = 2; i <= m; i++)
            {
                if (n % i == 0)
                {
                    check = 1;
                    break;
                }
            }
            if (check == 0) isPrime = true;
            return isPrime;
        }

        private void OnEnable()
        {
            if (Frames == null)
            {
                Frames = new List<int>();
                IsFrameExitPoint = new List<bool>();
            }
        }

        public int FrameCount()
        {
            return Frames.Count;
        }

        public void AddFrame(int newFrame = 0, int index = -1)
        {
            if (index == -1)
            {
                Frames.Add(newFrame);
                IsFrameExitPoint.Add(false);
            }
            else
            {
                Frames.Insert(index, newFrame);
                IsFrameExitPoint.Insert(index, false);
            }
            EditorUtility.SetDirty(this);
        }

        public void RemoveFrame(int removeFrameIndex)
        {
            Frames.RemoveAt(removeFrameIndex);
            IsFrameExitPoint.RemoveAt(removeFrameIndex);
            EditorUtility.SetDirty(this);
        }

        public void ChangeFrame(int frameIndex, int newFrame)
        {
            if (Frames.Count < frameIndex) return;
            Frames[frameIndex] = newFrame;
            EditorUtility.SetDirty(this);
        }
    }
}
