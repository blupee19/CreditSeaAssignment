using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
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

    private void OnEnable() { Dice.OnDiceRolled += HandleDiceRoll; }
    private void OnDisable() { Dice.OnDiceRolled -= HandleDiceRoll; }

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

        if (grantAnotherTurn)
        {
            grantAnotherTurn = false;
            Debug.Log("Rolled a 6! Player gets another turn.");
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
        Debug.Log($"Rolled a {currentDiceRoll}");

        PlayerToken[] activePlayerTokens = isBlueTurn ? blueTokens : greenTokens;

        if (currentDiceRoll == 6 && activePlayerTokens.All(t => t.CurrentState == PlayerToken.TokenState.InBase))
        {
            MoveToken(activePlayerTokens[0]);
            return;
        }

        choosableTokens = GetMovableTokens(activePlayerTokens);

        if (choosableTokens.Count == 0)
        {
            Debug.Log("No movable tokens. Switching turn.");
            grantAnotherTurn = false;
            Invoke(nameof(EndTurn), 1f);
        }
        else if (choosableTokens.Count == 1)
        {
            MoveToken(choosableTokens[0]);
        }
        else
        {
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
        foreach (var t in choosableTokens) { t.SetSelectable(false); }
        MoveToken(token);
    }

    private void MoveToken(PlayerToken token)
    {
        if (token.CurrentState == PlayerToken.TokenState.InBase && currentDiceRoll == 6)
        {
            grantAnotherTurn = false;
            token.EnterBoard();
        }
        else if (token.CurrentState == PlayerToken.TokenState.OnBoard)
        {
            token.Move(currentDiceRoll);
        }
    }

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
                if (Vector3.Distance(targetWaypoint.position, opponentWaypoint.position) < 0.1f)
                {
                    opponentToken.ReturnToBase();
                    break;
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
    private List<PlayerToken> GetMovableTokens(PlayerToken[] tokens)
    {
        var movableTokens = new List<PlayerToken>();
        foreach (var token in tokens)
        {
            // Case 1: Token is in the base and player rolled a 6
            if (token.CurrentState == PlayerToken.TokenState.InBase && currentDiceRoll == 6)
            {
                movableTokens.Add(token);
            }
            // Case 2: Token is already on the board
            else if (token.CurrentState == PlayerToken.TokenState.OnBoard)
            {
                // Calculate how many steps are needed to reach the final waypoint
                int stepsToHome = token.waypoints.Length - 1 - token.WaypointIndex;

                // Only consider this token movable if the dice roll is less than or equal to the steps needed
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
        SceneManager.LoadScene(0);
    }
}
