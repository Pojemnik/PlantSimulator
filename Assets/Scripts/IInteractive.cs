using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractive
{
    public abstract void OnInteraction(Vector2 position);
    public abstract void OnHoverStart();
    public abstract void OnHoverEnd();
}
