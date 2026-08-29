using UnityEngine;

namespace RuralGames.Rules
{
    /// <summary>
    /// Day 1 Rule: A token sitting in Base can only leave if the player rolls a 6.
    /// </summary>
    public class BaseExitRule : IGameRule
    {
        public string RuleName => "BaseExitRule";
        public int Priority => 0;

        public bool CanEvaluate(RuleContext context)
        {
            // This rule only matters if the token is still in Base
            return context.CurrentState == TokenState.InBase;
        }

        public bool Evaluate(RuleContext context)
        {
            if (context.DiceValue != 6)
            {
                Debug.Log($"[BaseExitRule] Player {context.PlayerId} Token {context.TokenId}: " +
                          $"Rolled {context.DiceValue} — cannot leave Base (needs 6).");
                return false;
            }

            Debug.Log($"[BaseExitRule] Player {context.PlayerId} Token {context.TokenId}: " +
                      $"Rolled 6 — allowed to leave Base.");
            return true;
        }
    }
}
