using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
	public float movementSpeed;

	private Vector2Int standingOnTile;

	public virtual void Init(Vector2Int spawnPosition)
	{
		standingOnTile = spawnPosition;
	}

	protected virtual void EndOfPathReached()
	{

	}

	protected virtual void TileReached()
	{

	}

	public void MoveToTile(Vector2Int target)
	{
		PathfindingTile[] path = DungeonManager.instance.FindPath(standingOnTile, target).ToArray();
		StartCoroutine(Moving(path));
	}

	private IEnumerator Moving(PathfindingTile[] path)
	{
		for (int i = 0; i < path.Length; i++)
		{
			while(transform.position != path[i].transform.position)
			{
				transform.position = Vector3.MoveTowards(transform.position, path[i].transform.position, movementSpeed * Time.deltaTime);
				yield return new WaitForEndOfFrame();
			}

			standingOnTile = path[i].placeInDictionary;
			transform.position = path[i].transform.position;
			TileReached();
		}

		EndOfPathReached();
	}

}
