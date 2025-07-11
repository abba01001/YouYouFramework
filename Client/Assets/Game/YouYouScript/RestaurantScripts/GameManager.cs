using UnityEngine;
using UnityEngine.SceneManagement;
using YouYou;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public int collectedMoney;
    public Color[] customerColors;
    public Mesh[] customerHats;

    private void Start()
    {
        collectedMoney = PlayerPrefs.GetInt("MoneyAmount", 0);
        GameEntry.Event.Dispatch(Constants.EventName.SetMoneyText,new SetMoneyTextEvent(collectedMoney));
        Application.targetFrameRate = 120; 
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
        GameEntry.Event.Dispatch(Constants.EventName.SetMoneyText,new SetMoneyTextEvent(collectedMoney));
        PlayerPrefs.SetInt("MoneyAmount", collectedMoney);
    }
}
