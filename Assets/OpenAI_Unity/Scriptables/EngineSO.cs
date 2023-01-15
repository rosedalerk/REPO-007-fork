using OpenAI_API;
using UnityEngine;
using CandyCoded.env;

namespace OpenAI_Unity
{
    [CreateAssetMenu(fileName = "OpenAI Engine", menuName = "ScriptableObjects/OpenAI Engine", order = 1)]
    public class EngineSO : ScriptableObject
    {
        public string ApiKey;

        public void Awake()
        {
            // Set env
            if (env.TryParseEnvironmentVariable("OPENAI_API_KEY", out string myApiKey))
            {
                ApiKey = myApiKey;
            }
        }
    }
}

