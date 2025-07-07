using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectServer : MonoBehaviour
{
    private UnityTcpStream unityTcpStream;

    private void Start()
    {
        unityTcpStream = GameObject.Find("NetworkManager").GetComponent<UnityTcpStream>();

        if (unityTcpStream == null)
        {
            Debug.LogError("NetworkManager not found or UnityTcpStream component is missing.");
            return;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 예시: 서버에 연결 요청
            string sendType = "name";
            string command = "get";
            string uuid = System.Guid.NewGuid().ToString();
            Dictionary<string, object> sendData = new Dictionary<string, object>
            {
                { "name", "와샌주" }
            };
            List<string> requestData = new List<string>();

            string jsonString = UnityTcpStream.GenerateJsonString(sendType, command, uuid, sendData, requestData);
            unityTcpStream.ServerWrite(jsonString);
        }
    }
}
