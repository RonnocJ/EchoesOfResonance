using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHints : MonoBehaviour
{
    [SerializeField] private float fadeSpeed;
    public float alpha;

    void Awake()
    {
        alpha = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            SetHintAlpha(i);
        }
    }

    public IEnumerator ShowHints()
    {
        yield return new WaitForSeconds(0.25f);
        GameManager.root.currentState = GameState.InPuzzle;
        yield return new WaitForSeconds(1f);
        for (int j = 0; j < transform.childCount; j++)
        {
            alpha = 0;
            while (alpha < 1f)
            {
                alpha += fadeSpeed;
                alpha = Mathf.Clamp(alpha, 0, 1);

                SetHintAlpha(j);

                yield return null;
            }

            yield return new WaitForSeconds(4f);

        }

        yield return new WaitForSeconds(4f);

        while (alpha > 0f)
        {
            alpha -= fadeSpeed;
            alpha = Mathf.Clamp(alpha, 0, 1);

            for (int k = 0; k < transform.childCount; k++)
            {
                SetHintAlpha(k);
            }

            yield return null;
        }
    }

    void SetHintAlpha(int child)
    {
        transform.GetChild(child).GetComponent<RawImage>().color = new Vector4(1, 1, 1, alpha);
        transform.GetChild(child).GetChild(0).GetComponent<TextMeshProUGUI>().color = new Vector4(1, 1, 1, alpha);
    }
}
