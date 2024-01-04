using UnityEditor;
using UnityEngine;

namespace StarForce.Editor
{
    public class KeystoreInfoSetter : EditorWindow
    {
        string keystoreRelativePath = "Assets/PackageTool/user.keystore";
        string keystorePassword = "FrameWork";
        string keyAlias = "key";
        string keyPassword = "FrameWork";

        [MenuItem("Tools/打包前设置Keystore信息", priority = 2)]
        static void OpenWindow()
        {
            GetWindow<KeystoreInfoSetter>("Set Keystore Info");
        }

        private void OnGUI()
        {
            GUILayout.Label("Keystore Information", EditorStyles.boldLabel);

            keystoreRelativePath = EditorGUILayout.TextField("Keystore Relative Path:", keystoreRelativePath);
            keystorePassword = EditorGUILayout.TextField("Keystore Password:", keystorePassword);
            keyAlias = EditorGUILayout.TextField("Key Alias:", keyAlias);
            keyPassword = EditorGUILayout.TextField("Key Password:", keyPassword);

            if (GUILayout.Button("Set Keystore Info"))
            {
                SetKeystoreInfo();
            }

            if (GUILayout.Button("Clear Keystore Info"))
            {
                ClearKeystoreInfo();
            }
        }

        private void SetKeystoreInfo()
        {
            PlayerSettings.Android.keystoreName = keystoreRelativePath;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = keyAlias;
            PlayerSettings.Android.keyaliasPass = keyPassword;

            Debug.Log("Keystore information set in Unity Editor:");
            Debug.Log("Keystore Path: " + PlayerSettings.Android.keystoreName);
            Debug.Log("Key Alias: " + PlayerSettings.Android.keyaliasName);
        }

        private void ClearKeystoreInfo()
        {
            PlayerSettings.Android.keystoreName = "";
            PlayerSettings.Android.keystorePass = "";
            PlayerSettings.Android.keyaliasName = "";
            PlayerSettings.Android.keyaliasPass = "";

            Debug.Log("Keystore information cleared in Unity Editor.");
        }
    }
}
