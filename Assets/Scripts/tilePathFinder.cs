using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class tilePathFinder : MonoBehaviour {
	private class SortByClosest : IComparer<int> { // Based off of https://forum.unity.com/threads/solved-sort-array-objects-by-distance.811056/
		private Vector2Int point;
		private Vector2Int[] values;
		private int length;
		public SortByClosest(Vector2Int center, int givenLength, Vector2Int[] givenValues) {
			point = center;
			length = givenLength;
			values = givenValues;
		}

		public int Compare(int firstIndex, int secondIndex) {
			int countOutOfBounds = (firstIndex >= length ? 1 : 0) + (secondIndex >= length ? 1 : 0);
			if (countOutOfBounds != 0) {
				if (countOutOfBounds == 1) {
					if (firstIndex < length) { // In bounds
						return -1;
					}
					if (secondIndex < length) {
						return 1;
					}
				}
				return 0;
			}
			Vector2Int first = values[firstIndex];
			Vector2Int second = values[secondIndex];
			return Vector2Int.Distance(point, first).CompareTo(Vector2Int.Distance(point, second));
		}
	}

	public Vector2 setTarget;
	public GameObject tilesObject;
	public int maxSearchDistance;
	public int maxTilesSearch;
	public int maxJumpHeight;
	public List<string> passableTiles; 

	private Hashtable passableTilesIndex = new Hashtable();
	public List<Vector2Int> activePath;
    private Tilemap tilemap;
    private Rigidbody2D rb;
	private int pathIndex;
	public int pathDelay;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()  {
        tilemap = tilesObject.GetComponent<Tilemap>();

		IndexPassables();
		activePath = FindPath(transform.position, setTarget);
    }

    void FixedUpdate() {
		if (activePath != null) {
			if (pathDelay == 1 * 50) {
				pathDelay = 0;
				pathIndex++;
				if (pathIndex == activePath.Count) {
					pathIndex = 0;
				}
				transform.position = activePath[pathIndex] + new Vector2(0.5f, 0.5f);
			}
			else {
				pathDelay++;
			}
		}
    }

	private void MoveAlongPath() {

	}

	private List<Vector2Int> FindPath(Vector3 start, Vector2 target) {
		return FindPath(new Vector2Int((int)start.x, (int)start.y), new Vector2Int((int)target.x, (int)target.y));
	}

	private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
        Hashtable processed = new Hashtable();
		List<Vector2Int> path = new List<Vector2Int>();
		if (FindPathSub(start - new Vector2Int(1, 1), target, processed, path)) return path;
		return null;
    }
    private bool FindPathSub(Vector2Int currentPosition2, Vector2Int target2, Hashtable processed, List<Vector2Int> path) {
		Vector3Int currentPosition3 = tilemap.WorldToCell(new Vector3(currentPosition2.x, currentPosition2.y));
		Vector3Int target3 = tilemap.WorldToCell(new Vector3(target2.x, target2.y));

		string key = currentPosition2.x + "," + currentPosition2.y;
        if (processed[key] != null) return false;
        processed[key] = 1;
		if (Vector3Int.Distance(currentPosition3, target3) > maxSearchDistance) return false;
		if (processed.Count > maxTilesSearch) return false;

		Vector2Int[] directions = new Vector2Int[4];
		path.Add(currentPosition2);
		if (
			currentPosition2.x == target2.x
			&& currentPosition2.y == target2.y
		) return true;


		int index = 0;
		Vector2Int direction2 = Vector2Int.left;
		if (isPassable(GetTileName(currentPosition3 + (Vector3Int)direction2))) {
			directions[index] = currentPosition2 + direction2;
			index++;
		}
		direction2 = Vector2Int.right;
		if (isPassable(GetTileName(currentPosition3 + (Vector3Int)direction2))) {
			directions[index] = currentPosition2 + direction2;
			index++;
		}

		if (isPassable(GetTileName(currentPosition3 + Vector3Int.down))) { // Can just fall
			directions[index] = currentPosition2;
			index++;
		}
		else {
			direction2 = Vector2Int.up;
			if (isPassable(GetTileName(currentPosition3 + (Vector3Int)direction2))) {
				int i = 1;
				while (isPassable(GetTileName(currentPosition3 + (Vector3Int)direction2))) {
					direction2 += Vector2Int.up;
					i++;
					if (i > maxJumpHeight) break;
				}

				directions[index] = currentPosition2 + direction2;
				index++;
			}
		}


		if (index == 0) return false;
        SortByClosest comparer = new SortByClosest(target2, index, directions);
		int[] indexes = Enumerable.Range(0, directions.Length).ToArray();
        Array.Sort(indexes, comparer);

		// Blank vectors are still in here but are all at the end
		for (int i = 0; i < index; i++) {
			int originalIndex = indexes[i];
			Vector2Int currentDirection = directions[originalIndex];
			if (isPassable(GetTileName(currentPosition3 + Vector3Int.down))) {
				currentDirection += Vector2Int.down;
			}

			List<Vector2Int> newPath = new List<Vector2Int>();
			bool output = FindPathSub(currentDirection, target2, processed, newPath);
			if (output) {
				foreach (Vector2Int item in newPath) {
					path.Add(item);
				}
				return true;
			}
			if (processed.Count > maxTilesSearch) return false;
		}
		return false;
    }
    private string GetTileName(Vector3Int tileCoord) {
        TileBase tile = tilemap.GetTile(tileCoord);
        if (tile) return tile.name;
        return null;
    }

	private void IndexPassables() {
		foreach (string i in passableTiles) {
			passableTilesIndex[i] = "1";
		}
	}
	private bool isPassable(string tileName) {
		if (tileName == null) return true;
		if ((string)passableTilesIndex[tileName] == "1") return true;
		return false;
	}
}
