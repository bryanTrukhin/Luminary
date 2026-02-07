using UnityEngine;

public interface IHidable
{
    // Called when the object enters the locker
    void EnterHiding(Vector3 hidePos);

    // Called when the object leaves
    void ExitHiding(Vector3 exitPos);
}