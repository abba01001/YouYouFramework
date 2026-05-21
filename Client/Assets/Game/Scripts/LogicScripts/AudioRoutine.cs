using System.Collections;
using System.Collections.Generic;
using GameScripts;
using UnityEngine;

namespace GameScripts
{
    public class AudioRoutine : MonoBehaviour
    {
        public AudioSource AudioSource;
        public GameObjectDespawnHandle AutoDespawnHandle;
    
        public void Init()
        {
            AudioSource = GetComponent<AudioSource>();
            AutoDespawnHandle = GetComponent<GameObjectDespawnHandle>();
        }
    }
}