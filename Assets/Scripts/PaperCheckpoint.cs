using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[DisallowMultipleComponent]
[RequireComponent(typeof(PaperSocketInteractor))]
public class PaperCheckpoint : MonoBehaviour
{
    [Min(1)] [SerializeField] private int checkpointIndex = 1;
    [Tooltip("Repere fixe au centre du papier lors d'un retour. Vide : Attach Transform du socket.")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private UnityEvent onCheckpointReached = new UnityEvent();

    private PaperSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<PaperSocketInteractor>();
    }

    private void OnEnable()
    {
        socket.RequiredCheckpointIndex = Mathf.Max(0, checkpointIndex - 1);
        socket.selectEntered.AddListener(OnPaperPlaced);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnPaperPlaced);
    }

    private void OnPaperPlaced(SelectEnterEventArgs args)
    {
        var paper = socket.AcceptedPaper;
        if (paper == null || args.interactableObject.transform != paper.transform)
            return;

        Transform point = respawnPoint != null ? respawnPoint : socket.attachTransform;
        if (paper.TrySetCheckpoint(checkpointIndex, point != null ? point : socket.transform))
            onCheckpointReached.Invoke();
    }
}
