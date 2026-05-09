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
    public float dashCooldown;
    public float flashAreaCooldown;
    public int skillNum;
    float dashTimer;

    LanternController lantern;

    void Start()
    {
        lantern = lanternController.GetComponent<LanternController>();

        imageCooldown.fillAmount = 0.0f;
        dashCooldown = 0.5f;
        dashTimer = 0f;

        if (skillNum == 1)
        {
            cooldown = lantern.flashCooldown;
        }
        if (skillNum == 2)
        {
            dashCooldown = 0.5f;
        }
        if (skillNum == 2)
        {
            flashAreaCooldown = lantern._bombCdTimer;
        }
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
        else{
            float timer = lantern._bombCdTimer;

            textCooldown.text = timer > 0.1f ? timer.ToString("F1") : "";
            imageCooldown.fillAmount = timer / flashAreaCooldown;
        }
    }

        public void TriggerDashCooldown()
        {
            dashTimer = dashCooldown;
        }
}
