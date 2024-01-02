using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObbyProgressUI : MonoBehaviour
{
    public Image progressBar;
    public TextMeshProUGUI progressText;

    void Start()
    {
        ObbyGameManager.instance.OnCurrentNodeChanged += UpdateText;
        UpdateText();
    }

    private void UpdateText()
    {
        progressBar.fillAmount = (float)ObbyGameManager.instance.currentCourseNodeIndex / (ObbyGameManager.instance.currentCourse.nodes.Length - 1);
        progressText.text = Mathf.RoundToInt(progressBar.fillAmount * 100) + "%";
    }
}
