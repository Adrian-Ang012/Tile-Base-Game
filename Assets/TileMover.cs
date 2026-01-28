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

    [Header("Conveyor")]
    public LayerMask conveyorRightMask;
    public LayerMask conveyorUpMask;

    [Header("Teleport")]
    public LayerMask teleportAMask;
    public LayerMask teleportBMask;
    public Transform spawnA;
    public Transform spawnB;
    public float teleportCooldown = 0.25f;

    [Header("Animator Params")]
    public string paramMoveX = "moveX";
    public string paramMoveY = "moveY";
    public string paramIsMoving = "isMoving";

    Animator anim;

    bool isMoving;
    bool autoMoveLock;

    Vector2 moveDir;
    Vector3 startPos;
    Vector3 targetPos;
    float t;

    float lastTeleportTime;

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

        if (!autoMoveLock)
        {
            ReadInput();
            return;
        }

        CheckSpecialTiles();
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

        TryMove(input, true);
    }

    void TryMove(Vector2 dir, bool fromPlayerInput)
    {
        if (anim != null)
        {
            anim.SetFloat(paramMoveX, dir.x);
            anim.SetFloat(paramMoveY, dir.y);
        }

        SnapToGrid();

        Vector3 nextPos = transform.position + (Vector3)(dir * tileSize);
        Vector2 boxSize = new Vector2(tileSize * 0.9f, tileSize * 0.9f);

        if (Physics2D.OverlapBox(nextPos, boxSize, 0f, blockingMask) != null)
        {
            if (anim != null) anim.SetBool(paramIsMoving, false);
            if (fromPlayerInput) autoMoveLock = false;
            return;
        }

        moveDir = dir;
        startPos = transform.position;
        targetPos = nextPos;
        t = 0f;
        isMoving = true;

        if (fromPlayerInput) autoMoveLock = false;

        if (anim != null) anim.SetBool(paramIsMoving, true);
    }

    void MoveLerp()
    {
        t += Time.deltaTime / moveTime;
        if (t > 1f) t = 1f;

        transform.position = Vector3.Lerp(startPos, targetPos, t);
        Physics2D.SyncTransforms();

        if (t >= 1f)
        {
            transform.position = targetPos;
            Physics2D.SyncTransforms();

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

        if (Time.time - lastTeleportTime >= teleportCooldown)
        {
            if (Physics2D.OverlapBox(pos, boxSize, 0f, teleportAMask) != null)
            {
                TeleportTo(spawnB);
                return;
            }

            if (Physics2D.OverlapBox(pos, boxSize, 0f, teleportBMask) != null)
            {
                TeleportTo(spawnA);
                return;
            }
        }

        if (Physics2D.OverlapBox(pos, boxSize, 0f, conveyorRightMask) != null)
        {
            autoMoveLock = true;
            TryMove(Vector2.right, false);
            return;
        }

        if (Physics2D.OverlapBox(pos, boxSize, 0f, conveyorUpMask) != null)
        {
            autoMoveLock = true;
            TryMove(Vector2.up, false);
            return;
        }

        if (Physics2D.OverlapBox(pos, boxSize, 0f, slipperyMask) != null)
        {
            autoMoveLock = true;
            TryMove(moveDir, false);
            return;
        }

        autoMoveLock = false;
    }

    void TeleportTo(Transform dest)
    {
        if (dest == null)
        {
            autoMoveLock = false;
            return;
        }

        lastTeleportTime = Time.time;
        isMoving = false;
        autoMoveLock = false;

        transform.position = dest.position;
        Physics2D.SyncTransforms();
        SnapToGrid();

        if (anim != null) anim.SetBool(paramIsMoving, false);
    }

    void SnapToGrid()
    {
        var p = transform.position;
        float x = Mathf.Floor(p.x / tileSize) * tileSize + (tileSize * 0.5f);
        float y = Mathf.Floor(p.y / tileSize) * tileSize + (tileSize * 0.5f);
        transform.position = new Vector3(x, y, p.z);
        Physics2D.SyncTransforms();
    }
}
