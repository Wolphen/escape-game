using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Accepte le papier du parcours, puis affiche l'indice à la demande.</summary>
[DisallowMultipleComponent]
public class PrinterClue : MonoBehaviour
{
    [Header("Interactions")]
    [SerializeField] private PaperSocketInteractor paperSocket;
    [SerializeField] private XRSimpleInteractable printInteraction;
    [SerializeField] private Gamerule gameRule;
    [Tooltip("Index du dernier checkpoint obligatoire. Mettre 0 pour un parcours sans checkpoint.")]
    [Min(0)] [SerializeField] private int requiredCheckpointIndex = 1;

    [Header("Impression")]
    [Min(0f)] [SerializeField] private float printDuration = 2f;
    [SerializeField] private UnityEvent onPrintingStarted = new UnityEvent();
    [SerializeField] private UnityEvent onPrintingCompleted = new UnityEvent();
    [SerializeField] private UnityEvent onClueRevealed = new UnityEvent();

    [Header("Fenêtre d'indice")]
    [Tooltip("Un panneau enfant ou un Canvas distinct : ne pas utiliser l'objet qui porte ce script.")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text popupText;
    [TextArea(4, 10)] [SerializeField] private string clueText =
        "À 14 h 20, le gardien quitte son poste et murmure : « Le coffre garde cette heure en mémoire. Tape ses quatre chiffres, sans espace. »\n\n" +
        "Attention : un code refusé renvoie le papier au départ et efface les checkpoints.\n\n" +
        "Pour fermer, vise « Fermer » et appuie sur la gâchette de tir.";
    [TextArea] [SerializeField] private string missingPaperMessage = "Insérez une feuille dans l'imprimante.";
    [TextArea] [SerializeField] private string missingCheckpointMessage = "La feuille doit d'abord terminer le parcours.";
    [TextArea] [SerializeField] private string printingMessage = "Impression en cours...";
    [TextArea] [SerializeField] private string readyMessage = "Impression terminée. Appuyez sur l'imprimante pour lire l'indice.";

    private Coroutine printingRoutine;
    private float remainingPrintTime;
    private bool isPrinting;
    private bool clueAvailable;
    private bool clueRevealed;

    public bool IsPrinting => isPrinting;
    public bool ClueAvailable => clueAvailable;

    private void OnEnable()
    {
        if (paperSocket != null)
        {
            paperSocket.RequiredCheckpointIndex = requiredCheckpointIndex;
            paperSocket.selectEntered.AddListener(OnPaperInserted);
        }
        if (printInteraction != null)
            printInteraction.selectEntered.AddListener(OnPrintInteraction);

        ClosePopup();
        if (isPrinting)
            ResumePrinting();
        else
            TryStartPrinting();
    }

    private void OnDisable()
    {
        if (paperSocket != null)
            paperSocket.selectEntered.RemoveListener(OnPaperInserted);
        if (printInteraction != null)
            printInteraction.selectEntered.RemoveListener(OnPrintInteraction);

        if (printingRoutine != null)
        {
            StopCoroutine(printingRoutine);
            printingRoutine = null;
        }

        ClosePopup();
    }

    private void OnPaperInserted(SelectEnterEventArgs args)
    {
        TryStartPrinting();
    }

    private void OnPrintInteraction(SelectEnterEventArgs args)
    {
        Interact();
    }

    /// <summary>Peut aussi être relié au On Click d'un bouton UI.</summary>
    public void Interact()
    {
        if (!isActiveAndEnabled)
            return;

        TryStartPrinting();
        if (clueAvailable)
        {
            if (ShowMessage(clueText) && !clueRevealed)
            {
                clueRevealed = true;
                if (gameRule != null)
                    gameRule.MarkClueRevealed();
                onClueRevealed.Invoke();
            }
        }
        else if (isPrinting)
        {
            ShowMessage(printingMessage);
        }
        else
        {
            var paper = GetInsertedPaper();
            ShowMessage(paper != null && !paper.IsResetting && !paper.IsDelivered
                ? missingCheckpointMessage
                : missingPaperMessage);
        }
    }

    public void ClosePopup()
    {
        if (popupRoot != null && popupRoot != gameObject && !transform.IsChildOf(popupRoot.transform))
            popupRoot.SetActive(false);
    }

    /// <summary>A relier a On Access Denied du keypad pour recommencer le parcours.</summary>
    public void RestartAfterAccessDenied()
    {
        if (!Application.isPlaying ||
            (gameRule != null && gameRule.IsSafeStageFinished))
            return;

        if (printingRoutine != null)
        {
            StopCoroutine(printingRoutine);
            printingRoutine = null;
        }
        remainingPrintTime = 0f;
        isPrinting = false;
        clueAvailable = false;
        clueRevealed = false;
        ClosePopup();
        if (popupText != null)
            popupText.text = string.Empty;

        if (gameRule != null)
            gameRule.RestartPaperPuzzle();
        // AcceptedPaper reste renseigne meme avant l'insertion ou apres la livraison.
        if (paperSocket != null && paperSocket.AcceptedPaper != null)
            paperSocket.AcceptedPaper.RestartFromBeginning();
    }

    private PaperPuzzleItem GetInsertedPaper()
    {
        if (paperSocket == null || !paperSocket.isActiveAndEnabled || paperSocket.AcceptedPaper == null)
            return null;

        // Une référence Inspector ne suffit pas : le socket doit réellement tenir ce papier.
        foreach (var selected in paperSocket.interactablesSelected)
        {
            if (selected.transform.GetComponentInParent<PaperPuzzleItem>() == paperSocket.AcceptedPaper)
                return paperSocket.AcceptedPaper;
        }

        return null;
    }

    private void TryStartPrinting()
    {
        if (!isActiveAndEnabled || isPrinting || clueAvailable)
            return;

        var paper = GetInsertedPaper();
        if (paper == null || paper.IsResetting || paper.IsDelivered || !paper.CanDeliver(requiredCheckpointIndex))
            return;

        if (!paper.TryDeliver(requiredCheckpointIndex))
            return;

        isPrinting = true;
        remainingPrintTime = Mathf.Max(0f, printDuration);
        if (gameRule != null)
            gameRule.MarkPaperDelivered();
        onPrintingStarted.Invoke();
        ResumePrinting();
    }

    private void ResumePrinting()
    {
        if (isActiveAndEnabled && isPrinting && printingRoutine == null)
            printingRoutine = StartCoroutine(Print());
    }

    private IEnumerator Print()
    {
        // Le premier yield évite un handle périmé lorsque la durée vaut zéro.
        yield return null;
        while (remainingPrintTime > 0f)
        {
            remainingPrintTime -= Time.deltaTime;
            yield return null;
        }

        printingRoutine = null;
        isPrinting = false;
        clueAvailable = true;
        if (popupRoot != null && popupRoot.activeSelf && popupText != null)
            popupText.text = readyMessage;
        onPrintingCompleted.Invoke();
    }

    private bool ShowMessage(string message)
    {
        if (popupRoot == null || popupText == null || popupRoot == gameObject || transform.IsChildOf(popupRoot.transform))
        {
            Debug.LogWarning("PrinterClue : reliez Popup Root à un panneau distinct et renseignez Popup Text.", this);
            return false;
        }

        popupText.text = message;
        popupRoot.SetActive(true);
        return popupText.isActiveAndEnabled && popupRoot.activeInHierarchy;
    }
}
