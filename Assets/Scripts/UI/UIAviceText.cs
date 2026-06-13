using TMPro;
using UnityEngine;

public class UIAviceText : MonoBehaviour
{
    // 每次启用时随机显示一条提示文本。
    private TextMeshProUGUI adviceText;

    [SerializeField] private string[] advices;

    void OnEnable()
    {
        // OnEnable 里刷新，保证重复打开面板时提示会变化。
        if (adviceText == null)
            adviceText = GetComponent<TextMeshProUGUI>();

        int randomIndex = Random.Range(0, advices.Length);
        adviceText.text = advices[randomIndex];
    }
}
