using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public int collectedMoney;
    private CanvasUiManager _CanvasUiManager;
    public Color[] customerColors;
    public Mesh[] customerHats;

    private void Start()
    {
        _CanvasUiManager = FindObjectOfType<CanvasUiManager>();
        collectedMoney = PlayerPrefs.GetInt("MoneyAmount", 0);
        _CanvasUiManager.SetMoneyText(collectedMoney);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayerPrefs.DeleteAll();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            AddMoney(500);
        }
    }

    public void AddMoney(int amount)
    {
        collectedMoney += amount;
        ShowAndSave();
    }

    public void LessMoney()
    {
        collectedMoney--;
        ShowAndSave();
    }

    public void LessMoneyinBulk(int amount)
    {
        collectedMoney -= amount;
        ShowAndSave();
    }

    public void ShowAndSave()
    {
        _CanvasUiManager.SetMoneyText(collectedMoney);
        PlayerPrefs.SetInt("MoneyAmount", collectedMoney);
    }
}
