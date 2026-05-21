using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GameScripts;
using Main;
using UniRx;
using UnityEngine;

namespace GameScripts
{
    public static class TimeUtils
    {
        private static long server_timestamp = 0;
        private static float duration;
        private static IEnumerator stopwatch = TimerCoroutine();
        private static long _pause_time = 0;
        public static int TimeZone;

        public static void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                _pause_time = UnixTimeSecond;
            }
            else
            {
                server_timestamp += UnixTimeSecond - _pause_time;
            }
        }


        private static IEnumerator TimerCoroutine()
        {
            while (true)
            {
                duration += Time.deltaTime;
                yield return null;
            }
        }

        public static long ServerTimestamp //{ get => server_timestamp + (int)stopwatch.Elapsed.TotalSeconds; }
        {
            get
            {
                if (server_timestamp == 0)
                    return UnixTimeSecond;

                return server_timestamp + (long)Math.Round(duration);
            }
            set
            {
                server_timestamp = value;
                duration = 0;
            }
        }

        public static DateTime ServerDateTime { get => UnixTimeStampToDateTime(ServerTimestamp); }
        public static long UnixTimeMs => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public static long UnixTimeSecond => DateTimeOffset.UtcNow.ToUnixTimeSeconds();


        public static void OpereateServerTime(int input = -1)
        {
            if (input != -1)
            {
                server_timestamp = input;
            }
            else
            {
                server_timestamp += (29 * 24 * 3600 + 3600 * 12 + 60 * 18);
            }
            duration = 0;
            GameEntry.Instance.StopCoroutine(stopwatch);
            GameEntry.Instance.StartCoroutine(stopwatch);
        }

        public static int GetOffsetDays(int time1, int time2)
        {
            var dt1 = DateTimeOffset.FromUnixTimeSeconds(time1);
            var dt2 = DateTimeOffset.FromUnixTimeSeconds(time2);
            return Converter.ToInt(Math.Floor(dt1.Subtract(dt2).TotalDays));
        }

        public static int GetDayIndex(long startTimestamp, long endTimestamp)
        {
            DateTime now = ServerDateTime;
            DateTime start = IntToDateTime(startTimestamp);
            DateTime end = IntToDateTime(endTimestamp);

            // 还没开始
            if (now < start)
                return 0;

            // 已超过结束
            if (now > end)
            {
                int totalDays = (int)Math.Floor((end - start).TotalHours / 24.0) + 1;
                return totalDays;
            }

            // 按小时计算天数
            double hours = (now - start).TotalHours;

            int day = (int)Math.Floor(hours / 24.0) + 1;

            return day;
        }

        public static bool IsSameDay(long time1, long time2)
        {
            DateTime dt1 = IntToDateTime(time1);
            DateTime dt2 = IntToDateTime(time2);
            return dt1.Date == dt2.Date;
        }

        public static int DayDiff(long lastTime, long nowTime)
        {
            DateTime last = IntToDateTime(lastTime).Date;
            DateTime today = IntToDateTime(nowTime).Date;
            return (today - last).Days;
        }

        public static int DayDiff7Clock(long lastTime, long nowTime)
        {
            DateTime last = IntToDateTime(lastTime);
            DateTime today = IntToDateTime(nowTime);
            DateTime thresholdTime = new DateTime(today.Year, today.Month, today.Day, 19, 0, 0);
            if (today >= thresholdTime && last < thresholdTime)
            {
                return 1;  // 不在同一天
            }
            if (last.Date == today.Date)
            {
                return 0;  // 在同一天
            }
            return (today - last).Days;  // 不在同一天
        }

        public static bool IsSameWeek(long time1, long time2)
        {
            DateTime dt1 = IntToDateTime(time1);
            DateTime dt2 = IntToDateTime(time2);

            // 获取两个日期的年和周数
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            var d1Week = cal.GetWeekOfYear(dt1, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            var d2Week = cal.GetWeekOfYear(dt2, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            return dt1.Year == dt2.Year && d1Week == d2Week;
        }

        public static bool IsSameMonth(int time1, int time2)
        {
            DateTime dt1 = IntToDateTime(time1);
            DateTime dt2 = IntToDateTime(time2);
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month;
        }

        public static int OneDaySeconds { get => 86400; }

        public static int GetDayOfWeek(DateTime date)
        {
            int dayOfWeek = Converter.ToInt(date.DayOfWeek);
            return dayOfWeek == 0 ? 7 : dayOfWeek;
        }

        public static string GetDateFromTimestamp(int timpStamp, string format)
        {
            DateTime dt = UnixTimeStampToDateTime(timpStamp);
            return dt.ToString(format);
        }

        public static DateTime IntToDateTime(int time) // time 单位 s
        {
            return UnixTimeStampToDateTime(time);
        }

        public static DateTime IntToDateTime(long time) // time 单位 s
        {
            return UnixTimeStampToDateTime(time);
        }

        public static long DateTimeToLong(DateTime time) // 返回 单位 s
        {
            return DateTimeToUnixTimeStamp(time);
        }

        public static string SecondFormatDdHhMmSs(int second)
        {
            StringBuilder sb = new StringBuilder();
            int day = second / 86400;
            int hour = second % 86400 / 3600;
            int min = second % 86400 % 3600 / 60;
            int sec = second % 60;
            if (day >= 1)
            {
                sb.Append(day).Append("d ");
            }

            if (hour < 10)
            {
                sb.Append($"0{hour}");
            }
            else
            {
                sb.Append(hour);
            }

            sb.Append(":");
            if (min < 10)
            {
                sb.Append($"0{min}");
            }
            else
            {
                sb.Append(min);
            }

            sb.Append(":");
            if (sec < 10)
            {
                sb.Append($"0{sec}");
            }
            else
            {
                sb.Append(sec);
            }

            return sb.ToString();
        }

        public static string GetTime(long nowDiffTime, StringBuilder sb)
        {
            sb.Clear();
            if (nowDiffTime >= 86400)
            {
                sb.AppendFormat("{0}d{1}h", (nowDiffTime / 86400), (nowDiffTime % 86400) / 3600);
            }
            else if (nowDiffTime >= 60)
            {
                sb.AppendFormat("{0:D2}:{1:D2}:{2:D2}", (nowDiffTime / 3600), ((nowDiffTime % 3600) / 60), (nowDiffTime % 60));
            }
            else
            {
                sb.AppendFormat("{0}s", nowDiffTime.ToString("D2"));
            }
            return sb.ToString();
        }

        public static string SecondFormatMmSs(long second)
        {
            var min = second / 60;
            var sec = second % 60;
            StringBuilder sb = new StringBuilder();
            if (min < 10)
            {
                sb.Append($"0{min}");
            }
            else
            {
                sb.Append(min);
            }

            sb.Append(":");
            if (sec < 10)
            {
                sb.Append($"0{sec}");
            }
            else
            {
                sb.Append(sec);
            }
            return sb.ToString();
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // 创建带时区偏移的自定义时区（兼容UTC±14范围）
            var timeZoneOffset = TimeZone;
            var offset = TimeSpan.FromHours(timeZoneOffset);
            var customZone = TimeZoneInfo.CreateCustomTimeZone(
                $"UTC{(timeZoneOffset >= 0 ? "+" : "")}{timeZoneOffset}",
                offset,
                $"UTC {timeZoneOffset}",
                $"UTC {timeZoneOffset}"
            );

            // 转换Unix时间戳到指定时区（自动处理UTC基准）
            var utcDateTime = DateTimeOffset.FromUnixTimeSeconds((long)unixTimeStamp).UtcDateTime;
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
                customZone
            );
        }

        public static long DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            // 创建目标时区（基于偏移参数）
            var timeZoneOffset = TimeZone;
            var offset = TimeSpan.FromHours(timeZoneOffset);
            var sourceZone = TimeZoneInfo.CreateCustomTimeZone(
                $"Source_{timeZoneOffset}",
                offset,
                $"Source {timeZoneOffset}",
                $"Source {timeZoneOffset}"
            );

            // 将输入时间视为目标时区时间，转换为UTC
            var sourceTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(sourceTime, sourceZone);

            // 计算精确到秒的Unix时间戳（基准时间明确指定UTC）
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(utcTime - epoch).TotalSeconds;
        }

        //获取注册天数
        public static int GetDaysFrom(long time)
        {
            DateTime nowDate = TimeUtils.IntToDateTime(ServerTimestamp);
            DateTime timeDate = TimeUtils.IntToDateTime(time);
            var temp1 = TimeUtils.DateTimeToLong(new DateTime(timeDate.Year, timeDate.Month, timeDate.Day, 0, 0, 0, DateTimeKind.Local));
            var temp2 = TimeUtils.DateTimeToLong(new DateTime(nowDate.Year, nowDate.Month, nowDate.Day, 23, 59, 59, DateTimeKind.Local));
            return Mathf.CeilToInt((float)(temp2 - temp1) / (24 * 60 * 60));
        }

        public static int ConvertToHHmmss(string timeStr)
        {
            try
            {
                DateTime dateTime = DateTime.ParseExact(timeStr, "HH:mm:ss", null);
                TimeSpan timeSpan = dateTime - dateTime.Date;
                return (int)timeSpan.TotalSeconds;
            }
            catch (FormatException)
            {
                Debugger.LogError($"Invalid time format: {timeStr}. Expected format: HH:mm:ss");
                return -1; // 返回-1表示错误
            }
        }

        public static int GetNowHHmmss()
        {
            DateTime midnight = ServerDateTime.Date;
            TimeSpan timeSinceMidnight = ServerDateTime - midnight;
            return (int)timeSinceMidnight.TotalSeconds;
        }

        // 自定义的 Observable.Timer，受 speedMultiplier 影响，返回 IDisposable
        public static IDisposable TimerWithSpeed(TimeSpan timeSpan, float speedMultiplier, Action action)
        {
            // 计算调整后的时间间隔
            float adjustedDuration = (float)timeSpan.TotalSeconds / speedMultiplier;
            float startTime = Time.realtimeSinceStartup;
            return Observable.EveryUpdate()
                .TakeWhile(_ => (Time.realtimeSinceStartup - startTime) < adjustedDuration)
                .LastOrDefault()
                .Subscribe(_ =>
                {
                    action?.Invoke();
                });
        }

        // 自定义的 Observable.TimerFrame，受 speedMultiplier 影响，返回 IDisposable
        public static IDisposable TimerFrameWithSpeed(int targetFrameCount, float speedMultiplier, Action action)
        {
            // 计算调整后的目标帧数
            int adjustedFrameCount = Mathf.CeilToInt(targetFrameCount / speedMultiplier);

            return Observable.EveryUpdate()
                .Scan(0, (currentFrameCount, _) => currentFrameCount + 1)
                .TakeWhile(frame => frame < adjustedFrameCount)
                .LastOrDefault()
                .Subscribe(_ =>
                {
                    action?.Invoke();
                });
        }

        public static int TimeStringToTimestamp(string timeString)
        {
            //string timeString = "2025-03-08 16:50:00";
            DateTime dateTime;
            bool success = DateTime.TryParseExact(timeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            if (success)
            {
                int timestamp = (int)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;//0时区的时间戳
                timestamp -= 8 * 3600;//转北京时间
                return timestamp;

            }
            return 0;
        }

        public static string SecondFormatSimpleTime(long second)
        {
            StringBuilder sb = new StringBuilder();
            var day = second / 86400;
            var hour = second % 86400 / 3600;
            var min = second % 86400 % 3600 / 60;
            var sec = second % 60;
            if (day > 0)
            {
                sb.Append(day).Append("日");
                if (hour > 0)
                {
                    sb.Append(" ").Append(hour).Append("d");
                }
                return sb.ToString();
            }

            if (hour > 0)
            {
                sb.Append(hour).Append("h");
                if (min > 0)
                {
                    sb.Append(" ").Append(min).Append("min");
                }
                return sb.ToString();
            }

            if (min > 0)
            {
                sb.Append(min).Append("min");
                if (sec > 0)
                {
                    sb.Append(" ").Append(sec).Append("s");
                }
                return sb.ToString();
            }

            sb.Append(sec).Append("s");
            return sb.ToString();
        }
    }
}
