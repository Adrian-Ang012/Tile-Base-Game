using UnityEngine;
using UnityEngine.InputSystem;

public class TileMover : MonoBehaviour
{
    [Header("Grid Move")]
    public float moveTime = 0.15f;
    public float tileSize = 1f;

    [Header("Collision")]
    public LayerMask blockingMask;
    public LayerMask slipperyMask;
    public LayerMask conveyorMask;

    [Header("Animator Params")]
    public string paramMoveX = "moveX";
    public string paramMoveY = "moveY";
    public string paramIsMoving = "isMoving";

    Animator anim;

    bool isMoving;
    Vector2 moveDir;
    Vector3 startPos;
    Vector3 targetPos;
    float t;

    void Awake()
    {
        anim = GetComponent<Animator>();
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.gravityScale = 0;
    }

    void Start()
    {
        SnapToGrid();
    }

    void Update()
    {
        if (isMoving)
        {
            MoveLerp();
            return;
        }

        ReadInput();
    }

    void ReadInput()
    {
        Vector2 input = Vector2.zero;
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.wKey.isPressed) input = Vector2.up;
        else if (kb.sKey.isPressed) input = Vector2.down;
        else if (kb.aKey.isPressed) input = Vector2.left;
        else if (kb.dKey.isPressed) input = Vector2.right;

        if (input == Vector2.zero)
        {
            if (anim != null) anim.SetBool(paramIsMoving, false);
            return;
        }

        TryMove(input);
    }

    void TryMove(Vector2 dir)
    {
        SnapToGrid();

        Vector3 nextPos = transform.position + (Vector3)(dir * tileSize);

        Vector2 boxSize = new Vector2(tileSize * 0.9f, tileSize * 0.9f);

        if (Physics2D.OverlapBox(nextPos, boxSize, 0f, blockingMask) != null)
        {
            if (anim != null) anim.SetBool(paramIsMoving, false);
            return;
        }

        moveDir = dir;
        startPos = transform.position;
        targetPos = nextPos;
        t = 0f;
        isMoving = true;

        if (anim != null)
        {
            anim.SetFloat(paramMoveX, dir.x);
            anim.SetFloat(paramMoveY, dir.y);
            anim.SetBool(paramIsMoving, true);
        }
    }

    void MoveLerp()
    {
        t += Time.deltaTime / moveTime;
        if (t > 1f) t = 1f;

        transform.position = Vector3.Lerp(startPos, targetPos, t);

        if (t >= 1f)
        {
            transform.position = targetPos;
            SnapToGrid();

            isMoving = false;
            if (anim != null) anim.SetBool(paramIsMoving, false);

            CheckSpecialTiles();
        }
    }

    void CheckSpecialTiles()
    {
        Vector2 pos = transform.position;
        Vector2 boxSize = new Vector2(tileSize * 0.9f, tileSize * 0.9f);

        if (Physics2D.OverlapBox(pos, boxSize, 0f, conveyorMask) != null)
        {
            TryMove(moveDir);
            return;
        }

        if (Physics2D.OverlapBox(pos, boxSize, 0f, slipperyMask) != null)
        {
            TryMove(moveDir);
        }
    }

    void SnapToGrid()
    {
        var p = transform.position;
        float x = Mathf.Floor(p.x / tileSize) * tileSize + (tileSize * 0.5f);
        float y = Mathf.Floor(p.y / tileSize) * tileSize + (tileSize * 0.5f);
        transform.position = new Vector3(x, y, p.z);
    }
}

