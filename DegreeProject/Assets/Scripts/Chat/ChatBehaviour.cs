using UnityEngine;
using Mirror;
using TMPro;
using System;

public class ChatBehaviour : NetworkBehaviour
{
    [SerializeField] private GameObject chatUI = null;
    [SerializeField] private TMP_Text chatText = null;
    [SerializeField] private TMP_InputField inputField = null;
    private PlayerController controller = null;

    private static event Action<string> OnMessage;

    public bool isActive { get; private set; }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        chatUI.SetActive(true);

        controller = GetComponent<PlayerController>();

        isActive = false;
        inputField.gameObject.SetActive(isActive);

        OnMessage += HandleNewMessage;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !isActive)
        {
            // Don't want to close chat if we are pressing t
            if (Input.GetKeyDown(KeyCode.T) && isActive)
                return;

            ActivateChat();
        }

        // Exit chat mode
        if (Input.GetKeyDown(KeyCode.Escape) && isActive)
        {
            ActivateChat();
        }
    }

    private void ActivateChat()
    {
        isActive = !isActive;

        inputField.gameObject.SetActive(isActive);

        // while we are in "chat mode" disable movement code
        controller.DeactivateScript(!isActive);
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if (!hasAuthority)
            return;

        OnMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(string message)
    {
        chatText.text += message;
    }

    [Client]
    public void Send(string message)
    {
        if (!Input.GetKeyDown(KeyCode.Return))
            return;

        CmdSendMessage(message);
        inputField.text = string.Empty;
    }

    [Command]
    private void CmdSendMessage(string message)
    {
        RpcHandleMessage($"[{connectionToClient.connectionId}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke($"\n{message}");
    }
}
