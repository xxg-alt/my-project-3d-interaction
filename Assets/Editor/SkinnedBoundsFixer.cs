using UnityEngine;
using UnityEditor;

// ============================================================
// 修复「模型在 Scene 视图能看见，但 Play 时 Game 视图里消失」的问题。
//
// 症状：从某些 FBX / GLB 导入的骨骼模型，SkinnedMeshRenderer 的
//       localBounds 被算成了几毫米大小。Unity 用这个包围盒做视锥剔除，
//       于是整个模型被判成「不在画面里」，直接不渲染。
//       看起来就像「材质丢失」「模型变透明」。
//
// 原因：模型骨架带 100 倍缩放（Mixamo / Sketchfab 常见），
//       Unity 计算 bindpose 包围盒时会算错。
//
// 这个脚本在每次导入模型时自动检查，发现异常就修正。
// 只会动「包围盒明显不合理」的模型，正常模型不受影响。
// ============================================================
public class SkinnedBoundsFixer : AssetPostprocessor
{
    // 小于这个尺寸（米）就判定为异常。正常人形模型不可能只有几厘米。
    const float MinReasonableExtent = 0.05f;

    void OnPostprocessModel(GameObject root)
    {
        SkinnedMeshRenderer[] rends = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < rends.Length; i++)
        {
            SkinnedMeshRenderer smr = rends[i];
            if (smr.sharedMesh == null) continue;

            Bounds local = smr.localBounds;
            if (local.extents.magnitude >= MinReasonableExtent) continue;

            Bounds meshBounds = smr.sharedMesh.bounds;
            Vector3 size = meshBounds.size;
            Vector3 center = meshBounds.center;

            // 网格自身的包围盒也是坏的，就用一个宽松的默认值兜底
            if (size.magnitude < MinReasonableExtent)
            {
                center = new Vector3(0f, 1.05f, 0f);
                size = new Vector3(2.4f, 2.6f, 2.0f);
            }

            // 放大 1.6 倍，给动作（抬手、跳）留余量，避免动作超出包围盒被裁掉
            smr.localBounds = new Bounds(center, size * 1.6f);
            smr.updateWhenOffscreen = false;

            Debug.Log("[SkinnedBoundsFixer] " + assetPath + " -> 修正 " + smr.name
                      + " 包围盒为 " + smr.localBounds.size.ToString("F2")
                      + "（原值 extents=" + local.extents.ToString("F3") + "）");
        }
    }
}
