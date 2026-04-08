using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine.TextCore;

public class TMPAtlasBuilder : EditorWindow
{
    private string saveAssetName = "FontTex";
    private string folderPath = "TempLocalization/FontTex";
    private string saveSpriteAssetFolder = "Assets/Res/Texes/FontTexture";
    private int padding = 2;
    private int maxAtlasSize = 4096;
    private Dictionary<string, TextureImportBackup> importerBackup = new Dictionary<string, TextureImportBackup>();

    [MenuItem("Tools/I2 Localization/生成多语言图集")]
    public static void ShowWindow()
    {
        GetWindow<TMPAtlasBuilder>("TMP Sprite Atlas Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("TMP 字体图集生成器", EditorStyles.boldLabel);

        saveAssetName = EditorGUILayout.TextField("保存资源名称", saveAssetName);
        folderPath = EditorGUILayout.TextField("字体贴图源文件夹", folderPath);
        saveSpriteAssetFolder = EditorGUILayout.TextField("生成图集保存路径", saveSpriteAssetFolder);
        padding = EditorGUILayout.IntField("图集间隔 (Padding)", padding);
        maxAtlasSize = EditorGUILayout.IntField("最大图集尺寸", maxAtlasSize);

        if (GUILayout.Button("生成 TMP 字体图集"))
        {
            BuildAtlasWithTempCopy(folderPath, saveSpriteAssetFolder, padding, maxAtlasSize);
        }
    }

    /// <summary>
    /// 带临时拷贝的 BuildAtlas 流程
    /// </summary>
    private void BuildAtlasWithTempCopy(string sourceFolder, string spriteAssetFolder, int padding, int maxSize)
    {
        string tempFolder = "Assets/TempFontTex";

        // 删除旧临时目录
        if (Directory.Exists(tempFolder))
            Directory.Delete(tempFolder, true);

        Directory.CreateDirectory(tempFolder);

        // 拷贝本地化 FontTex
        CopyDirectory(sourceFolder, tempFolder);
        AssetDatabase.Refresh();

        // 调用原 BuildAtlas
        BuildAtlas(tempFolder, spriteAssetFolder, padding, maxSize);

        // 删除临时目录和 meta
        Directory.Delete(tempFolder, true);
        File.Delete(tempFolder + ".meta");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 递归拷贝目录
    /// </summary>
    private void CopyDirectory(string sourceDir, string destDir)
    {
        foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourceDir, destDir));
        }

        foreach (string newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourceDir, destDir), true);
        }
    }

    private void BuildAtlas(string folder, string spriteAssetFolder, int padding, int maxSize)
    {
        if (!Directory.Exists(folder))
        {
            Debug.LogError("Sprites folder does not exist: " + folder);
            return;
        }

        if (!Directory.Exists(spriteAssetFolder))
        {
            Directory.CreateDirectory(spriteAssetFolder);
            AssetDatabase.Refresh();
        }

        string[] files = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            Debug.LogWarning("No PNG found in folder: " + folder);
            return;
        }

        string tmpAssetPath = Path.Combine(spriteAssetFolder, $"{saveAssetName}.asset");

        TMP_SpriteAsset oldAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(tmpAssetPath);

        Dictionary<string, Vector2> oldBearing = new Dictionary<string, Vector2>();

        if (oldAsset != null)
        {
            foreach (var character in oldAsset.spriteCharacterTable)
            {
                if (character == null || character.glyph == null) 
                    continue;
                var glyph = character.glyph;
                oldBearing[character.name] = new Vector2(
                    glyph.metrics.horizontalBearingX,
                    glyph.metrics.horizontalBearingY
                );
            }
        }

        List<CroppedTexture> croppedTextures = new List<CroppedTexture>();

        foreach (var file in files)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
            if (tex == null) continue;

            string path = AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                if (!importerBackup.ContainsKey(path))
                {
                    importerBackup[path] = new TextureImportBackup
                    {
                        path = path,
                        isReadable = importer.isReadable,
                        compression = importer.textureCompression,
                        mipmapEnabled = importer.mipmapEnabled,
                        filterMode = importer.filterMode
                    };
                }

                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;

                importer.SaveAndReimport();
            }

            CroppedTexture cropped = CropTransparent(tex);
            cropped.name = Path.GetFileNameWithoutExtension(file);

            croppedTextures.Add(cropped);
        }

        croppedTextures.Sort((a, b) => (b.width * b.height).CompareTo(a.width * a.height));

        int totalArea = 0;
        foreach (var ct in croppedTextures)
            totalArea += ct.width * ct.height;

        int targetWidth = Mathf.Min(maxSize, NextPowerOfTwo(Mathf.CeilToInt(Mathf.Sqrt(totalArea))));

        List<Row> rows = new List<Row>();
        Row currentRow = new Row();

        int currentWidth = 0;
        int rowHeight = 0;

        foreach (var ct in croppedTextures)
        {
            if (currentWidth + ct.width + padding > targetWidth)
            {
                currentRow.height = rowHeight;
                rows.Add(currentRow);

                currentRow = new Row();
                currentWidth = 0;
                rowHeight = 0;
            }

            currentRow.textures.Add(ct);

            currentWidth += ct.width + padding;

            if (ct.height > rowHeight)
                rowHeight = ct.height;
        }

        currentRow.height = rowHeight;
        rows.Add(currentRow);

        int atlasWidth = targetWidth;
        int atlasHeight = 0;
        foreach (var row in rows)
            atlasHeight += row.height + padding;
        atlasHeight -= padding;

        Texture2D atlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.RGBA32, false);

        Color32[] fill = new Color32[atlasWidth * atlasHeight];
        for (int i = 0; i < fill.Length; i++)
            fill[i] = new Color32(0, 0, 0, 0);
        atlas.SetPixels32(fill);

        int offsetY = 0;

        List<SpriteMetaData> spriteMetas = new List<SpriteMetaData>();
        List<TMP_SpriteGlyph> glyphTable = new List<TMP_SpriteGlyph>();
        List<TMP_SpriteCharacter> characterTable = new List<TMP_SpriteCharacter>();

        uint glyphIndex = 0;

        foreach (var row in rows)
        {
            int offsetX = 0;

            foreach (var ct in row.textures)
            {
                atlas.SetPixels(offsetX, offsetY, ct.width, ct.height, ct.pixels);

                SpriteMetaData meta = new SpriteMetaData();
                meta.name = ct.name;
                meta.rect = new Rect(offsetX, offsetY, ct.width, ct.height);
                meta.alignment = (int)SpriteAlignment.Custom;
                meta.pivot = new Vector2(
                    (ct.originalWidth * 0.5f - ct.offsetX) / ct.width,
                    (ct.originalHeight * 0.5f - ct.offsetY) / ct.height
                );
                spriteMetas.Add(meta);

                float bx = 0;
                float by = 0;
                if (oldBearing.TryGetValue(ct.name, out Vector2 old))
                {
                    bx = old.x;
                    by = old.y;
                }

                GlyphMetrics metrics = new GlyphMetrics(
                    ct.width,
                    ct.height,
                    bx,
                    by,
                    ct.width
                );

                GlyphRect rect = new GlyphRect(
                    offsetX,
                    offsetY,
                    ct.width,
                    ct.height
                );

                TMP_SpriteGlyph glyph = new TMP_SpriteGlyph(glyphIndex, metrics, rect, 1.0f, 0);
                glyphTable.Add(glyph);

                TMP_SpriteCharacter character = new TMP_SpriteCharacter(0, glyph);
                character.name = ct.name;
                characterTable.Add(character);

                glyphIndex++;
                offsetX += ct.width + padding;
            }

            offsetY += row.height + padding;
        }

        atlas.Apply();

        string atlasPath = Path.Combine(spriteAssetFolder, $"{saveAssetName}Png.png");
        File.WriteAllBytes(atlasPath, atlas.EncodeToPNG());
        AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);

        TextureImporter ti = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritesheet = spriteMetas.ToArray();
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.filterMode = FilterMode.Point;

        TextureImporterPlatformSettings androidSettings = ti.GetPlatformTextureSettings("Android");
        androidSettings.overridden = true;
        androidSettings.format = TextureImporterFormat.ASTC_4x4;
        androidSettings.maxTextureSize = Mathf.Max(atlasWidth, atlasHeight);
        ti.SetPlatformTextureSettings(androidSettings);
        ti.SaveAndReimport();

        TMP_SpriteAsset spriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(tmpAssetPath);
        if (spriteAsset == null)
        {
            spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            AssetDatabase.CreateAsset(spriteAsset, tmpAssetPath);
        }

        spriteAsset.spriteSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
        spriteAsset.spriteGlyphTable.Clear();
        spriteAsset.spriteCharacterTable.Clear();
        spriteAsset.fallbackSpriteAssets.Clear();
        spriteAsset.spriteGlyphTable.AddRange(glyphTable);
        spriteAsset.spriteCharacterTable.AddRange(characterTable);
        spriteAsset.UpdateLookupTables();

        // 删除旧 material
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(tmpAssetPath);
        foreach (var obj in subAssets)
        {
            if (obj is Material)
            {
                AssetDatabase.RemoveObjectFromAsset(obj);
                DestroyImmediate(obj, true);
            }
        }

        // 创建 internal material
        Material mat = new Material(Shader.Find("TextMeshPro/Sprite"));
        mat.name = "SpriteAtlas_Internal Material";
        mat.hideFlags = HideFlags.HideInHierarchy;
        mat.mainTexture = spriteAsset.spriteSheet;
        AssetDatabase.AddObjectToAsset(mat, spriteAsset);
        spriteAsset.material = mat;

        EditorUtility.SetDirty(mat);
        EditorUtility.SetDirty(spriteAsset);

        RestoreImporterSettings(importerBackup);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("TMP Sprite Atlas rebuild complete.");
    }

    private void RestoreImporterSettings(Dictionary<string, TextureImportBackup> backup)
    {
        foreach (var pair in backup)
        {
            TextureImporter importer = AssetImporter.GetAtPath(pair.Key) as TextureImporter;
            if (importer == null) continue;

            importer.isReadable = pair.Value.isReadable;
            importer.textureCompression = pair.Value.compression;
            importer.mipmapEnabled = pair.Value.mipmapEnabled;
            importer.filterMode = pair.Value.filterMode;
            importer.SaveAndReimport();
        }
    }

    private CroppedTexture CropTransparent(Texture2D tex)
    {
        int xMin = tex.width, xMax = 0, yMin = tex.height, yMax = 0;
        Color[] pixels = tex.GetPixels();

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                if (pixels[y * tex.width + x].a > 0f)
                {
                    if (x < xMin) xMin = x;
                    if (x > xMax) xMax = x;
                    if (y < yMin) yMin = y;
                    if (y > yMax) yMax = y;
                }
            }
        }

        int width = xMax - xMin + 1;
        int height = yMax - yMin + 1;
        Color[] croppedPixels = tex.GetPixels(xMin, yMin, width, height);

        return new CroppedTexture
        {
            pixels = croppedPixels,
            width = width,
            height = height,
            offsetX = xMin,
            offsetY = yMin,
            originalWidth = tex.width,
            originalHeight = tex.height
        };
    }

    private int NextPowerOfTwo(int x)
    {
        int pow = 1;
        while (pow < x) pow <<= 1;
        return pow;
    }

    private class Row
    {
        public List<CroppedTexture> textures = new List<CroppedTexture>();
        public int height;
    }

    private class CroppedTexture
    {
        public string name;
        public Color[] pixels;
        public int width;
        public int height;
        public int offsetX;
        public int offsetY;
        public int originalWidth;
        public int originalHeight;
    }

    class TextureImportBackup
    {
        public string path;
        public bool isReadable;
        public TextureImporterCompression compression;
        public bool mipmapEnabled;
        public FilterMode filterMode;
    }
}