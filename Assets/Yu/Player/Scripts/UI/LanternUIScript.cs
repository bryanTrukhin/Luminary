using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanternUIScript : MonoBehaviour
{
    public Sprite frame0;
    public Sprite frame1;
    public Sprite frame2;
    public Sprite frame3;
    public Sprite frame4;
    public Sprite frame5;
    public Sprite frame6;
    public Sprite frame7;

    public Sprite frame8;
    public Sprite frame9;
    public Sprite frame10;

    public LanternController lanternController;
    public float max;
    private float step;

    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        step = max / 10f;
    }

    void Update()
    {
        float capacity = lanternController._currentFireflies;

        if (capacity < 0f) capacity = 0f;
        if (capacity > max) capacity = max;

        if (capacity < step)
            img.sprite = frame0;
        else if (capacity < 2*step)
            img.sprite = frame1;
        else if (capacity < 3*step)
            img.sprite = frame2;
        else if (capacity < 4*step)
            img.sprite = frame3;
        else if (capacity < 5*step)
            img.sprite = frame4;
        else if (capacity < 6*step)
            img.sprite = frame5;
        else if (capacity < 7*step)
            img.sprite = frame6;
        else if (capacity < 8*step)
            img.sprite = frame7;
        else if (capacity < 8*step)
            img.sprite = frame7;
        else if (capacity < 9*step)
            img.sprite = frame8;
        else if (capacity < 10*step)
            img.sprite = frame9;
        else img.sprite = frame10;
    }
}
