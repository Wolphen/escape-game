using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRBaseInteractable))]
public class HandleRotator : MonoBehaviour
{
    [SerializeField] Transform objectToRotate;
    [SerializeField] Transform pivot;
    [SerializeField] Vector3 axis = Vector3.up;

    XRBaseInteractable interactable;
    IXRSelectInteractor interactor;
    Quaternion initialRotation;
    Vector3 previousDir;
    float currentAngle;

    void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        initialRotation = objectToRotate.localRotation;
        axis = axis.normalized;
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnGrab);
        interactable.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnGrab);
        interactable.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject;
        previousDir = GetFlatDirection();
    }

    void OnRelease(SelectExitEventArgs args)
    {
        interactor = null;
    }

    Vector3 GetFlatDirection()
    {
        Vector3 handPos = interactor.GetAttachTransform(interactable).position;
        Vector3 worldAxis = pivot.TransformDirection(axis);
        Vector3 dir = handPos - pivot.position;
        return Vector3.ProjectOnPlane(dir, worldAxis).normalized;
    }

    void Update()
    {
        if (interactor == null) return;

        Vector3 worldAxis = pivot.TransformDirection(axis);
        Vector3 dir = GetFlatDirection();

        // On accumule le delta à chaque frame : pas de saut à 180°, rotation continue possible.
        float delta = Vector3.SignedAngle(previousDir, dir, worldAxis);
        previousDir = dir;

        currentAngle += delta;

        objectToRotate.localRotation = initialRotation * Quaternion.AngleAxis(currentAngle, axis);
    }
}
