using UnityEngine;

// ============================================================
// 学习用最小版本。只有「跟着走」和「鼠标转视角」两件事。
// 挂到 Main Camera 上，把 Target 拖进来就能跑。
//
// 【重要】和 Cameramove 是同一个功能，相机上只能启用其中一个！
//         想看这一版，先把 Cameramove 组件的勾去掉。
// ============================================================
public class CameraSimple : MonoBehaviour
{
    public Transform target;          // 跟着谁
    public float height = 1.5f;       // 轴心比角色脚底高多少
    public float distance = 3f;       // 相机离轴心多远
    public float sensitivity = 2f;    // 鼠标灵敏度

    private float yaw;                // 水平角，左右看
    private float pitch;              // 俯仰角，上下看

    // 为什么是 LateUpdate 而不是 Update？
    // 因为角色在 Update 里移动，相机必须等它动完再跟，否则永远慢一帧、画面抖。
    void LateUpdate()
    {
        if (target == null) return;

        // ① 累积角度：鼠标动多少，角度就加多少
        yaw   += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch  = Mathf.Clamp(pitch, -30f, 60f);   // 别让人翻跟头

        // ② 角度 → 四元数（旋转）
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        // ③ 位置 = 轴心 + 背后方向 × 距离
        Vector3 pivot = target.position + Vector3.up * height;
        transform.position = pivot + rot * Vector3.back * distance;

        // ④ 朝向 = 同一个 rot（所以永远看向轴心）
        transform.rotation = rot;
        //Debug.Log(rot);
    }
}
