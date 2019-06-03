using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelEditor
{
    public class Transition : ScriptableObject
    {
        //transitions will basically dynamic micro-scripts that change aniamtions
        //based on the current paramters.

        //eg: if (parameter) =/>/</<=/>= then (go to animation) at exit#/any frame/ any exit

        //if
        public List<Parameter> TargetParameters = new List<Parameter>();

        public int parameterCount;

        public int numberOfConditions;

        //the operator that compares the parameter and the required value
        public List<int> SelectedPrimaryOperators = new List<int>();
        public string[] PrimaryOperators = new string[] { ">", "<", "=", "!=", "<=", ">=" };
        public void SetSelectedPrimaryOperator(int index, int newValue)
        {
            while(SelectedPrimaryOperators.Count <= index)
            {
                SelectedPrimaryOperators.Add(0);
            }

            SelectedPrimaryOperators[index] = newValue;
        }

        public int GetSelectedPrimaryOperator(int index)
        {
            while (SelectedPrimaryOperators.Count <= index)
            {
                SelectedPrimaryOperators.Add(0);
            }
            return SelectedPrimaryOperators[index];
        }
        
        //the secondary operators that are required if multiple primary comparisons are made
        public List<int> ActiveOperators = new List<int>();
        public static string[] Operators = new string[] { "And", "Or" };

        //it would be ideal to use Parameters here instead, but I don't want scriptable object 
        //instances floating around so im just going to create multiple arrays. This will be 
        //simpler to serialize as well.
        public List<bool> boolRightOperand = new List<bool>();
        public List<string> stringRightOperand = new List<string>();
        public List<int> intRightOperand = new List<int>();
        public List<float> floatRightOperand = new List<float>();
        public T GetRightOperandValue<T>(int index)
        {
            Type type = typeof(T);
            if (type == typeof(bool))
            {
                if (boolRightOperand.Count > index)
                {
                    bool val = boolRightOperand[index];
                    return (T)Convert.ChangeType(val, typeof(T));
                }
            }
            if(type == typeof(string))
            {
                if(stringRightOperand.Count > index)
                {
                    string val = stringRightOperand[index];
                    return (T)Convert.ChangeType(val, typeof(T));
                }
            }
            if(type == typeof(int))
            {
                if(intRightOperand.Count > index)
                {
                    int val = intRightOperand[index];
                    return (T)Convert.ChangeType(val, typeof(T));
                }
            }
            if(type == typeof(float))
            {
                if (floatRightOperand.Count > index)
                {
                    float val = floatRightOperand[index];
                    return (T)Convert.ChangeType(val, typeof(T));
                }

            }

            return default(T);
        }
        public void SetRightOperandValue(int index, bool newValue)
        {
            SizeOperandLists(index);
            boolRightOperand[index] = newValue;
        }
        public void SetRightOperandValue(int index, string newValue)
        {
            SizeOperandLists(index);
            stringRightOperand[index] = newValue;
        }
        public void SetRightOperandValue(int index, int newValue)
        {
            SizeOperandLists(index);
            intRightOperand[index] = newValue;
        }
        public void SetRightOperandValue(int index, float newValue)
        {
            SizeOperandLists(index);
            floatRightOperand[index] = newValue;
        }
        private void SetOperandsToDefault(int index)
        {
            SizeOperandLists(index);
        }

        private void SizeOperandLists(int count)
        {
            while (boolRightOperand.Count < count+1)
            {
                boolRightOperand.Add(false);
                stringRightOperand.Add("");
                intRightOperand.Add(0);
                floatRightOperand.Add(0f);
            }
        }

        public ExitType exitType;

        public enum ExitType
        {
            anyFrame,
            targetExitPoint,
            anyExitPoint,
            targetFrame
        }
        //which exitpoints/frames to quit at if the condition is right
        public int[] exitFrames;
        
        //which timeline is transitioned to if conditions are correct.
        public Timeline targetTimeline;

    }
}
