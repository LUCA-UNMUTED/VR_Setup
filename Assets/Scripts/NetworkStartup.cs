using Unity.Netcode;
using UnityEngine;

public class NetworkStartup : MonoBehaviour
{
    void Start()
    {
        if (SceneTransitionHandler.Instance.InitializeAsHost)
        {
            Debug.Log("starting as host");
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.Log("starting as client");

            NetworkManager.Singleton.StartClient();
        }
    }
}
