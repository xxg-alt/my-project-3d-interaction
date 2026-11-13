using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 登录页 UI：点「进入展厅」→ 画面淡入纯黑 → 异步加载展厅 → 黑幕淡出
/// </summary>
public class lonUI : MonoBehaviour
{
    [Header("转场设置")]
    public string targetScene = "SampleScene"; // 要跳去的场景名（注意拼写和大小写）
    public float fadeDuration = 0.6f;         // 每次渐变持续的秒数

    // Inspector 里 Button 的 On Click () 绑定的就是这个方法（别改名！）
    public void Onclick()
    {
        // 1. 造一个"过渡宿主"物体：协程必须挂在它身上，才能活着穿过场景切换
        GameObject host = new GameObject("FadeOverlay");
        DontDestroyOnLoad(host);              // 切场景时不会被销毁

        // 2. 挂上真正干活的组件，开始转场
        FadeOverlay overlay = host.AddComponent<FadeOverlay>();
        overlay.Begin(targetScene, fadeDuration);
    }
}

/// <summary>
/// 全屏黑色遮罩：淡入黑 → 加载新场景 → 淡出 → 自我销毁。
/// UI 全部运行时动态生成，不需要任何图片素材。
/// </summary>
public class FadeOverlay : MonoBehaviour
{
    private CanvasGroup cg;   // 透明度由它控制
    private float duration;
    private string sceneName;

    public void Begin(string scene, float fadeSeconds)
    {
        sceneName = scene;
        duration = fadeSeconds;
        BuildUI();
        StartCoroutine(Run());
    }

    // 用代码拼一块全屏黑布：Canvas > Image > CanvasGroup
    private void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;            // 盖在一切 UI 之上

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>(); // 让黑布能挡住点击

        GameObject imgGo = new GameObject("Black");
        imgGo.transform.SetParent(transform, false);
        Image img = imgGo.AddComponent<Image>();
        img.color = Color.black;               // Image 不设 Sprite 时默认画纯色矩形
        img.raycastTarget = true;              // 转场期间挡住下面的按钮

        RectTransform rt = imgGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;           // 四角锚点 = 全屏铺满
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        cg = imgGo.AddComponent<CanvasGroup>();
        cg.alpha = 0f;                         // 初始完全透明
    }

    private IEnumerator Run()
    {
        // 阶段 1：透明 → 纯黑
        yield return Fade(0f, 1f);

        // 阶段 2：黑布盖着屏幕时，后台加载下一个场景
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (op != null && !op.isDone)
        {
            yield return null;                 // 等一帧，再看看加载完了没
        }

        yield return new WaitForSecondsRealtime(0.15f); // 给新场景喘一口气

        // 阶段 3：新场景已在黑布后面就绪 → 纯黑 → 透明
        yield return Fade(1f, 0f);

        // 阶段 4：功成身退
        Destroy(gameObject);
    }

    // 通用渐变：from → to，每帧挪一点
    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;       // 不受 timeScale 影响的时间增量
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;                         // 保证收尾值精确
    }
}
