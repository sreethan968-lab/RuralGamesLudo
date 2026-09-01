using RuralGames.Rules;
using System.Collections.Generic;
using UnityEngine;

namespace RuralGames.Board
{
    public class BoardManager : MonoBehaviour
    {
        [Header("Path Configuration")]
        [SerializeField] private int mainPathLength = 52;
        [SerializeField] private int homePathLength = 6;

        private readonly List<Vector2> _mainPath = new();
        private readonly List<Vector2>[] _homePaths = new List<Vector2>[4];
        private readonly HashSet<int> _safeZones = new() { 0, 8, 13, 21, 26, 34, 39, 47 };

        private void Awake()
        {
            GenerateMainPath();
            GenerateHomePaths();
        }

        public Vector2 GetPosition(int playerId, int boardIndex, TokenState state)
        {
            return state switch
            {
                TokenState.InBase => GetBasePosition(playerId),
                TokenState.OnBoard => _mainPath[boardIndex],
                TokenState.InHomePath => _homePaths[playerId][boardIndex - mainPathLength],
                TokenState.ReachedHome => _homePaths[playerId][homePathLength - 1],
                _ => Vector2.zero
            };
        }

        public bool IsSafeZone(int mainPathIndex)
        {
            return _safeZones.Contains(mainPathIndex);
        }

        public int CalculateTargetIndex(int playerId, int currentIndex, int diceValue, TokenState state)
        {
            // NEW: Handle leaving Base
            if (state == TokenState.InBase)
            {
                return 0; // Enter main path at start
            }

            if (state == TokenState.OnBoard)
            {
                int target = currentIndex + diceValue;
                int homeEntryIndex = GetHomeEntryIndex(playerId);

                if (target > homeEntryIndex)
                {
                    int homeSteps = target - homeEntryIndex;
                    if (homeSteps <= homePathLength)
                        return mainPathLength + homeSteps - 1;
                    return -1; // Overshot home
                }
                return target;
            }

            if (state == TokenState.InHomePath)
            {
                int homeIndex = currentIndex - mainPathLength;
                int targetHomeIndex = homeIndex + diceValue;
                if (targetHomeIndex < homePathLength)
                    return mainPathLength + targetHomeIndex;
                return -1; // Overshot home
            }

            return -1;
        }

        private void GenerateMainPath()
        {
            float radius = 5f;
            for (int i = 0; i < mainPathLength; i++)
            {
                float angle = (i / (float)mainPathLength) * Mathf.PI * 2;
                _mainPath.Add(new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }
        }

        private void GenerateHomePaths()
        {
            for (int p = 0; p < 4; p++)
            {
                _homePaths[p] = new List<Vector2>();
                Vector2 start = _mainPath[GetHomeEntryIndex(p)];
                Vector2 direction = GetHomeDirection(p);

                for (int i = 0; i < homePathLength; i++)
                    _homePaths[p].Add(start + direction * (i + 1));
            }
        }

        private Vector2 GetBasePosition(int playerId)
        {
            float angle = (playerId / 4f) * Mathf.PI * 2;
            return new Vector2(Mathf.Cos(angle) * 8f, Mathf.Sin(angle) * 8f);
        }

        private int GetHomeEntryIndex(int playerId)
        {
            return playerId switch
            {
                0 => 51,
                1 => 12,
                2 => 25,
                3 => 38,
                _ => 51
            };
        }

        private Vector2 GetHomeDirection(int playerId)
        {
            float angle = (playerId / 4f) * Mathf.PI * 2 + Mathf.PI;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
}