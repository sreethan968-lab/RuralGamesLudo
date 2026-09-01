using UnityEngine;
using RuralGames.Board;

namespace RuralGames.Rules
{
    public class SafeZoneRule : IGameRule
    {
        public string RuleName => "SafeZoneRule";
        public int Priority => 1;

        private BoardManager _board;

        public SafeZoneRule(BoardManager board)
        {
            _board = board;
        }

        public bool CanEvaluate(RuleContext context)
        {
            // Only check moves on the main board (not Base, not Home)
            return context.CurrentState == TokenState.OnBoard
                && context.TargetBoardIndex >= 0
                && context.TargetBoardIndex < 52;
        }

        public bool Evaluate(RuleContext context)
        {
            if (_board.IsSafeZone(context.TargetBoardIndex))
            {
                Debug.Log($"[SafeZoneRule] Index {context.TargetBoardIndex} is a SAFE ZONE.");
            }
            return true; // Safe zones are always valid to land on
        }
    }
}