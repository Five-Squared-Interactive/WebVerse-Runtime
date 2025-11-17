using Newtonsoft.Json;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Data
{
    public class AsyncJSON
    {
        public static void Parse(string rawText, string onComplete, object context = null)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                var result = JsonConvert.DeserializeObject<dynamic>(rawText);
                if (context != null)
                {
                    DataAPIHelper.QueueJavascript(onComplete, new object[] { result, context });
                }
                else
                {
                    DataAPIHelper.QueueJavascript(onComplete, new object[] { result });
                }
            });
        }

        public static void Stringify(dynamic jsonObject, string onComplete, object context = null)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                string result = JsonConvert.SerializeObject(jsonObject);
                if (context != null)
                {
                    DataAPIHelper.QueueJavascript(onComplete, new object[] { result, context });
                    return;
                }
                else
                {
                    DataAPIHelper.QueueJavascript(onComplete, new object[] { result });
                }
            });
        }
    }
}