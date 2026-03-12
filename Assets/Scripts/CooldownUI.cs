using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownUI : MonoBehaviour

{
    public GameObject lanternController;
    public Image imageCooldown;
    public TMP_Text textCooldown;
    public float cooldown;
    // Start is called before the first frame update
    void Start()
    {
        imageCooldown.fillAmount = 0.0f;
        cooldown= lanternController.GetComponent<LanternController>().flashCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        float timer = lanternController.GetComponent<LanternController>()._flashCooldownTimer;
        textCooldown.text = timer.ToString("F1");
        textCooldown.text = timer > 0.1f ? timer.ToString("F1") : "";
        imageCooldown.fillAmount = timer / cooldown;
    }
}
