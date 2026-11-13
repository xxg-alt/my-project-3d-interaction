using UnityEngine;

public class Cameramove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("跟随目标")]
    public Transform target;
    [Header("轴心")]
    public float height = 1.5f;
    public float distance = 3.0f;
    [Header("视角")]
    public float MouseSensitivity = 1.0f;
    public float MinPitch = -30.0f;
    public float MaxPitch = 60.0f;
    [Header("跟随")]
    public float FollowSpeed = 20.0f;
    [Header("防穿墙")]
    public LayerMask collisionMask = ~0;
    public float collisionRadius = 0.2f;
    public float minDistance = 0.5f;

    private static bool uiMode = false;   // 界面按钮出现时打开：暂停视角控制，放开鼠标

    // 给别的脚本（ArtTrigger）用：进入界面模式 → 放开鼠标；退出 → 重新锁定
    public static void SetUIMode(bool on)
    {
        uiMode = on;
        Cursor.lockState = on ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = on;
    }

    private float yaw;
    private float pitch;
    private int realMask;      // collisionMask 去掉角色自身所在层后的结果
    private Vector3 smoothPivot;   // 平滑后的轴心：只吸收角色移动的抖，不吸收鼠标转视角
    private float currentDist;     // 当前臂长：被挡住立刻缩，解除后缓慢伸回
    private bool inited;

    void Start()
    {
        // 初始朝向就当作轨道角：只要场景里相机是「看着角色」摆好的，yaw/pitch 天然自洽
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = Mathf.DeltaAngle(0f, e.x);

        // 把角色身上的碰撞体所在层排除掉，否则球形射线第一个撞到的就是自己
        realMask = collisionMask;
        if (target != null)
        {
            Collider[] cols = target.GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
                realMask &= ~(1 << cols[i].gameObject.layer);
        }

        LockCursor(true);
    }

    // Update is called once per frame
    void Update()
    {
        // UI 模式：不转视角、不抢鼠标，否则点左键会把光标重新锁回屏幕中心，按钮永远点不到
        if (uiMode) return;

        // 这一帧只负责「记录轨道角」，位置和朝向统一在 LateUpdate 里由轨道角推算
        yaw = Mathf.Repeat(yaw + Input.GetAxis("Mouse X") * MouseSensitivity, 360f);
        pitch -= Input.GetAxis("Mouse Y") * MouseSensitivity;
        pitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);

        if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivotRaw = target.position + Vector3.up * height;

        if (!inited)
        {
            smoothPivot = pivotRaw;
            currentDist = distance;
            inited = true;
        }

        // 1. 只平滑「轴心」，绝不平滑相机位置。
        //    相机位置 = 轴心 + dir × 臂长，和朝向共用同一个 rot —— 快速转视角时零延迟，永不脱节。
        //    （平滑相机位置的做法会让朝向先到位、站位后到位，画面中心来回漂，看起来就是抖）
        float t = 1f - Mathf.Exp(-FollowSpeed * Time.deltaTime);
        smoothPivot = Vector3.Lerp(smoothPivot, pivotRaw, t);

        Vector3 dir = rot * Vector3.back;

        // 2. 防穿墙：臂长被挡住时立刻缩短，解除后缓慢伸回。
        //    直接赋值会造成镜头猛冲——射线擦过柱子边缘时臂长在两帧之间来回跳。
        float targetDist = distance;
        if (Physics.SphereCast(smoothPivot, collisionRadius, dir, out RaycastHit hit,
                               distance + collisionRadius, realMask, QueryTriggerInteraction.Ignore))
        {
            targetDist = Mathf.Max(hit.distance - 0.05f, minDistance);
        }

        if (targetDist < currentDist)
            currentDist = targetDist;                       // 缩：立即，绝不穿墙
        else
            currentDist = Mathf.Lerp(currentDist, targetDist, 1f - Mathf.Exp(-6f * Time.deltaTime));  // 伸：慢慢来

        // 3. 位置和朝向同帧写入，永远对齐
        transform.position = smoothPivot + dir * currentDist;
        transform.rotation = rot;
    }

    void OnDisable()
    {
        LockCursor(false);
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
