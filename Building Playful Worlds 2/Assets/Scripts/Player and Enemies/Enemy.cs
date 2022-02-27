using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Pawn
{

	private void Start()
	{
		MoveToRandomTile();
	}

	private void MoveToRandomTile()
	{
		MoveToTile(DungeonManager.instance.GetRandomWalkableTilePosition());
	}

	protected override void EndOfPathReached()
	{
		MoveToRandomTile();
	}

}
