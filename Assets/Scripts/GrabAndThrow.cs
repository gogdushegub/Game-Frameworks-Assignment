
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabAndThrow : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float throwForce = 8f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private GameObject nearbyObject;
    private GameObject heldObject;

    private static readonly int GrabHash =
        Animator.StringToHash("Grab");

    private static readonly int ThrowHash =
        Animator.StringToHash("Throw");

    private static readonly int IsHoldingHash =
        Animator.StringToHash("IsHolding");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (heldObject == null)
            PickUp();
        else
            Throw();
    }

    private void PickUp()
    {
        if (nearbyObject == null)
            return;

        heldObject = nearbyObject;
        nearbyObject = null;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider col = heldObject.GetComponent<Collider>();

        if (col != null)
            col.enabled = false;

        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        if (animator != null)
        {
            animator.SetBool(IsHoldingHash, true);
            animator.ResetTrigger(ThrowHash);
            animator.SetTrigger(GrabHash);
        }
    }

    private void Throw()
    {
        if (heldObject == null)
            return;

        GameObject objectToThrow = heldObject;
        heldObject = null;

        if (animator != null)
        {
            animator.SetBool(IsHoldingHash, false);
            animator.ResetTrigger(GrabHash);
            animator.SetTrigger(ThrowHash);
        }

        objectToThrow.transform.SetParent(null);

        Rigidbody rb = objectToThrow.GetComponent<Rigidbody>();
        Collider col = objectToThrow.GetComponent<Collider>();

        if (col != null)
            col.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(
                transform.forward * throwForce,
                ForceMode.Impulse
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickUp") ||
            other.CompareTag("Crop"))
        {
            nearbyObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearbyObject)
            nearbyObject = null;
    }
}
