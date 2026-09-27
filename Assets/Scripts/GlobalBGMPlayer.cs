using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GlobalBGMPlayer : MonoBehaviour
{
    public static GlobalBGMPlayer instance;

    AudioSource source;
    AudioResource theme;
    bool themeLoops;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);

            // Remember the menu theme: the ending swaps in its own track and then stops it.
            source = GetComponent<AudioSource>();
            if (source != null)
            {
                theme = source.resource;
                themeLoops = source.loop;
            }

            SceneManager.sceneLoaded += HandleSceneLoaded;
        }
        else
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    // Any new scene starts with the theme again, unless it is already playing (no restart).
    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (source == null || theme == null)
            return;

        if (source.isPlaying && source.resource == theme)
            return;

        source.Stop();
        source.resource = theme;
        source.loop = themeLoops;
        source.Play();
    }
}
