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
        progressBar.fillAmount = (float)ObbyGameManager.instance.currentCourseNodeIndex / (float)ObbyGameManager.instance.currentCourse.nodes.Length;
        progressText.text = Mathf.RoundToInt(progressBar.fillAmount * 100) + "%";
    }

    private void UpdateText()
    {
        progressBar.fillAmount = (float)ObbyGameManager.instance.currentCourseNodeIndex / (float)ObbyGameManager.instance.currentCourse.nodes.Length;
        progressText.text = Mathf.RoundToInt(progressBar.fillAmount * 100) + "%";
    }
}
