using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChangeAllAudioImporter
{
    [MenuItem("Tools/音频/修改所有音频格式")]
    public static void Change()
    {
        var audiosGuids = AssetDatabase.FindAssets("t:audioclip");
        int i = 0;
        int total = audiosGuids.Length;
        foreach (var audioGuid in audiosGuids)
        {
            i++;

            var audiopath = AssetDatabase.GUIDToAssetPath(audioGuid);
            var importer = AssetImporter.GetAtPath(audiopath) as AudioImporter;
            EditorUtility.DisplayProgressBar("修改音频", audiopath, (float)i / total);

            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audiopath);
            AudioImporterSampleSettings audioImporterSampleSettingsios = new AudioImporterSampleSettings();
            AudioImporterSampleSettings audioImporterSampleSettingsAndroid = new AudioImporterSampleSettings();

            //需要优化的点有
            //【双声道】 全部采用单声道模式, 如不需要立体声,开启forceMono可以减少内存和磁盘占用
            importer.forceToMono = true;

            // 大于10s 表示 BGM/环境音,音效不会弄这么长
            if (audioClip.length >= 10)
            {
                //播放音频的时候流式加载，好处是文件不占用内存，坏处是加载的时候对IO、CPU都会有开销。即使没有加载音频文件，也会占有一个200KB的空间。
                // Vorbis / MP3: 有压缩，比PCM质量有下降，配合 Quality 值进行压缩。适合中等长度声音。

                audioImporterSampleSettingsios.loadType = AudioClipLoadType.Streaming;
                audioImporterSampleSettingsios.compressionFormat = AudioCompressionFormat.Vorbis;
                audioImporterSampleSettingsios.quality = 65f;

                audioImporterSampleSettingsAndroid.loadType = AudioClipLoadType.Streaming;
                audioImporterSampleSettingsAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
                audioImporterSampleSettingsAndroid.quality = 65f;

                importer.loadInBackground = false;
            }
            // 3-10s 表示中断音效
            if (audioClip.length >= 2 && audioClip.length < 10)
            {
                audioImporterSampleSettingsios.loadType = AudioClipLoadType.CompressedInMemory;
                audioImporterSampleSettingsAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
                audioImporterSampleSettingsios.quality = 70f;

                audioImporterSampleSettingsAndroid.loadType = AudioClipLoadType.CompressedInMemory;
                audioImporterSampleSettingsAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
                audioImporterSampleSettingsAndroid.quality = 70f;

                importer.loadInBackground = false;
            }
            // 段音效
            if (audioClip.length < 2)
            {
                importer.loadInBackground = true;

                audioImporterSampleSettingsios.loadType = AudioClipLoadType.DecompressOnLoad;
                audioImporterSampleSettingsios.compressionFormat = AudioCompressionFormat.ADPCM;

                audioImporterSampleSettingsAndroid.loadType = AudioClipLoadType.DecompressOnLoad;
                audioImporterSampleSettingsAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
            }

            //关闭预加载
            importer.preloadAudioData = false;

            importer.SetOverrideSampleSettings(BuildTargetGroup.iOS.ToString(), audioImporterSampleSettingsios);
            importer.SetOverrideSampleSettings(BuildTargetGroup.Android.ToString(), audioImporterSampleSettingsAndroid);

        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }
}
