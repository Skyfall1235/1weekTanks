using System.Collections;
using UnityEngine;

public static class ExtensionMethods
{

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
    public static void FadeGroup(this CanvasGroup groupToFade, MonoBehaviour monoBehaviourForRoutine, float desiredAlpha, float timeToFade = 0.5f)
    {
        monoBehaviourForRoutine.StartCoroutine(FadeCanvasGroupAsync(groupToFade, desiredAlpha, timeToFade));
    }
    static IEnumerator FadeCanvasGroupAsync(CanvasGroup groupToFade, float desiredAlpha, float timeToFade = 0.5f)
    {
        float currentTime = 0;
        float startingAlpha = groupToFade.alpha;
        while(currentTime <= timeToFade)
        {
            currentTime += Time.deltaTime;
            groupToFade.alpha = Mathf.Lerp(startingAlpha, desiredAlpha, currentTime / timeToFade);
            yield return null;
        }
        groupToFade.interactable = groupToFade.alpha > 0;
        groupToFade.blocksRaycasts = groupToFade.alpha > 0;
    }
    	//Depth-first search
	public static Transform FindDeepChild(this Transform aParent, string aName)
	{
		foreach(Transform child in aParent)
		{
			if(child.name == aName )
				return child;
			var result = child.FindDeepChild(aName);
			if (result != null)
				return result;
		}
		return null;
	}

}