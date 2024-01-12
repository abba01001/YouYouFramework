using UnityEngine;
using UnityEditor;

public class BMFontEditor : EditorWindow
{
    [MenuItem("Tools/BMFont Maker")]
    static public void OpenBMFontMaker()
    {
        EditorWindow.GetWindow<BMFontEditor>(false, "BMFont Maker", true).Show();
    }

    [SerializeField]
    private Font targetFont;
    [SerializeField]
    private TextAsset fntData;
    [SerializeField]
    private Material fontMaterial;
    [SerializeField]
    private Texture2D fontTexture;

    private BMFont bmFont = new BMFont();

    public BMFontEditor()
    {
    }

    void OnGUI()
    {
        targetFont = EditorGUILayout.ObjectField("Target Font", targetFont, typeof(Font), false) as Font;
        fntData = EditorGUILayout.ObjectField("Fnt Data", fntData, typeof(TextAsset), false) as TextAsset;
        fontMaterial = EditorGUILayout.ObjectField("Font Material", fontMaterial, typeof(Material), false) as Material;
        fontTexture = EditorGUILayout.ObjectField("Font Texture", fontTexture, typeof(Texture2D), false) as Texture2D;

        if (GUILayout.Button("Create BMFont"))
        {
            BMFontReader.Load(bmFont, fntData.name, fntData.bytes); 
            CharacterInfo[] characterInfo = new CharacterInfo[bmFont.glyphs.Count];
            for (int i = 0; i < bmFont.glyphs.Count; i++)
            {
                BMGlyph bmInfo = bmFont.glyphs[i];
                CharacterInfo info = new CharacterInfo();
                info.index = bmInfo.index;
               Rect r1 = new Rect((float)bmInfo.x / (float)bmFont.texWidth,1 - (float)bmInfo.y / (float)bmFont.texHeight,(float)bmInfo.width / (float)bmFont.texWidth,-1f * (float)bmInfo.height / (float)bmFont.texHeight);
               info.uvBottomLeft = new Vector2(r1.xMin, r1.yMin);
               info.uvBottomRight = new Vector2(r1.xMax, r1.yMin);
               info.uvTopLeft = new Vector2(r1.xMin, r1.yMax);
               info.uvTopRight = new Vector2(r1.xMax, r1.yMax);
               Rect r2 =  new Rect(0,-(float)bmInfo.height, (float)bmInfo.width,(float)bmInfo.height);
               info.minX = (int)r2.xMin;
               info.maxX = (int)r2.xMax;
               info.minY = (int)r2.yMax;
               info.maxY = (int)r2.yMin;
               info.advance = bmInfo.advance;
                characterInfo[i] = info;
            }
            targetFont.characterInfo = characterInfo;
            if (fontMaterial)
            {
                fontMaterial.mainTexture = fontTexture;
            }
            targetFont.material = fontMaterial;
            fontMaterial.shader = Shader.Find("UI/Default");//这一行很关键，如果用standard的shader，放到Android手机上，第一次加载会很慢

            Debug.Log("create font <" + targetFont.name + "> success");
            // 复制一份，删除原来的，再保存，否则不会保存
            var instance_obj = GameObject.Instantiate(targetFont);
            var path = AssetDatabase.GetAssetPath(targetFont);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(instance_obj, path);
            AssetDatabase.SaveAssets();
            Close();
        }
    }
}