using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;

public class SpriteAtlasBuilder : EditorWindow
{
    private string saveAssetName = "NewSpriteAtlas";
    private int padding = 2;
    private int maxAtlasSize = 4096;
    private float frameRate = 12f; 
    private Dictionary<string, TextureImportBackup> importerBackup = new Dictionary<string, TextureImportBackup>();

    private List<Texture2D> selectedTextures;
    private string targetFolderPath;

    [MenuItem("Assets/生成序列帧动画预制体", false, 101)]
    public static void CombineSelectedSprites()
    {
        var textures = Selection.objects
            .Where(obj => obj is Texture2D)
            .Cast<Texture2D>()
            .OrderBy(t => t.name) 
            .ToList();

        if (textures.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "请在 Project 窗口选择多个图片", "确定");
            return;
        }
        
        string firstPath = AssetDatabase.GetAssetPath(textures[0]);
        string folderPath = Path.GetDirectoryName(firstPath);

        SpriteAtlasBuilder window = GetWindow<SpriteAtlasBuilder>("图集/动画/预制体工具");
        window.selectedTextures = textures;
        window.targetFolderPath = folderPath;
        window.Show();
    }

    private void OnGUI()
    {
        if (selectedTextures.Count > 0)
        {
            string path = AssetDatabase.GetAssetPath(selectedTextures[0]);
            string folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));
            saveAssetName = folderName;
        }

        
        GUILayout.Label("全流程生成器 (图集+动画+预制体)", EditorStyles.boldLabel);
        saveAssetName = EditorGUILayout.TextField("资源名称", saveAssetName);
        padding = EditorGUILayout.IntField("图集间隔 (Padding)", padding);
        maxAtlasSize = EditorGUILayout.IntField("最大尺寸", maxAtlasSize);
        frameRate = EditorGUILayout.FloatField("动画帧率 (FPS)", frameRate);

        EditorGUILayout.Space();
        if (GUILayout.Button("一键生成所有资源", GUILayout.Height(30)))
        {
            if (selectedTextures != null && selectedTextures.Count > 0)
            {
                BuildAtlas(selectedTextures, targetFolderPath, padding, maxAtlasSize);
                Close();
            }
        }

        if (!isDoing)
        {
            isDoing = true;
            if (selectedTextures != null && selectedTextures.Count > 0)
            {
                BuildAtlas(selectedTextures, targetFolderPath, padding, maxAtlasSize);
                Close();
            }
        }
    }

    private bool isDoing = false;
    
    private void BuildAtlas(List<Texture2D> textures, string spriteAssetFolder, int padding, int maxSize)
    {
        importerBackup.Clear();
        List<CroppedTexture> croppedTextures = new List<CroppedTexture>();

        // 1. 预处理
        foreach (var tex in textures)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                if (!importerBackup.ContainsKey(path))
                {
                    importerBackup[path] = new TextureImportBackup {
                        path = path, isReadable = importer.isReadable,
                        compression = importer.textureCompression,
                        mipmapEnabled = importer.mipmapEnabled, filterMode = importer.filterMode
                    };
                }
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            CroppedTexture cropped = CropTransparent(tex);
            cropped.name = tex.name;
            croppedTextures.Add(cropped);
        }

        // 2. 布局算法
        croppedTextures.Sort((a, b) => (b.width * b.height).CompareTo(a.width * a.height));

        int totalArea = 0;
        foreach (var ct in croppedTextures) totalArea += (ct.width + padding) * (ct.height + padding);
        int targetWidth = Mathf.Min(maxSize, NextPowerOfTwo(Mathf.CeilToInt(Mathf.Sqrt(totalArea))));

        List<Row> rows = new List<Row>();
        Row currentRow = new Row();
        int currentWidth = 0, rowHeight = 0;

        foreach (var ct in croppedTextures)
        {
            if (currentWidth + ct.width + padding > targetWidth)
            {
                currentRow.height = rowHeight;
                rows.Add(currentRow);
                currentRow = new Row();
                currentWidth = 0; rowHeight = 0;
            }
            currentRow.textures.Add(ct);
            currentWidth += ct.width + padding;
            if (ct.height > rowHeight) rowHeight = ct.height;
        }
        currentRow.height = rowHeight;
        rows.Add(currentRow);

        // 计算最终高度，确保至少有 1 像素
        int atlasHeight = 0;
        foreach (var row in rows) atlasHeight += row.height + padding;
        atlasHeight = Mathf.Max(1, atlasHeight); 

        // 3. 绘制
        Texture2D atlas = new Texture2D(targetWidth, atlasHeight, TextureFormat.RGBA32, false);

        Color32[] fill = new Color32[targetWidth * atlasHeight];
        for (int i = 0; i < fill.Length; i++) fill[i] = new Color32(0, 0, 0, 0);
        atlas.SetPixels32(fill);

        int offsetY = 0;
        List<SpriteMetaData> spriteMetas = new List<SpriteMetaData>();
        foreach (var row in rows)
        {
            int offsetX = 0;
            foreach (var ct in row.textures)
            {
                atlas.SetPixels(offsetX, offsetY, ct.width, ct.height, ct.pixels);
                spriteMetas.Add(new SpriteMetaData {
                    name = ct.name,
                    rect = new Rect(offsetX, offsetY, ct.width, ct.height),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = new Vector2((ct.originalWidth * 0.5f - ct.offsetX) / ct.width, (ct.originalHeight * 0.5f - ct.offsetY) / ct.height)
                });
                offsetX += ct.width + padding;
            }
            offsetY += row.height + padding;
        }
        atlas.Apply();

        string atlasPath = Path.Combine(spriteAssetFolder, $"{saveAssetName}.png");
        File.WriteAllBytes(atlasPath, atlas.EncodeToPNG());
        AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);

        TextureImporter ti = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritesheet = spriteMetas.ToArray();
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.filterMode = FilterMode.Point;
        ti.SaveAndReimport();

        RestoreImporterSettings(importerBackup);
        AssetDatabase.Refresh();

        AnimationClip clip = CreateAnimation(atlasPath);
        if (clip != null)
        {
            CreatePrefabWithAnimator(atlasPath, clip);
        }

        Debug.Log("所有资源生成完成！");
    }

    private AnimationClip CreateAnimation(string atlasPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
        // 这里按名称排序确保动画帧顺序
        List<Sprite> sprites = assets.OfType<Sprite>().OrderBy(s => s.name).ToList();
        if (sprites.Count == 0) return null;

        AnimationClip animClip = new AnimationClip();
        animClip.frameRate = frameRate;
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(animClip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(animClip, settings);

        EditorCurveBinding spriteBinding = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe { time = i / frameRate, value = sprites[i] };
        }
        AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, keyframes);

        string animPath = Path.Combine(Path.GetDirectoryName(atlasPath), $"{saveAssetName}.anim");
        AssetDatabase.CreateAsset(animClip, animPath);
        return animClip;
    }

    private void CreatePrefabWithAnimator(string atlasPath, AnimationClip clip)
    {
        string dir = Path.GetDirectoryName(atlasPath);
        string controllerPath = Path.Combine(dir, $"{saveAssetName}.controller");
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.layers[0].stateMachine.AddState(clip.name).motion = clip;

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
        Sprite firstSprite = assets.OfType<Sprite>().OrderBy(s => s.name).FirstOrDefault();

        GameObject go = new GameObject(saveAssetName);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = firstSprite;
        var animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        string prefabPath = Path.Combine(dir, $"{saveAssetName}.prefab");
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);
    }

    private void RestoreImporterSettings(Dictionary<string, TextureImportBackup> backup)
    {
        foreach (var pair in backup)
        {
            TextureImporter importer = AssetImporter.GetAtPath(pair.Key) as TextureImporter;
            if (importer != null) {
                importer.isReadable = pair.Value.isReadable;
                importer.textureCompression = pair.Value.compression;
                importer.SaveAndReimport();
            }
        }
    }

    private CroppedTexture CropTransparent(Texture2D tex)
    {
        int xMin = tex.width, xMax = 0, yMin = tex.height, yMax = 0;
        Color[] pixels = tex.GetPixels();
        bool hasPixel = false;
        for (int y = 0; y < tex.height; y++) {
            for (int x = 0; x < tex.width; x++) {
                if (pixels[y * tex.width + x].a > 0.001f) {
                    if (x < xMin) xMin = x; if (x > xMax) xMax = x;
                    if (y < yMin) yMin = y; if (y > yMax) yMax = y;
                    hasPixel = true;
                }
            }
        }
        if (!hasPixel) return new CroppedTexture { pixels = pixels, width = tex.width, height = tex.height, offsetX = 0, offsetY = 0, originalWidth = tex.width, originalHeight = tex.height };

        int width = xMax - xMin + 1; int height = yMax - yMin + 1;
        return new CroppedTexture {
            pixels = tex.GetPixels(xMin, yMin, width, height),
            width = width, height = height, offsetX = xMin, offsetY = yMin,
            originalWidth = tex.width, originalHeight = tex.height
        };
    }

    private int NextPowerOfTwo(int x) { int pow = 1; while (pow < x) pow <<= 1; return pow; }
    private class Row { public List<CroppedTexture> textures = new List<CroppedTexture>(); public int height; }
    private class CroppedTexture { public string name; public Color[] pixels; public int width, height, offsetX, offsetY, originalWidth, originalHeight; }
    class TextureImportBackup { public string path; public bool isReadable, mipmapEnabled; public TextureImporterCompression compression; public FilterMode filterMode; }
}