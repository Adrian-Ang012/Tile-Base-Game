using UnityEngine;
using UnityEngine.InputSystem;

public class TileMover : MonoBehaviour
{
    public float moveTime = 0.15f;
    public float tileSize = 1f;

    Animator anim;

    bool isMoving;
    Vector3 startPos;
    Vector3 targetPos;
    float t;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        SnapToGrid();
    }

    void Update()
    {
        if (isMoving)
        {
            t += Time.deltaTime / moveTime;
            if (t >= 1f)
            {
                t = 1f;
                isMoving = false;
            }

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            return;
        }

        int dx = 0;
        int dy = 0;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) dy = 1;
        else if (kb.sKey.isPressed || kb.downArrowKey.isPressed) dy = -1;
        else if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) dx = -1;
        else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dx = 1;

        if (anim != null)
        {
            anim.SetFloat("name x", dx);
            anim.SetFloat("name y", dy);
        }

        if (dx == 0 && dy == 0) return;

        startPos = transform.position;
        targetPos = startPos + new Vector3(dx * tileSize, dy * tileSize, 0f);
        t = 0f;
        isMoving = true;
    }

    void SnapToGrid()
    {
        var p = transform.position;
        float x = Mathf.Round(p.x / tileSize) * tileSize;
        float y = Mathf.Round(p.y / tileSize) * tileSize;
        transform.position = new Vector3(x, y, p.z);
    }
}
