using RuralGames.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuralGames.Rules
{
    public class RuleManager : MonoBehaviour
    {
        [NonSerialized] private List<IGameRule> _rules = new();

        private void Awake()
        {
            _rules = new List<IGameRule>();
            RegisterDefaultRules();
            Debug.Log($"[RuleManager] Awake complete. Registered {_rules.Count} rules.");
        }

        public void RegisterRule(IGameRule rule)
        {
            if (rule == null) return;
            _rules.Add(rule);
            _rules.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            Debug.Log($"[RuleManager] Registered: {rule.RuleName}");
        }

        public bool IsMoveValid(RuleContext context)
        {
            var applicableRules = _rules.Where(r => r.CanEvaluate(context)).ToList();

            if (applicableRules.Count == 0)
            {
                Debug.Log("[RuleManager] No rules applied — move allowed by default.");
                return true;
            }

            foreach (var rule in applicableRules)
            {
                if (!rule.Evaluate(context))
                {
                    Debug.Log($"[RuleManager] BLOCKED by {rule.RuleName}");
                    return false;
                }
            }

            Debug.Log("[RuleManager] All rules passed — move allowed.");
            return true;
        }

        private void RegisterDefaultRules()
        {
            RegisterRule(new BaseExitRule());

            var board = UnityEngine.Object.FindAnyObjectByType<BoardManager>();
            if (board != null)
                RegisterRule(new SafeZoneRule(board));

            Debug.Log("[RuleManager] BaseExitRule registered.");
        }
    }
}