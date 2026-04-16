package com.framework.app;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.Configuration;
import android.os.Bundle;
import android.util.Log;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerGameActivity;

public class CustomUnityActivity extends UnityPlayerGameActivity {
    private static Activity MainActivity = null;
    public static final int INSTALL_REQUEST_CODE = 998;
    // 增加这个静态方法给工具类调用
    public static Activity getMainActivity() {
        return MainActivity;
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        MainActivity = this;

        InjectPrivacyPolicy();//注入隐私弹窗
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == INSTALL_REQUEST_CODE) {
            // 如果代码运行到这里，说明安装进程没有把当前进程覆盖（即安装未完成或被取消）
            Log.d("Unity", "用户从安装界面返回，安装未成功执行");

            // 方案 A: 通知 Unity 侧弹出“必须更新”的强更提示
            UnityPlayer.UnitySendMessage("MainEntry", "OnInstallResult", "canceled");

            // 方案 B: 如果是强制更新，直接在这里干掉游戏进程，不让进游戏
            // finish();
            // System.exit(0);
        }
    }

    public void onStart() {
        super.onStart();
    }

    public void onPause() {
        super.onPause();
    }

    public void onResume() {
        super.onResume();
    }

    public void onNewIntent(Intent newIntent) {
        super.onNewIntent(newIntent);
    }

    public void onStop() {
        super.onStop();
    }

    public void onDestroy() {
        super.onDestroy();
    }

    public void onRestart() {
        super.onRestart();
    }

    public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
    }


    public void InjectPrivacyPolicy()
    {
        SharedPreferences base = getSharedPreferences("base", MODE_PRIVATE);
        Boolean privacyFlag = base.getBoolean("PrivacyFlag", true);
        if (privacyFlag == true) {
            AlertDialog.Builder dialog = new AlertDialog.Builder(MainActivity);
            dialog.setTitle("隐私政策");  // 设置标题
            dialog.setMessage("作为“YFYouYou”的运营者，深知个人信息对您的重要性，我们将按照法律法规的规定，保护您的个人信息及隐私安全。我们制定本“隐私政策”并特别提示：希望您在使用“YFYouYou”及相关服务前仔细阅读并理解本隐私政策，以便做出适当的选择。如您同意，请点击“同意”开始进入游戏。本隐私政策将帮助您了解：1、我们会遵循隐私政策收集、使用您的信息，但不会仅因您同意本隐私政策而采用强制捆绑的方式一揽子收集个人信息。2、当您使用或开启相关功能或使用服务时，为实现功能、服务所必需，我们会收集、使用相关信息。3、相关敏感权限均不会默认开启，只有经过您的明示授权才会在为实现特定功能或服务时使用，您也可以撤回授权。");
            dialog.setCancelable(false);  // 是否可以取消
            dialog.setNegativeButton("拒绝", new DialogInterface.OnClickListener() {
                @Override
                public void onClick(DialogInterface dialogInterface, int i) {
                    dialogInterface.dismiss();
                    android.os.Process.killProcess(android.os.Process.myPid());
                }
            });

            dialog.setPositiveButton("同意", new DialogInterface.OnClickListener() {
                @Override
                public void onClick(DialogInterface dialog, int which) {
                    SharedPreferences.Editor editor = base.edit();
                    editor.putBoolean("PrivacyFlag", false);
                    editor.commit();
                }
            });
            dialog.show();
        }
    }
}
