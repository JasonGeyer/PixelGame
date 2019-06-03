using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that holds all of the animation data - includes the texture and frames.
/// </summary>
namespace PixelEditor
{
    //TODO
    //can probably roll all of this into PixelAnimator.
    public class PixelAnimation : MonoBehaviour
    {
        public Texture2D texture;
        public int Rows;
        public int Columns;

        public List<Timeline> Timelines;
    }
}
