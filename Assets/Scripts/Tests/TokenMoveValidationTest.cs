using UnityEngine;
using System.Collections.Generic;
using RuralGames.Token;
using RuralGames.Board;
using RuralGames.Rules;

namespace RuralGames.Tests
{
    public class TokenMoveValidationTest : MonoBehaviour
    {
        private int _passed = 0;
        private int _failed = 0;
        private GameObject _tokenContainer;

        void Start()
        {
            Debug.Log("=== Token Move Validation Tests ===\n");

            var board = UnityEngine.Object.FindAnyObjectByType<BoardManager>();
            if (board == null)
            {
                var boardGo = new GameObject("BoardSetup");
                board = boardGo.AddComponent<BoardManager>();
            }

            var rules = UnityEngine.Object.FindAnyObjectByType<RuleManager>();
            if (rules == null)
            {
                rules = gameObject.AddComponent<RuleManager>();
            }

            _tokenContainer = new GameObject("TestTokens");

            Test_AllBase_Roll3();
            Test_AllBase_Roll6();
            Test_MixedBaseAndBoard_Roll3();
            Test_MixedBaseAndBoard_Roll6();
            Test_OnBoard51_Roll1_HomeEntry();
            Test_HomePath52_Roll6_Overshoot();
            Test_SafeZone_Roll2();

            Debug.Log($"\n=== Results: {_passed} passed, {_failed} failed ===");

            Destroy(_tokenContainer);
        }

        private List<TokenController> CreateTokens(int baseCount, int boardCount, int boardStartIndex = 0)
        {
            var tokens = new List<TokenController>();

            for (int i = 0; i < baseCount; i++)
            {
                var go = new GameObject($"TestToken_Base_{i}");
                go.transform.SetParent(_tokenContainer.transform);
                var t = go.AddComponent<TokenController>();
                t.SetState(TokenState.InBase, -1);
                tokens.Add(t);
            }

            for (int i = 0; i < boardCount; i++)
            {
                var go = new GameObject($"TestToken_Board_{i}");
                go.transform.SetParent(_tokenContainer.transform);
                var t = go.AddComponent<TokenController>();
                t.SetState(TokenState.OnBoard, boardStartIndex);
                tokens.Add(t);
            }

            return tokens;
        }

        private void Test_AllBase_Roll3()
        {
            var tokens = CreateTokens(4, 0);
            int validCount = CountValid(tokens, 3);
            Assert("All Base + Roll 3 = 0 valid", validCount == 0);
        }

        private void Test_AllBase_Roll6()
        {
            var tokens = CreateTokens(4, 0);
            int validCount = CountValid(tokens, 6);
            Assert("All Base + Roll 6 = 4 valid", validCount == 4);
        }

        private void Test_MixedBaseAndBoard_Roll3()
        {
            var tokens = CreateTokens(2, 2, 10);
            int validCount = CountValid(tokens, 3);
            Assert("2 Base + 2 OnBoard + Roll 3 = 2 valid", validCount == 2);
        }

        private void Test_MixedBaseAndBoard_Roll6()
        {
            var tokens = CreateTokens(2, 2, 10);
            int validCount = CountValid(tokens, 6);
            Assert("2 Base + 2 OnBoard + Roll 6 = 4 valid", validCount == 4);
        }

        private void Test_OnBoard51_Roll1_HomeEntry()
        {
            var tokens = CreateTokens(0, 1, 51);
            int validCount = CountValid(tokens, 1);
            Assert("OnBoard idx 51 + Roll 1 = 1 valid (home entry)", validCount == 1);
        }

        private void Test_HomePath52_Roll6_Overshoot()
        {
            // FIX: Create with valid index 0 first, then switch to HomePath
            var tokens = CreateTokens(0, 1, 0);
            tokens[0].SetState(TokenState.InHomePath, 52);
            int validCount = CountValid(tokens, 6);
            Assert("HomePath idx 52 + Roll 6 = 0 valid (overshoot)", validCount == 0);
        }

        private void Test_SafeZone_Roll2()
        {
            var tokens = CreateTokens(0, 1, 8);
            int validCount = CountValid(tokens, 2);
            Assert("OnBoard safe zone idx 8 + Roll 2 = 1 valid", validCount == 1);
        }

        private int CountValid(List<TokenController> tokens, int roll)
        {
            int count = 0;
            foreach (var t in tokens)
                if (t.CanMove(roll)) count++;
            return count;
        }

        private void Assert(string testName, bool condition)
        {
            if (condition)
            {
                Debug.Log($"<color=green>[PASS]</color> {testName}");
                _passed++;
            }
            else
            {
                Debug.Log($"<color=red>[FAIL]</color> {testName}");
                _failed++;
            }
        }
    }
}