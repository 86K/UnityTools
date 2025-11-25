using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Tools
{
    public static class HttpHelper
    {
        /* UnityWebRequest > HttpClient > HttpWebRequest */

        /// <summary>
        /// 推荐此方法 UnityWebRequest -> post(json)
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="json"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async UniTask<string> Post(string uri, string json, float timeout = 3f)
        {
            string result = string.Empty;
            UnityWebRequest request = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST);
            // 添加header
            request.SetRequestHeader("Content-Type", "application/json");

            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                await request.SendWebRequest().ToUniTask().Timeout(TimeSpan.FromSeconds(timeout));

                if (request.isDone && string.IsNullOrEmpty(request.error))
                {
                    result = request.downloadHandler.text;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Post exception:\n{e}");
            }
            finally
            {
                request.Dispose();
            }

            return result;
        }
        

        /// <summary>
        /// [] UnityWebRequest + Post + WWWForm
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="wwwForm"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async UniTask<string> Post(string uri, WWWForm wwwForm, float timeout = 3f)
        {
            string result = string.Empty;
            UnityWebRequest request = UnityWebRequest.Post(uri, wwwForm);

            try
            {
                await request.SendWebRequest().ToUniTask().Timeout(TimeSpan.FromSeconds(timeout));

                if (request.isDone && string.IsNullOrEmpty(request.error))
                {
                    result = request.downloadHandler.text;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Post exceptioned:\n{e}");
            }
            finally
            {
                request.Dispose();
            }

            return result;
        }

        /// <summary>
        /// [√] HttpClient + Post  + MultipartFormDataContent
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="formData"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [Obsolete]
        public static async UniTask<string> Post(string uri, MultipartFormDataContent formData, float timeout = 3f)
        {
            // // eg：
            // formData.Add(new StringContent("contentValue", Encoding.UTF8), "key");// 添加String Content
            // formData.Add(new StreamContent(null), "key", "fileName");// 添加Stream Content(不带Progress)
            // formData.Add(new StreamProgressContent(null, 4096, null), "file", "fileName"); // 带progress

            string result = string.Empty;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeout);

            try
            {
                var response = await client.PostAsync(uri, formData);

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Post exceptioned:\n{e}");
            }
            finally
            {
                client.Dispose();
            }

            return result;
        }

        /// <summary>
        /// [] UnityWebRequest + Json + Headers
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="json"></param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async UniTask<string> Post(string uri, string json, Dictionary<string, string> headers, float timeout = 3f)
        {
            string result = string.Empty;
            UnityWebRequest request = UnityWebRequest.PostWwwForm(uri, "POST");
            if (headers != null)
            {
                if (headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        request.SetRequestHeader(header.Key, header.Value);
                    }
                }
            }

            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                await request.SendWebRequest().ToUniTask().Timeout(TimeSpan.FromSeconds(timeout));

                if (request.isDone && string.IsNullOrEmpty(request.error))
                {
                    result = request.downloadHandler.text;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Post exception:\n{e}");
            }
            finally
            {
                request.Dispose();
            }

            return result;
        }

        /// <summary>
        /// [] HttpWebRequest + Json
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="json"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [Obsolete]
        public static async UniTask<string> PostY(string uri, string json, float timeout = 3f)
        {
            string result = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.Timeout = (int)(timeout * 1000);
            request.ContentType = "application/json";
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            request.ContentLength = bytes.Length;

            try
            {
                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                await using Stream responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    using StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    result = await reader.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Post exceptioned:\n{e}");
            }
            
            return result;
        }


        /// <summary>
        /// [] UnityWebRequest + Uri
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async UniTask<string> Get(string uri, Dictionary<string, string> headers, float timeout = 3f)
        {
            string result = string.Empty;
            UnityWebRequest request = UnityWebRequest.Get(uri);
            if (headers != null)
            {
                if (headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        request.SetRequestHeader(header.Key, header.Value);
                    }
                }
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                await request.SendWebRequest().ToUniTask().Timeout(TimeSpan.FromSeconds(timeout));
                
                if (request.isDone && string.IsNullOrEmpty(request.error))
                {
                    result = request.downloadHandler.text;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Get exceptioned:\n{e}");
            }
            finally
            {
                request.Dispose();
            }

            return result;
        }

        /// <summary>
        /// [] HttpClient + Uri
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [Obsolete]
        public static async UniTask<string> Get(string uri, float timeout = 3f)
        {
            string result = string.Empty;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeout);

            try
            {
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Get exception:\n{e}");
            }
            finally
            {
                client.Dispose();
            }

            return result;
        }
        
        /// <summary>
        /// [] HttpWebRequest + Uri
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [Obsolete]
        public static async UniTask<string> GetAsync(string uri, float timeout = 3f)
        {
            string result = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Timeout = (int)(timeout * 1000);

            try
            {
                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                await using Stream responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    using StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    result = await reader.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Get exception:\n{e}");
            }

            return result;
        }
    }
}