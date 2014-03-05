using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class Tile : MonoBehaviour {
    public Grid gridManager;
    public GameObject effectPrefab;
    public int idx = 0;
    public float height = 1f;
    int _type = 0;
    Transform tf;
    Renderer choiceRenderer;

    public List<Tile> nearTiles;

    Color color = Color.white;

    public Line lineScript;

    public bool isMove = false;

    public Color[] colors = new Color[]{
    	Color.red
		, Color.green
		, Color.blue
		, Color.white
		, Color.yellow
		, Color.cyan
		, Color.magenta
		, Color.gray
		, Color.black
    };

    void Awake()
    {
        tf = transform;
        choiceRenderer = tf.FindChild("Spot").renderer;
        UnSetChoice();
    }

    public void SetTileType(int type)
    {
        _type = type;
        SetColor(colors[type]);
    }

    public int GetTileType()
    {
        return _type;
    }

    public void OnClickDown()
    {
        gridManager.ChoiceCell(this, lineScript.idx, idx, _type, isMove);
    }

    public void MoveTo(int seq)
    {
        if (effectPrefab)
        {
            Instantiate(effectPrefab, tf.position, Quaternion.identity);
        }
        tf.localPosition = Vector3.forward * (seq * height);
    }

    public void SetChoice()
    {
        choiceRenderer.enabled = true;
    }
    public void UnSetChoice()
    {
        choiceRenderer.enabled = false;
    }


    public void TweenMoveTo(int seq)
    {
        TweenMove(tf, tf.localPosition, Vector3.forward * (seq * height));
    }

    void TweenMove(Transform tr, Vector3 pos1, Vector3 pos2)
    {
        tr.localPosition = pos1;
        choiceRenderer.enabled = false;
        if (isMove)
        {
            renderer.enabled = false;
        }
        StartCoroutine(DelayAction(0.5f, () =>
        {
            if (isMove)
            {
                renderer.enabled = true;
                isMove = false;
            }
            TweenParms parms = new TweenParms().Prop("localPosition", pos2).Ease(EaseType.EaseOutBounce);
            HOTween.To(tr, 0.4f, parms);
        }));
    }

    public void ResetColor()
    {
        renderer.material.color = color;
    }

    public void SetColor(Color c)
    {
        renderer.material.color = c;
    }

    IEnumerator DelayAction(float dTime, System.Action callback)
    {
        yield return new WaitForSeconds(dTime);
        callback();
    }
}
