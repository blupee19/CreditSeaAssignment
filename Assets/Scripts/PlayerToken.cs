using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToken : MonoBehaviour
{
    // ENUMS
    public enum PlayerColor { Blue, Green }
    public enum TokenState { InBase, OnBoard, Home }

    // PUBLIC FIELDS
    [Tooltip("The color of this player's token")]
    public PlayerColor playerColor;
    [Tooltip("The path this token follows")]
    public Transform[] waypoints;
    [Tooltip("The starting position in the base")]
    public Transform baseTransform;
    [Tooltip("The UI Button used to select this specific token")]
    [SerializeField] private Button selectionButton;


    // PROPERTIES 
    public TokenState CurrentState { get; private set; } = TokenState.InBase;
    public int WaypointIndex { get; private set; } = -1; // -1 means it's in the base

    // PRIVATE FIELDS 
    private bool isMoving = false;
    private const float MOVE_SPEED = 10f;

    private void Start()
    {
        if (selectionButton != null)
        {
            selectionButton.onClick.AddListener(OnSelectionButtonClick);
            SetSelectable(false);
        }
        else
        {
            Debug.LogError($"Selection Button not assigned for {gameObject.name} in the Inspector!");
        }
    }

    public void OnSelectionButtonClick()
    {
        GameLogic.Instance.PlayerSelectedToken(this);
    }

    public void SetSelectable(bool isSelectable)
    {
        if (selectionButton != null)
        {
            selectionButton.gameObject.SetActive(isSelectable);
        }
    }

    public void Move(int steps)
    {
        if (isMoving) return;
        StartCoroutine(MoveStepsCoroutine(steps));
    }

    public void EnterBoard()
    {
        if (CurrentState != TokenState.InBase) return;
        WaypointIndex = 0;

        Vector3 targetPos = waypoints[WaypointIndex].position;
        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);

        CurrentState = TokenState.OnBoard;
        GameLogic.Instance.OnTokenMoveComplete(this);
    }

    public void ReturnToBase()
    {
        WaypointIndex = -1;

        Vector3 basePos = baseTransform.position;
        transform.position = new Vector3(basePos.x, basePos.y, transform.position.z);

        CurrentState = TokenState.InBase;
    }

    // === COROUTINES ===
    private IEnumerator MoveStepsCoroutine(int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            // The check in GameLogic already prevents overshooting, so we just move.
            WaypointIndex++;

            Vector3 targetPosition = waypoints[WaypointIndex].position;
            Vector3 moveTarget = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);

            while (Vector3.Distance(transform.position, moveTarget) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, moveTarget, MOVE_SPEED * Time.deltaTime);
                yield return null;
            }
            transform.position = moveTarget;
        }

        // After all movement is done, check if the token has landed on the final waypoint.
        if (WaypointIndex == waypoints.Length - 1)
        {
            CurrentState = TokenState.Home;
            Debug.Log($"{playerColor} token reached home!");
            // A token that is home can no longer be selected.
            SetSelectable(false);
        }

        isMoving = false;
        // Notify GameLogic that the move is complete so it can check for wins.
        GameLogic.Instance.OnTokenMoveComplete(this);
    }
}
