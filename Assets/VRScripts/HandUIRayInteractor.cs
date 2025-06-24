using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HandUIRayInteractor : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public enum HandSide { Left, Right }
    public HandSide handSide = HandSide.Right;

    [Header("Input Action (auto-set if null)")]
    public InputActionProperty selectAction;

    private bool wasPressedLastFrame = false;

    void Start()
    {
        // Auto-assign input if not set manually
        if (selectAction.reference == null)
        {
            string path = handSide == HandSide.Left ?
                "<XRController>{LeftHand}/trigger" :
                "<XRController>{RightHand}/trigger";

            var action = new InputAction("Select", binding: path);
            action.Enable();
            selectAction = new InputActionProperty(action);
        }
    }

    void Update()
    {
        float triggerValue = selectAction.action.ReadValue<float>();
        bool isPressedNow = triggerValue > 0.75f;

        if (isPressedNow && !wasPressedLastFrame)
        {
            // Trigger just got pressed this frame
            if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult result))
            {
                if (result.gameObject.TryGetComponent(out Button btn))
                {
                    btn.onClick.Invoke();
                }
            }
        }

        wasPressedLastFrame = isPressedNow;
    }
}
