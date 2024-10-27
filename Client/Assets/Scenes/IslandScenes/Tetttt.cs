using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Tetttt : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTest()
    {
        StartCoroutine(DownloadAndInstall("https://abab01001-1318826377.cos.ap-guangzhou.myqcloud.com/APK/Demo.apk"));
    }
    
    private IEnumerator DownloadAndInstall(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                // 将 APK 数据保存到临时文件
                string tempPath = Path.Combine(Application.persistentDataPath, "测试");
                File.WriteAllBytes(tempPath, www.downloadHandler.data);

                // 调用内部安装方法
                //InstallAPKInternal(tempPath);
            }
        }
    }
}
