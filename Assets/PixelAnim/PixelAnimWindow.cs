using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PixelEditor
{
    public class PixelAnimWindow : EditorWindow
    {
        [MenuItem("Window/PixelAnim")]
        public static void ShowWindow()
        {
            GetWindow(typeof(PixelAnimWindow));
        }

        //The windows' variables.
        Vector2 scrollPos = Vector2.zero;
        Vector2 editScrollPos = Vector2.zero;
        int selectedFrame = -1;
        private PixelAnimation pxAnim;
        private PixelAnimator pxAnimator;
        private MouseImage mouseImage = null;

        private TextureValues spriteValues = new TextureValues();
        public int GetFrameScale() { return spriteValues.frameScale; }

        delegate void GUIMethods(PixelAnimWindow window, Event e);
        GUIMethods DoGUIMethods = null;

        delegate void ScrollViewDisplay();
        ScrollViewDisplay scrollViewDisplay = null;

        private FrameSplitter frameSplitter = null;
        //split frames are stored here
        Texture2D[] SplitFrames = null;

        string newTimelineName = "New Timeline Name";

        int editingTimeline = 0;

        string newTransitionName = "New Transition Name";

        Transition editingTransition;


        /// <summary>
        /// Class to hold data that describes how this texture will be split into sprites or frames.
        /// </summary>
        private class TextureValues
        {
            public int previewScale = 1;
            public int frameScale = 1;
            public int numberRows = 1;
            public int numberColumns = 1;

            /// <summary>
            /// Clamps values between 1 and 100 to avoid divide-by-zero errors or huge scales.
            /// </summary>
            public void ClampValues()
            {
                previewScale = Mathf.Clamp(previewScale, 1, 100);
                frameScale = Mathf.Clamp(frameScale, 1, 100);
                numberRows = Mathf.Clamp(numberRows, 1, 100);
                numberColumns = Mathf.Clamp(numberColumns, 1, 100);
            }

            public void Update(PixelAnimation pxAnim)
            {
                if (pxAnim == null) return;
                pxAnim.Rows = numberRows;
                pxAnim.Columns = numberColumns;
            }
        }

        /// <summary>
        /// A Utility class that splits the texture attached to the PixelAnimation argument into smaller textures specified by rows/columns
        /// Use: instantiate, give it a texture, row, column in constrcutor. GetFrames() to get the Texture2D array. 
        /// </summary>
        private class FrameSplitter
        {
            //A framesplitter will need to be marked as expired when
            //the fields that change how the texture is split are modified.
            private bool expired = false;
            public bool IsExpired() { return expired; }
            public void Expire() { expired = true; }

            private List<Texture2D> Frames = new List<Texture2D>();
            public Texture2D[] GetFrames() { return Frames.ToArray(); }

            public FrameSplitter(Texture2D sourceTex, int rows, int columns)
            {
                Frames = new List<Texture2D>();
                if (!sourceTex.isReadable) throw new System.Exception("Source texture is not readable. Enable Read/Write.");
                int texWidth = sourceTex.width / columns;
                int texHeight = sourceTex.height / rows;

                for(int row = rows-1; row >= 0; row--)
                {
                    for(int col = 0; col < columns; col++)
                    {
                        int startX = col * texWidth;
                        int startY = row * texHeight;
                        Texture2D tex = new Texture2D(texWidth, texHeight);
                        tex.filterMode = FilterMode.Point;
                        Color[] Colors = sourceTex.GetPixels(startX, startY, texWidth, texHeight, 0);
                        tex.SetPixels(Colors,0);
                        Frames.Add(tex);
                    }
                }
            }

            //deallocate if it's not being used.
            public void Clear()
            {
                Frames = null;
            }

            public int GetFrameCount() { return Frames.Count; }
        }

        /// <summary>
        /// On enable the Frames and Timelines will display by default.
        /// </summary>
        private void OnEnable()
        {
            Debug.Log("enabled PixelAnimWindow");
            scrollViewDisplay += DisplaySplitFrames;
            scrollViewDisplay += ShowTimelines;
        }

        /// <summary>
        /// The main gui for the window.
        /// </summary>
        private void OnGUI()
        {
            GetSelectedPixelAnimation();
            BaseUserInterface();

            if (!pxAnim) return;
            
            spriteValues.ClampValues();
            spriteValues.Update(pxAnim);

            if(DoGUIMethods != null)
                DoGUIMethods(this, Event.current);
            

            if (pxAnimator.IsPlaying())
            {
                pxAnimator.EditorWindowUpdate();
            }

            Repaint();
        }
        
        /// <summary>
        /// Display the main user interface - labels, fields, and images go here.
        /// </summary>
        private void BaseUserInterface()
        {
            GUILayout.Label("Display:", EditorStyles.helpBox);

            pxAnim = GetSelectedComponentOfType(typeof(PixelAnimation)) as PixelAnimation;

            pxAnimator = GetSelectedComponentOfType(typeof(PixelAnimator)) as PixelAnimator;

            if(!pxAnim)
            {
                GUILayout.Label("No object with a PixelAnimator is selected.", EditorStyles.helpBox);
                return;
            }

            DisplayAnimationInfo();
            DisplaySourceImage();
            
            //split the frames
            if (frameSplitter == null || frameSplitter.IsExpired())
            {
                spriteValues.ClampValues();
                frameSplitter = new FrameSplitter(pxAnim.texture, spriteValues.numberRows, spriteValues.numberColumns);
                SplitFrames = frameSplitter.GetFrames();
                frameSplitter.Clear();
            }

            
            //Scroll view. curly brackets for organization.
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            {
                scrollViewDisplay?.Invoke();
            }
            EditorGUILayout.EndScrollView();

        }

        /// <summary>
        /// Selects the frame.
        /// </summary>
        /// <param name="index">Index.</param>
        private void SelectFrame(int index)
        {
            selectedFrame = index;
        }
    
        private Component GetSelectedComponentOfType(System.Type objType)
        {
            if(Selection.activeGameObject != null)
            {
                if(Selection.activeGameObject.transform.GetComponent(objType) != null )
                {
                    //Debug.Log(Selection.activeGameObject.transform.GetComponent(objType));
                    return Selection.activeGameObject.transform.GetComponent(objType);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the PixelAnimation from a selected object.
        /// </summary>
        /// <returns>The selected pixel animation. Null if none.</returns>
        private PixelAnimation GetSelectedPixelAnimation()
        {
            if (Selection.activeGameObject != null)
            {
                if (Selection.activeGameObject.transform.GetComponent<PixelAnimation>() != null)
                {
                    return Selection.activeGameObject.transform.GetComponent<PixelAnimation>();
                }
            }
            return null;
        }

        /// <summary>
        /// Shows the timeline. Note: notice Rect pos is used to manually layout
        /// the gui, since editorguilayout/guilayout cannot be used here for some functions.
        /// </summary>
        private void ShowTimelines()
        {
            Rect space = Rect.zero;
            if(pxAnim.Timelines.Count > 0)
            {
                GUILayout.Label("Set Frames:", EditorStyles.helpBox);

                pxAnimator.SetFlipX(GUILayout.Toggle(pxAnimator.GetFlipX(), "Flip X:(non functional)"));
                pxAnimator.SetFlipY(GUILayout.Toggle(pxAnimator.GetFlipY(), "Flip Y:(non functional)"));

                Rect pos = EditorGUILayout.GetControlRect();
                Rect startingPos = pos;
                space.x = pos.x;
                space.y = pos.y;
                pos.width = 50;
                pos.height = 50;
                float startX = pos.x;
                for (int i = 0; i < pxAnim.Timelines.Count; i++)
                {
                    if (i != 0)pos.y += 55;
                    pos.x = startingPos.x;
                    Rect labelRect = new Rect(pos.x, pos.y, 25, 25);
                    //play the animation
                    string btnLabel = pxAnimator.IsPlaying() ? "☐" : "▶";
                    if (GUI.Button(labelRect, btnLabel))
                    {
                        if(pxAnim.Timelines.Count > 0)
                            pxAnimator.Play(pxAnim.Timelines[i].name);
                    }
                    labelRect.x += labelRect.width;
                    GUIStyle custom = new GUIStyle("button");
                    custom.fontSize = 16;
                    if(GUI.Button(labelRect, "✂",custom))
                    {
                        editingTimeline = i;
                        scrollViewDisplay -= ShowTimelines;
                        scrollViewDisplay += DisplayTimelineOptions;
                    }
                    labelRect.x += labelRect.width;
                    labelRect.width = 200;
                    GUI.Label(labelRect, "Name: " + pxAnim.Timelines[i].name);


                    pos.y += 25;
                    
                    for (int j = 0; j < pxAnim.Timelines[i].FrameCount(); j++)
                    {
                        GUILayoutOption[] layout = new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) };
                        Texture2D tex = SplitFrames[pxAnim.Timelines[i].GetFrame(j)];
                        if(GUI.Button(pos, "", new GUIStyle()))
                        {
                            if (selectedFrame >= 0)
                            {
                                pxAnim.Timelines[i].ChangeFrame(j, selectedFrame);
                            }
                        }
                        GUI.DrawTexture(pos, tex);
                        pos.x += 55;

                        if (pos.x + 75 > position.width)
                        {
                            pos.x = startX;
                            pos.y += 55;

                        }
                    }
                    Rect addPos = pos;
                    addPos.width = 25;
                    addPos.height = 25;
                    if (GUI.Button(pos, "+"))
                    {
                        pxAnim.Timelines[i].AddFrame();
                    }
                }
                GUILayout.Space(pos.y + 55);
            }
        }

        private void DisplayAnimationInfo()
        {
            //curly brackets for organization.
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("preview scale:");
                spriteValues.previewScale = EditorGUILayout.IntField(spriteValues.previewScale, GUILayout.Width(50));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label("frame scale:");
                spriteValues.frameScale = EditorGUILayout.IntField(spriteValues.frameScale, GUILayout.Width(50));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Label("Split Image:", EditorStyles.helpBox);

                GUILayout.BeginHorizontal();
                GUILayout.Label("rows:");
                int rowVal = pxAnim.Rows;
                spriteValues.numberRows = EditorGUILayout.IntField(rowVal, GUILayout.Width(50));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label("columns");
                int colVal = pxAnim.Columns;
                spriteValues.numberColumns = EditorGUILayout.IntField(colVal, GUILayout.Width(50));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (frameSplitter != null)
                    frameSplitter.Expire();
            }
        }

        void DisplaySourceImage()
        {
            GUILayout.Label("Source Image:", EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            pxAnim.texture = EditorGUILayout.ObjectField(pxAnim.texture, typeof(Texture2D), false) as Texture2D;
            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(pxAnim);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Rect control = EditorGUILayout.GetControlRect();
            float windowWidth = EditorGUIUtility.currentViewWidth;
            control.width = pxAnim.texture.width * spriteValues.previewScale;
            control.height = pxAnim.texture.height * spriteValues.previewScale;

            GUI.DrawTexture(control, pxAnim.texture);
            GUILayout.Space(control.height);

            GUILayout.Label("Frames:", EditorStyles.helpBox, GUILayout.Width(position.width));
        }

        void DisplaySplitFrames()
        {
            Rect control = EditorGUILayout.GetControlRect();
            if (SplitFrames.Length > 0)
            {
                int subTexWidth = pxAnim.texture.width / spriteValues.numberColumns;
                int subTexHeight = pxAnim.texture.height / spriteValues.numberRows;

                int frameSpacing = 5;
                control = EditorGUILayout.GetControlRect();
                int totalHeight = subTexHeight * spriteValues.frameScale;
                for (int i = 0; i < SplitFrames.Length; i++)
                {
                    control.width = subTexWidth * spriteValues.frameScale;
                    control.height = subTexHeight * spriteValues.frameScale;
                    if (control.x + (subTexWidth * spriteValues.frameScale) > position.width)
                    {
                        control.x = frameSpacing;
                        control.y += subTexHeight * spriteValues.frameScale + frameSpacing;
                        totalHeight += subTexHeight * spriteValues.frameScale + frameSpacing;
                    }

                    GUI.DrawTexture(control, SplitFrames[i]);
                    GUIStyle style = new GUIStyle();

                    if (GUI.Button(control, "", style))
                    {
                        SelectFrame(i);
                        mouseImage = new MouseImage(SplitFrames[selectedFrame]);
                        DoGUIMethods += mouseImage.DrawImage;
                    }
                    control.x += subTexWidth * spriteValues.frameScale + frameSpacing;
                }
                GUILayout.Space(totalHeight);
            }

            GUILayout.Label("Animations", EditorStyles.helpBox);
            //brackets for organization
            GUILayout.Label("Name:");
            newTimelineName = GUILayout.TextField(newTimelineName, GUILayout.Width(100));
            GUILayout.Label("TODO: add auto-rename. Will fail to create w/o unique name");
            if (GUILayout.Button("Create New", GUILayout.Width(100)))
            {
                Timeline newTimeline = CreateScriptableObject.Create(typeof(Timeline), "PixelAnim/DataObjects/Timelines", newTimelineName) as Timeline;
                pxAnim.Timelines.Add(newTimeline);
            }
        }

        private void DisplayTimelineOptions()
        {
            GUILayout.Label("Timeline:", EditorStyles.helpBox);

            Rect pos = EditorGUILayout.GetControlRect();
            Rect guiRect = new Rect(pos.x, pos.y, 60, 25);
            Timeline curTimeline = pxAnim.Timelines[editingTimeline];

            GUIStyle font = new GUIStyle("button");
            font.fontSize = 12;
            if (GUI.Button(guiRect, "◀ back", font))
            {
                scrollViewDisplay += ShowTimelines;
                scrollViewDisplay -= DisplayTimelineOptions;
            }
            guiRect = pos;
            guiRect = new Rect(pos.x, pos.y+25, 100, 25);
            GUI.Label(guiRect,"Frame Rate:"  + curTimeline.framesPerSecond);
            guiRect.x += guiRect.width;
            guiRect.width = 200;
            float FPS = GUI.HorizontalSlider(guiRect, curTimeline.framesPerSecond, 1, 100);
            curTimeline.framesPerSecond = Mathf.RoundToInt(FPS);
            guiRect.y += guiRect.height;

            GUIStyle ft16 = new GUIStyle("button");
            guiRect = new Rect(pos.x, guiRect.y, 75, 25);
            string loopText = (curTimeline.GetLoopTimeline())? "✔ loop" : "✘ loop";
            if (GUI.Button(guiRect, loopText))
            {
                curTimeline.SetLoopTimeline( !curTimeline.GetLoopTimeline() );
            }
            guiRect.x += guiRect.width + 10;
            if(GUI.Button(guiRect,"transitions"))
            {
                Debug.Log("Showing Transitions");

                scrollViewDisplay += DisplayTransitions;
                scrollViewDisplay -= DisplayTimelineOptions;
            }


            guiRect.y += guiRect.height + 15;

            ft16.fontSize = 16;

            guiRect = new Rect(pos.x, guiRect.y, 50, 50);
            GUIStyle empty = new GUIStyle();
            for (int i = 0; i < curTimeline.FrameCount(); i++)
            {
                guiRect.x = pos.x;
                guiRect.width = 50;
                guiRect.height = 50;

                if (GUI.Button(guiRect, "",empty))
                {
                    Debug.Log("FRAME BUTTON");
                }
                Texture2D tex = SplitFrames[curTimeline.GetFrame(i)];
                GUI.DrawTexture(guiRect, tex);
                guiRect.x += guiRect.width;


                //First Row
                {
                    guiRect.width = 25;
                    guiRect.height = 25;
                    //remove a frame.
                    if(GUI.Button(guiRect, "✘", ft16))
                    {
                        curTimeline.RemoveFrame(i);
                    }


                    guiRect.x += guiRect.width;
                    if(GUI.Button(guiRect, "Ⓔ", ft16))
                    {
                        throw new System.Exception("Event Editor Not Implemented Yet");
                    }

                    guiRect.x += guiRect.width;
                    guiRect.width = 60;
                    GUI.Label(guiRect, "◅ edit");
                    

                    guiRect.x += guiRect.width;
                    guiRect.width = 75;
                    if(GUI.Button(guiRect, "overlap"))
                    {
                        throw new System.Exception("Frame overlap not implemented yet");
                    }
                }

                //second row
                {
                    guiRect.x -= 110;
                    guiRect.y += guiRect.height;
                    guiRect.width = 25;
                    //insert a frame button.
                    if(GUI.Button(guiRect, "⊕", ft16))
                    {
                        curTimeline.AddFrame(0, i);
                    }

                    //exit pixels button
                    guiRect.x += guiRect.width;
                    if(GUI.Button(guiRect, "✎", ft16))
                    {
                        //will open a new editor that allows for pixel editing.
                        throw new System.Exception("Pixel Sketch Not Implemented Yet.");
                    }

                    guiRect.x += guiRect.width;
                    guiRect.width = 60;
                    GUI.Label(guiRect, "options ▻");

                    guiRect.x += guiRect.width;
                    guiRect.width = 75;
                    string exitPointText = (curTimeline.GetIsFrameExitPoint(i)) ? "✔ exit point" : "✘ exit point";
                    if(GUI.Button(guiRect, exitPointText))
                    {
                        curTimeline.SetIsFrameExitPoint(i, !curTimeline.GetIsFrameExitPoint(i));
                    }
                }

                guiRect.y += guiRect.height + 5;
            }
            //need the space so the scrollbar can get the controlRect
            GUILayout.Space(guiRect.y + guiRect.height);
        }

        /// <summary>
        /// Displays the Edit Transition interface. Allows for Creating/Editing Parameters and setting conditions in which
        /// the timeline to which the transition is attached can be changed. Main usage should be through the In-Engine
        /// Editor Window.
        /// </summary>
        void DisplayTransitions()
        {
            GUILayout.Label("Transitions:", EditorStyles.helpBox);

            if (GUILayout.Button("◀ back",GUILayout.Width(60), GUILayout.Height(25)))
            {
                scrollViewDisplay -= DisplayTransitions;
                scrollViewDisplay += DisplayTimelineOptions;
            }

            GUILayout.BeginHorizontal();
            {
                GUIStyle btnStyle = new GUIStyle("button");
                btnStyle.fontSize = 8;
                GUILayout.Label("New Parameter:", GUILayout.Width(100), GUILayout.Height(25));
                if(GUILayout.Button("string", GUILayout.Width(40), GUILayout.Height(25)))
                {
                    Parameter para = CreateScriptableObject.Create(typeof(Parameter), "PixelAnim/DataObjects/Parameters", "StringParameter") as Parameter;
                    para.SetValue("hello.");
                }
                if(GUILayout.Button("bool", GUILayout.Width(40), GUILayout.Height(25)))
                {
                    Parameter para = CreateScriptableObject.Create(typeof(Parameter), "PixelAnim/DataObjects/Parameters", "BoolParameter") as Parameter;
                    para.SetValue(false);
                }
                if(GUILayout.Button("int", GUILayout.Width(40), GUILayout.Height(25)))
                {
                    Parameter para = CreateScriptableObject.Create(typeof(Parameter), "PixelAnim/DataObjects/Parameters", "IntParameter") as Parameter;
                    int val = 0;
                    para.SetValue(val);
                }
                if(GUILayout.Button("float", GUILayout.Width(40), GUILayout.Height(25)))
                {
                    Parameter para = CreateScriptableObject.Create(typeof(Parameter), "PixelAnim/DataObjects/Parameters", "FloatParameter") as Parameter;
                    float val = 0f;
                    para.SetValue(val);
                }
                if(GUILayout.Button("trigger", GUILayout.Width(50), GUILayout.Height(25)))
                {
                    Parameter para = CreateScriptableObject.Create(typeof(Parameter), "PixelAnim/DataObjects/Parameters", "TriggerParameter") as Parameter;
                    bool b = false;
                    para.SetTrigger(b);
                }
            }
            GUILayout.EndHorizontal();
            
            Timeline tl = pxAnim.Timelines[editingTimeline];
            bool needRefresh = false;
            GUILayout.Label("➤Parameters:");
            for (int i = 0; i < tl.Parameters.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                string paraType = tl.Parameters[i].GetTypeAsString();
                Parameter p = tl.Parameters[i];
                EditorGUILayout.LabelField(tl.Parameters[i].name + ":" + paraType, GUILayout.Width(160));
                switch(paraType)
                {
                    case "string":
                        string s = p.GetValue<string>();
                        EditorGUI.BeginChangeCheck();
                        s = EditorGUILayout.TextField(s, GUILayout.Width(50));
                        if (EditorGUI.EndChangeCheck())
                        {
                            p.SetValue(s);
                            EditorUtility.SetDirty(p);
                            needRefresh = true;
                        }
                        break;

                    case "bool":
                        bool b = p.GetValue<bool>();
                        EditorGUI.BeginChangeCheck();
                        b = EditorGUILayout.Toggle(b);
                        if(EditorGUI.EndChangeCheck())
                        {
                            p.SetValue(b);
                            EditorUtility.SetDirty(p);
                            needRefresh = true;
                        }
                        break;
                    
                    case "int":
                        int iVal = p.GetValue<int>();
                        EditorGUI.BeginChangeCheck();
                        iVal = EditorGUILayout.IntField(iVal,GUILayout.Width(50));
                        if(EditorGUI.EndChangeCheck())
                        {
                            p.SetValue(iVal);
                            EditorUtility.SetDirty(p);
                            needRefresh = true;
                        }
                        break;

                    case "float":
                        float fVal = p.GetValue<float>();
                        EditorGUI.BeginChangeCheck();
                        float rVal = EditorGUILayout.DelayedFloatField(fVal, GUILayout.Width(50));
                        if (EditorGUI.EndChangeCheck())
                        {
                            p.SetValue(rVal);
                            EditorUtility.SetDirty(p);
                            needRefresh = true;
                        }
                        break;

                    case "trigger":
                        bool t = p.PeekTrigger();
                        EditorGUI.BeginChangeCheck();
                        t = EditorGUILayout.Toggle(t);
                        if (EditorGUI.EndChangeCheck())
                        {
                            p.SetTrigger(t);
                            EditorUtility.SetDirty(p);
                            needRefresh = true;
                        }
                        break;
                }

                //tl.Parameters[i].ChangeInt(555);//this works without issue.
                EditorGUILayout.EndHorizontal();
            }
            if(needRefresh)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            //display available transitions and button to edit them
            GUILayout.Label("➤Transitions: ");
            for(int i = 0; i < tl.Transitions.Count; i++)
            {
                Transition tr;
                if (tl.Transitions[i] == null) continue;

                tr = tl.Transitions[i];
                GUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button("edit", GUILayout.Width(40), GUILayout.Height(20)))
                    {
                        editingTransition = tr;
                        scrollViewDisplay -= DisplayTransitions;
                        scrollViewDisplay += DisplayEditTransition;
                    }
                    if(tr.targetTimeline != null)
                        GUILayout.Label("" + tr.name + ":" + tl.name + " ➜ " + tr.targetTimeline.name);
                }
                GUILayout.EndHorizontal();


            }
            GUILayout.Space(10);
            if (GUILayout.Button("New Transition", GUILayout.Width(100), GUILayout.Height(25)))
            {
                //TODO
                Debug.Log("New Transition");
                CreateScriptableObject.Create(typeof(Transition), "PixelAnim/DataObjects/Transitions", "NewTransition");
            }
        }

        void DisplayEditTransition()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Edit Transition: " + editingTransition.name, EditorStyles.helpBox);
            if (GUILayout.Button("◀ back", GUILayout.Height(25), GUILayout.Width(60)))
            {
                scrollViewDisplay += DisplayTransitions;
                scrollViewDisplay -= DisplayEditTransition;
            }
            
            if(editingTransition.parameterCount == 0)
            {
                if (GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(25)))
                {
                    Debug.Log("+");
                    editingTransition.parameterCount++;
                }
            }
            for (int i = 0; i < editingTransition.parameterCount; i++)
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        editingTransition.parameterCount++;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        editingTransition.parameterCount--;
                    }

                    Parameter newP = null;
                    if (editingTransition.TargetParameters.Count > i && editingTransition.TargetParameters[i] != null) newP = editingTransition.TargetParameters[i];
                    Parameter assignedP = EditorGUILayout.ObjectField("", newP , typeof(Parameter), false) as Parameter;
                    if(editingTransition.TargetParameters.Count > i)
                    {
                        editingTransition.TargetParameters[i] = assignedP;
                    }
                    else
                    {
                        editingTransition.TargetParameters.Add(assignedP);
                    }

                    //set the operator
                    if(assignedP != null)
                    {
                        int currentValue = editingTransition.GetSelectedPrimaryOperator(i);
                        int value = EditorGUILayout.Popup(currentValue,editingTransition.PrimaryOperators, GUILayout.Width(40));
                        editingTransition.SetSelectedPrimaryOperator(i, value);
                        //TODO doesnt work yet.
                    }

                    //set the right hand opperand
                    if (assignedP != null)
                    {
                        switch(assignedP.GetTypeAsString())
                        {
                            //these are all wrong. i dont want to set the parameter's value.
                            //i need to set a desired value inside the transition.
                            case "string":
                                string text = EditorGUILayout.TextField(editingTransition.GetRightOperandValue<string>(i));
                                editingTransition.SetRightOperandValue(i, text);
                                break;
                            case "int":
                                int iVal = EditorGUILayout.IntField(editingTransition.GetRightOperandValue<int>(i));
                                editingTransition.SetRightOperandValue(i, (int)iVal);
                                break;
                            case "float":
                                float fVal = EditorGUILayout.FloatField(editingTransition.GetRightOperandValue<float>(i));
                                editingTransition.SetRightOperandValue(i, fVal);
                                break;
                            case "bool":
                                bool boolVal = EditorGUILayout.Toggle(editingTransition.GetRightOperandValue<bool>(i));
                                string s1 = boolVal ? "true" : "false";
                                editingTransition.SetRightOperandValue(i, boolVal);
                                GUILayout.Label(s1);
                                break;
                            case "trigger":
                                bool trigVal = EditorGUILayout.Toggle(editingTransition.GetRightOperandValue<bool>(i));
                                string s2 = trigVal ? "true" : "false";
                                editingTransition.SetRightOperandValue(i, trigVal);
                                GUILayout.Label(s2);
                                break;
                        }
                    }
                    

                }
                GUILayout.EndHorizontal();

                //then add or assign to list.
                if (editingTransition.parameterCount > 1 && i != editingTransition.parameterCount-1)
                {
                    GUILayout.Label("Operator: ");
                    if (i >= editingTransition.ActiveOperators.Count) editingTransition.ActiveOperators.Add(0);
                    int value = EditorGUILayout.Popup(editingTransition.ActiveOperators[i], Transition.Operators, GUILayout.Width(50));
                    editingTransition.ActiveOperators[i] = value;
                }
            }

            //the target timeline of this transition
            Timeline curTimeline = editingTransition.targetTimeline;
            Timeline newTimeline = EditorGUILayout.ObjectField("Target Timeline:", curTimeline, typeof(Timeline), false) as Timeline;
            editingTransition.targetTimeline = newTimeline;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(editingTransition);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Display the interface to create and modify frame events.
        /// </summary>
        void DisplayFrameEvents()
        {

        }
    }

}
