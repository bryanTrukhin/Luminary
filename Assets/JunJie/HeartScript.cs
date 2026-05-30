using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartScript : MonoBehaviour
{
    public LanternController lanternController;
    private float health;
    public int heartNumber;
    public Image heartImage;
    public Sprite fullHeart;
    public Sprite brokenHeart;
    
    // Start is called before the first frame update
    void Start()
    {
        lanternController = FindObjectOfType<LanternController>();
        heartImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        health = lanternController._currentHealth;
        if (health <= heartNumber){
            heartImage.sprite = brokenHeart;
        }
        else
        {
            heartImage.sprite = fullHeart;
        }

    }
}
