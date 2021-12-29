using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractive
{
    public abstract void OnInteraction();
    public abstract void OnHoverStart();
    public abstract void OnHoverEnd();
}
