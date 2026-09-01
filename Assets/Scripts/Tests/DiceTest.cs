using UnityEngine;
using RuralGames.Dice;
using RuralGames.Token;

namespace RuralGames.Tests
{
    public class DiceTest : MonoBehaviour
    {
        [SerializeField] private DiceRoller diceRoller;
        [SerializeField] private TokenController token;

        private void Start()
        {
            if (diceRoller == null)
                diceRoller = gameObject.AddComponent<DiceRoller>();

            // When dice finishes rolling, automatically try to move the token
            diceRoller.OnDiceRolled += HandleDiceResult;

            Debug.Log("=== DiceTest Started ===");
            Debug.Log("Press SPACE to roll dice.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                diceRoller.Roll();
            }
        }

        private void HandleDiceResult(int roll)
        {
            Debug.Log($"[DiceTest] Dice rolled {roll}, attempting to move token...");
            bool moved = token.TryMove(roll);

            if (moved)
                Debug.Log($"<color=green>[DiceTest] Token moved successfully with roll {roll}</color>");
            else
                Debug.Log($"<color=red>[DiceTest] Token could not move with roll {roll}</color>");
        }
    }
}