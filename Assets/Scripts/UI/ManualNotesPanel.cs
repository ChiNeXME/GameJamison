using System.Linq.Expressions;
using UnityEngine;

public class ManualNotes : MonoBehaviour
{
    [SerializeField] GameObject Panel;

    public void Open()
    {
        Time.timeScale = 0f;
        Panel.SetActive(true);
    }

    public void Close()
    {
        Time.timeScale = 1f;
        Panel.SetActive(false);
    }
}
