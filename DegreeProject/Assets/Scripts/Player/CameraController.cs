using UnityEngine;

public class CameraController : MonoBehaviour
{
    private void Update()
    {
        // If the camera's parent is not the local player, disable that camera and audio listener.
        if (!this.transform.parent.GetComponent<Player>().isLocalPlayer)
        {
            gameObject.GetComponent<Camera>().enabled = false;
            gameObject.GetComponent<AudioListener>().enabled = false;
        }
    }
}
