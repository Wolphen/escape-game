using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Remplace le XR Socket Interactor standard sur les sockets de cette enigme.
[DisallowMultipleComponent]
public class PaperSocketInteractor : XRSocketInteractor
{
    [Header("Papier accepte")]
    [SerializeField] private PaperPuzzleItem acceptedPaper;

    public PaperPuzzleItem AcceptedPaper => acceptedPaper;
    // Configure automatiquement par PaperCheckpoint ou PrinterClue.
    public int RequiredCheckpointIndex { get; set; }

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return Accepts(interactable.transform) && base.CanHover(interactable);
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return Accepts(interactable.transform) &&
               base.CanSelect(interactable);
    }

    private bool Accepts(Transform candidate)
    {
        return acceptedPaper != null && acceptedPaper.isActiveAndEnabled &&
               candidate == acceptedPaper.transform && !acceptedPaper.IsResetting &&
               (!acceptedPaper.IsDelivered || acceptedPaper.IsDeliverySocket(this)) &&
               acceptedPaper.CurrentCheckpointIndex >= RequiredCheckpointIndex;
    }
}
