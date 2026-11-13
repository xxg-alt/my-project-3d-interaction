using UnityEngine;

public class ArtTrigger : MonoBehaviour
{
    [Header("进入按钮（三个触发区共用同一块）")]
    public GameObject artButton;

    [Header("玩家根物体")]
    public GameObject player;

    [Header("这块触发区对应哪幅画的面板")]
    public GameObject panel;

    // 按钮只有一块，它记不了三幅画，所以点击时来问这两个值
    static ArtTrigger current;          // 玩家现在站在哪个触发区
    static GameObject openPanel;        // 当前打开着的面板（玩家可能已经走开了）

    void Start()
    {
        Show(false);
    }

    // 显示/隐藏按钮的同时切换鼠标模式：
    // 相机脚本 Camera move 会把光标锁在屏幕中心（CursorLockMode.Locked），
    // 那样 EventSystem 的射线永远落在屏幕中心，左下角的按钮点不到。
    void Show(bool on)
    {
        if (artButton != null) artButton.SetActive(on);
        Cameramove.SetUIMode(on);
    }

    // 触发事件里的 other 是玩家脚下那个缩放 0.01 的传感器，
    // 不是 Player 本身，所以不能用 other.gameObject == player 来比。
    bool IsPlayer(Collider other)
    {
        if (player == null || other == null) return false;
        return other.transform.IsChildOf(player.transform);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        current = this;          // 记下"现在站在这幅画前"
        Show(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        if (current == this) current = null;

        if (artButton != null) artButton.SetActive(false);

        // 面板还开着就先别收鼠标，否则「退出」按钮点不到
        if (openPanel == null) Cameramove.SetUIMode(false);
    }

    // 「进入」按钮的 On Click 绑这个：打开"脚下这幅画"的面板
    public void EnterCurrent()
    {
        if (current == null || current.panel == null) return;

        openPanel = current.panel;
        openPanel.SetActive(true);

        if (artButton != null) artButton.SetActive(false);   // 按钮自己收起来
    }

    // 三块面板里的「退出」按钮都绑这个
    public void ClosePanel()
    {
        if (openPanel != null)
        {
            openPanel.SetActive(false);
            openPanel = null;
        }

        // 人还站在画前 → 按钮重新出现；已经走开了 → 保持隐藏并锁回鼠标
        if (current != null)
        {
            if (current.artButton != null) current.artButton.SetActive(true);
            Cameramove.SetUIMode(true);
        }
        else
        {
            Cameramove.SetUIMode(false);
        }
    }
}
