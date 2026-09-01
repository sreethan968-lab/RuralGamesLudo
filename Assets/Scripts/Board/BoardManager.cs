using UnityEngine;
using RuralGames.Board;
using RuralGames.Rules;

namespace RuralGames.Tests
{
    public class BoardTest : MonoBehaviour
    {
        private BoardManager _board;

        private void Start()
        {
            _board = gameObject.AddComponent<BoardManager>();

            Debug.Log("=== BoardManager Tests ===\n");

            TestSafeZones();
            TestTargetCalculation();
        }

        private void TestSafeZones()
        {
            Debug.Log("Safe Zone Tests:");
            Debug.Log($"Index 0 is safe: {_board.IsSafeZone(0)} (expected: True)");
            Debug.Log($"Index 5 is safe: {_board.IsSafeZone(5)} (expected: False)");
            Debug.Log($"Index 8 is safe: {_board.IsSafeZone(8)} (expected: True)");
        }

        private void TestTargetCalculation()
        {
            Debug.Log("\nTarget Calculation Tests:");

            // Player 0 on board at index 50, rolls 3 → should enter home path
            int target = _board.CalculateTargetIndex(0, 50, 3, TokenState.OnBoard);
            Debug.Log($"P0 at 50 + 3 = {target} (expected: 52, home path index 0)");

            // Player 0 on board at index 50, rolls 5 → overshoots
            target = _board.CalculateTargetIndex(0, 50, 5, TokenState.OnBoard);
            Debug.Log($"P0 at 50 + 5 = {target} (expected: -1, overshoot)");

            // Player 0 in home path at index 52, rolls 3
            target = _board.CalculateTargetIndex(0, 52, 3, TokenState.InHomePath);
            Debug.Log($"P0 home 52 + 3 = {target} (expected: 55)");
        }
    }
}