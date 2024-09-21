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
		private GameObject player;
		private PlayerUI playerUI;
		private RuntimeDungeon runtimeDungeon;
		private GameObject spawnedPlayerInstance;
		public void InitParams(object[] param)
		{
			player = param[0] as GameObject;
		}
		
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
			// We're only interested in completion events
			if (status != GenerationStatus.Complete)
				return;
			// If there's already a player instance, destroy it. We'll spawn a new one
			if (spawnedPlayerInstance != null)
				Destroy(spawnedPlayerInstance);

			// Find an object inside the start tile that's marked with the PlayerSpawn component
			var playerSpawn = generator.CurrentDungeon.MainPathTiles[0].GetComponentInChildren<PlayerSpawn>();

			Vector3 spawnPosition = playerSpawn.transform.position;

			// player = FindObjectOfType<PlayerCtrl>().gameObject;
			// player.transform.position = spawnPosition;
			// player.transform.rotation = Quaternion.identity;
			// spawnedPlayerInstance = player;
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
