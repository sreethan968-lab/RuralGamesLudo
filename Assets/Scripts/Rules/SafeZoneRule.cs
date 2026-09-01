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
            // Only evaluate when token is moving ON the board (not leaving base)
            return context.CurrentState == TokenState.OnBoard
                && context.TargetBoardIndex < 52;
        }

        public bool Evaluate(RuleContext context)
        {
            if (_board.IsSafeZone(context.TargetBoardIndex))
            {
                Debug.Log($"[SafeZoneRule] Index {context.TargetBoardIndex} is a SAFE ZONE.");
                return true; // Safe zones are always valid to land on
            }
            return true; // Not a safe zone, allow other rules to handle it
        }
    }
}