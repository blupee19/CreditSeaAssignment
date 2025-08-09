using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{
    // === PUBLIC FIELDS (Inspector) ===
    [Tooltip("Assign all 6 dice face sprites in order (1-6)")]
    [SerializeField] private Sprite[] diceSides;
    [Tooltip("The button to click to roll the dice")]
    [SerializeField] private Button rollButton;

    // === EVENTS ===
    // GameLogic will listen to this event to get the dice roll result
    public static event Action<int> OnDiceRolled;

    private SpriteRenderer rend;

    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        // Listener to the button's click event
        rollButton.onClick.AddListener(RollDice);
    }

    public void RollDice()
    {
        // Disable the button immediately to prevent multiple rolls
        rollButton.interactable = false;
        StartCoroutine(RollTheDiceCoroutine());
    }

    private IEnumerator RollTheDiceCoroutine()
    {
        // Animation Part
        for (int i = 0; i <= 20; i++)
        {
            // Show a random face for the flicker effect
            rend.sprite = diceSides[UnityEngine.Random.Range(0, diceSides.Length)];
            yield return new WaitForSeconds(0.05f);
        }

        // Result Part
        int finalSideIndex = UnityEngine.Random.Range(0, diceSides.Length);
        int finalNumber = finalSideIndex + 1; // Add 1 for the actual dice number (1-6)

        rend.sprite = diceSides[finalSideIndex];

        // Notify any listening scripts (like GameLogic) about the result
        OnDiceRolled?.Invoke(finalNumber);
    }

    // Allows GameLogic to re-enable the button when it's ready
    public void SetInteractable(bool isInteractable)
    {
        rollButton.interactable = isInteractable;
    }
}
