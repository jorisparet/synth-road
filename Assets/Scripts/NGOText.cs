using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using TMPro;

public class NGOText : MonoBehaviour
{
    [SerializeField] string textURL;
    [SerializeField] TextMeshProUGUI displayedText;

    void Start()
    {
        StartCoroutine(DownloadTextFromPastebin());
    }

    IEnumerator DownloadTextFromPastebin()
    {
        UnityWebRequest www = UnityWebRequest.Get(textURL);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            print("Error downloading: " + www.error);
        }
        else
        {
            displayedText.text = www.downloadHandler.text;
        }
    }

}
