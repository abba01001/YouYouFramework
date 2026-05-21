using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace OctoberStudio.Extensions
{
    public static class ToolkitExtensions
    {
        public static List<K> GetAssets<T, K>(this PlayableDirector director) where T : TrackAsset where K : PlayableAsset
        {
            var result = new List<K>();

            foreach (var output in director.playableAsset.outputs)
            {
                if (output.sourceObject is T track)
                {
                    foreach (var clip in track.GetClips())
                    {
                        if (clip.asset is K asset)
                        {
                            result.Add(asset);
                        }
                    }
                }
            }

            return result;
        }

        public static List<K> GetSortAssets<T, K>(this PlayableDirector director) where T : TrackAsset where K : PlayableAsset
        {
            var result = new List<K>();
            var timelineAsset = director.playableAsset as TimelineAsset;
    
            if (timelineAsset == null) return result;

            // 1. 获取所有轨道（包括组内的嵌套轨道）
            // 使用 IEnumerable<TrackAsset> 兼容所有版本
            IEnumerable<TrackAsset> allTracks = timelineAsset.GetRootTracks();

            foreach (var track in allTracks)
            {
                // 递归处理轨道（解决嵌套组的问题）
                ExtractClipsFromTrack<T, K>(track, result);
            }

            return result;
        }

        private static void ExtractClipsFromTrack<T, K>(TrackAsset track, List<K> result) where T : TrackAsset where K : PlayableAsset
        {
            // 如果是我们要找的轨道类型
            if (track is T targetTrack)
            {
                // 按时间线上的开始时间排序，确保逻辑顺序正确
                var clips = targetTrack.GetClips().OrderBy(c => c.start);
                foreach (var clip in clips)
                {
                    if (clip.asset is K asset)
                    {
                        result.Add(asset);
                    }
                }
            }

            // 递归检查子轨道（Group 里的轨道）
            foreach (var childTrack in track.GetChildTracks())
            {
                ExtractClipsFromTrack<T, K>(childTrack, result);
            }
        }
        
        public static List<TimelineClip> GetClips<T, K>(this PlayableDirector director) where T : TrackAsset where K : PlayableAsset
        {
            var result = new List<TimelineClip>();

            foreach (var output in director.playableAsset.outputs)
            {
                if (output.sourceObject is T track)
                {
                    foreach (var clip in track.GetClips())
                    {
                        if (clip.asset is K)
                        {
                            result.Add(clip);
                        }
                    }
                }
            }

            return result;
        }
    }
}