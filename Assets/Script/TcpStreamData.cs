using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct TcpStreamData
{
    public string send_type; // uuid, name, connect
    public string command; // check, get, send, request, both
    public string uuid; // UUID
    public Dictionary<string, object> send_data; // 전송할 데이터
    public List<string> request_data; // 요청할 데이터
}
