using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Linq;
using CandyCoded.env;
using UnityEngine.UI;

public class EmotionManager: MonoBehaviour {

    public GameObject textInput;
    private TextMeshProUGUI textReal;

    public GameObject textOut;
    private TextMeshProUGUI textOutReal;
    
    
    
    private string API_URL = "https://api-inference.huggingface.co/models/arpanghoshal/EmoRoBERTa";
    private Dictionary < string, string > headers = new Dictionary < string, string > () {
        {
            "Authorization",
            "api string here"
        }
    };

    private Dictionary<string, double> dict = new Dictionary<string, double>()
    {
        {"admiration", 0.0f},
        {"amusement", 0.0f},
        {"anger", 0.0f},
        {"annoyance", 0.0f},
        {"approval", 0.0f},
        {"caring", 0.0f},
        {"confusion", 0.0f},
        {"curiosity", 0.0f},
        {"desire", 0.0f},
        {"disappointment", 0.0f},
        {"disapproval", 0.0f},
        {"disgust", 0.0f},
        {"embarrassment", 0.0f},
        {"excitement", 0.0f},
        {"fear", 0.0f},
        {"gratitude", 0.0f},
        {"grief", 0.0f},
        {"joy", 0.0f},
        {"love", 0.0f},
        {"nervousness", 0.0f},
        {"optimism", 0.0f},
        {"pride", 0.0f},
        {"realization", 0.0f},
        {"relief", 0.0f},
        {"remorse", 0.0f},
        {"sadness", 0.0f},
        {"surprise", 0.0f},
        {"neutral", 0.0f}
    };



    //and then do top 5 emotions for the whole convo (from the user)
    private List<string> list_emotions = new List<string> { "admiration", "amusement", "anger", "annoyance", "approval", "caring", "confusion", "curiosity", "desire", "disappointment", "disapproval", "disgust", "embarrassment", "excitement", "fear", "gratitude", "grief", "joy", "love", "nervousness", "optimism", "pride", "realization", "relief", "remorse", "sadness", "surprise", "neutral" };
    // private List<string> list_emotions = new List<string> { "nervousness", "grief", "sadness", "joy" };
    // private Dictionary<string, double> dict = new Dictionary<string, double> { { "nervousness", 0 }, { "grief", 0 }, { "sadness", 0 }, { "joy", 0 } };


    // Start is called before the first frame update
    void Start()
    {




        // Set env
        if (env.TryParseEnvironmentVariable("HUGGING_FACE_API_KEY_STRING", out string moo))
        {
            headers = new Dictionary<string, string>()
            {
                {
                    "Authorization",
                    moo
                }
            };


            textOutReal = textOut.GetComponentInChildren<TextMeshProUGUI>();
            textReal = textInput.GetComponentInChildren<TextMeshProUGUI>();
        }
    }




    public void UpdateDictionary(JArray analysis)
    {
        for (int i = 0; i < 28; i++)
        {
            // Debug.Log(analysis[0][i].GetType());
            JObject item = (JObject)analysis[0][i];
            if (list_emotions.Contains(item["label"].Value<string>()))
            {
                // Console.WriteLine(1);
                dict[item["label"].Value<string>()] += item["score"].Value<double>();
            }
        }
        // Debug.Log(dict[list_emotions[0]]);
    }

    public void PrintEmotions() {
        foreach (KeyValuePair<string, double> entry in dict)
        {
            Debug.Log(entry.Key + ": " + entry.Value);
        }
    }




    //You can call this function by using:
    // List<string> topFiveEmotions = topFive();
    // foreach(string emotion in topFiveEmotions)
    //   Debug.Log(emotion);

    public List<string> topFive() {
        // Create a new list to store the top 5 emotions
        List<string> topFive = new List<string>();

        var sortedDict = dict.OrderByDescending(x => x.Value);
        // Use Select method to extract the keys and return them as a List<string>
        topFive = sortedDict.Select(x => x.Key).Take(5).ToList();

        return topFive;
    }

    public void PrintTopFive() {
            List<string> topFiveEmotions = topFive();
    foreach(string emotion in topFiveEmotions)
      Debug.Log(emotion);
    }

    public string prettyTopFive()
    {
        string prettyString ="";
        List<string> topFiveEmotions = topFive();
        foreach (string emotion in topFiveEmotions)
        {
            prettyString = prettyString + "/n" + emotion ;
        }

        prettyString = "Your top five emotions: /n" + prettyString;
        return prettyString;
    }

    public void setPrettyTopFive()
    {
        string t5string = prettyTopFive();
        textOutReal.text = t5string;
    }



    public void SubmitFromTextField() {
        
        // Debug.Log(textReal.text);
        Submit(textReal.text);
    }


    //Gets the emotions of a string with hugging face emoroberta, then add...
    // ...the emotion values to the total for the entire conversation.
    public void Submit(string payload) {
        StartCoroutine(PostRequest(API_URL, headers, payload, HandleQueryResponse));
    }

    private IEnumerator PostRequest(string url, Dictionary<string,string> headers, string jsonPayload, System.Action < string > callback) {
        UnityWebRequest www = UnityWebRequest.Post(url, jsonPayload);
        foreach(KeyValuePair<string,string> header in headers) {
            www.SetRequestHeader(header.Key, header.Value);
        }
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        } else {
            callback(www.downloadHandler.text);
        }
    }

    private void HandleQueryResponse(string jsonResponse) {
        JArray array = JArray.Parse(jsonResponse);
        UpdateDictionary(array);
    }
}



// public class EmotionData
// {
//     public string label;
//     public float score;
// }
