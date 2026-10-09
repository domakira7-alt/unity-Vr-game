using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace UISystem
{
    [Serializable]
    public sealed class SheetRegistration
    {
        public string name;
        public string email;
        public string dateTime;
    }

    public interface IRegistrationUploader
    {
        IEnumerator Upload(SheetRegistration registration, Action<bool, string> completed);
    }

    public sealed class RegistrationSheetClient : MonoBehaviour, IRegistrationUploader
    {
        public const string Endpoint = "https://script.google.com/macros/s/AKfycbwXD9anTvFpBC5al7oipy0XlcdVd7SG4JyXbshi-Wka5RM3LWRjkHLKouYFWqCjjDrw/exec";
        public const string InternetAlert = "Connexion Internet requise. Connectez-vous à Internet puis réessayez.";
        private UnityWebRequest activeRequest;

        [Serializable]
        private sealed class SheetResponse { public string status; }

        public IEnumerator Upload(SheetRegistration registration, Action<bool, string> completed)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                completed(false, InternetAlert);
                yield break;
            }
            using (var request = new UnityWebRequest(Endpoint, "POST"))
            {
                activeRequest = request;
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(registration)));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 25;
                request.redirectLimit = 8;
                yield return request.SendWebRequest();
                activeRequest = null;
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    completed(false, InternetAlert);
                    yield break;
                }
                bool saved = false;
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try { saved = JsonUtility.FromJson<SheetResponse>(request.downloadHandler.text)?.status == "success"; }
                    catch (ArgumentException) { }
                }
                completed(saved, saved ? null : "Impossible d’enregistrer en ligne. Veuillez réessayer.");
            }
        }

        public void Cancel()
        {
            if (activeRequest != null) activeRequest.Abort();
            StopAllCoroutines();
            if (activeRequest != null) activeRequest.Dispose();
            activeRequest = null;
        }

        private void OnDisable() { Cancel(); }
    }
}
