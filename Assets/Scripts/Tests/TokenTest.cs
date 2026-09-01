using UnityEngine;
using RuralGames.Token;

namespace RuralGames.Tests
{
    public class TokenTest : MonoBehaviour
    {
        [SerializeField] private TokenController token;

        private void Update()
        {
            for (int i = 1; i <= 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    token.TryMove(i);
                }
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                token.SetState(Rules.TokenState.InBase, -1);
                Debug.Log("[TokenTest] Token reset to Base");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                token.SetState(Rules.TokenState.OnBoard, 10);
                Debug.Log("[TokenTest] Token reset to OnBoard index 10");
            }
        }
    }
}