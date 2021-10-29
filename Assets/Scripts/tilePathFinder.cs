using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class tilePathFinder : MonoBehaviour
{
    private class SortByClosest : IComparer<Vector2> // Based off of https://forum.unity.com/threads/solved-sort-array-objects-by-distance.811056/
    {
        private Vector2 point;
        public SortByClosest(Vector2 center)
        {
            point = center;
        }

        public int Compare(Vector2 first, Vector2 second)
        {
			return Vector2.Distance(point, first).CompareTo(Vector2.Distance(point, second));
			/*
			return (
				(int)Mathf.Abs(point.x - first.x) - (int)Mathf.Abs(point.x - second.x)
				+ ((int)Mathf.Abs(point.y - first.y) - (int)Mathf.Abs(point.y - second.y) / 2) // Divide by 2 to discourage jumping
			);
			*/
        }
    }

    public Vector2 setTarget;
    public GameObject tilesObject;
	public int maxSearchDistance;
	public int maxJumpHeight;

	private List<Vector2> activePath;
    private Tilemap tilemap;
    private Rigidbody2D rb;
	private int pathIndex;
	private int pathDelay = 200;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        tilemap = tilesObject.GetComponent<Tilemap>();

		activePath = FindPath(transform.position, setTarget);
    }

    void Update()
    {
        if (pathDelay == 0) {
			pathDelay = 200;
			pathIndex++;
			if (pathIndex == activePath.Count) {
				pathIndex = 0;
			}
			transform.position = activePath[pathIndex] - new Vector2(0.5f, 0.5f);
		}
		else {
			pathDelay--;
		}
    }

	private void MoveAlongPath() {

	}

    private List<Vector2> FindPath(Vector2 start, Vector2 target)
    {
        Hashtable processed = new Hashtable();
		List<Vector2> path = new List<Vector2>();
		if (! FindPathSub(new Vector2((int)(start.x + 0.5f), (int)(start.y + 0.5f)), new Vector2((int)target.x, (int)target.y), processed, path, 0, 0)) return null;
		return path;
    }
    private bool FindPathSub(Vector2 currentPosition, Vector2 target, Hashtable processed, List<Vector2> path, int jump, int debug)
    {
		Debug.Log(debug);
		debug++;
		//Debug.Log(tilemap.WorldToCell(currentPosition));
		//Debug.Log(currentPosition);
		string key = currentPosition.x + "," + currentPosition.y;
        if (processed[key] != null) return false;
        processed[key] = 1;
		if (Vector3Int.Distance(tilemap.WorldToCell(currentPosition), tilemap.WorldToCell(target)) > maxSearchDistance) return false;

		Vector2[] directions = new Vector2[4];
		Vector3Int center = tilemap.WorldToCell(currentPosition);
		path.Add(new Vector2(center.x, center.y));
		if (
			(int)currentPosition.x == (int)target.x
			&& (int)currentPosition.y == (int)target.y
		) return true;

		int index = 0;

		Vector2Int direction = Vector2Int.left;
        if (GetTileName(center + (Vector3Int)direction) == null)
        {
            directions[index] = currentPosition + direction;
			index++;
        }
		direction = Vector2Int.right;
		if (GetTileName(center + (Vector3Int)direction) == null)
		{
			directions[index] = currentPosition + direction;
			index++;
		}
		direction = Vector2Int.down;
		if (GetTileName(center + (Vector3Int)direction) == null) {
			directions[index] = currentPosition + direction;
			index++;
		}
		direction = Vector2Int.up;
		int jumpIndex = -1;
		if (GetTileName(center + (Vector3Int)direction) == null && jump < maxJumpHeight) {
			directions[index] = currentPosition + direction;
			jumpIndex = index;
			index++;
		}

		if (index == 0) return false;
		Vector3Int target3 = tilemap.WorldToCell(target);
        SortByClosest comparer = new SortByClosest(new Vector2(target3.x, target3.y));
        Array.Sort(directions, comparer);

		for (int i = 0; i < index; i++) {
			Vector2 currentDirection = directions[i];
			List<Vector2> newPath = new List<Vector2>();
			bool output = FindPathSub(currentDirection, target, processed, newPath, i == jumpIndex? jump + 1 : jump, debug);
			if (output) {
				foreach (Vector2 item in newPath) {
					path.Add(item);
				}
				return true;
			}
		}
		return false;
    }
    private string GetTileName(Vector3Int tileCoord)
    {
        TileBase tile = tilemap.GetTile(tileCoord);
        if (tile) return tile.name;
        return null;
    }
}
