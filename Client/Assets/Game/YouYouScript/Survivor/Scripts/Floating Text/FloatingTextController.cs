using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    public class FloatingTextController
    {
        private static FloatingTextController _instance;
        public static FloatingTextController Instance => _instance ??= new FloatingTextController();
        List<FloatingTextCase> floatingTextCases = new List<FloatingTextCase>();
        private Dictionary<int, FloatingTextCase> floatingTextLink;

        public async UniTask Initialise()
        {
            foreach (var name in new List<string>(){"Floating Text","Following Text","Unlockable Tool Floating Text"})
            {
                PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/ProjectFiles/Game/Prefabs/Floating Text/{name}.prefab");
                FloatingTextCase t = new FloatingTextCase();
                t.Name = name;
                if (name == "Floating Text")
                {
                    t.FloatingTextBehavior = obj.GetComponent<FloatingTextBehavior>();
                }
                else if (name == "Following Text")
                {
                    t.FloatingTextBehavior = obj.GetComponent<FollowingTextBehavior>();
                }   
                else
                {
                    t.FloatingTextBehavior = obj.GetComponent<UnlockableToolFloatingText>();
                }
                obj.gameObject.MSetActive(false);
                floatingTextCases.Add(t);
            }

            floatingTextLink = new Dictionary<int, FloatingTextCase>();
            for (int i = 0; i < floatingTextCases.Count; i++)
            {
                FloatingTextCase floatingText = floatingTextCases[i];
                if(string.IsNullOrEmpty(floatingText.Name))
                {
                    Debug.LogError("[Floating Text]: Floating Text initialization failed. A unique name (ID) must be provided. Please ensure the 'name' field is not empty before proceeding.");

                    continue;
                }

                if (floatingText.FloatingTextBehavior == null)
                {
                    Debug.LogError(string.Format("Floating Text ({0}) initialization failed. No Floating Text Behavior linked. Please assign a valid Floating Text Behavior before proceeding.", floatingText.Name));

                    continue;
                }

                floatingText.Init();

                floatingTextLink.Add(floatingText.Name.GetHashCode(), floatingText);
            }

            await UniTask.NextFrame();
        }

        private void OnDestroy()
        {
            if(!floatingTextCases.IsNullOrEmpty())
            {
                for (int i = 0; i < floatingTextCases.Count; i++)
                {
                    PoolManager.DestroyPool(floatingTextCases[i].FloatingTextPool);
                }
            }
        }

        public FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, Vector3 position)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), string.Empty, position, Quaternion.identity, 1.0f, Color.white);
        }

        public FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, Vector3 position)
        {
            return SpawnFloatingText(floatingTextNameHash, string.Empty, position, Quaternion.identity, 1.0f, Color.white);
        }

        public FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, Quaternion.identity, 1.0f, Color.white);
        }

        public FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position)
        {
            return SpawnFloatingText(floatingTextNameHash, text, position, Quaternion.identity, 1.0f, Color.white);
        }

        public FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, rotation, 1.0f, Color.white);
        }

        public FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position, Quaternion rotation)
        {
            return SpawnFloatingText(floatingTextNameHash, text, position, rotation, 1.0f, Color.white);
        }

        public FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation, float scaleMultiplier)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, rotation, scaleMultiplier, Color.white);
        }

        public FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position, Quaternion rotation, float scaleMultiplier)
        {
            return SpawnFloatingText(floatingTextNameHash, text, position, rotation, scaleMultiplier, Color.white);
        }

        public FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation, float scaleMultiplier, Color color)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, rotation, scaleMultiplier, color);
        }

        public FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position, Quaternion rotation, float scaleMultiplier, Color color)
        {
            if (floatingTextLink.ContainsKey(floatingTextNameHash))
            {
                FloatingTextCase floatingTextCase = floatingTextLink[floatingTextNameHash];

                GameObject floatingTextObject = floatingTextCase.FloatingTextPool.GetPooledObject();
                floatingTextObject.transform.position = position;
                floatingTextObject.transform.rotation = rotation;
                floatingTextObject.SetActive(true);

                FloatingTextBaseBehavior floatingTextBehavior = floatingTextObject.GetComponent<FloatingTextBaseBehavior>();
                floatingTextBehavior.Activate(text, scaleMultiplier, color);
                return floatingTextBehavior;
            }

            return null;
        }

        public void Unload()
        {
            List<FloatingTextCase> ca = floatingTextCases;
            for (int i = 0; i < ca.Count; i++)
            {
                ca[i].FloatingTextPool.ReturnToPoolEverything(true);
            }
        }
    }
}