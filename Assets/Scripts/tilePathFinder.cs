using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class tilePathFinder : MonoBehaviour
{
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
        Debug.Log(tilemap.origin.x);
        Debug.Log(tilemap.origin.y);
        Debug.Log(tilemap.origin.z);

        Debug.Log(tilemap.GetTile(new Vector3Int(-15, -6, 0)));
        Debug.Log(tilemap.GetTile(tilemap.WorldToCell(new Vector3(0, -1, 0))));
    }

    void Update()
    {
        
    }

    private void FindPath(Vector2 target)
    {
        
    }
    private void FindPathSub(Vector2 target, Hashtable processed)
    {
        string key = target.x + "," + target.y;
        if ((bool)processed[key]) return;
        processed[key] = 1;

    }
}
