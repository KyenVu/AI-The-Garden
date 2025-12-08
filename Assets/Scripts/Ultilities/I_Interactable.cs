using UnityEngine;

/// <summary>
/// Interface for any object that can be interacted with by the player (via mouse clicks or agent commands).
/// </summary>
public interface I_Interactable
{
    /// <summary>
    /// Called when the object is clicked or otherwise interacted with.
    /// </summary>
    /// <param name="interactor">Optional — the object or agent performing the interaction.</param>
    public void Interact(GameObject interactor);

    /// <summary>
    /// Called when the mouse cursor hovers over this object.
    /// </summary>
    public void OnHoverEnter();

    /// <summary>
    /// Called when the mouse cursor stops hovering over this object.
    /// </summary>
    public void OnHoverExit();
}
