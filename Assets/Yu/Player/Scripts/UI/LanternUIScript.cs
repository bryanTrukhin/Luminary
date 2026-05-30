using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanternUIScript : MonoBehaviour
{
    public Sprite[] frames;

    public LanternController lanternController;
    public float max;
    private float step;

    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        step = max / (frames.Length - 1);
    }

    void Update()
    {
        float capacity = lanternController._currentFireflies;

        if (capacity < 0f) capacity = 0f;
        if (capacity > max) capacity = max;

        int frameIndex = Mathf.FloorToInt(capacity / step);

        if (frameIndex >= frames.Length)
            frameIndex = frames.Length - 1;

        img.sprite = frames[frames.Length - 1 - frameIndex];
    }
}