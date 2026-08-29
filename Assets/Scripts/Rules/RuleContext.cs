using UnityEngine;

namespace RuralGames.Rules
{
    /// <summary>
    /// Immutable context passed to rules when evaluating a move.
    /// Add fields here as the game grows (e.g., board state, opponent positions).
    /// </summary>
    public readonly struct RuleContext
    {
        public readonly int PlayerId;
        public readonly int TokenId;
        public readonly int DiceValue;
        public readonly TokenState CurrentState;
        public readonly int CurrentBoardIndex;   // -1 = in Base, 0-51 = main path, 52-56 = home path
        public readonly int TargetBoardIndex;

        public RuleContext(
            int playerId,
            int tokenId,
            int diceValue,
            TokenState currentState,
            int currentBoardIndex,
            int targetBoardIndex)
        {
            PlayerId = playerId;
            TokenId = tokenId;
            DiceValue = diceValue;
            CurrentState = currentState;
            CurrentBoardIndex = currentBoardIndex;
            TargetBoardIndex = targetBoardIndex;
        }
    }

    public enum TokenState
    {
        InBase,      // Token has not entered the board yet
        OnBoard,     // Token is on the main path
        InHomePath,  // Token is on the colored home stretch
        ReachedHome  // Token has finished
    }
}
