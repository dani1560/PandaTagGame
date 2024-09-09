using UnityEngine;
using UnityEngine.UI;

public class PinchEffect : MonoBehaviour
{
    public Button myButton;
    public float animationDuration = 0.2f;
    public float scaleDecreaseAmount = 0.2f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isAnimating = false;

    void Start()
    {
        if (myButton == null)
        {
            myButton = GetComponent<Button>();
        }

        originalScale = myButton.transform.localScale;
        targetScale = originalScale - new Vector3(scaleDecreaseAmount, scaleDecreaseAmount, scaleDecreaseAmount);
        myButton.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateButton());
        }
    }

    System.Collections.IEnumerator AnimateButton()
    {
        isAnimating = true;

        // Step 1: Reduce size
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            myButton.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        myButton.transform.localScale = targetScale;

        // Step 2: Increase size back to original
        elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            myButton.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        myButton.transform.localScale = originalScale;

        isAnimating = false;
    }
}
