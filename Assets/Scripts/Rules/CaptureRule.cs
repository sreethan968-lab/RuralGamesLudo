using UnityEngine;

namespace RuralGames.Rules
{
    public class CaptureRule : IGameRule
    {
        public string RuleName => "CaptureRule";
        public int Priority => 2;

        public bool CanEvaluate(RuleContext context)
        {
            // Only evaluate when moving on the board
            return context.CurrentState == TokenState.OnBoard
                && context.TargetBoardIndex < 52;
        }

        public bool Evaluate(RuleContext context)
        {
            // For now: log that capture check happened
            // Later: check if opponent token exists at target index
            Debug.Log($"[CaptureRule] Checking for opponents at index {context.TargetBoardIndex}");
            return true;
        }
    }
}