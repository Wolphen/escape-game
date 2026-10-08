using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Gamerule : MonoBehaviour
{
    // Conserver la valeur 3 de Completed pour les anciennes sauvegardes de scene.
    public enum PuzzleStage { TransportPaper = 0, ReadClue = 1, OpenSafe = 2, PowerComputer = 4, Completed = 3 }

    [Tooltip("Quand active, ouvrir le coffre debloque l'epreuve des cibles du PC.")]
    [SerializeField] private bool requireComputerPuzzle;

    [Header("Progression (lecture pendant le jeu)")]
    [SerializeField] private PuzzleStage currentStage = PuzzleStage.TransportPaper;

    [Header("Evenements optionnels")]
    [SerializeField] private UnityEvent onPaperDelivered = new UnityEvent();
    [SerializeField] private UnityEvent onClueRevealed = new UnityEvent();
    [SerializeField] private UnityEvent onSafeOpened = new UnityEvent();
    [SerializeField] private UnityEvent onComputerPowered = new UnityEvent();

    public PuzzleStage CurrentStage => currentStage;
    public bool IsSafeStageFinished => currentStage == PuzzleStage.PowerComputer || currentStage == PuzzleStage.Completed;

    private void Awake()
    {
        currentStage = PuzzleStage.TransportPaper;
    }

    public void MarkPaperDelivered()
    {
        AdvanceTo(PuzzleStage.ReadClue, onPaperDelivered);
    }

    public void MarkClueRevealed()
    {
        AdvanceTo(PuzzleStage.OpenSafe, onClueRevealed);
    }

    public void MarkSafeOpened()
    {
        // Un joueur qui devine le code peut aussi debloquer l'etape suivante.
        AdvanceTo(requireComputerPuzzle ? PuzzleStage.PowerComputer : PuzzleStage.Completed, onSafeOpened);
    }

    public void MarkComputerPowered()
    {
        if (requireComputerPuzzle && currentStage == PuzzleStage.PowerComputer)
            AdvanceTo(PuzzleStage.Completed, onComputerPowered);
    }

    public void RestartPaperPuzzle()
    {
        if (!IsSafeStageFinished)
            currentStage = PuzzleStage.TransportPaper;
    }

    private void AdvanceTo(PuzzleStage stage, UnityEvent notification)
    {
        if (StageOrder(stage) <= StageOrder(currentStage))
            return;

        currentStage = stage;
        notification.Invoke();
    }

    private static int StageOrder(PuzzleStage stage) => stage == PuzzleStage.Completed ? 5 : (int)stage;
}
