namespace RuralGames.Rules
{
    /// <summary>
    /// Implement this for every Ludo rule (movement, capture, safe zones, etc.).
    /// </summary>
    public interface IGameRule
    {
        /// <summary>
        /// Human-readable name for debugging and logging.
        /// </summary>
        string RuleName { get; }

        /// <summary>
        /// Priority order. Lower = evaluated first. Use 0 for base mechanics.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Does this rule apply to the given context?
        /// e.g., BaseExitRule only applies when CurrentState == InBase.
        /// </summary>
        bool CanEvaluate(RuleContext context);

        /// <summary>
        /// Returns true if the move is ALLOWED under this rule.
        /// </summary>
        bool Evaluate(RuleContext context);
    }
}
