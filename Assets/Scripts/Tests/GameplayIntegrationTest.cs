using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using RuralGames.Core;
using RuralGames.Token;
using RuralGames.Board;
using RuralGames.Rules;

namespace RuralGames.Tests
{
    public class GameplayIntegrationTest : MonoBehaviour
    {
        private int _passed = 0;
        private int _failed = 0;
        private GameObject _testContainer;

        void Start()
        {
            Debug.Log("=== Day 3: Connected Gameplay Tests ===\n");

            // Ensure scene has required managers
            var board = UnityEngine.Object.FindAnyObjectByType<BoardManager>();
            if (board == null)
            {
                var go = new GameObject("BoardSetup");
                board = go.AddComponent<BoardManager>();
            }

            var rules = UnityEngine.Object.FindAnyObjectByType<RuleManager>();
            if (rules == null)
            {
                rules = gameObject.AddComponent<RuleManager>();
            }

            _testContainer = new GameObject("TestContainer");

            // Run tests
            Test_BasePlus1to5_Blocked();
            Test_BasePlus6_Allowed();
            Test_ActiveTokenMovement();
            Test_NoValidTokens_AllInBase();
            Test_InvalidTokenChoice_Rejected();

            Debug.Log($"\n=== Results: {_passed} passed, {_failed} failed ===");

            if (_failed > 0)
            {
                Debug.Log("<color=red>FAILURES DETECTED. See logs above. Do NOT add workarounds to RuleManager — fix the responsible system instead.</color>");
            }

            Destroy(_testContainer);
        }

        // ───────────────────────────────────────────────
        // TEST 1: Base + roll 1-5 = BLOCKED
        // Responsible system: RuleManager / BaseExitRule
        // ───────────────────────────────────────────────
        private void Test_BasePlus1to5_Blocked()
        {
            var token = CreateToken(0, TokenState.InBase, -1);

            bool canMove1 = token.CanMove(1);
            bool canMove3 = token.CanMove(3);
            bool canMove5 = token.CanMove(5);

            bool allBlocked = !canMove1 && !canMove3 && !canMove5;

            if (allBlocked)
            {
                Pass("Base + roll 1,3,5 = all BLOCKED");
            }
            else
            {
                Fail("Base + roll 1,3,5 should be BLOCKED. " +
                     $"Got: roll1={canMove1}, roll3={canMove3}, roll5={canMove5}. " +
                     "BUG LOCATION: RuleManager / BaseExitRule");
            }

            Destroy(token.gameObject);
        }

        // ───────────────────────────────────────────────
        // TEST 2: Base + roll 6 = ALLOWED
        // Responsible system: RuleManager / BaseExitRule + BoardManager
        // ───────────────────────────────────────────────
        private void Test_BasePlus6_Allowed()
        {
            var token = CreateToken(0, TokenState.InBase, -1);

            bool canMove = token.CanMove(6);

            if (canMove)
            {
                // Also verify it actually moves to index 0
                bool moved = token.TryMove(6);
                bool atIndex0 = token.CurrentBoardIndex == 0;
                bool onBoard = token.CurrentState == TokenState.OnBoard;

                if (moved && atIndex0 && onBoard)
                {
                    Pass("Base + roll 6 = ALLOWED, moves to index 0, state=OnBoard");
                }
                else
                {
                    Fail($"Base + roll 6 allowed but move failed. " +
                         $"moved={moved}, index={token.CurrentBoardIndex}, state={token.CurrentState}. " +
                         "BUG LOCATION: TokenController.ExecuteMove or BoardManager");
                }
            }
            else
            {
                Fail("Base + roll 6 should be ALLOWED but was blocked. " +
                     "BUG LOCATION: RuleManager / BaseExitRule");
            }

            Destroy(token.gameObject);
        }

        // ───────────────────────────────────────────────
        // TEST 3: Active token movement (OnBoard + roll)
        // Responsible system: TokenController + BoardManager
        // ───────────────────────────────────────────────
        private void Test_ActiveTokenMovement()
        {
            var token = CreateToken(0, TokenState.OnBoard, 5);

            bool moved = token.TryMove(3);
            bool atIndex8 = token.CurrentBoardIndex == 8;
            bool stillOnBoard = token.CurrentState == TokenState.OnBoard;

            if (moved && atIndex8 && stillOnBoard)
            {
                Pass("OnBoard idx 5 + roll 3 = moved to idx 8, state=OnBoard");
            }
            else
            {
                Fail($"Active token movement failed. " +
                     $"moved={moved}, index={token.CurrentBoardIndex} (expected 8), state={token.CurrentState}. " +
                     "BUG LOCATION: TokenController or BoardManager.CalculateTargetIndex");
            }

            Destroy(token.gameObject);
        }

        // ───────────────────────────────────────────────
        // TEST 4: No valid tokens (all in Base, roll not 6)
        // Responsible system: GameManager
        // ───────────────────────────────────────────────
        private void Test_NoValidTokens_AllInBase()
        {
            var tokens = new List<TokenController>();
            for (int i = 0; i < 4; i++)
                tokens.Add(CreateToken(0, TokenState.InBase, -1));

            // Simulate what GameManager does: filter valid tokens
            int validCount = 0;
            foreach (var t in tokens)
                if (t.CanMove(3)) validCount++;

            if (validCount == 0)
            {
                Pass("4 tokens in Base + roll 3 = 0 valid tokens (turn should auto-skip)");
            }
            else
            {
                Fail($"Expected 0 valid tokens but got {validCount}. " +
                     "BUG LOCATION: GameManager.HandleDiceResult or BaseExitRule");
            }

            foreach (var t in tokens) Destroy(t.gameObject);
        }

        // ───────────────────────────────────────────────
        // TEST 5: Invalid token choice rejected
        // Responsible system: GameManager.MoveSelectedToken
        // ───────────────────────────────────────────────
        private void Test_InvalidTokenChoice_Rejected()
        {
            var gmObj = new GameObject("TestGameManager");
            var gm = gmObj.AddComponent<GameManager>();

            // Create 4 tokens: only 1 can move (OnBoard), 3 blocked (Base + roll 3)
            var movable = CreateToken(0, TokenState.OnBoard, 10);
            var blocked1 = CreateToken(0, TokenState.InBase, -1);
            var blocked2 = CreateToken(0, TokenState.InBase, -1);
            var blocked3 = CreateToken(0, TokenState.InBase, -1);

            // Use reflection to access private _validTokens and test bounds check
            var validField = typeof(GameManager).GetField("_validTokens", BindingFlags.NonPublic | BindingFlags.Instance);
            var moveMethod = typeof(GameManager).GetMethod("MoveSelectedToken", BindingFlags.NonPublic | BindingFlags.Instance);

            // Simulate: only 1 valid token
            var validList = new List<TokenController> { movable };
            validField.SetValue(gm, validList);

            // Try to select index 5 when only 1 exists — should fail silently
            int beforeIndex = movable.CurrentBoardIndex;
            moveMethod.Invoke(gm, new object[] { 5 });

            bool didNotMove = movable.CurrentBoardIndex == beforeIndex;

            if (didNotMove)
            {
                Pass("Invalid token choice (index 5 with only 1 valid) = correctly rejected");
            }
            else
            {
                Fail("Invalid token choice was NOT rejected — token moved unexpectedly. " +
                     "BUG LOCATION: GameManager.MoveSelectedToken bounds check");
            }

            Destroy(movable.gameObject);
            Destroy(blocked1.gameObject);
            Destroy(blocked2.gameObject);
            Destroy(blocked3.gameObject);
            Destroy(gmObj);
        }

        // ───────────────────────────────────────────────
        // HELPERS
        // ───────────────────────────────────────────────
        private TokenController CreateToken(int playerId, TokenState state, int boardIndex)
        {
            var go = new GameObject($"TestToken_P{playerId}_{state}");
            go.transform.SetParent(_testContainer.transform);
            var t = go.AddComponent<TokenController>();

            // Set via reflection to bypass private fields
            typeof(TokenController).GetField("playerId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(t, playerId);
            t.SetState(state, boardIndex);
            return t;
        }

        private void Pass(string msg)
        {
            Debug.Log($"<color=green>[PASS]</color> {msg}");
            _passed++;
        }

        private void Fail(string msg)
        {
            Debug.Log($"<color=red>[FAIL]</color> {msg}");
            _failed++;
        }
    }
}