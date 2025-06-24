using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DirectUIButtonClicker : MonoBehaviour
{
    public XRRayInteractor rayInteractor;

    [Header("Input (use XR Input Action binding or fallback to key)")]
    public InputActionProperty selectAction;
    public bool fallbackToKey = true;
    public KeyCode testKey = KeyCode.Space;

    private bool wasPressedLastFrame = false;

    void Update()
    {
        if (rayInteractor == null)
            return;

        // Use input system
        bool isPressed = false;
        if (selectAction.reference != null)
        {
            isPressed = selectAction.action.ReadValue<float>() > 0.75f;
        }
        else if (fallbackToKey)
        {
            isPressed = Input.GetKey(testKey);
        }

        // Detect press transition
        if (isPressed && !wasPressedLastFrame)
        {
            if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult result))
            {
                if (result.gameObject.TryGetComponent(out Button btn))
                {
                    btn.onClick.Invoke();
                }
            }
        }

        wasPressedLastFrame = isPressed;
    }
}
