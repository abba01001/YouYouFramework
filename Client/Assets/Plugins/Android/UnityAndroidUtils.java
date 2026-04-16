package com.framework.app;

import static com.framework.app.CustomUnityActivity.INSTALL_REQUEST_CODE;

import android.app.Activity;
import android.app.ActivityManager;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.net.ConnectivityManager;
import android.net.NetworkCapabilities;
import android.net.Uri;
import android.os.Build;
import android.provider.Settings;
import android.util.Log;
import android.view.DisplayCutout;
import android.view.View;
import android.view.WindowInsets;
import android.widget.Toast;

import java.io.File;
import androidx.core.content.FileProvider;

public class UnityAndroidUtils {

    /**
     * 内部获取 Activity 实例
     */
    private static Activity getActivity() {
        return CustomUnityActivity.getMainActivity();
    }

    // --- 1. UI 与 交互 ---

    public static void showToast(final String message) {
        Activity activity = getActivity();
        if (activity != null) {
            activity.runOnUiThread(() -> Toast.makeText(activity, message, Toast.LENGTH_SHORT).show());
        }
    }

    public static void copyToClipboard(final String text) {
        Activity activity = getActivity();
        if (activity != null) {
            activity.runOnUiThread(() -> {
                ClipboardManager clipboard = (ClipboardManager) activity.getSystemService(Context.CLIPBOARD_SERVICE);
                ClipData clip = ClipData.newPlainText("UnityLabel", text);
                if (clipboard != null) clipboard.setPrimaryClip(clip);
            });
        }
    }

    // --- 2. 设备信息 ---

    public static long getAvailableMemory() {
        Activity activity = getActivity();
        if (activity == null) return 0;
        ActivityManager.MemoryInfo mi = new ActivityManager.MemoryInfo();
        ActivityManager activityManager = (ActivityManager) activity.getSystemService(Context.ACTIVITY_SERVICE);
        activityManager.getMemoryInfo(mi);
        return mi.availMem / 1048576L;
    }

    public static long getTotalMemory() {
        Activity activity = getActivity();
        if (activity == null) return 0;
        ActivityManager.MemoryInfo mi = new ActivityManager.MemoryInfo();
        ActivityManager activityManager = (ActivityManager) activity.getSystemService(Context.ACTIVITY_SERVICE);
        activityManager.getMemoryInfo(mi);
        return mi.totalMem / 1048576L;
    }

    public static String getAndroidID() {
        Activity activity = getActivity();
        if (activity == null) return "";
        return Settings.Secure.getString(activity.getContentResolver(), Settings.Secure.ANDROID_ID);
    }

    // --- 3. 屏幕与适配 ---

    public static int getNotchHeight() {
        Activity activity = getActivity();
        if (activity != null && Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
            View decorView = activity.getWindow().getDecorView();
            WindowInsets insets = decorView.getRootWindowInsets();
            if (insets != null) {
                DisplayCutout cutout = insets.getDisplayCutout();
                if (cutout != null) {
                    return cutout.getSafeInsetTop();
                }
            }
        }
        return 0;
    }

    // --- 4. 网络状态 ---

    public static int getNetworkStatus() {
        Activity activity = getActivity();
        if (activity == null) return 0;
        ConnectivityManager cm = (ConnectivityManager) activity.getSystemService(Context.CONNECTIVITY_SERVICE);
        if (cm == null) return 0;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            NetworkCapabilities capabilities = cm.getNetworkCapabilities(cm.getActiveNetwork());
            if (capabilities == null) return 0;
            if (capabilities.hasTransport(NetworkCapabilities.TRANSPORT_WIFI)) return 1;
            if (capabilities.hasTransport(NetworkCapabilities.TRANSPORT_CELLULAR)) return 2;
        } else {
            android.net.NetworkInfo info = cm.getActiveNetworkInfo();
            if (info == null || !info.isConnected()) return 0;
            if (info.getType() == ConnectivityManager.TYPE_WIFI) return 1;
            if (info.getType() == ConnectivityManager.TYPE_MOBILE) return 2;
        }
        return 3;
    }

    // --- 5. 系统功能 ---

    public static void installApk(String apkPath) {
        Activity activity = getActivity();
        if (activity == null || apkPath == null) return;

        File apkFile = new File(apkPath);
        if (!apkFile.exists()) {
            Log.e("UnityAndroidUtils", "安装失败：文件不存在 -> " + apkPath);
            return;
        }

        // 1. Android 8.0+ 未知来源权限预检
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            if (!activity.getPackageManager().canRequestPackageInstalls()) {
                Log.w("UnityAndroidUtils", "未开启安装未知应用权限，系统将弹窗提醒");
            }
        }

        Intent intent = new Intent(Intent.ACTION_VIEW);
        // 注意：如果要触发 onActivityResult，严禁添加 FLAG_ACTIVITY_NEW_TASK
        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
        intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);

        Uri uri;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
            // 动态获取包名拼接 fileprovider
            String authority = activity.getPackageName() + ".fileprovider";
            uri = FileProvider.getUriForFile(activity, authority, apkFile);
        } else {
            uri = Uri.fromFile(apkFile);
        }

        intent.setDataAndType(uri, "application/vnd.android.package-archive");

        try {
            // 关键点：使用 CustomUnityActivity 中定义的请求码
            activity.startActivityForResult(intent, INSTALL_REQUEST_CODE);
            Log.d("UnityAndroidUtils", "安装界面已弹出，等待结果...");
        } catch (Exception e) {
            Log.e("UnityAndroidUtils", "安装跳转失败: " + e.getMessage());
            // 如果跳转彻底失败，通常意味着环境异常，直接退出
            activity.finish();
        }
    }

    public static long getVersionCode() {
        Activity activity = getActivity();
        if (activity == null) return 0;
        try {
            PackageInfo pi = activity.getPackageManager().getPackageInfo(activity.getPackageName(), 0);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                return pi.getLongVersionCode();
            } else {
                return pi.versionCode;
            }
        } catch (Exception e) {
            return 0;
        }
    }
}