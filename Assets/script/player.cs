using UnityEngine;

public class player : MonoBehaviour
{


    public float speed = 5f;         // 移动速度
    public float turnSpeed = 10f;    // 转身平滑度（越大转得越快）
    public float gravity = -20f;     // 重力加速度（负数=向下）
    public float jumpHeight = 1.5f;  // 跳跃高度（米）
    public float animDamp = 0.1f;      // 动画过渡平滑度（越大越快）
    private CharacterController controller;
    private Animator animator;
    private Vector3 yvelocity;        // 只存纵向速度（重力/跳跃用）
    private Camera mainCamera; 
    private bool isDancing = false;           // 主摄像机（用于获取朝向）

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponentInChildren<CharacterController>();
        mainCamera = Camera.main;
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isDancing = !isDancing;
            if(animator != null)
            {
                animator.SetBool("isDancing",isDancing);
            }
        }
        float h =isDancing ? 0f : Input.GetAxis("Horizontal"); // A/D 或左右
        float v =isDancing ? 0f : Input.GetAxis("Vertical");   // W/S 或前后
        Vector3 Cameraforward = mainCamera.transform.forward;
        Cameraforward.y = 0;
        Cameraforward.Normalize();
        Vector3 Cameraright = mainCamera.transform.right;
        Cameraright.y = 0f;
        Cameraright.Normalize();
        
        Vector3 move = (Cameraforward * v + Cameraright * h) * speed;
        
        if (controller.isGrounded && yvelocity.y < 0)
            yvelocity.y = -2f;                  // 落地时给一个小下压力，保持贴地
            yvelocity.y += gravity * Time.deltaTime; // 每帧累积下落速度
            move.y = yvelocity.y;                    // 把纵向速度并进移动向量

        
        if (!isDancing && controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            yvelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        controller.Move(move * Time.deltaTime);
        
        if (move.x != 0 || move.z != 0)
        {
            Vector3 face = new Vector3(move.x, 0, move.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(face), turnSpeed * Time.deltaTime);
        }
        if(animator != null)
        {
            float horizontalSpeed = new Vector2(move.x, move.z).magnitude;
            animator.SetFloat("speed", horizontalSpeed, animDamp, Time.deltaTime);
        }
    }
}
