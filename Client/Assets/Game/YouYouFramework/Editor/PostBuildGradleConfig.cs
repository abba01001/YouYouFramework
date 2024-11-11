using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;

public class GradlePostBuildHandler : IPostprocessBuildWithReport
{
    // 这个方法在构建完成后执行
    public void OnPostprocessBuild(BuildReport report)
    {
        // 判断构建平台是否为 Android
        if (report.summary.platform == BuildTarget.Android && EditorUserBuildSettings.exportAsGoogleAndroidProject)
        {
            string gradlePath = Path.Combine(report.summary.outputPath, "gradle", "wrapper");

            // 检查 gradle/wrapper 目录是否存在
            if (!Directory.Exists(gradlePath))
            {
                Directory.CreateDirectory(gradlePath); // 创建该目录
            }

            string wrapperFilePath = Path.Combine(gradlePath, "gradle-wrapper.properties");
            Debug.LogError($"配置文件路径{wrapperFilePath}");
            // 如果 gradle-wrapper.properties 文件不存在，则创建它
            if (!File.Exists(wrapperFilePath))
            {
                string content = @"
# Fri Nov 07 10:53:15 CST 2024
distributionBase=GRADLE_USER_HOME
distributionPath=wrapper/dists
zipStorePath=wrapper/dists
zipStoreBase=GRADLE_USER_HOME
distributionUrl=https\://services.gradle.org/distributions/gradle-6.1.1-bin.zip";

                // 将内容写入文件
                File.WriteAllText(wrapperFilePath, content);
                Debug.Log("Gradle Wrapper 配置文件已生成！");
            }
            else
            {
                Debug.Log("Gradle Wrapper 配置文件已存在！");
            }
        }
    }

    // 定义回调执行的顺序
    public int callbackOrder => 0;
}