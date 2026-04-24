#if UNITY_ANDROID

using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEditor.Callbacks;

class AndroidPostBuildProcessor : IPostGenerateGradleAndroidProject
{
    private static string privacyAppName = Application.productName;
    public int callbackOrder => 999;
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        
    }

    private static string privacyImport = @"
import android.content.SharedPreferences;
import android.app.AlertDialog;
import android.content.DialogInterface;
    ";

    // 负责包名注入，先执行
    public class EarlyAndroidProcessor : IPostGenerateGradleAndroidProject {
        public int callbackOrder => 0; 
        public void OnPostGenerateGradleAndroidProject(string path) {
            // 1. 注入产品名称
            InjectProductName(path);
    
            // 2. 动态创建 file_paths.xml
            CreateFileProviderXml(path);
        }
    }

    public class LateAndroidProcessor : IPostGenerateGradleAndroidProject {
        public int callbackOrder => 999; 
        public void OnPostGenerateGradleAndroidProject(string path) {
            
        }
    }

    private static void InjectProductName(string path)
    {
        string stringXML =  "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
                            "<resources>\n"+
                            "    <string name=\"app_name\">{0}</string>\n"+
                            "</resources>";

        SetStringsFile( path+"/res/values", "strings.xml", stringXML, privacyAppName );
    }
    
    private static void SetStringsFile(string folder, string fileName, string stringXML, string appName)
    {
        try
        {
            appName = appName.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "\\\"").Replace("'", "\\'");
            appName = appName.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);

            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            if (!System.IO.File.Exists(folder + "/" + fileName))
            {
                // create the string file if it doesn't exist
                stringXML = string.Format(stringXML, appName);
            }
            else
            {
                stringXML = System.IO.File.ReadAllText(folder + "/" + fileName);
                // find app_name
                var pattern = "\"app_name\">(.*)<\\/string>";
                var regexPattern = new System.Text.RegularExpressions.Regex(pattern);
                if (regexPattern.IsMatch(stringXML))
                {
                    // Override the AppName if it was found
                    stringXML = regexPattern.Replace(stringXML, string.Format("\"app_name\">{0}</string>", appName));
                }
                else
                {
                    // insert the appName if it wasn't there
                    int idx = stringXML.IndexOf("<resources>");
                    if (idx > 0)
                        stringXML = stringXML.Insert(idx + "</resources>".Length, string.Format("\n    <string name=\"app_name\">{0}</string>\n", appName));
                }
            }
            System.IO.File.WriteAllText(folder + "/" + fileName, stringXML);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    private static void CreateFileProviderXml(string path)
    {
        // 定义 res/xml 文件夹路径
        string xmlFolder = Path.Combine(path, "src/main/res/xml");
        string filePath = Path.Combine(xmlFolder, "file_paths.xml");

// file_paths.xml 的内容
        string xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<paths>
    <external-files-path name=""my_external_files"" path=""."" />
    <external-cache-path name=""my_external_cache"" path=""."" />
    <files-path name=""my_inner_files"" path=""."" />
    <cache-path name=""my_inner_cache"" path=""."" />
    <external-path name=""external_root"" path=""."" />
</paths>";

        try
        {
            // 如果目录不存在则创建
            if (!Directory.Exists(xmlFolder))
            {
                Directory.CreateDirectory(xmlFolder);
                Debug.Log($"[AndroidPostBuild] Created directory: {xmlFolder}");
            }

            // 写入文件
            File.WriteAllText(filePath, xmlContent);
            Debug.Log($"[AndroidPostBuild] Successfully created file_paths.xml at: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AndroidPostBuild] Failed to create file_paths.xml: {e.Message}");
        }
    }
    
    private static void InjectPrivacyPolicy(string path)
    {
        // UnityPlayerActivity.java 文件路径
        string unityPlayerJavaFilePath = path + "/src/main/java/com/unity3d/player/UnityPlayerGameActivity.java";

        // 读取文件内容
        string content = File.ReadAllText(unityPlayerJavaFilePath);

        // 检查文件中是否已经包含隐私政策相关代码
        if (!content.Contains("privacyFlag"))
        {
            Debug.Log("Injecting Privacy Policy into UnityPlayerActivity.java");
            content = content.Replace("import com.google.androidgamesdk.GameActivity;",
                "import com.google.androidgamesdk.GameActivity;\n" + privacyImport);
            // 找到合适的位置插入隐私政策逻辑
            content = content.Replace("super.onCreate(savedInstanceState);",
                GetPrivacyPolicyString(privacyAppName) + "\n" + "super.onCreate(savedInstanceState);");

            // 写回文件
            File.WriteAllText(unityPlayerJavaFilePath, content);

            Debug.Log("隐私政策注入成功");
        }
        else
        {
            Debug.Log("隐私政策已添加.");
        }
    }

    // 返回隐私政策的插入代码
    private static string GetPrivacyPolicyString(string appName)
    {
        // 格式化 C# 端的隐私政策消息
        string privacyMessage = string.Format(@"作为“{0}”的运营者，深知个人信息对您的重要性，我们将按照法律法规的规定，保护您的个人信息及隐私安全。我们制定本“隐私政策”并特别提示：希望您在使用“{0}”及相关服务前仔细阅读并理解本隐私政策，以便做出适当的选择。如您同意，请点击“同意”开始进入游戏。本隐私政策将帮助您了解：1、我们会遵循隐私政策收集、使用您的信息，但不会仅因您同意本隐私政策而采用强制捆绑的方式一揽子收集个人信息。2、当您使用或开启相关功能或使用服务时，为实现功能、服务所必需，我们会收集、使用相关信息。3、相关敏感权限均不会默认开启，只有经过您的明示授权才会在为实现特定功能或服务时使用，您也可以撤回授权。", appName);

        // 返回注入的 Java 代码
        return $@"
        SharedPreferences base = getSharedPreferences(""base"", MODE_PRIVATE);
        Boolean privacyFlag = base.getBoolean(""PrivacyFlag"", true);
        if (privacyFlag == true) {{
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            dialog.setTitle(""隐私政策"");  // 设置标题
            dialog.setMessage(""{privacyMessage}"");
            dialog.setCancelable(false);  // 是否可以取消
            dialog.setNegativeButton(""拒绝"", new DialogInterface.OnClickListener() {{
                @Override
                public void onClick(DialogInterface dialogInterface, int i) {{
                    dialogInterface.dismiss();
                    android.os.Process.killProcess(android.os.Process.myPid());
                }}
            }});

            dialog.setPositiveButton(""同意"", new DialogInterface.OnClickListener() {{
                @Override
                public void onClick(DialogInterface dialog, int which) {{
                    SharedPreferences.Editor editor = base.edit();
                    editor.putBoolean(""PrivacyFlag"", false);
                    editor.commit();
                }}
            }});
            dialog.show();
        }}
";
    }
}

#endif