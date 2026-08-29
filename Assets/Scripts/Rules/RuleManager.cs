using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuralGames.Rules
{
    /// <summary>
    /// Central authority for move validation.
    /// Rules are evaluated in Priority order; ALL applicable rules must pass.
    /// </summary>
    public class RuleManager : MonoBehaviour
    {
        [SerializeField] private bool logResults = true;

        private readonly List<IGameRule> _rules = new();

        private void Awake()
        {
            RegisterDefaultRules();
        }

        /// <summary>
        /// Add a rule at runtime (useful for power-ups or special board events later).
        /// </summary>
        public void RegisterRule(IGameRule rule)
        {
            _rules.Add(rule);
            _rules.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        /// <summary>
        /// Remove a rule at runtime.
        /// </summary>
        public void UnregisterRule(IGameRule rule)
        {
            _rules.Remove(rule);
        }

        /// <summary>
        /// Checks whether a move is legal under all currently active rules.
        /// </summary>
        public bool IsMoveValid(RuleContext context)
        {
            var applicableRules = _rules.Where(r => r.CanEvaluate(context)).ToList();

            if (applicableRules.Count == 0)
            {
                if (logResults)
                    Debug.Log("[RuleManager] No rules applied — move allowed by default.");
                return true;
            }

            foreach (var rule in applicableRules)
            {
                bool passed = rule.Evaluate(context);
                if (!passed)
                {
                    if (logResults)
                        Debug.Log($"[RuleManager] BLOCKED by {rule.RuleName}");
                    return false;
                }
            }

            if (logResults)
                Debug.Log("[RuleManager] All applicable rules passed — move allowed.");
            return true;
        }

        /// <summary>
        /// Returns a report of which rules were checked and their results.
        /// Useful for UI feedback (e.g., "You need a 6 to leave Base!").
        /// </summary>
        public MoveValidationReport ValidateWithReport(RuleContext context)
        {
            var report = new MoveValidationReport();
            var applicableRules = _rules.Where(r => r.CanEvaluate(context)).ToList();

            foreach (var rule in applicableRules)
            {
                bool passed = rule.Evaluate(context);
                report.AddResult(rule.RuleName, passed);
                if (!passed)
                {
                    report.IsValid = false;
                    return report;
                }
            }

            report.IsValid = true;
            return report;
        }

        private void RegisterDefaultRules()
        {
            // Day 1: only the base exit rule
            RegisterRule(new BaseExitRule());

            // Future rules will be registered here:
            // RegisterRule(new SafeZoneRule());
            // RegisterRule(new CaptureRule());
            // RegisterRule(new HomeEntryRule());
            // RegisterRule(new BlockadeRule());
        }
    }

    public class MoveValidationReport
    {
        public bool IsValid { get; set; }
        public readonly List<RuleResult> Results = new();

        public void AddResult(string ruleName, bool passed)
        {
            Results.Add(new RuleResult(ruleName, passed));
        }
    }

    public readonly struct RuleResult
    {
        public readonly string RuleName;
        public readonly bool Passed;

        public RuleResult(string ruleName, bool passed)
        {
            RuleName = ruleName;
            Passed = passed;
        }
    }
}
