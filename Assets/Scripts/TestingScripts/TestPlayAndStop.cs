using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelEditor;

public class TestPlayAndStop : MonoBehaviour
{
    PixelAnimator pixelAnimator;
    
    void Start()
    {
        pixelAnimator = GetComponent<PixelAnimator>();
    }
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (pixelAnimator.IsPlaying())
            {
                pixelAnimator.Stop();
            }
            else
            {
                pixelAnimator.Play("Walk");
            }
        }
    }
}
