// #pragma warning disable 0649
//
// using UnityEngine;
//
// namespace Watermelon
// {
//     [DefaultExecutionOrder(-999)]
//     public class Initializer : MonoBehaviour
//     {
//         private static Initializer initializer;
//
//         [SerializeField] ProjectInitSettings initSettings;
//
//         public static GameObject GameObject { get; private set; }
//         public static Transform Transform { get; private set; }
//
//
//         public void Awake()
//         {
//             if (initializer != null) return;
//             initializer = this;
//             GameObject = gameObject;
//             Transform = transform;
//             DontDestroyOnLoad(gameObject);
//             initSettings.Init();
//             // GameLoading.SimpleLoad();
//             Debug.LogError("11111111");
//         }
//     }
// }
