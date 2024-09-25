using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using YouYou;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Performs some game-specific logic after the dungeon has been generated.
	/// Must be attached to the same GameObject as the dungeon generator
	/// </summary>
	[RequireComponent(typeof(RuntimeDungeon))]
	sealed class DungeonSetup : MonoBehaviour
	{
		private PlayerUI playerUI;
		private RuntimeDungeon runtimeDungeon;
		private GameObject spawnedPlayerInstance;

		private void OnEnable()
		{
			//playerUI = FindObjectOfType<PlayerUI>();
			runtimeDungeon = GetComponent<RuntimeDungeon>();
			runtimeDungeon.Generator.OnGenerationStatusChanged += OnDungeonGenerationStatusChanged;
		}

		private void OnDisable()
		{
			runtimeDungeon.Generator.OnGenerationStatusChanged -= OnDungeonGenerationStatusChanged;
		}

		private void OnDungeonGenerationStatusChanged(DungeonGenerator generator, GenerationStatus status)
		{
			if (status != GenerationStatus.Complete) return;
			if (spawnedPlayerInstance != null) Destroy(spawnedPlayerInstance);

			var playerSpawn = generator.CurrentDungeon.MainPathTiles[0].GetComponentInChildren<PlayerSpawn>();
			Vector3 spawnPosition = playerSpawn.transform.position;
			GameEntry.Event.Dispatch(EventName.UpdatePlayerPos,spawnPosition);
			
			//playerUI.SetPlayer(spawnedPlayerInstance);
			HandleBatchObjects();
			HideableObject.RefreshHierarchies();
		}
		
		void HandleBatchObjects()
		{
			GameObject dungeon = GameObject.Find("Dungeon");
			List<GameObject> list = new List<GameObject>();
			foreach (Transform child in dungeon.GetComponentsInChildren<Transform>())
			{
				if (child.gameObject.CompareTag("Door"))
				{
					child.gameObject.SetActive(false);
					list.Add(child.gameObject);
				}
			}
			// 合并剩余的静态物体
			StaticBatchingUtility.Combine(dungeon);
			foreach (var go in list)
			{
				go.SetActive(true);
			}
		}
	}
}
