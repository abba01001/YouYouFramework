using System;
using UnityEngine;
using System.Collections;

namespace GameScripts
{
    public class DeviceUtil
    {
        /// <summary>
        /// 获取设备标识符
        /// </summary>
        public static string DeviceIdentifier
        {
            get { return SystemInfo.deviceUniqueIdentifier; }
        }
    
        /// <summary>
        /// 获取设备型号
        /// </summary>
        public static string DeviceModel
        {
            get
            {
    #if UNITY_IPHONE && !UNITY_EDITOR
            return UnityEngine.iOS.Device.generation.ToString();
    #else
                return SystemInfo.deviceModel;
    #endif
            }
        }
    
        public static (int milliseconds, int amplitude) HeavyPower = (50, 120);
        public static (int milliseconds, int amplitude) MediumPower = (30, 80);
        public static (int milliseconds, int amplitude) LightPower = (15, 40);
    
        public static bool vibrate = true;
    
        //有效值为：heavy/medium/light
        public static void VibrateShort(string type = "light")
        {
            if (!vibrate)
                return;
    
    #if UNITY_EDITOR || UNITY_STANDALONE
            return;
    #elif UNITY_ANDROID
            if(type == "heavy")
                VibrateWithEffect(HeavyPower.milliseconds, HeavyPower.amplitude);
            else if(type == "medium")
                VibrateWithEffect(MediumPower.milliseconds, MediumPower.amplitude);
            else if(type == "light")
                VibrateWithEffect(LightPower.milliseconds, LightPower.amplitude);
    #endif
    
        }
    
        public static void VibrateShortFromInput(int milliseconds = 15, int amplitude = 40)
        {
            if (!vibrate)
                return;
    
    #if UNITY_EDITOR || UNITY_STANDALONE
            return;
    #elif UNITY_ANDROID
                VibrateWithEffect(milliseconds, amplitude);
    #endif
    
        }
    
        public static void VibrateWithEffect(long milliseconds = 500, int amplitude = 50)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaObject vibrator =
                           currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                    {
                        if (vibrator != null)
                        {
                            using (AndroidJavaClass vibrationEffectClass =
                                   new AndroidJavaClass("android.os.VibrationEffect"))
                            {
                                AndroidJavaObject vibrationEffect =
                                    vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds,
                                        amplitude);
                                vibrator.Call("vibrate", vibrationEffect);
                            }
                        }
                    }
                }
            }
        }
    
        //复制文本
        public static void CopyToClipboard(string text)
        {
    #if UNITY_EDITOR || UNITY_STANDALONE
            GUIUtility.systemCopyBuffer = text;
    #elif UNITY_ANDROID
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
                    string clipboardService = contextClass.GetStatic<string>("CLIPBOARD_SERVICE");
                    AndroidJavaObject clipboardManager =
                        context.Call<AndroidJavaObject>("getSystemService", clipboardService);
                    if (clipboardManager != null)
                    {
                        AndroidJavaObject clipData =
                            new AndroidJavaClass("android.content.ClipData").CallStatic<AndroidJavaObject>(
                                "newPlainText", "label", text);
                        clipboardManager.Call("setPrimaryClip", clipData);
    
                        using (AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast"))
                        {
                            AndroidJavaObject toast =
                                toastClass.CallStatic<AndroidJavaObject>("makeText", context, "Copied to clipboard", 0);
                            toast.Call("show");
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
    #else
            GUIUtility.systemCopyBuffer = text;
    #endif
        }
    }
}