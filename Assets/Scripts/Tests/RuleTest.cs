using UnityEngine;
using RuralGames.Rules;

namespace RuralGames.Tests
{
    /// <summary>
    /// Attach this to an empty GameObject in the RuleTest scene.
    /// Press keys to simulate dice rolls and see rule evaluation in the Console.
    /// </summary>
    public class RuleTest : MonoBehaviour
    {
        [Header("Test Setup")]
        [SerializeField] private int testPlayerId = 0;
        [SerializeField] private int testTokenId = 0;

        [Header("Current Simulation State")]
        [SerializeField] private TokenState currentTokenState = TokenState.InBase;
        [SerializeField] private int currentBoardIndex = -1; // -1 = Base
        [SerializeField] private int targetBoardIndex = 0;

        private RuleManager _ruleManager;

        private void Start()
        {
            _ruleManager = gameObject.AddComponent<RuleManager>();
            Debug.Log("=== RuleTest Scene Started ===");
            Debug.Log("Press keys 1-6 to simulate dice rolls.");
            Debug.Log("Press B to toggle token between Base and OnBoard.");
            Debug.Log("Press R to run a validation with current state.");
            Debug.Log("Current state: Token is IN BASE (needs 6 to leave).");
        }

        private void Update()
        {
            // Simulate dice rolls with number keys
            for (int i = 1; i <= 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    RunTest(i);
                }
            }

            // Toggle token state for testing
            if (Input.GetKeyDown(KeyCode.B))
            {
                ToggleTokenState();
            }

            // Manual re-run with current values
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("--- Manual Re-run ---");
                // Use last dice value or default to 1
                RunTest(1);
            }
        }

        private void RunTest(int diceValue)
        {
            var context = new RuleContext(
                playerId: testPlayerId,
                tokenId: testTokenId,
                diceValue: diceValue,
                currentState: currentTokenState,
                currentBoardIndex: currentBoardIndex,
                targetBoardIndex: targetBoardIndex
            );

            Debug.Log($"<color=cyan>[TEST] Dice roll: {diceValue} | Token state: {currentTokenState}</color>");

            bool isValid = _ruleManager.IsMoveValid(context);

            if (isValid)
                Debug.Log($"<color=green>[RESULT] Move is VALID</color>\n");
            else
                Debug.Log($"<color=red>[RESULT] Move is INVALID</color>\n");
        }

        private void ToggleTokenState()
        {
            if (currentTokenState == TokenState.InBase)
            {
                currentTokenState = TokenState.OnBoard;
                currentBoardIndex = 0;
                Debug.Log("<color=yellow>[STATE] Token moved to ON BOARD (BaseExitRule no longer applies).</color>");
            }
            else
            {
                currentTokenState = TokenState.InBase;
                currentBoardIndex = -1;
                Debug.Log("<color=yellow>[STATE] Token moved to IN BASE (BaseExitRule now applies).</color>");
            }
        }
    }
}
