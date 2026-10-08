using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Tir a la gachette du pistolet tenu. Le premier obstacle solide arrete le jet.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(XRGrabInteractable))]
public class PuzzleBlaster : MonoBehaviour
{
    [SerializeField] private Transform muzzle;
    [Min(0.1f)] [SerializeField] private float range = 12f;
    [Min(0.02f)] [SerializeField] private float fireCooldown = 0.25f;
    [SerializeField] private LayerMask hitMask = Physics.DefaultRaycastLayers;
    [SerializeField] private LineRenderer jetLine;
    [SerializeField] private AudioSource shotSource;
    [SerializeField] private AudioClip shotClip;

    private XRGrabInteractable grab;
    private float nextShotTime;
    private float hideJetTime;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        if (jetLine != null)
            jetLine.enabled = false;
    }

    private void OnEnable() => grab.activated.AddListener(OnActivated);

    private void OnDisable()
    {
        grab.activated.RemoveListener(OnActivated);
        if (jetLine != null)
            jetLine.enabled = false;
    }

    private void Update()
    {
        if (jetLine != null && jetLine.enabled && Time.time >= hideJetTime)
            jetLine.enabled = false;
    }

    private void OnActivated(ActivateEventArgs args) => Fire();

    public void Fire()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || muzzle == null || Time.time < nextShotTime)
            return;
        bool heldByHand = false;
        foreach (var interactor in grab.interactorsSelecting)
        {
            if (!(interactor is XRSocketInteractor))
                heldByHand = true;
        }
        if (!heldByHand)
            return;

        nextShotTime = Time.time + fireCooldown;
        Vector3 origin = muzzle.position;
        Vector3 direction = muzzle.forward;
        Vector3 end = origin + direction * range;
        var hits = Physics.RaycastAll(origin, direction, range, hitMask, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform))
                continue;
            end = hit.point;
            var target = hit.collider.GetComponentInParent<PcPowerTarget>();
            if (target != null)
                target.ReceiveShot(this);
            break;
        }
        if (jetLine != null)
        {
            jetLine.useWorldSpace = true;
            jetLine.positionCount = 2;
            jetLine.SetPosition(0, origin);
            jetLine.SetPosition(1, end);
            jetLine.enabled = true;
            hideJetTime = Time.time + 0.09f;
        }
        if (shotSource != null && shotClip != null)
            shotSource.PlayOneShot(shotClip);
    }
}
