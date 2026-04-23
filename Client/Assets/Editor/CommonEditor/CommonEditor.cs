using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CommonEditor : OdinMenuEditorWindow
{
    [MenuItem("工具类/编辑器工具",false,-100)]
    private static void OpenCommonEditor()
    {
        var window = GetWindow<CommonEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 500);
    }
    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true);
        //参数设置
        tree.AddAssetAtPath("参数相关", "Game/Scripts/FrameworkScripts/ScriptableObject/ParamsSettings.asset");
        
        //宏设置
        tree.AddAssetAtPath("宏定义相关", "Game/Scripts/FrameworkScripts/ScriptableObject/MacroSettings.asset");

        //AssetBundle打包管理
        tree.AddAssetAtPath("打包相关", "Game/Scripts/FrameworkScripts/ScriptableObject/AssetBundleSettings.asset");

        //通用工具
        tree.AddAssetAtPath("通用工具", "Game/Scripts/FrameworkScripts/ScriptableObject/CommonToolsSettings.asset");
        return tree;
    }


    #region 提取动画片段
    [MenuItem("Assets/工具/提取动画片段")]
    public static void PickNPCClip()
    {
        UnityEngine.Object[] objs = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
        foreach (var item in objs)
        {
            string path = AssetDatabase.GetAssetPath(item);
            if (!Directory.Exists(path))
            {
                continue;
            }
            string[] files = Directory.GetFiles(path);
            string dest_path = path + "/Animation";
            if (!Directory.Exists(dest_path))
            {
                Directory.CreateDirectory(dest_path);
            }
            foreach (var file_path in files)
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(file_path);
                if (clip == null)
                {
                    continue;
                }
                string file_name = Path.GetFileName(file_path);
                file_name = file_name.Substring(0, file_name.LastIndexOf("."));
                string dest_path2 = dest_path + "/" + file_name + ".anim";
                if (File.Exists(dest_path2))
                {
                    AssetDatabase.DeleteAsset(dest_path2);
                }
                clip.wrapMode = WrapMode.Loop;
                AnimationClip tempClip = new AnimationClip();
                EditorUtility.CopySerialized(clip, tempClip);
                OptionalFloatCurves(tempClip);//对动画片段进行压缩优化，减小精度，删除无效关键帧
                AssetDatabase.CreateAsset(tempClip, dest_path2);
                AssetDatabase.Refresh();
            }
        }
    }
    #endregion

    #region Fbx生成预制体
    [MenuItem("Assets/工具/Fbx生成预制体")]
    public static void GeneratePrefab()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            return;
        }
        string prefab_name = go.name + "_pb";
        GameObject pb_go = new GameObject();
        pb_go.name = prefab_name;

        GameObject mesh_go = GameObject.Instantiate(go);
        mesh_go.transform.SetParent(pb_go.transform);

        string path = AssetDatabase.GetAssetPath(go);
        int i = path.LastIndexOf("/");
        path = path.Substring(0, i + 1) + prefab_name + ".prefab";

        PrefabUtility.SaveAsPrefabAsset(pb_go, path);
        GameObject.DestroyImmediate(pb_go);
        AssetDatabase.SaveAssets();
    }
    
    [MenuItem("Assets/工具/Fbx生成预制体(文件夹)")]
    public static void GeneratePrefabFromFbxInFolder()
    {
        // 获取当前选中的文件夹路径
        string selectedFolder = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(selectedFolder) || !Directory.Exists(selectedFolder))
        {
            Debug.LogError("请选择一个有效的文件夹");
            return;
        }

        // 检查是否选择的是文件夹
        if (!AssetDatabase.IsValidFolder(selectedFolder))
        {
            Debug.LogError("请选择一个有效的文件夹");
            return;
        }

        // 设置预制体存放的新文件夹路径
        string prefabFolder = selectedFolder + "/Prefabs";
        
        // 如果文件夹不存在，就创建它
        if (!Directory.Exists(prefabFolder))
        {
            Directory.CreateDirectory(prefabFolder);
        }

        // 获取文件夹内所有的 .fbx 文件
        string[] fbxFiles = Directory.GetFiles(selectedFolder, "*.fbx", SearchOption.AllDirectories);
        
        if (fbxFiles.Length == 0)
        {
            Debug.LogWarning("没有找到任何 .fbx 文件");
            return;
        }

        // 循环遍历每个 .fbx 文件
        foreach (string fbxFilePath in fbxFiles)
        {
            // 获取 .fbx 文件的名称
            string fbxFileName = Path.GetFileNameWithoutExtension(fbxFilePath);

            // 载入 .fbx 文件
            GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxFilePath);

            if (fbxPrefab != null)
            {
                // 创建预制体并保存
                string prefabPath = prefabFolder + "/" + fbxFileName + "_pb.prefab";
                PrefabUtility.SaveAsPrefabAsset(fbxPrefab, prefabPath);
                Debug.Log($"成功将 {fbxFileName} 转换为预制体，并保存到 {prefabPath}");
            }
            else
            {
                Debug.LogWarning($"无法加载 {fbxFileName}，请确保它是一个有效的 .fbx 文件");
            }
        }

        // 刷新 AssetDatabase，确保所有资源已保存
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("所有 .fbx 文件已成功转换为预制体！");
    }
    #endregion

    #region 优化动画片段
    [MenuItem("Assets/工具/优化动画片段")]
    static void CompressAnimClip()
    {
        var objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            var obj = objs[i];
            if (obj is AnimationClip)
            {
                OptionalFloatCurves(obj as AnimationClip);
            }
        }
        // 重新保存
        AssetDatabase.SaveAssets();
    }

    static string floatFormat = "f4";//精度
    public static void OptionalFloatCurves(AnimationClip activeObject)
    {
        var animation_go = activeObject;

        var clip = animation_go as AnimationClip;

        //获取动画片段的曲线信息
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
        AnimationClipCurveData[] curves = new AnimationClipCurveData[curveBindings.Length];
        for (int index = 0; index < curves.Length; ++index)
        {
            curves[index] = new AnimationClipCurveData(curveBindings[index]);
            curves[index].curve = AnimationUtility.GetEditorCurve(clip, curveBindings[index]);
        }

        clip.ClearCurves();//清除所有曲线，筛选必须的曲线数据再加入

        for (int j = 0; j < curves.Length; j++)
        {
            var curveDate = curves[j];
            var keyFrames = curveDate.curve.keys;//初始数据

            List<Keyframe> resultKeyFrames = new List<Keyframe>();//结果数据

            int sameKeyCount = 0;//值相同的帧数量，若多于两个，剔除中间关键帧，保留首尾两帧

            float currentValue = 0;//当前值
            float currentInTangent = 0;//除value外，in/out tangent也需要判断
            float currnetOutTangent = 0;

            Keyframe lasetKey = default;//上一帧的数据，若当真帧值与上一帧不同，则把上一帧数据加入保存

            //赋初始值
            if (keyFrames.Length > 0)
            {
                currentValue = float.Parse(keyFrames[0].value.ToString(floatFormat));
                currentInTangent = float.Parse(keyFrames[0].inTangent.ToString(floatFormat));
                currnetOutTangent = float.Parse(keyFrames[0].outTangent.ToString(floatFormat));

            }

            for (int i = 0; i < keyFrames.Length; i++)
            {
                var key = keyFrames[i];
                //优化精度
                key.value = float.Parse(key.value.ToString(floatFormat));
                key.inTangent = float.Parse(key.inTangent.ToString(floatFormat));
                key.outTangent = float.Parse(key.outTangent.ToString(floatFormat));
                key.inWeight = float.Parse(key.inWeight.ToString(floatFormat));
                key.outWeight = float.Parse(key.outWeight.ToString(floatFormat));
                key.time = float.Parse(key.time.ToString(floatFormat));
                keyFrames[i] = key;

                if (i == 0 || i == keyFrames.Length - 1)
                {
                    resultKeyFrames.Add(key);//把首帧和尾帧加入结果列表，防止预制体数据异常导致动画异常（预制体初始scale为0，但是首尾关键帧都为1，此时去除首尾帧会异常）
                }
                else
                {
                    if (currentValue == key.value && currentInTangent == key.inTangent && currnetOutTangent == key.outTangent)//当前帧与上一帧相同
                    {
                        sameKeyCount++;
                    }
                    else//当前帧与上一帧不同
                    {
                        if (sameKeyCount == 0)//匹配到的相同帧数量 == 0，表示，上一帧已经通过以下逻辑添加到列表中了，只需要添加当前帧
                        {

                        }
                        else//匹配到的帧数量 ！= 0 ，把相同帧的最后一帧加入列表
                        {
                            resultKeyFrames.Add(lasetKey);
                        }

                        //把当前帧加入到列表中
                        resultKeyFrames.Add(key);
                        sameKeyCount = 0;
                        currentValue = float.Parse(key.value.ToString(floatFormat));
                        currentInTangent = float.Parse(key.inTangent.ToString(floatFormat));
                        currnetOutTangent = float.Parse(key.outTangent.ToString(floatFormat));
                    }
                }
                lasetKey = key;
            }

            if (resultKeyFrames.Count == 1)//只有一个关键帧，说明动画有问题
            {

            }
            else
            {
                //设置曲线
                curveDate.curve.keys = resultKeyFrames.ToArray();
                clip.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
            }
        }

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
    }
    #endregion
}
