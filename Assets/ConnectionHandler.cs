using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class ConnectionHandler : NetworkBehaviour
{
    UnityEvent<string> ConnectionApprovedEvent;
    NetworkManager m_networkManager;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_InputField ipInputField;
    [SerializeField] TMP_InputField portInputField;
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            NetworkManager.ConnectionApprovalCallback = ClientApproval;
        }
    }
    public void OnConnectButtonPressed()
    {
        StartCoroutine(LoadSceneAndConnectionDataAsync());
    }
    void ClientApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        ConnectionApprovedRPC(request.Payload);
    }
    IEnumerator LoadSceneAndConnectionDataAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Lvl1", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);
        //Fuck you unity
        yield return null;
        m_networkManager = (NetworkManager)FindFirstObjectByType(typeof(NetworkManager));
        m_networkManager.NetworkConfig.ConnectionData = ToByteArray(nameInputField.text);
        m_networkManager.gameObject.GetComponent<UnityTransport>().SetConnectionData(ipInputField.text, ushort.Parse(portInputField.text));
        m_networkManager.StartClient();
    }
    public static byte[] ToByteArray(string stringToConvert)
    {
        return System.Text.Encoding.UTF8.GetBytes(stringToConvert);
    }
    public static string ToString(byte[] bytesToConvert)
    {
        return System.Text.Encoding.UTF8.GetString(bytesToConvert);
    }
    [Rpc(SendTo.Everyone)]
    public void ConnectionApprovedRPC(byte[] connectionPayload)
    {
        ConnectionApprovedEvent.Invoke(ToString(connectionPayload));
    }
}
