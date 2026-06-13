using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // 简单音频管理器：播放 UI/攻击音效，并在需要时随机轮播 BGM。
    public static AudioManager instance;

    [SerializeField] private bool isBGMPlay;
    [SerializeField] private AudioSource[] bgms;
    private int currentBGMIndex;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        InvokeRepeating(nameof(PlayMusicIfNeeded), 0, 2);
    }

    public void PlaySFX(AudioSource audioToPlay, bool isPitchRandom = false)
    {
        // isPitchRandom 用来让重复音效稍微变化，避免听起来太机械。
        if (audioToPlay == null)
        {
            Debug.LogWarning("Could not play " + audioToPlay.gameObject.name + ". There is no audio clip assigned");
            return;
        }
        
        // Safety check to prevent the audio from playing on top of each other
        if (audioToPlay.isPlaying)
            audioToPlay.Stop();

        audioToPlay.pitch = isPitchRandom ? Random.Range(.9f, 1.1f) : 1;
        audioToPlay.Play();
    }

    private void PlayMusicIfNeeded()
    {
        // 定时检查当前 BGM 是否结束，结束后自动随机下一首。
        if (bgms.Length <= 0)
        {
            Debug.LogWarning("No music was assigned. Check AudioManager");
            return;
        }

        if (isBGMPlay == false)
            return;

        if (bgms[currentBGMIndex].isPlaying == false)
            PlayRandomBGM();
    }

    [ContextMenu("Play Random Music")]
    public void PlayRandomBGM()
    {
        // Inspector 右键也能手动测试随机音乐。
        currentBGMIndex = Random.Range(0, bgms.Length);
        PlayBGM(currentBGMIndex);
    }

    public void PlayBGM(int bgmToPlay)
    {
        if (bgms.Length <= 0)
        {
            Debug.LogWarning("No music was assigned. Check AudioManager");
            return;
        }

        StopAllBGM();
        currentBGMIndex = bgmToPlay;
        bgms[bgmToPlay].Play();
    }

    [ContextMenu("Stop All Music")]
    public void StopAllBGM()
    {
        // 切歌前先停掉全部 BGM，保证同一时间只播放一首。
        for (int i = 0; i < bgms.Length; i++)
        {
            bgms[i].Stop();
        }
    }

}
