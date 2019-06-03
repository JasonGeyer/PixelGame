using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelEditor
{
    public class MouseImage
    {
        private Texture2D tex;
        private bool isMouseDown = false;

        public MouseImage(Texture2D inTex)
        {
            Debug.Log("new mouse image.");
            tex = inTex;
        }

        public void DrawImage(PixelAnimWindow window, Event e)
        {
            Vector2 mousePos = Vector2.zero;
            if (e != null) mousePos = e.mousePosition;

            int scale = window.GetFrameScale();

            GUI.DrawTexture(new Rect(mousePos.x, mousePos.y, tex.width * scale, tex.height * scale), tex);
        }

        /// <summary>
        /// Update the mouseimage. only give it the event if displayImage will not change.
        /// </summary>
        /// <param name="e">the current event</param>
        /// <param name="scale">image scale - defaults to 1.</param>
        /// <param name="displayImage">image to display - defaults to previous</param>
        public bool Update(bool setMouseDown, Event e, int scale = 1, Texture2D displayImage = null)
        {
            Debug.Log("update mouse image");
            //If a new display image isnt given and an old one doesn't exist, then dont update.
            /*
            if (displayImage != null)
            {
                tex = displayImage;
            }
            else
            {
                if (tex == null) return false;
            }

            //when the button is clicked.
            if (e.type == EventType.Used)
            {
                Debug.Log("toggled.");
                isMouseDown = !isMouseDown;
            }
            */

            //if (isMouseDown)
            {
                Vector2 mousePos = Vector2.zero;
                if (e != null) mousePos = e.mousePosition;

                GUI.DrawTexture(new Rect(mousePos.x, mousePos.y, tex.width * scale, tex.height * scale), tex);
            }

            return isMouseDown;
        }

    }
}
