using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObject : MonoBehaviour
{

	[System.Serializable]
	public class SceneObjectSaveFile : SerializableData
	{
		public SceneObjectSaveFile(string instanceIdentifier) : base(instanceIdentifier)
		{
			this.instanceIdentifier = instanceIdentifier;
		}

		public Vector2Int savedPosition;
		//public string gameObjectName;
		//public string prefabName;
	}

	public Vector2Int standingOnTile;

	protected virtual void Awake()
	{
		SerializationManager.WantsToSave += SavePawn;
	}

	protected virtual void OnDestroy()
	{
		SerializationManager.WantsToSave -= SavePawn;
	}

	public virtual void Init(string objectName)
	{
		SceneObjectSaveFile saveFile = DungeonManager.useSaveFile ? SerializationManager.GetDataByInstanceIdentifier<SceneObjectSaveFile>(objectName) : null;

		if (saveFile != null)
		{
			standingOnTile = saveFile.savedPosition;
			//gameObject.name = saveFile.gameObjectName;
			//prefabName = saveFile.prefabName;
		}
		else
		{
			standingOnTile = DungeonManager.instance.GetRandomWalkableTilePosition();
			gameObject.name = objectName;
		}

		gameObject.name = objectName;
		transform.position = new Vector3(standingOnTile.x, standingOnTile.y, 0);
	}

	private void SavePawn()
	{
		SceneObjectSaveFile file = new SceneObjectSaveFile(gameObject.name);
		file.savedPosition = standingOnTile;
		//file.gameObjectName = gameObject.name;
		//file.prefabName= prefabName;
		SerializationManager.AddFileToSaveSlot(file);
	}

}
