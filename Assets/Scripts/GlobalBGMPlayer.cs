using UnityEngine;

public class GlobalBGMPlayer : MonoBehaviour
{
    public static GlobalBGMPlayer instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }
}
