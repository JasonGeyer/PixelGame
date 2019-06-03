using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for playing the animation on the object.
/// </summary>
namespace PixelEditor
{
    public class PixelAnimator : MonoBehaviour
    {
        //references to animation classes
        private PixelAnimation pxAnim;
        private SpriteRenderer spriteRenderer;
        private Timeline currentTimeline;

        //the current animation's frames/sprites
        private List<Sprite> Sprites = new List<Sprite>();

        //control variables
        private bool isInitialized = false;
        private bool isPlaying = false;
        public bool IsPlaying() { return isPlaying; }

        //timer and animation variables
        private int currentFrame = 0; //the current frame from Sprites that's being used
        private int indexFrame = 0; //the current frame in the timeline. (counted from start-end in timeline)
        private float framePerSecond = 10f; //frames per second

        private long nextFrameTime = 0;

        //mirroring
        private bool isFlippedX = false;
        public bool GetFlipX() { return isFlippedX; }
        public void SetFlipX(bool b) { isFlippedX = b; }

        private bool isFlippedY = false;
        public bool GetFlipY() { return isFlippedY; }
        public void SetFlipY(bool b) { isFlippedY = b; }

        private void Initialize()
        {
            isInitialized = true;

            spriteRenderer = transform.GetComponent<SpriteRenderer>();
            pxAnim = transform.GetComponent<PixelAnimation>();

            if (!pxAnim.texture.isReadable) throw new System.Exception("Source texture is not readable. Enable Read/Write.");
            int spriteWidth = pxAnim.texture.width / pxAnim.Columns;
            int spriteHeight = pxAnim.texture.height / pxAnim.Rows;

            for (int r = pxAnim.Rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < pxAnim.Columns; c++)
                {
                    int startX = c * spriteWidth;
                    int startY = r * spriteHeight;
                    Sprite s = Sprite.Create(pxAnim.texture, new Rect(startX, startY, spriteWidth, spriteHeight), new Vector2(.5f, .5f), 1);
                    Sprites.Add(s);
                }
            }
        }

        private void Awake()
        {
            if (!isInitialized) Initialize();
        }

        //only called by the editor.
        public void EditorWindowUpdate()
        {
            if (!isInitialized) Initialize();

            if (currentTimeline == null)
            {
                Debug.Log("Error: currentTimeline is null");
                return;
            }

            PlayAnimation();
        }

        private void Update()
        {
            if(isPlaying)PlayAnimation();
        }

        private void PlayAnimation()
        {
            if (System.DateTime.Now.Ticks > nextFrameTime)
            {
                indexFrame++;
                if (indexFrame >= currentTimeline.FrameCount())
                {
                    if (currentTimeline.GetLoopTimeline())
                    {
                        indexFrame = 0;
                    }
                    else
                    {
                        indexFrame--;
                    }
                }
                SetNextFrameTime();
            }

            int newFrame = currentTimeline.GetFrame(indexFrame);
            if (newFrame > -1)
                currentFrame = newFrame;

            if (Sprites.Count > currentFrame)
            {
                spriteRenderer.sprite = Sprites[currentFrame];
            }
        }

        public void SetTimeline(Timeline newTimeline)
        {
            currentTimeline = newTimeline;
        }

        public void SetNextFrameTime()
        {
            double frameRate = 1.0 / currentTimeline.framesPerSecond;
            nextFrameTime = System.TimeSpan.FromSeconds(frameRate).Ticks + System.DateTime.Now.Ticks;
        }

        public void Play(string AnimationName, int start = 0)
        {
            #if UNITY_EDITOR
            Initialize();
            #endif

            if (pxAnim.Timelines.Count <= 0) return;

            if (isPlaying)
            {
                #if UNITY_EDITOR
                Stop();
                #endif
                return;
            }
            else
            {
                isPlaying = true;
            }

            Timeline newTimeline = null;
            foreach (Timeline t in pxAnim.Timelines)
            {
                if (t.name == AnimationName)
                {
                    newTimeline = t;
                    SetTimeline(newTimeline);
                    break;
                }
            }

            if (newTimeline == null)
            {
                Debug.Log("Error: cannot find a timeline with that name.");
            }
            SetNextFrameTime();
            
            indexFrame = 0;

            Update();
        }

        public void Stop()
        {
            isPlaying = false;
        }
    }
}
