using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class FlashBangScript : MonoBehaviour
{
    public Color start;
    public Color end;
    public float duration;
    public float elapsed;
    public SpriteRenderer render;
    public float factor;
    public LaunchScript l;

    // Start is called before the first frame update
    void Start()
    {
        l = GameObject.FindGameObjectWithTag("Launcher").GetComponent<LaunchScript>();
        elapsed = l.flashCooldown;
        Debug.Log($"the duration of this object should be {elapsed}");
        render = this.GetComponent<SpriteRenderer>();
        render.color = start;
        factor = 1.5f;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed -= Time.deltaTime;
        float t = Mathf.Clamp01(elapsed/duration);
        // Color current = Color.Lerp(start,end,t);
        render.color = Color.Lerp(render.color, end, Time.deltaTime*factor);
        if(elapsed <= 0f)
        {
            Destroy(gameObject);
        }
    }
}