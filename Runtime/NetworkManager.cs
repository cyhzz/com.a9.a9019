using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Com.A9.Singleton;

namespace Com.A9.A9019
{
    public enum LogLevel
    {
        NONE,
        NORMAL,
        FORCE_ERROR
    }
    public class NetworkManager : Singleton<NetworkManager>
    {
        public Queue<IEnumerator> corontine_queue = new Queue<IEnumerator>();
        //Hierarchy
        public bool is_internet;
        public LogLevel log = LogLevel.NORMAL;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(CoroutineCoordinator());
        }
        public void SendRequest(string addr, object t, bool full_type = false, Action<string> CallBack = null, Action OnFail = null)
        {
            corontine_queue.Enqueue(Post(addr, t, full_type, CallBack, OnFail));
        }

        public static IEnumerator Post(string url, object pst, bool full_type, Action<string> OnSuccessCallBack = null, Action OnFail = null)
        {
            if (instance.is_internet)
            {
                Debug.Log("net traffic!");
                yield break;
            }
            instance.is_internet = true;

            var str = "";
            if (full_type)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                str = JsonConvert.SerializeObject(pst, settings);
            }
            else
            {
                str = JsonConvert.SerializeObject(pst);
            }

            if (instance.log == LogLevel.NORMAL)
                Debug.Log(str);
            if (instance.log == LogLevel.FORCE_ERROR)
                Debug.LogError(str);

            byte[] bt = System.Text.Encoding.UTF8.GetBytes(str);

            UnityWebRequest req = new UnityWebRequest(url, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(bt));

            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                if (instance.log == LogLevel.FORCE_ERROR || instance.log == LogLevel.NORMAL)
                {
                    Debug.LogError(req.error);
                }
                OnFail?.Invoke();
            }
            else
            {
                if (instance.log == LogLevel.NORMAL)
                    Debug.Log(req.downloadHandler.text);
                if (instance.log == LogLevel.FORCE_ERROR)
                    Debug.LogError(req.downloadHandler.text);

                OnSuccessCallBack?.Invoke(req.downloadHandler.text);
            }
            instance.is_internet = false;
        }

        IEnumerator CoroutineCoordinator()
        {
            while (true)
            {
                while (corontine_queue.Count > 0)
                    yield return StartCoroutine(corontine_queue.Dequeue());
                yield return null;
            }
        }
    }

}
