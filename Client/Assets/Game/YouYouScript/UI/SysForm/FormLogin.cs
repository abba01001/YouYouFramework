using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


// "登录"界面
public class FormLogin : UIFormBase
{
    [SerializeField] private TMP_InputField account;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private Button loginBtn;
    [SerializeField] private Image bgImage;
    protected override void Awake()
    {
        base.Awake();
        LoadLoginBg();
        loginBtn.SetButtonClick(Login);
// #if !UNITY_EDITOR
//         GameUtil.GetSignatureMD5Hash();
// #endif
        //GameEntry.SDK.DownloadAvatar("1", null);
    }

    private void Login()
    {
        //if(account.text == "" || password.text == "") return;
        GameEntry.SDK.LoginAsync(account.text, password.text);
    }
    
    public async UniTask LoadLoginBg()
    {
        // Texture2D texture = await GameEntry.Loader.LoadMainAssetAsync<Texture2D>($"Assets/Game/Download/Textures/BackGround/LoginBg.png",this.gameObject);
        // if (texture != null)
        // {
        //     // 将纹理转换为 Sprite
        //     Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        //     // 将加载的 sprite 赋值给 Image 组件
        //     GameUtil.LogError(bgImage == null,"===",sprite == null);
        //     bgImage.sprite = sprite;
        // }
    }
}
