#if UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine;
using System;
#endif
using System.Runtime.InteropServices; // 确保在外部也引入

namespace OctoberStudio.Vibration
{
    public class IOSVibrationHandler : SimpleVibrationHandler
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _Initialize();

        [DllImport("__Internal")]
        private static extern void _Play(float duration, float intensity);

        // 彻底改用 IntPtr，让 HybridCLR 无法报错
        [DllImport("__Internal")]
        private static extern void _PlayPattern(IntPtr patternIDPtr);

        [DllImport("__Internal")]
        private static extern void _RegisterPattern(IntPtr jsonPatternDataPtr);
#endif

        public IOSVibrationHandler()
        {
#if UNITY_IOS
            try
            {
                _Initialize();
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
            }
#endif
        }

        // 外部调用的公共方法，在这里做逻辑中转
        public void PlayPattern(string patternID)
        {
#if UNITY_IOS
            if (string.IsNullOrEmpty(patternID)) return;
            IntPtr ptr = Marshal.StringToHGlobalAnsi(patternID);
            try
            {
                _PlayPattern(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr); // 释放非托管内存
            }
#endif
        }

        public void RegisterPattern(string jsonPatternData)
        {
#if UNITY_IOS
            if (string.IsNullOrEmpty(jsonPatternData)) return;
            IntPtr ptr = Marshal.StringToHGlobalAnsi(jsonPatternData);
            try
            {
                _RegisterPattern(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr); // 释放非托管内存
            }
#endif
        }

        public override bool Vibrate(float duration, float intensity)
        {
#if UNITY_IOS
            try 
            {
                _Play(duration, intensity);
                return true;
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
        }
    }
}