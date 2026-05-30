using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownUI : MonoBehaviour
{
    public LanternController lanternController;
    public Image imageCooldown;
    public TMP_Text textCooldown;

    public float cooldown;
    public float dashCooldown;
    public float flashAreaCooldown;
    public float healCooldown;
    public int skillNum;
    float dashTimer;

    LanternController lantern;

    void Start()
    {
        lanternController = FindObjectOfType<LanternController>();
        lantern = FindObjectOfType<LanternController>();
        cooldown = lantern.flashCooldown;
        flashAreaCooldown = lantern.projectileCooldown;
        healCooldown = lantern.healHealthCooldown;
        imageCooldown.fillAmount = 0.0f;
        dashCooldown = 0.5f;
        dashTimer = 0f;
    }

    void Update()
    {
        if (skillNum == 1)
        {
            float timer = lantern._flashCooldownTimer;

            textCooldown.text = timer > 0.1f ? timer.ToString("F1") : "";
            imageCooldown.fillAmount = timer / cooldown;
        }
        else if(skillNum == 2)
        {
            if (dashTimer > 0)
                dashTimer -= Time.deltaTime;

            float timer = dashTimer;

            textCooldown.text = timer > 0.1f ? timer.ToString("F1") : "";
            imageCooldown.fillAmount = timer / dashCooldown;
        }
        else if(skillNum == 3){
            float timer = lantern._bombCdTimer;

            textCooldown.text = timer > 0.1f ? timer.ToString("F1") : "";
            imageCooldown.fillAmount = timer / lantern.projectileCooldown;
        }
        else{
            float timer = lantern._healHealthCooldownTimer;

            textCooldown.text = timer > 0.1f ? timer.ToString("F1") : "";
            imageCooldown.fillAmount = timer / healCooldown;
        }
    }

        public void TriggerDashCooldown()
        {
            dashTimer = dashCooldown;
        }
}
