using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    // Singleton to allow other scripts to easily access this instance
    public static GameLogic Instance { get; private set; }

    [Header("Player Tokens")]
    [SerializeField] private PlayerToken[] blueTokens;
    [SerializeField] private PlayerToken[] greenTokens;

    [Header("UI Elements")]
    [SerializeField] private Text turnIndicatorText;
    [SerializeField] private Text winnerText;
    [SerializeField] private Dice dice;
    [SerializeField] private Canvas winScreen;

    private bool isBlueTurn = true;
    private int currentDiceRoll;
    private List<PlayerToken> choosableTokens;
    private bool grantAnotherTurn = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); } else { Instance = this; }
    }

    private void OnEnable()
    {
        // Subscribe to the dice roll event
        Dice.OnDiceRolled += HandleDiceRoll;
    }
    private void OnDisable()
    {
        // Unsubscribe to prevent errors
        Dice.OnDiceRolled -= HandleDiceRoll;
    }

    private void Start()
    {
        if (winnerText != null) { winnerText.gameObject.SetActive(false); }
        if (winScreen != null) { winScreen.gameObject.SetActive(false); }
        StartTurn();
    }

    private void StartTurn()
    {
        turnIndicatorText.text = isBlueTurn ? "Blue's Turn" : "Green's Turn";
        dice.SetInteractable(true);
    }

    public void EndTurn()
    {
        if (CheckForWin()) return;

        // If a 6 was rolled (and not used to unlock a token), grant another turn
        if (grantAnotherTurn)
        {
            grantAnotherTurn = false; // Use up the extra turn
            StartTurn();
        }
        else
        {
            isBlueTurn = !isBlueTurn;
            StartTurn();
        }
    }

    private void HandleDiceRoll(int diceResult)
    {
        currentDiceRoll = diceResult;
        grantAnotherTurn = (currentDiceRoll == 6);

        PlayerToken[] activePlayerTokens = isBlueTurn ? blueTokens : greenTokens;

        // Special case: If a 6 is rolled and all tokens are in base, auto-move the first one
        if (currentDiceRoll == 6 && activePlayerTokens.All(t => t.CurrentState == PlayerToken.TokenState.InBase))
        {
            MoveToken(activePlayerTokens[0]);
            return;
        }

        choosableTokens = GetMovableTokens(activePlayerTokens);

        if (choosableTokens.Count == 0)
        {
            // No possible moves, end the turn
            grantAnotherTurn = false;
            Invoke(nameof(EndTurn), 1f);
        }
        else if (choosableTokens.Count == 1)
        {
            // Only one move is possible, so make it automatically
            MoveToken(choosableTokens[0]);
        }
        else
        {
            // Let the player choose which token to move
            turnIndicatorText.text = "Choose a token to move";
            foreach (var token in choosableTokens)
            {
                token.SetSelectable(true);
            }
        }
    }

    public void PlayerSelectedToken(PlayerToken token)
    {
        if (!choosableTokens.Contains(token)) { return; }

        // Hide selection buttons once a choice is made
        foreach (var t in choosableTokens) { t.SetSelectable(false); }

        MoveToken(token);
    }

    private void MoveToken(PlayerToken token)
    {
        if (token.CurrentState == PlayerToken.TokenState.InBase && currentDiceRoll == 6)
        {
            // Using a 6 to get out of base forfeits the extra turn
            grantAnotherTurn = false;
            token.EnterBoard();
        }
        else if (token.CurrentState == PlayerToken.TokenState.OnBoard)
        {
            token.Move(currentDiceRoll);
        }
    }

    // Called by a token after it finishes its move coroutine
    public void OnTokenMoveComplete(PlayerToken movedToken)
    {
        CheckForCapture(movedToken);
        EndTurn();
    }

    public void CheckForCapture(PlayerToken movingToken)
    {
        if (movingToken.CurrentState != PlayerToken.TokenState.OnBoard) return;

        Transform targetWaypoint = movingToken.waypoints[movingToken.WaypointIndex];
        if (targetWaypoint.CompareTag("SafeZone")) { return; }

        PlayerToken[] opponentTokens = movingToken.playerColor == PlayerToken.PlayerColor.Blue ? greenTokens : blueTokens;
        foreach (var opponentToken in opponentTokens)
        {
            if (opponentToken.CurrentState == PlayerToken.TokenState.OnBoard)
            {
                Transform opponentWaypoint = opponentToken.waypoints[opponentToken.WaypointIndex];
                // Compare waypoint positions to see if they landed on the same spot
                if (Vector3.Distance(targetWaypoint.position, opponentWaypoint.position) < 0.1f)
                {
                    opponentToken.ReturnToBase();
                    break; // Only one capture per turn
                }
            }
        }
    }

    private bool CheckForWin()
    {
        if (blueTokens.All(t => t.CurrentState == PlayerToken.TokenState.Home)) { DeclareWinner("Blue"); return true; }
        if (greenTokens.All(t => t.CurrentState == PlayerToken.TokenState.Home)) { DeclareWinner("Green"); return true; }
        return false;
    }

    private void DeclareWinner(string winner)
    {
        winnerText.text = $"Paid Match - {winner} gets ₹18 (after 10% fee)!";
        winScreen.gameObject.SetActive(true);
        winnerText.gameObject.SetActive(true);
        dice.SetInteractable(false);
    }

    // Determines which of the current player's tokens are legally allowed to move
    private List<PlayerToken> GetMovableTokens(PlayerToken[] tokens)
    {
        var movableTokens = new List<PlayerToken>();
        foreach (var token in tokens)
        {
            // Rule 1: Can move from base if a 6 is rolled
            if (token.CurrentState == PlayerToken.TokenState.InBase && currentDiceRoll == 6)
            {
                movableTokens.Add(token);
            }
            // Rule 2: Can move if on the board
            else if (token.CurrentState == PlayerToken.TokenState.OnBoard)
            {
                // But only if the move doesn't overshoot the final 'home' square
                int stepsToHome = token.waypoints.Length - 1 - token.WaypointIndex;
                if (currentDiceRoll <= stepsToHome)
                {
                    movableTokens.Add(token);
                }
            }
        }
        return movableTokens;
    }

    public void MainMenuButton()
    {
        // Load scene with build index 0
        SceneManager.LoadScene(0);
    }
}
