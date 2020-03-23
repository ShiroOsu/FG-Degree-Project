using UnityEngine;
using Mirror;

[RequireComponent(typeof(Player))]
public class PlayerInput : NetworkBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            Vector2 directionalInput = new Vector2(Input.GetAxisRaw(StringData.horizontal), Input.GetAxisRaw(StringData.vertical));
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
