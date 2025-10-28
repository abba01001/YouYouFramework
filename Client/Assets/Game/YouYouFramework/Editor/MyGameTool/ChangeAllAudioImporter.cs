// using System.Collections;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
//
// public class ChangeAllAudioImporter
// {
//     [MenuItem("Tools/��Ƶ/�޸�������Ƶ��ʽ")]
//     public static void Change()
//     {
//         var audiosGuids = AssetDatabase.FindAssets("t:audioclip");
//         int i = 0;
//         int total = audiosGuids.Length;
//         foreach (var audioGuid in audiosGuids)
//         {
//             i++;
//
//             var audiopath = AssetDatabase.GUIDToAssetPath(audioGuid);
//             var importer = AssetImporter.GetAtPath(audiopath) as AudioImporter;
//             EditorUtility.DisplayProgressBar("�޸���Ƶ", audiopath, (float)i / total);
//
//             AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audiopath);
//             AudioImporterSampleSettings audioImporterSampleSettingsios = new AudioImporterSampleSettings();
//             AudioImporterSampleSettings audioImporterSampleSettingsAndroid = new AudioImporterSampleSettings();
//
//             //��Ҫ�Ż��ĵ���
//             //��˫������ ȫ�����õ�����ģʽ, �粻��Ҫ������,����forceMono���Լ����ڴ�ʹ���ռ��
//             importer.forceToMono = true;
//
//             // ����10s ��ʾ BGM/������,��Ч����Ū��ô��
//             if (audioClip.length >= 10)
//             {
//                 //������Ƶ��ʱ����ʽ���أ��ô����ļ���ռ���ڴ棬�����Ǽ��ص�ʱ���IO��CPU�����п�������ʹû�м�����Ƶ�ļ���Ҳ��ռ��һ��200KB�Ŀռ䡣
//                 // Vorbis / MP3: ��ѹ������PCM�������½������ Quality ֵ����ѹ�����ʺ��еȳ���������
//
//                 audioImporterSampleSettingsios.loadType = AudioClipLoadType.Streaming;
//                 audioImporterSampleSettingsios.compressionFormat = AudioCompressionFormat.Vorbis;
//                 audioImporterSampleSettingsios.quality = 65f;
//
//                 audioImporterSampleSettingsAndroid.loadType = AudioClipLoadType.Streaming;
//                 audioImporterSampleSettingsAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
//                 audioImporterSampleSettingsAndroid.quality = 65f;
//
//                 importer.loadInBackground = false;
//             }
//             // 3-10s ��ʾ�ж���Ч
//             if (audioClip.length >= 2 && audioClip.length < 10)
//             {
//                 audioImporterSampleSettingsios.loadType = AudioClipLoadType.CompressedInMemory;
//                 audioImporterSampleSettingsAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
//                 audioImporterSampleSettingsios.quality = 70f;
//
//                 audioImporterSampleSettingsAndroid.loadType = AudioClipLoadType.CompressedInMemory;
//                 audioImporterSampleSettingsAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
//                 audioImporterSampleSettingsAndroid.quality = 70f;
//
//                 importer.loadInBackground = false;
//             }
//             // ����Ч
//             if (audioClip.length < 2)
//             {
//                 importer.loadInBackground = true;
//
//                 audioImporterSampleSettingsios.loadType = AudioClipLoadType.DecompressOnLoad;
//                 audioImporterSampleSettingsios.compressionFormat = AudioCompressionFormat.ADPCM;
//
//                 audioImporterSampleSettingsAndroid.loadType = AudioClipLoadType.DecompressOnLoad;
//                 audioImporterSampleSettingsAndroid.compressionFormat = AudioCompressionFormat.Vorbis;
//             }
//
//             //�ر�Ԥ����
//             importer.preloadAudioData = false;
//
//             importer.SetOverrideSampleSettings(BuildTargetGroup.iOS.ToString(), audioImporterSampleSettingsios);
//             importer.SetOverrideSampleSettings(BuildTargetGroup.Android.ToString(), audioImporterSampleSettingsAndroid);
//
//         }
//         EditorUtility.ClearProgressBar();
//         AssetDatabase.Refresh();
//     }
// }
