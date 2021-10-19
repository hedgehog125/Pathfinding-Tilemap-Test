using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class tilePathFinder : MonoBehaviour
{
    public struct SortableByDistance : IComparable // Based off of https://forum.unity.com/threads/solved-sort-array-objects-by-distance.811056/
    {
        private Vector3Int point;
        public SortableByDistance(Vector3Int center)
        {
            point = center;
        }

        public int Compare(SortableByDistance other)
        {
            return (int)Math.Sqrt(Math.Pow(other.point.x - point.x, 2) + Math.Pow(other.point.y - point.y, 2) + Math.Pow(other.point.z - point.z, 2));
        }
    }

    public Vector2 setTarget;
    public GameObject tilesObject;

    private Tilemap tilemap;
    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        tilemap = tilesObject.GetComponent<Tilemap>();

        FindPath(transform.position, setTarget);
    }

    void Update()
    {
        
    }

    private void FindPath(Vector2 start, Vector2 target)
    {
        Hashtable processed = new Hashtable();
        FindPathSub(start, target, processed);
    }
    private void FindPathSub(Vector2 currentPosition, Vector2 target, Hashtable processed)
    {
        string key = target.x + "," + target.y;
        if ((string)processed[key] == "1") return;
        processed[key] = 1;

        Vector3Int center = tilemap.WorldToCell((Vector3)currentPosition);
        Vector3Int[] directions = {};

        Vector3Int direction = center + Vector3Int.left;
        if (GetTileName(direction) == null)
        {
            directions[directions.Length - 1] = direction;
        }
        direction = center + Vector3Int.right;
        if (GetTileName(direction) == null)
        {
            directions[directions.Length - 1] = direction;
        }

        if (directions.Length == 0) return;
        SortableByDistance comparer = new SortableByDistance(tilemap.WorldToCell((Vector3)target));
        Array.Sort(directions, comparer);
    }
    private string GetTileName(Vector3Int tileCoord)
    {
        TileBase tile = tilemap.GetTile(tileCoord);
        if (tile) return tile.name;
        return null;
    }
}
