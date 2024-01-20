using LitJson;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class YouYouEditor : OdinMenuEditorWindow
{
    [MenuItem("YouYouTools/YouYouEditor")]
    private static void OpenYouYouEditor()
    {
        var window = GetWindow<YouYouEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 500);
    }
    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true);

        //宏设置
        tree.AddAssetAtPath("MacroSettings", "Game/YouYouFramework/YouYouAssets/MacroSettings.asset");

        //参数设置
        tree.AddAssetAtPath("ParamsSettings", "Game/YouYouFramework/YouYouAssets/ParamsSettings.asset");

        //AssetBundle打包管理
        tree.AddAssetAtPath("AssetBundleSettings", "Game/YouYouFramework/YouYouAssets/AssetBundleSettings.asset");

        //类对象池
        tree.AddAssetAtPath("PoolAnalyze/ClassObjectPool", "Game/YouYouFramework/YouYouAssets/PoolAnalyze_ClassObjectPool.asset");
        //AssetBundele池
        tree.AddAssetAtPath("PoolAnalyze/AssetBundlePool", "Game/YouYouFramework/YouYouAssets/PoolAnalyze_AssetBundlePool.asset");
        //Asset池
        tree.AddAssetAtPath("PoolAnalyze/AssetPool", "Game/YouYouFramework/YouYouAssets/PoolAnalyze_AssetPool.asset");

        return tree;
    }

    #region SetFBXAnimationMode 设置文件动画循环为true
    [MenuItem("YouYouTools/设置文件动画循环为true")]
    public static void SetFBXAnimationMode()
    {
        Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            string relatepath = AssetDatabase.GetAssetPath(objs[i]);

            if (relatepath.IsSuffix(".FBX", System.StringComparison.CurrentCultureIgnoreCase))
            {
                string path = Application.dataPath.Replace("Assets", "") + relatepath + ".meta";
                path = path.Replace("\\", "/");
                StreamReader fs = new StreamReader(path);
                List<string> ret = new List<string>();
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    line = line.Replace("\n", "");
                    if (line.IndexOf("loopTime: 0") != -1)
                    {
                        line = "      loopTime: 1";
                    }
                    ret.Add(line);
                }
                fs.Close();
                File.Delete(path);
                StreamWriter writer = new StreamWriter(path + ".tmp");
                foreach (var each in ret)
                {
                    writer.WriteLine(each);
                }
                writer.Close();
                File.Copy(path + ".tmp", path);
                File.Delete(path + ".tmp");
            }

            if (relatepath.IsSuffix(".Anim", System.StringComparison.CurrentCultureIgnoreCase))
            {
                string path = Application.dataPath.Replace("Assets", "") + relatepath;
                path = path.Replace("\\", "/");
                StreamReader fs = new StreamReader(path);
                List<string> ret = new List<string>();
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    line = line.Replace("\n", "");
                    if (line.IndexOf("m_LoopTime: 0") != -1)
                    {
                        line = "    m_LoopTime: 1";
                    }
                    ret.Add(line);
                }
                fs.Close();
                File.Delete(path);
                StreamWriter writer = new StreamWriter(path + ".tmp");
                foreach (var each in ret)
                {
                    writer.WriteLine(each);
                }
                writer.Close();
                File.Copy(path + ".tmp", path);
                File.Delete(path + ".tmp");
            }
        }
        AssetDatabase.Refresh();
    }
    #endregion


    #region GetAssetsPath 收集多个文件的路径到剪切板
    [MenuItem("YouYouTools/收集多个文件的路径到剪切板")]
    public static void GetAssetsPath()
    {
        Object[] objs = Selection.objects;
        string relatepath = string.Empty;
        for (int i = 0; i < objs.Length; i++)
        {
            relatepath += AssetDatabase.GetAssetPath(objs[i]);
            if (i < objs.Length - 1) relatepath += "\n";
        }
        Clipboard.Copy(relatepath);
        AssetDatabase.Refresh();
    }
    #endregion


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

    #region 生成图集
    [MenuItem("Assets/工具/生成图集")]
    public static void GenerateSprites()
    {
        Texture2D tex = Selection.activeObject as Texture2D;
        if (tex == null)
        {
            return;
        }
        tex.alphaIsTransparency = true;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.alphaIsTransparency = true;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path = Application.dataPath.Replace("Assets", "") + path;
        int i = path.LastIndexOf("/");
        path = path.Substring(0, i + 1) + tex.name + ".txt";
        if (!File.Exists(path))
        {
            Debug.LogError(string.Format("File not exist:{0}", path));
        }
        TextureImporter assetImp = null;
        Dictionary<string, Vector4> tIpterMap = new Dictionary<string, Vector4>();
        assetImp = GetTextureIpter(tex);
        SaveBoreder(tIpterMap, assetImp);
        string json_txt = File.ReadAllText(path);
        JsonData json_data = JsonMapper.ToObject(json_txt);
        WriteMeta(json_data, tex, tIpterMap);
        File.Delete(path);
        AssetDatabase.Refresh();
    }

    //如果这张图集已经拉好了9宫格，需要先保存起来
    static void SaveBoreder(Dictionary<string, Vector4> tIpterMap, TextureImporter tIpter)
    {
        for (int i = 0, size = tIpter.spritesheet.Length; i < size; i++)
        {
            tIpterMap.Add(tIpter.spritesheet[i].name, tIpter.spritesheet[i].border);
        }
    }
    static TextureImporter GetTextureIpter(Texture2D texture)
    {
        TextureImporter textureIpter = null;
        string impPath = AssetDatabase.GetAssetPath(texture);
        textureIpter = TextureImporter.GetAtPath(impPath) as TextureImporter;
        return textureIpter;
    }
    //写信息到SpritesSheet里
    static void WriteMeta(JsonData json_data, Texture2D tex, Dictionary<string, Vector4> borders)
    {
        var assetImp = GetTextureIpter(tex);
        JsonData json_frames = json_data["frames"];
        SpriteMetaData[] metaData = new SpriteMetaData[json_frames.Count];
        Debug.Log(string.Format("WriteMeta:{0}", json_frames.Count));
        int count = 0;
        foreach (var key in json_frames.Keys)
        {
            int i = key.LastIndexOf(".");
            string name = key.Substring(0, i);
            Rect rect = new Rect();
            var value = json_frames[key]["frame"];
            rect.width = int.Parse(value["w"].ToString());
            rect.height = int.Parse(value["h"].ToString());
            rect.x = int.Parse(value["x"].ToString());
            rect.y = tex.height - int.Parse(value["y"].ToString()) - rect.height;
            metaData[count].rect = rect;
            metaData[count].pivot = new Vector2(0.5f, 0.5f);
            metaData[count].name = name;
            if (borders.ContainsKey(name))
            {
                metaData[count].border = borders[name];
            }
            ++count;
        }
        assetImp.spritesheet = metaData;
        assetImp.textureType = TextureImporterType.Sprite;
        assetImp.spriteImportMode = SpriteImportMode.Multiple;
        assetImp.mipmapEnabled = false;

        // iphone 默认作用RGBA16
        TextureImporterPlatformSettings textureSetting = assetImp.GetPlatformTextureSettings("iPhone");
        if (!textureSetting.overridden)
        {
            textureSetting.name = "iPhone";
            textureSetting.overridden = true;
            textureSetting.format = TextureImporterFormat.RGBA16;
            assetImp.SetPlatformTextureSettings(textureSetting);
        }

        assetImp.SaveAndReimport();
    }
    #endregion

    #region 打包前设置Keystore信息
    public class KeystoreInfoSetter : EditorWindow
    {
        string keystoreRelativePath = "Assets/PackageTool/user.keystore";
        string keystorePassword = "FrameWork";
        string keyAlias = "key";
        string keyPassword = "FrameWork";

        [MenuItem("YouYouTools/打包前设置Keystore信息")]
        static void OpenWindow()
        {
            GetWindow<KeystoreInfoSetter>("Set Keystore Info");
        }

        private void OnGUI()
        {
            GUILayout.Label("Keystore Information", EditorStyles.boldLabel);

            keystoreRelativePath = EditorGUILayout.TextField("Keystore Relative Path:", keystoreRelativePath);
            keystorePassword = EditorGUILayout.TextField("Keystore Password:", keystorePassword);
            keyAlias = EditorGUILayout.TextField("Key Alias:", keyAlias);
            keyPassword = EditorGUILayout.TextField("Key Password:", keyPassword);

            if (GUILayout.Button("Set Keystore Info"))
            {
                SetKeystoreInfo();
            }

            if (GUILayout.Button("Clear Keystore Info"))
            {
                ClearKeystoreInfo();
            }
        }

        private void SetKeystoreInfo()
        {
            PlayerSettings.Android.keystoreName = keystoreRelativePath;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = keyAlias;
            PlayerSettings.Android.keyaliasPass = keyPassword;

            Debug.Log("Keystore information set in Unity Editor:");
            Debug.Log("Keystore Path: " + PlayerSettings.Android.keystoreName);
            Debug.Log("Key Alias: " + PlayerSettings.Android.keyaliasName);
        }

        private void ClearKeystoreInfo()
        {
            PlayerSettings.Android.keystoreName = "";
            PlayerSettings.Android.keystorePass = "";
            PlayerSettings.Android.keyaliasName = "";
            PlayerSettings.Android.keyaliasPass = "";

            Debug.Log("Keystore information cleared in Unity Editor.");
        }
    }
    #endregion
}
