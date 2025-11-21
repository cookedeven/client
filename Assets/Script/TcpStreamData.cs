using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct TcpStreamData
{
    public string send_type; // uuid, name, connect
    public string command; // check, get
    public string uuid; // UUID
    public Dictionary<string, Dictionary<string, objct>> send_data; // Map<UUID, Map<DataName, Data>>
    public Dictionary<string, object> request_data; // Map<UUID, List<DataName>>
}
