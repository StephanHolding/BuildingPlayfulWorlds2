using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : SceneObject, ITurnReciever
{
	public int allowedMovement;
	public float movementSpeed;

	private Coroutine movingCoroutine;

	private int originalAllowedMovement;

	//public void SetTarget(Vector2Int targetPos)
	//{
	//	MoveToTile(targetPos);
	//}

	protected override void Awake()
	{
		base.Awake();
		originalAllowedMovement = allowedMovement;
	}

	protected virtual void EndOfPathReached()
	{

	}

	protected virtual void TileReached()
	{

	}

	public void ChangeAllowedMovement(int changeBy)
	{
		allowedMovement += changeBy;
	}

	public void MoveToTile(Vector2Int target, bool useAllowedMovement = true)
	{
		if (movingCoroutine == null)
		{
			if (DungeonManager.instance.FindPath(standingOnTile, target, out List<PathfindingTile> path))
			{
				print("moving to: " + target);
				movingCoroutine = StartCoroutine(Moving(path.ToArray(), useAllowedMovement ? allowedMovement : -1));
			}
		}
	}

	private IEnumerator Moving(PathfindingTile[] path, int tileCountUntilStop)
	{
		int currentTileCount = 0;

		for (int i = 0; i < path.Length; i++)
		{
			while (transform.position != path[i].transform.position)
			{
				transform.position = Vector3.MoveTowards(transform.position, path[i].transform.position, movementSpeed * Time.deltaTime);
				yield return new WaitForEndOfFrame();
			}

			standingOnTile = path[i].placeInDictionary;
			transform.position = path[i].transform.position;
			currentTileCount++;
			TileReached();

			if (tileCountUntilStop > -1)
			{
				if (currentTileCount >= tileCountUntilStop)
				{
					break;
				}
			}
		}

		EndOfPathReached();
		movingCoroutine = null;
	}

	public virtual void OnTurnRecieved()
	{

	}

	public virtual void EndTurn()
	{

	}

	public virtual void OnTurnEnded()
	{
		ResetStatsToOriginal();
	}

	protected virtual void ResetStatsToOriginal()
	{
		allowedMovement = originalAllowedMovement;
	}
}
