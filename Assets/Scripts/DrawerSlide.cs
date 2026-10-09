using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Un clic ouvre le tiroir, le suivant le referme.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(XRSimpleInteractable))]
public class DrawerSlide : MonoBehaviour
{
    [Tooltip("Partie mobile. Vide : l'objet portant ce script.")]
    [SerializeField] private Transform drawer;
    [Tooltip("Deplacement depuis la position fermee, dans les axes du parent du tiroir.")]
    [SerializeField] private Vector3 openOffset = new Vector3(0.35f, 0f, 0f);
    [Min(0f)] [SerializeField] private float moveDuration = 0.45f;

    public bool IsOpen => openAmount >= 1f;
    public bool IsMoving => openAmount != (wantsOpen ? 1f : 0f);

    private XRSimpleInteractable interaction;
    private Vector3 closedPosition;
    private float openAmount;
    private bool wantsOpen;

    private void Awake()
    {
        interaction = GetComponent<XRSimpleInteractable>();
        if (drawer == null)
            drawer = transform;
        closedPosition = drawer.localPosition;
    }

    private void OnEnable()
    {
        interaction.selectEntered.AddListener(OnSelected);
    }

    private void OnDisable()
    {
        interaction.selectEntered.RemoveListener(OnSelected);
    }

    private void Update()
    {
        if (drawer == null || !IsMoving)
            return;

        openAmount = moveDuration <= 0f ? (wantsOpen ? 1f : 0f) :
            Mathf.MoveTowards(openAmount, wantsOpen ? 1f : 0f, Time.deltaTime / moveDuration);
        ApplyPosition();
    }

    private void OnSelected(SelectEnterEventArgs args) => Toggle();

    public void Toggle() => SetOpen(!wantsOpen);
    public void Open() => SetOpen(true);
    public void Close() => SetOpen(false);

    private void SetOpen(bool open)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || drawer == null)
            return;
        wantsOpen = open;
        if (moveDuration <= 0f)
        {
            openAmount = wantsOpen ? 1f : 0f;
            ApplyPosition();
        }
    }

    private void ApplyPosition()
    {
        // Toujours repartir de la pose initiale, pour eviter une derive apres plusieurs clics.
        drawer.localPosition = closedPosition + openOffset * Mathf.SmoothStep(0f, 1f, openAmount);
    }
}
