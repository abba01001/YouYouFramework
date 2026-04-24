using OctoberStudio.Timeline;
using System;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace OctoberStudio.Timeline.Editor
{
    [CustomTimelineEditor(typeof(WaveAsset))]
    public class WaveEditor : ClipEditor
    {
        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            var clipOptions = base.GetClipOptions(clip);
            Color color = Color.clear;
            string displayName = "";
            switch (clip.asset)
            {
                case BurstWave:
                    color = Color.blue;
                    displayName = "爆发波";
                    break;
                case ContinuousWave:
                    color = Color.yellow;
                    displayName = "持续波";
                    break;
                case MaintainWave:
                    color = Color.red;
                    displayName = "维持数量波";
                    break;
                case RushWave:
                    color = Color.cyan;
                    displayName = "突袭冲刺波";
                    break;
                case FlashWave:
                    color = Color.indigo;
                    displayName = "瞬闪波";
                    break;
                case CircleWave:
                    color = Color.slateBlue;
                    displayName = "圆圈波";
                    break;
            }
            
            clipOptions.highlightColor = color;
            clip.displayName = displayName;

            // var fromTo = $"{Math.Round(clip.start, 2)}s - {Math.Round(clip.end, 2)}s";
            var fromTo = $"{FormatTime(clip.start)} - {FormatTime(clip.end)}，持续{Math.Round(clip.end - clip.start, 2)}秒";
            clipOptions.tooltip = fromTo;

            return clipOptions;
        }
        
        string FormatTime(double t)
        {
            int min = (int)(t / 60);
            int sec = (int)(t % 60);
            return $"{min}分{sec}秒";
        }
    }
}