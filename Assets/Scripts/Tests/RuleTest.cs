using UnityEngine;
using RuralGames.Rules;

namespace RuralGames.Tests
{
    public class RuleTest : MonoBehaviour
    {
        private RuleManager _ruleManager;
        private int _testsPassed = 0;
        private int _testsFailed = 0;

        private void Start()
        {
            _ruleManager = gameObject.AddComponent<RuleManager>();
            Debug.Log("=== Week 1 Rule Validation Tests ===\n");

            RunAllTests();

            Debug.Log($"\n=== Results: {_testsPassed} passed, {_testsFailed} failed ===");
        }

        private void RunAllTests()
        {
            // --- BASE EXIT TESTS ---
            Test("Base + Roll 1 = INVALID",
                new RuleContext(0, 0, 1, TokenState.InBase, -1, 0),
                expectedValid: false);

            Test("Base + Roll 3 = INVALID",
                new RuleContext(0, 0, 3, TokenState.InBase, -1, 0),
                expectedValid: false);

            Test("Base + Roll 5 = INVALID",
                new RuleContext(0, 0, 5, TokenState.InBase, -1, 0),
                expectedValid: false);

            Test("Base + Roll 6 = VALID",
                new RuleContext(0, 0, 6, TokenState.InBase, -1, 0),
                expectedValid: true);

            // --- ON-BOARD NORMAL MOVE TESTS ---
            Test("OnBoard + Roll 1 = VALID",
                new RuleContext(0, 0, 1, TokenState.OnBoard, 5, 6),
                expectedValid: true);

            Test("OnBoard + Roll 3 = VALID",
                new RuleContext(0, 0, 3, TokenState.OnBoard, 10, 13),
                expectedValid: true);

            Test("OnBoard + Roll 6 = VALID",
                new RuleContext(0, 0, 6, TokenState.OnBoard, 20, 26),
                expectedValid: true);

            // --- HOME PATH TESTS ---
            Test("HomePath + Roll 2 = VALID",
                new RuleContext(0, 0, 2, TokenState.InHomePath, 53, 55),
                expectedValid: true);

            // --- REACHED HOME TESTS ---
            Test("ReachedHome + Roll 4 = VALID",
                new RuleContext(0, 0, 4, TokenState.ReachedHome, 56, 56),
                expectedValid: true);
        }

        private void Test(string testName, RuleContext context, bool expectedValid)
        {
            bool result = _ruleManager.IsMoveValid(context);
            bool passed = result == expectedValid;

            if (passed)
            {
                Debug.Log($"<color=green>[PASS]</color> {testName}");
                _testsPassed++;
            }
            else
            {
                Debug.Log($"<color=red>[FAIL]</color> {testName} | Expected: {expectedValid}, Got: {result}");
                _testsFailed++;
            }
        }
    }
}
