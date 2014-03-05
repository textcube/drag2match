using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Line : MonoBehaviour {
    public int idx = 0;
    public Tile[] items;

    void Start()
    {
    }

    public void SortCells()
    {
        List<Tile> tlist = new List<Tile>();
        int y = 0, t = 7;
        for (int i = 0; i < 7; i++)
            if (!items[i].isMove)
            {
                tlist.Add(items[i]);
                items[i].idx = y++;
            }
        for (int i = 0; i < 7; i++)
            if (items[i].isMove)
            {
                tlist.Add(items[i]);
                items[i].idx = y++;
                items[i].MoveTo(t++);
                items[i].SetTileType(Random.Range(0, 5) % 5);
            }
        items = tlist.ToArray();
        for (int i = 0; i < 7; i++) items[i].TweenMoveTo(i);
    }

    void Update()
    {
    }
}
