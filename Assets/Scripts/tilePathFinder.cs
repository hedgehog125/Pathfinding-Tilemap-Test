using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class tilePathFinder : MonoBehaviour {
	public Vector2 setTarget;
	public GameObject tilesObject;
	public int maxSearchDistance;
	public int maxAwaySearchDistance;
	public int maxTilesSearch;
	public int maxJumpHeight;
	public List<string> passableTiles; 

	private Hashtable passableTilesIndex = new Hashtable();
	private List<Vector2Int> activePath;
    private Tilemap tilemap;
    private Rigidbody2D rb;
	private int pathIndex;
	private int pathDelay;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()  {
        tilemap = tilesObject.GetComponent<Tilemap>();

		IndexPassables();
		activePath = FindPath(transform.position, setTarget);
    }

    void FixedUpdate() {
		if (activePath != null && activePath.Count != 0) {
			if (pathDelay == 10) {
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
		if (FindPathSub(start - new Vector2Int(1, 1), target, processed, path, 0.0f)) return path;
		return null;
    }
    private bool FindPathSub(Vector2Int currentPosition2, Vector2Int target2, Hashtable processed, List<Vector2Int> path, float distanceTravelledAway) {
		Vector3Int currentPosition3 = tilemap.WorldToCell(new Vector3(currentPosition2.x, currentPosition2.y));
		Vector3Int target3 = tilemap.WorldToCell(new Vector3(target2.x, target2.y));

		string key = currentPosition2.x + "," + currentPosition2.y;
        if (processed[key] != null) return false;
        processed[key] = 1;
		if (Vector3Int.Distance(currentPosition3, target3) > maxSearchDistance) return false;
		if (distanceTravelledAway > maxAwaySearchDistance) return false;
		if (processed.Count > maxTilesSearch) return false;

		Vector2Int[] directions = new Vector2Int[4];
		path.Add(currentPosition2);
		if (
			currentPosition2.x == target2.x
			&& currentPosition2.y == target2.y
		) return true;

		bool falling = isPassable(GetTileName(currentPosition3 + Vector3Int.down));
		if (falling) {
			currentPosition2 += Vector2Int.down;
			currentPosition3 += Vector3Int.down;
		}


		int index = 0;
		int jumpIndex = -1;
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

		if (falling) { // Can just fall
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
				jumpIndex = index;
				index++;
			}
		}


		if (index == 0) return false;

		float min = 0;
		int minIndex = -1;
		for (int i = 0; i < index; i++) {
			float distance = Vector2Int.Distance(target2, directions[i]);
			if (distance < min || i == 0) {
				min = distance;
				minIndex = i;
			}
		}


		List<Vector2Int>[] newPaths = new List<Vector2Int>[2];
		int pathCount = 0;
		bool[] outputs = new bool[2];

		// Shortest route in the short term
		List<Vector2Int> newPath = new List<Vector2Int>();
		outputs[0] = FindPathSub(directions[minIndex], target2, processed, newPath, GetDistanceTravelledAway(distanceTravelledAway, currentPosition2, target2, directions[minIndex]));
		if (processed.Count > maxTilesSearch) return false;
		newPaths[0] = newPath;
		pathCount++;

		// Possibly shorter overall, jumping can require getting further away initially
		if (jumpIndex != -1 && minIndex != jumpIndex) {
			newPath = new List<Vector2Int>();
			outputs[1] = FindPathSub(directions[jumpIndex], target2, processed, newPath, GetDistanceTravelledAway(distanceTravelledAway, currentPosition2, target2, directions[jumpIndex]));
			if (processed.Count > maxTilesSearch) return false;
			newPaths[1] = newPath;
			pathCount++;
		}
		
		min = 0;
		minIndex = -1;
		for (int i = 0; i < pathCount; i++) {
			int value = newPaths[i].Count;
			if (outputs[i] && value < min) {
				min = value;
				minIndex = i;
			}
		}
		if (minIndex == -1) return false;

		foreach (Vector2Int item in newPaths[minIndex]) {
			path.Add(item);
		}
		return true;
	}
	private float GetDistanceTravelledAway(float original, Vector2Int currentPosition2, Vector2Int target2, Vector2Int newPoint) {
		return original - Vector2Int.Distance(
			newPoint, target2
		).CompareTo(
			Vector2Int.Distance(
				currentPosition2, target2
			)
		);
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
