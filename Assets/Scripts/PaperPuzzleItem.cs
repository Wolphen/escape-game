using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class PaperPuzzleItem : MonoBehaviour, IXRSelectFilter
{
    [Header("Retour du papier")]
    [Tooltip("Repere fixe, jamais enfant du papier. Vide : position initiale du papier.")]
    [SerializeField] private Transform startPoint;
    [Min(0.05f)] [SerializeField] private float resetDelay = 0.25f;
    [Tooltip("Masque les visuels pendant le retour, sans detruire l'objet ni ses references.")]
    [SerializeField] private bool hideDuringReset = true;
    [Tooltip("Sous cette hauteur mondiale, retour au checkpoint. Adapter a la salle.")]
    [SerializeField] private float fallResetHeight = -5f;

    [Header("Saisie")]
    [Tooltip("Limite la distance entre la main et le centre du papier au debut de la saisie.")]
    [SerializeField] private bool limitGrabDistance = true;
    [Min(0.05f)] [SerializeField] private float maxGrabDistance = 0.3f;

    [Header("Evenements optionnels")]
    [SerializeField] private UnityEvent onReset = new UnityEvent();
    [SerializeField] private UnityEvent onDestroyed = new UnityEvent();

    public int CurrentCheckpointIndex { get; private set; }
    public bool IsResetting { get; private set; }
    public bool IsDelivered { get; private set; }
    public bool canProcess => isActiveAndEnabled;

    private Rigidbody body;
    private XRGrabInteractable grab;
    private Pose initialPose;
    private Pose checkpointPose;
    private bool savedKinematic;
    private bool savedGravity;
    private bool savedThrow;
    private bool resettingToStart;
    private bool destructionNotified;
    private Coroutine resetRoutine;
    private XRSocketInteractor deliverySocket;
    private Renderer[] visualRenderers;
    private bool[] rendererEnabledBeforeReset;
    private bool visualsHidden;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        initialPose = new Pose(transform.position, transform.rotation);
        checkpointPose = initialPose;
        visualRenderers = GetComponentsInChildren<Renderer>(true);
        rendererEnabledBeforeReset = new bool[visualRenderers.Length];
    }

    private void OnEnable()
    {
        grab.selectFilters.Add(this);
    }

    private void OnDisable()
    {
        grab.selectFilters.Remove(this);
        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }
        if (IsResetting)
            FinishReset();
    }

    private void Update()
    {
        if (!IsResetting && !IsDelivered && transform.position.y < fallResetHeight)
            ResetToCheckpoint();
    }

    private void OnCollisionEnter(Collision collision)
    {
        ResetOnContact(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        ResetOnContact(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        ResetOnContact(other);
    }

    private void OnTriggerStay(Collider other)
    {
        ResetOnContact(other);
    }

    private void ResetOnContact(Collider other)
    {
        if (!isActiveAndEnabled || IsDelivered || other == null)
            return;

        // Un groupe d'obstacles peut porter la regle sur son parent, et les colliders sur ses enfants.
        // Ne pas exclure IsResetting : une destruction doit pouvoir prendre priorite sur un retour.
        var zone = other.GetComponentInParent<PaperResetZone>();
        if (zone != null)
            zone.ResetPaper(this);
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (IsResetting)
            return false;

        bool alreadySelecting = interactor.IsSelecting(interactable);
        if (IsDelivered)
            return interactor is XRSocketInteractor socket && IsDeliverySocket(socket);

        if (interactor is XRSocketInteractor || alreadySelecting || !limitGrabDistance)
            return true;

        return (interactor.transform.position - transform.position).sqrMagnitude <=
               maxGrabDistance * maxGrabDistance;
    }

    public bool TrySetCheckpoint(int index, Transform respawnPoint)
    {
        if (IsResetting || IsDelivered || respawnPoint == null ||
            index != CurrentCheckpointIndex + 1)
            return false;

        CurrentCheckpointIndex = index;
        // Copie de la pose : deplacer un socket ne deplace pas un retour deja valide.
        checkpointPose = new Pose(respawnPoint.position, respawnPoint.rotation);
        return true;
    }

    public bool CanDeliver(int requiredCheckpointIndex)
    {
        return isActiveAndEnabled && !IsResetting && !IsDelivered &&
               CurrentCheckpointIndex >= Mathf.Max(0, requiredCheckpointIndex);
    }

    public bool TryDeliver(int requiredCheckpointIndex)
    {
        if (!CanDeliver(requiredCheckpointIndex))
            return false;

        // La livraison n'est valide que si un socket tient effectivement le papier.
        foreach (var interactor in grab.interactorsSelecting)
        {
            if (interactor is PaperSocketInteractor socket && socket.AcceptedPaper == this)
            {
                deliverySocket = socket;
                IsDelivered = true;
                return true;
            }
        }
        return false;
    }

    public bool IsDeliverySocket(XRSocketInteractor socket)
    {
        return IsDelivered && deliverySocket == socket;
    }

    [ContextMenu("Test/Retour au checkpoint (Play Mode)")]
    public void ResetToCheckpoint()
    {
        RequestReset(false);
    }

    [ContextMenu("Test/Destruction et retour au depart (Play Mode)")]
    public void DestroyAndRespawn()
    {
        RequestReset(true);
    }

    // Le refus du keypad peut aussi reprendre un papier deja livre a l'imprimante.
    public void RestartFromBeginning()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        IsDelivered = false;
        deliverySocket = null;
        RequestReset(true);
    }

    private void RequestReset(bool toStart)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || IsDelivered)
            return;

        if (IsResetting)
        {
            // Si deux zones se chevauchent, la destruction reste prioritaire.
            if (toStart && !resettingToStart)
            {
                resettingToStart = true;
                CurrentCheckpointIndex = 0;
                PlaceAtReturnPoint();
                NotifyDestruction();
            }
            return;
        }

        resettingToStart = toStart;
        destructionNotified = false;
        resetRoutine = StartCoroutine(ResetRoutine());
        // Le handle existe avant les evenements, meme s'ils desactivent cet objet.
        onReset.Invoke();
        NotifyDestruction();
    }

    private IEnumerator ResetRoutine()
    {
        IsResetting = true;
        if (resettingToStart)
            CurrentCheckpointIndex = 0;

        // XRI termine le lancer dans LateUpdate : on coupe le lancer AVANT de liberer.
        savedThrow = grab.throwOnDetach;
        grab.throwOnDetach = false;
        if (grab.interactionManager != null)
            grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grab);

        // Apres annulation, XRI a restaure les proprietes du Rigidbody au repos.
        savedKinematic = body.isKinematic;
        savedGravity = body.useGravity;
        StopMotion();
        body.isKinematic = true;
        body.useGravity = false;
        HideVisuals();
        PlaceAtReturnPoint();

        // Laisser passer LateUpdate et eviter une reprise dans la meme frame.
        yield return null;
        yield return null;
        yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, resetDelay));
        FinishReset();
        resetRoutine = null;
    }

    private void PlaceAtReturnPoint()
    {
        Pose pose = CurrentCheckpointIndex > 0 ? checkpointPose :
            startPoint != null ? new Pose(startPoint.position, startPoint.rotation) : initialPose;
        body.position = pose.position;
        body.rotation = pose.rotation;
        transform.SetPositionAndRotation(pose.position, pose.rotation);
    }

    private void FinishReset()
    {
        // Couvre aussi une desactivation avant la phase Late du gestionnaire XR.
        grab.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Late);
        body.isKinematic = savedKinematic;
        body.useGravity = savedGravity;
        StopMotion();
        grab.throwOnDetach = savedThrow;
        RestoreVisuals();
        IsResetting = false;
    }

    private void HideVisuals()
    {
        if (!hideDuringReset || visualsHidden)
            return;

        visualsHidden = true;
        for (int i = 0; i < visualRenderers.Length; i++)
        {
            if (visualRenderers[i] == null)
                continue;
            rendererEnabledBeforeReset[i] = visualRenderers[i].enabled;
            visualRenderers[i].enabled = false;
        }
    }

    private void RestoreVisuals()
    {
        if (!visualsHidden)
            return;

        for (int i = 0; i < visualRenderers.Length; i++)
        {
            if (visualRenderers[i] != null)
                visualRenderers[i].enabled = rendererEnabledBeforeReset[i];
        }
        visualsHidden = false;
    }

    private void NotifyDestruction()
    {
        if (!resettingToStart || destructionNotified)
            return;
        destructionNotified = true;
        onDestroyed.Invoke();
    }

    private void StopMotion()
    {
        if (body.isKinematic)
            return;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}
