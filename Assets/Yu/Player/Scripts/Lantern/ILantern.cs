using UnityEngine;

public interface ILantern
{
    // Energy / Mana System
    bool TryConsumeEnergy(float amount);

    // Abilities
    void TriggerFlash();
    void TriggerFireflyBomb();
    void SetVisible(bool state);
    
    // Effects (Added so GhostTrainEvent and cutscenes can use it)
    void Flicker(float amplitude, float speed, float duration);

    bool TryUseDash();
    bool TryUseDoubleJump();

    void TakeDamage(float amount, int health_amount = 1);
    // void UpdateFacingDirection(bool facingLeft);
    void ResetHealth();
}