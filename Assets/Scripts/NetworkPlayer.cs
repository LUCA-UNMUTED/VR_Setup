using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField]
    private Vector2 placementArea = new Vector2(-10.0f, 10.0f);

    public override void OnNetworkSpawn() => DisableClientInput();

    private void DisableClientInput()
    {
        //if we don't have ownership over a client, we shouldn't be able to move it
        if (IsClient && !IsOwner)
        {
            var clientMoveProvider = GetComponent<NetworkMoveProvider>();
            var clientControllers = GetComponentsInChildren<ActionBasedController>();
            var clientTurnProvider = GetComponent<ActionBasedSnapTurnProvider>();
            var clientHead = GetComponentInChildren<TrackedPoseDriver>();
            var clientCamera = GetComponentInChildren<Camera>();

            clientCamera.enabled = false; 
            clientMoveProvider.enableInputActions = false;
            clientTurnProvider.enableTurnAround = false;
            clientTurnProvider.enableTurnLeftRight = false;
            clientHead.enabled = false;

            foreach (var input in clientControllers)
            {
                input.enableInputActions = false;
                input.enableInputTracking = false;
            }
        }
    }

    private void Start()
    {
        // if this is us, being owner & client, we want to position the player object
        if (IsClient && IsOwner)
        {
            transform.position = new Vector3(Random.Range(placementArea.x, placementArea.y),
                transform.position.y, Random.Range(placementArea.x, placementArea.y));
        }
    }

    public void TestDebug()
    {
        Debug.Log("got trigger");
    }

    public void OnSelectGrabbable(SelectEnterEventArgs eventArgs)
    {
        Debug.Log("trying to grab");
        if (IsClient && IsOwner)
        {
            NetworkObject networkObjectSelected = eventArgs.interactableObject.transform.GetComponent<NetworkObject>();
            Debug.Log("network object selected " + networkObjectSelected.name);
            if (networkObjectSelected != null)
                RequestGrabbableOwnershipServerRpc(OwnerClientId, networkObjectSelected);
        }
    }

    [ServerRpc]
    public void RequestGrabbableOwnershipServerRpc(ulong newOwnerClientId, NetworkObjectReference networkObjectReference)
    {
        if (networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            networkObject.ChangeOwnership(newOwnerClientId);
            Debug.Log("ownership is now owned by server?" + networkObject.IsOwnedByServer);
        }
        else
        {
            Debug.LogWarning("something went wrong getting the reference of the network object");
        }
    }
}
