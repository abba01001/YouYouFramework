using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(AbstractSkinDatabase), true)]
    public class AbstractSkinsProviderEditor : CustomInspector
    {
        private bool isRegistered;

        protected override void OnEnable()
        {
            base.OnEnable();

            AbstractSkinDatabase database = (AbstractSkinDatabase)target;

            EditorSkinsProvider.AddDatabase(database);

            if(SkinController.Instance.Handler != null)
            {
                isRegistered = SkinController.Instance.Handler.HasSkinsProvider(database);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(SkinController.Instance != null)
            {
                if (!isRegistered)
                {
                    GUILayout.Space(12);

                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.HelpBox("This database isn't linked to SkinsController", MessageType.Warning);

                    // if (GUILayout.Button("Add to Skins Handler"))
                    // {
                    //     SerializedObject skinsHandlerSerializedObject = new SerializedObject();
                    //
                    //     skinsHandlerSerializedObject.Update();
                    //
                    //     SerializedProperty handlerProperty = skinsHandlerSerializedObject.FindProperty("handler");
                    //
                    //     SerializedProperty providersProperty = handlerProperty.FindPropertyRelative("skinProviders");
                    //     int index = providersProperty.arraySize;
                    //
                    //     providersProperty.arraySize = index + 1;
                    //
                    //     SerializedProperty providerProperty = providersProperty.GetArrayElementAtIndex(index);
                    //     providerProperty.objectReferenceValue = target;
                    //
                    //     skinsHandlerSerializedObject.ApplyModifiedProperties();
                    //
                    //     isRegistered = true;
                    // }

                    EditorGUILayout.EndVertical();
                }
            }
        }
    }
}
