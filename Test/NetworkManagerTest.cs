using System.Collections;
using System.Collections.Generic;
using Com.A9.A9019;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerTest : MonoBehaviour
{
    public string address;
    public InputField data;
    public string txt;

    public void Send()
    {
        NetworkManager.instance.SendRequest(address, new { text = txt }, false);
    }
}
