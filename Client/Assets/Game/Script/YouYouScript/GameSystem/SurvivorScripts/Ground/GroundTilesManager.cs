using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class GroundTilesManager : MonoBehaviour
    {
        public bool Enable = false;
        public static GroundTilesManager Instance;
        public List<Transform> parentList = new List<Transform>();
        public List<GameObject> cacheList = new List<GameObject>();
        public float HideDistance = 10f;
        private void Awake()
        {
            Instance = this;

            foreach (var t in parentList)
            {
                for (int i = 0; i < t.childCount; i++)
                {
                    cacheList.Add(t.GetChild(i).gameObject);
                }
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void LateUpdate()
        {
            if (!Enable) return;
            foreach (var obj in cacheList)
            {
                Vector3 objPosition = obj.transform.position;
                Vector3 playerPosition = PlayerBehavior.Position;
                float horizontalDistance = Vector3.Distance(new Vector3(objPosition.x, 0, objPosition.z), new Vector3(playerPosition.x, 0, playerPosition.z));
                obj.gameObject.MSetActive(horizontalDistance <= HideDistance);
            }
        }
    }
}
