using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {
    public GameObject cellItemPrefab;
    Transform[] lines;
    public Tile[] items;
    public bool isTube = true;
    public bool isAuto = false;
    float angle = 10f;
    public Vector3 cellSize = new Vector3(1f, 0.96f, 1f);
    public Vector3 cellScale = new Vector3(1f, 1f, 1f);

    List<Tile> choiceList;

    Line[] lineList;

    int total = 10;

    Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
        Vector3 pos = cam.position;
        pos.y = 8f;
        if (!isTube) cam.position = pos;
        choiceList = new List<Tile>();
        IniteArena();
        isInput = true;
    }

    void IniteArena()
    {
        if (isTube) total = (int) 360 / 10;
        lines = new Transform[total];
        lineList = new Line[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            GameObject pgo = new GameObject();
            pgo.name = "Base" + (i+1).ToString("000");
            pgo.transform.parent = transform;
            GameObject go = new GameObject();
            go.transform.parent = pgo.transform;
            if (isTube) pgo.transform.localRotation = Quaternion.Euler(0f, 0f, i * angle);
            Line script = go.AddComponent<Line>();
            script.idx = i;
            Transform tf = go.transform;
            lines[i] = tf;
            tf.parent = pgo.transform;
            if (isTube) tf.localPosition = Vector3.up * 4f + Vector3.forward * (2 - (i % 2) * 0.5f) * cellSize.y;
            else tf.localPosition = (i-4.5f) * Vector3.right * cellSize.x + Vector3.forward * (2 - (i % 2) * 0.5f) * cellSize.y;
            tf.localScale = Vector3.one;
            go.name = "Line" + (i+1).ToString("000");

            script.items = new Tile[7];
            for (int j = 0; j < script.items.Length; j++)
            {
                GameObject g = Instantiate(cellItemPrefab) as GameObject;
                g.name = "Tile" + (j+1).ToString("000");
                Transform t = g.transform;
                Tile c = g.GetComponent<Tile>();
                c.height = cellSize.y;
                c.gridManager = this;
                c.SetTileType(Random.Range(0, 5) % 5);
                c.lineScript = script;
                script.items[j] = c;
                c.idx = j;
                t.parent = tf;
                t.localPosition = Vector3.forward * j * cellSize.y;
                t.localScale = cellScale;
                t.localRotation = Quaternion.identity;
            }
            script.idx = i;
            lineList[i] = script;
        }

        items = GetComponentsInChildren<Tile>();
    }

    void InitNears()
    {
        for (int i = 0; i < lineList.Length; i++)
        {
            Line ln = lineList[i];
            for (int j = 0; j < ln.items.Length; j++)
            {
                Tile c = ln.items[j];
                c.nearTiles = new List<Tile>();
                if (isTube || i + 1 < lines.Length)
                {
                    c.nearTiles.Add(lineList[(i + 1) % lineList.Length].items[j]);
                    if (i % 2 == 1 && j - 1 >= 0)
                        c.nearTiles.Add(lineList[(i + 1) % lineList.Length].items[j - 1]);
                    if (i % 2 == 0 && j + 1 < ln.items.Length)
                        c.nearTiles.Add(lineList[(i + 1) % lineList.Length].items[j + 1]);
                }
                if (isTube || i - 1 >= 0)
                {
                    c.nearTiles.Add(lineList[(i + lineList.Length - 1) % lineList.Length].items[j]);
                    if (i % 2 == 1 && j - 1 >= 0)
                        c.nearTiles.Add(lineList[(i + lineList.Length - 1) % lineList.Length].items[j - 1]);
                    if (i % 2 == 0 && j + 1 < ln.items.Length)
                        c.nearTiles.Add(lineList[(i + lineList.Length - 1) % lineList.Length].items[j + 1]);
                }
                if (j - 1 >= 0)
                    c.nearTiles.Add(lineList[i].items[j - 1]);
                if (j + 1 < ln.items.Length)
                    c.nearTiles.Add(lineList[i].items[j + 1]);
            }
        }
    }


    public void ChoiceCell(Tile cell, int x, int y, int type, bool isMove)
    {
        if (oldX == x && oldY == y) return;
        if ((oldType != -1 && oldType != type) || choiceList.Contains(cell))
        {
            DoneDrag();
            cell.UnSetChoice();
            return;
        }
        if (isMove) return;
        choiceList.Add(cell);
        cell.isMove = true;
        cell.SetChoice();
        oldX = x;
        oldY = y;
        oldType = type;
    }

    bool isDrag = false, isInput = false;
    int oldX = -1, oldY = -1, oldType = -1;

    void DoneDrag()
    {
        choiceList.Clear();
        isDrag = false;
        oldX = -1;
        oldY = -1;
        oldType = -1;
        foreach (Transform item in lines)
            item.SendMessage("SortCells", SendMessageOptions.DontRequireReceiver);

        isInput = false;
        StartCoroutine(DelayAction(1f, () =>
        {
            isInput = true;
        }));
    }

    IEnumerator DelayAction(float dTime, System.Action callback)
    {
        yield return new WaitForSeconds(dTime);
        callback();
    }

    public void RotateLeft()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, transform.localRotation * Quaternion.Euler(0f, 0f, 10f), 4f * Time.deltaTime);
    }
    public void RotateRight()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, transform.localRotation * Quaternion.Euler(0f, 0f, -10f), 4f * Time.deltaTime);
    }

    void DoAutoClear(Tile Tile)
    {
        Stack<Tile> stack = new Stack<Tile>();
        List<Tile> searchList = new List<Tile>();

        InitNears();

        choiceList.Add(Tile);
        searchList.Add(Tile);
        foreach (Tile cb in Tile.nearTiles)
        {
            searchList.Add(cb);
            if (cb.GetTileType() == Tile.GetTileType())
            {
                stack.Push(cb);
                choiceList.Add(cb);
            }
        }
        
        while (stack.Count > 0)
        {
            foreach (Tile cb in stack.Pop().nearTiles)
            {
                if (searchList.Contains(cb)) continue;
                searchList.Add(cb);
                if (cb.GetTileType() != Tile.GetTileType()) continue;
                stack.Push(cb);
                if (!choiceList.Contains(cb)) choiceList.Add(cb);
            }
        }
        
        foreach (Tile cb in choiceList)
            cb.isMove = true;
        foreach (Transform item in lines)
            item.SendMessage("SortCells", SendMessageOptions.DontRequireReceiver);
        choiceList.Clear();
        isInput = false;
        StartCoroutine(DelayAction(1f, () =>
        {
            isInput = true;
        }));
    }

    public bool isLeftMove=false, isRightMove=false;
    void OnGUI()
    {
        Event e = Event.current;
        if (GUI.Button(new Rect(10, 10, 50, 40), "Q")) Application.LoadLevel("Game1");
        if (GUI.Button(new Rect(10, 50, 50, 40), "W")) Application.LoadLevel("Game2");
        if (GUI.Button(new Rect(10, 90, 50, 40), "E")) Application.LoadLevel("Game3");
        if (GUI.Button(new Rect(10, 130, 50, 40), "R")) Application.LoadLevel("Game4");
        Rect area = new Rect(10, 170, 50, 40);
        if (area.Contains(e.mousePosition) && e.isMouse)
        {
            if (e.type == EventType.MouseDown) isLeftMove = true;
            if (e.type == EventType.MouseUp) isLeftMove = false;
        }
        GUI.Button(area, "<");
        area = new Rect(10, 210, 50, 40);
        if (area.Contains(e.mousePosition) && e.isMouse)
        {
            if (e.type == EventType.MouseDown) isRightMove = true;
            if (e.type == EventType.MouseUp) isRightMove = false;
        }
        GUI.Button(area, ">");
    }

    void Update()
    {
        if (isLeftMove) RotateLeft();
        if (isRightMove) RotateRight();
        if (Input.GetKey(KeyCode.LeftArrow)) RotateLeft();
        if (Input.GetKey(KeyCode.RightArrow)) RotateRight();
        if (Input.GetKey(KeyCode.Q)) Application.LoadLevel("Game1");
        if (Input.GetKey(KeyCode.W)) Application.LoadLevel("Game2");
        if (Input.GetKey(KeyCode.E)) Application.LoadLevel("Game3");
        if (Input.GetKey(KeyCode.R)) Application.LoadLevel("Game4");

        if (!isInput) return;
        if (isAuto)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000f))
                {
                    Tile cb = hit.collider.GetComponent<Tile>();
                    if (cb)
                    {
                        DoAutoClear(cb);
                    }
                }
            }
            return;
        }
        if (Input.GetMouseButtonDown(0)) isDrag = true;
        if (Input.GetMouseButtonUp(0)) DoneDrag();

        if (isDrag)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Tile cb = hit.collider.GetComponent<Tile>();
                if (cb)
                {
                    cb.OnClickDown();
                }
            }
        }
    }
}
