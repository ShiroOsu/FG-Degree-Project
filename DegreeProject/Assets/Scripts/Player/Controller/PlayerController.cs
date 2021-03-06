﻿using UnityEngine;
using Mirror;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(BoxcastController))]
public class PlayerController : NetworkBehaviour
{
    enum MouseButtons
    {
        Left = 0,
        Right = 1,
    }

    public BoxcastController boxController
    {
        get;
        private set;
    }

    private Player player = null;
    private ChatBehaviour chat = null;

    private float magicNumber = 0.001f;

    private void Start()
    {
        boxController = GetComponent<BoxcastController>();
        player = GetComponent<Player>();
        chat = GetComponent<ChatBehaviour>();
    }

    private void Update()
    {
        // Only get access to controls if it is a Local player.
        if (!isLocalPlayer) return;

        Vector2 directionalInput = new Vector2(Input.GetAxisRaw(StringData.horizontal), Input.GetAxisRaw(StringData.vertical));
        player.CmdSetDirectionalInput(directionalInput);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.CmdOnJumpInputDown();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            player.CmdOnJumpInputUp();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            player.CmdOnShiftInputDown();
        }

        if (Input.GetMouseButtonDown((int)MouseButtons.Left))
        {
            player.CmdOnAttackInputDown();
        }
    }

    public void DeactivateScript(bool active)
    {
        // Make sure we are not moving in a direction
        player.CmdSetDirectionalInput(new Vector2(0f, 0f));
        enabled = active;
    }

    public void Move(Vector2 velocity)
    {
        boxController.collisionsInfo.Reset();

        if (velocity.sqrMagnitude > magicNumber)
        {
            transform.Translate(velocity);
        }
    }
}