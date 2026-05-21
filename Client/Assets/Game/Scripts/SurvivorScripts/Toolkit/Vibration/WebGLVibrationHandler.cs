#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using System.Text; // 引入 Encoding
#endif

namespace OctoberStudio.Vibration
{
    public class WebGLVibrationHandler : SimpleVibrationHandler
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _Initialize();

        [DllImport("__Internal")]
        private static extern void _Play(int duration);

        // 将 string 改为 IntPtr，绕过 HybridCLR 的限制
        [DllImport("__Internal")]
        private static extern void _RegisterPattern(IntPtr idPtr, int idLen, int[] ptr, int length);

        [DllImport("__Internal")]
        private static extern void _PlayPattern(IntPtr idPtr, int idLen);
#endif

        public WebGLVibrationHandler()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                _Initialize();
                
                // 仅用于让编译器保留引用（如果不需要立刻测试，可以先不传有效值）
                if (Time.fixedTime < 0) 
                {
                    CallRegisterPatternPlaceholder();
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
            }
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // 临时的伪调用，通过安全的方式传递指针，确保编译通过并保留代码
        private void CallRegisterPatternPlaceholder()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("test");
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                _RegisterPattern(handle.AddrOfPinnedObject(), bytes.Length, null, 0);
                _PlayPattern(handle.AddrOfPinnedObject(), bytes.Length);
            }
            finally
            {
                handle.Free();
            }
        }
#endif

        public override bool Vibrate(float duration, float intensity)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!Application.isMobilePlatform) return false;

            try 
            {
                _Play((int)(duration * 1000));
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