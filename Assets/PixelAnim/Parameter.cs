using System;
using UnityEngine;

//moving this int Timeline since it will only be used here.

namespace PixelEditor
{
    /// <summary>
    /// This is a data class that function as a string, bool, int, float, or 'trigger'(null bool). NOT meant to replace 'var'.
    /// This is only usefull in cases where the values isn't known at compile-time. Self initializes upon SetValue(). Once type is set, 
    /// it cannot be changed. trigger: triggers are just bools that switch from true to false when GetValue is called.
    /// </summary>
    /// <remarks>
    /// usage: 
    /// This class should only be accessed with SetValue() and GetValue().
    /// </remarks>

    public class Parameter : ScriptableObject
    {
        bool isInitialized = false;
        
        [SerializeField]
        private bool isString;
        [SerializeField]
        private string s;

        [SerializeField]
        private bool isBool;
        [SerializeField]
        private bool b;

        [SerializeField]
        private bool isInt;
        [SerializeField]
        private int i;

        [SerializeField]
        private bool isFloat;
        [SerializeField]
        private float f;

        [SerializeField]
        private bool isTrigger;
        [SerializeField]
        private bool t;

        public void Initialize(Type T, bool isTrigger = false)
        {
            Debug.Log("initialzing parameter.");

            isInitialized = true;
            isString = isBool = isInt = isFloat = false;
            if (T == typeof(string)) isString = true;
            if (T == typeof(bool)) isBool = true;
            if (T == typeof(int)) isInt = true;
            if (T == typeof(float)) isFloat = true;
            if (T == typeof(bool) && isTrigger) isTrigger = true;
        }
        
        public string GetTypeAsString()
        {
            if (isString) return "string";
            if (isBool) return "bool";
            if (isInt) return "int";
            if (isFloat) return "float";
            if (isTrigger) return "trigger";

            return "none";
        }

        public T GetValue<T>()
        {
            Type pType = typeof(T);

            if (pType == typeof(string))
            {
                var value = s;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            if (pType == typeof(bool))
            {
                var value = b;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            if (pType == typeof(int))
            {
                var value = i;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            if (pType == typeof(float))
            {
                var value = f;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return default(T);
        }

        //changes the triggers value when checked
        public bool GetTrigger()
        {
            bool rVal = (t == true) ? true : false;
            if (rVal) t = false;
            return rVal;
        }
        
        //allows peeking at a trigger without changing it from true to false.
        public bool PeekTrigger()
        {
            bool boolVal = false;
            if (t == true) boolVal = true;
            return boolVal;
        }

        public void SetValue(string newString)
        {
            if (!isInitialized) Initialize(typeof(string));
            if (isString) s = newString;
            else UnityEngine.Debug.Log("Warning: Parameter " + name + " is not a string.");
        }
        public void SetValue(bool newBool)
        {
            if (!isInitialized) Initialize(typeof(bool));
            if (isBool) b = newBool;
            else UnityEngine.Debug.Log("Warning: Parameter " + name + " is not a bool.");
        }
        public void SetValue(int newInt)
        {
            if (!isInitialized) Initialize(typeof(int));
            if (isInt) i = newInt;
            else UnityEngine.Debug.Log("Warning: Parameter " + name + " is not a int.");
        }
        public void SetValue(float newFloat)
        {
            if (!isInitialized) Initialize(typeof(float));
            if (isFloat) f = newFloat;
            else UnityEngine.Debug.Log("Warning: Parameter " + name + " is not a float.");
        }
        public void SetTrigger(bool newTrigger)
        {
            if (!isInitialized) Initialize(typeof(bool), true);
            if (isTrigger) t = newTrigger;
            else UnityEngine.Debug.Log("Warning: Parameter " + name + " is not a trigger.");
        }

    }
}

