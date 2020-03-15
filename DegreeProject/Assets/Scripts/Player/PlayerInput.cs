using UnityEngine;
using Mirror;

public class PlayerInput : NetworkBehaviour
{
    Player player;

    private void Start()
    {
        if (!player)
        {
            player = GetComponent<Player>();
        }
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            Vector2 directionalInput = new Vector2(Input.GetAxis(StringData.horizontal), Input.GetAxis(StringData.vertical));
            player.CmdSetDirectionalInput(directionalInput);

            if (Input.GetKey(KeyCode.Escape))
            {
                player.OnEscapeInputDown();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                player.CmdOnJumpInputDown();
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                player.CmdOnJumpInputUp();
            }

            if (Input.GetMouseButtonDown(0))
            {
                player.CmdOnAttackInputDown();
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                player.CmdOnShiftInputDown();
            }
        }
    }
}
