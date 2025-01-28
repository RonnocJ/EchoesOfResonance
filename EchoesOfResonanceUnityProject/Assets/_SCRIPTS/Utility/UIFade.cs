using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFade : Singleton<UIFade>
{
    public void SetAlpha(float alpha, List<GameObject> gameObjects)
    {
        foreach (GameObject obj in gameObjects)
        {
            if (alpha > 0)
            {
                obj.SetActive(true);
            }

            if (obj.TryGetComponent(out Image image))
                image.color = new Color(image.color.r, image.color.b, image.color.g, alpha);
            if (obj.TryGetComponent(out RawImage rawImage))
                rawImage.color = new Color(rawImage.color.r, rawImage.color.b, rawImage.color.g, alpha);
            if (obj.TryGetComponent(out TextMeshProUGUI text))
                text.color = new Color(text.color.r, text.color.b, text.color.g, alpha);

            if (alpha == 0)
            {
                obj.SetActive(false);
            }
        }
    }
    public IEnumerator FadeItems(float duration, float delay, bool fadeOut, List<GameObject> gameObjects)
    {
        yield return new WaitForSeconds(delay);

        List<Image> images = new();
        List<RawImage> rawImages = new();
        List<TextMeshProUGUI> texts = new();

        foreach (GameObject obj in gameObjects)
        {
            if (obj.TryGetComponent(out Image image))
                images.Add(image);
            if (obj.TryGetComponent(out RawImage rawImage))
                rawImages.Add(rawImage);
            if (obj.TryGetComponent(out TextMeshProUGUI text))
                texts.Add(text);

            if (!fadeOut)
            {
                obj.SetActive(true);
            }
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            foreach (var image in images)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, fadeOut ? Mathf.Abs(1 - elapsed / duration) : elapsed / duration);
            }
            foreach (var rawImage in rawImages)
            {
                rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, fadeOut ? Mathf.Abs(1 - elapsed / duration) : elapsed / duration);
            }
            foreach (var text in texts)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, fadeOut ? Mathf.Abs(1 - elapsed / duration) : elapsed / duration);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (fadeOut)
        {
            foreach (GameObject obj in gameObjects)
            {
                obj.SetActive(false);
            }
        }
    }
}