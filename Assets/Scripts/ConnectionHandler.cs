using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class ConnectionHandler : NetworkBehaviour
{
    UnityEvent<ulong, string> ConnectionApprovedEvent;
    NetworkManager m_networkManager;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_InputField ipInputField;
    [SerializeField] TMP_InputField portInputField;
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            NetworkManager.ConnectionApprovalCallback = ClientApproval;
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }
    }
    public void OnConnectButtonPressed()
    {
        StartCoroutine(LoadSceneAndConnectionDataForClientAsync());
    }
    public void OnHostButtonPressed()
    {
        StartCoroutine(LoadSceneAndConnectionDataForHostAsync());
    }
    void ClientApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        ConnectionApprovedRPC(request.ClientNetworkId, request.Payload);
        response.Approved = true;
    }
    IEnumerator LoadSceneAndConnectionDataForClientAsync()
    {
        AsyncOperation asyncLoadLvl1 = SceneManager.LoadSceneAsync("Lvl1", LoadSceneMode.Additive);
        AsyncOperation asyncLoadNGO = SceneManager.LoadSceneAsync("NGO_Setup", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoadLvl1.isDone && asyncLoadNGO.isDone);
        //Fuck you unity
        yield return null;
        m_networkManager = (NetworkManager)FindFirstObjectByType(typeof(NetworkManager));
        if(nameInputField.text != string.Empty && ipInputField.text != string.Empty && portInputField.text != string.Empty)
        {
            m_networkManager.NetworkConfig.ConnectionData = ToByteArray(nameInputField.text);
            m_networkManager.gameObject.GetComponent<UnityTransport>().SetConnectionData(ipInputField.text, ushort.Parse(portInputField.text));
        }
        m_networkManager.StartClient();
    }
    IEnumerator LoadSceneAndConnectionDataForHostAsync()
    {
        AsyncOperation asyncLoadLvl1 = SceneManager.LoadSceneAsync("Lvl1", LoadSceneMode.Additive);
        AsyncOperation asyncLoadNGO = SceneManager.LoadSceneAsync("NGO_Setup", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoadLvl1.isDone && asyncLoadNGO.isDone);
        //Fuck you unity
        yield return null;
        m_networkManager = (NetworkManager)FindFirstObjectByType(typeof(NetworkManager));
        if(nameInputField.text != string.Empty && ipInputField.text != string.Empty && portInputField.text != string.Empty)
        {
            m_networkManager.NetworkConfig.ConnectionData = ToByteArray(nameInputField.text);
            m_networkManager.gameObject.GetComponent<UnityTransport>().SetConnectionData(ipInputField.text, ushort.Parse(portInputField.text));
        }
        m_networkManager.StartHost();
    }
    public static byte[] ToByteArray(string stringToConvert)
    {
        return System.Text.Encoding.UTF8.GetBytes(stringToConvert);
    }
    public static string ToString(byte[] bytesToConvert)
    {
        return System.Text.Encoding.UTF8.GetString(bytesToConvert);
    }
    [Rpc(SendTo.Server)]
    public void ConnectionApprovedRPC(ulong clientID, byte[] connectionPayload)
    {
        ConnectionApprovedEvent?.Invoke(clientID, ToString(connectionPayload));
    }
    void OnClientConnected(ulong clientConnected)
    {
        SceneManager.UnloadSceneAsync("Title Scene");
    }
}
