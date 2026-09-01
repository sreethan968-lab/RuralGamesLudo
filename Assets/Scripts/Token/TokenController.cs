using UnityEngine;
using RuralGames.Board;
using RuralGames.Rules;

namespace RuralGames.Token
{
    public class TokenController : MonoBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private int playerId = 0;
        [SerializeField] private Color playerColor = Color.red;

        [Header("State")]
        [SerializeField] private TokenState currentState = TokenState.InBase;
        [SerializeField] private int currentBoardIndex = -1;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        private BoardManager _board;
        private RuleManager _rules;
        private Vector3 _targetPosition;
        private bool _isMoving = false;

        private void Awake()
        {
            _board = Object.FindAnyObjectByType<BoardManager>();
            _rules = Object.FindAnyObjectByType<RuleManager>();

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
                sr.sprite = CreateCircleSprite();
            }
            sr.color = playerColor;
        }

        private void Update()
        {
            if (_isMoving)
            {
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
                {
                    transform.position = _targetPosition;
                    _isMoving = false;
                    Debug.Log($"[Token] Player {playerId} arrived at index {currentBoardIndex}");
                }
            }
        }

        /// <summary>
        /// Checks if this token CAN move, without actually moving it.
        /// </summary>
        public bool CanMove(int diceValue)
        {
            if (_board == null || _rules == null) return false;

            int targetIndex = _board.CalculateTargetIndex(playerId, currentBoardIndex, diceValue, currentState);
            if (targetIndex == -1) return false;

            var context = new RuleContext(playerId, 0, diceValue, currentState, currentBoardIndex, targetIndex);
            return _rules.IsMoveValid(context);
        }

        /// <summary>
        /// Validates AND executes the move.
        /// </summary>
        public bool TryMove(int diceValue)
        {
            if (!CanMove(diceValue)) return false;

            int targetIndex = _board.CalculateTargetIndex(playerId, currentBoardIndex, diceValue, currentState);
            ExecuteMove(targetIndex, diceValue);
            return true;
        }

        private void ExecuteMove(int targetIndex, int diceValue)
        {
            if (currentState == TokenState.InBase && diceValue == 6)
            {
                currentState = TokenState.OnBoard;
                currentBoardIndex = 0;
            }
            else if (targetIndex >= 52)
            {
                currentState = TokenState.InHomePath;
                currentBoardIndex = targetIndex;
            }
            else
            {
                currentState = TokenState.OnBoard;
                currentBoardIndex = targetIndex;
            }

            _targetPosition = _board.GetPosition(playerId, currentBoardIndex, currentState);
            _targetPosition.z = 0;
            _isMoving = true;

            Debug.Log($"[Token] Player {playerId} moving to index {currentBoardIndex}");
        }

        public void SetState(TokenState state, int boardIndex)
        {
            currentState = state;
            currentBoardIndex = boardIndex;
            if (_board != null)
            {
                transform.position = _board.GetPosition(playerId, boardIndex, state);
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            }
        }

        private Sprite CreateCircleSprite()
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            Vector2 center = new Vector2(32, 32);
            float radius = 30f;

            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                    pixels[y * 64 + x] = Vector2.Distance(new Vector2(x, y), center) < radius ? Color.white : Color.clear;

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }
    }
}