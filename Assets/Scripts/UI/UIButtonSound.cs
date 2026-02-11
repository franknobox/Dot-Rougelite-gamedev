using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Clips")]
    [Tooltip("鼠标悬停时的音效 (可选)")]
    public AudioClip hoverSound;
    
    [Tooltip("点击时的音效 (可选)")]
    public AudioClip clickSound;

    [Header("Settings")]
    [Tooltip("音量大小 (0-1)")]
    [Range(0f, 1f)]
    public float volume = 1.0f;

    private AudioSource audioSource;

    void Start()
    {
        // 尝试获取 AudioSource组件，不仅限于自身，也可以是父物体或者场景里面统一的
        // 这里为了简单，我们优先使用自身的 AudioSource
        audioSource = GetComponent<AudioSource>();

        // 如果自身没有 AudioSource，则添加一个
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    // 鼠标进入 (Hover)
    // 当鼠标移动到按钮上方时触发
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && audioSource != null)
        {
            // PlayOneShot 适合播放短音效，且不会打断正在播放的音频
            audioSource.PlayOneShot(hoverSound, volume);
        }
    }

    // 鼠标点击 (Click)
    // 当鼠标点击按钮时触发
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
}
