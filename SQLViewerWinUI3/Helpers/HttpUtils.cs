using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SQLViewerWinUI3.Helpers
{
    public static class HttpUtils
    {
        private static string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36";
        private static HttpClient? _httpClient;
        
        private static HttpClient GetHttpClient()
        {
            if (_httpClient == null)
            {
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                };
                _httpClient = new HttpClient(handler);
                _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            }
            return _httpClient;
        }

        public static bool Login(string username, string password)
        {
            try
            {
                string? csrftoken = ConfigHelper.Get("csrftoken");
                if (csrftoken == null)
                {
                    csrftoken = "0w8mYnqK82gNrkNmgs9CIn3UaaHpmaxY";
                }

                var formData = new Dictionary<string, string>
                {
                    ["username"] = username,
                    ["password"] = password
                };

                string url = "https://sql-out.sdcreditech.com/authenticate/";
                
                var httpClient = GetHttpClient();
                var handler = httpClient as HttpClientHandler;
                var cookieContainer = (handler as HttpClientHandler)?.CookieContainer ?? new CookieContainer();
                
                var uri = new Uri(url);
                cookieContainer.Add(uri, new Cookie("csrftoken", csrftoken));

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("x-csrftoken", csrftoken);
                request.Content = new FormUrlEncodedContent(formData);

                var response = httpClient.Send(request);
                
                if (response.IsSuccessStatusCode)
                {
                    // Extract cookies
                    var cookies = cookieContainer.GetCookies(uri);
                    foreach (Cookie cookie in cookies)
                    {
                        if (cookie.Name.Equals("csrftoken", StringComparison.OrdinalIgnoreCase))
                            ConfigHelper.Set("csrftoken", cookie.Value);
                        else if (cookie.Name.Equals("sessionid", StringComparison.OrdinalIgnoreCase))
                            ConfigHelper.Set("sessionid", cookie.Value);
                    }

                    string result = response.Content.ReadAsStringAsync().Result;
                    JObject? jo = JsonConvert.DeserializeObject<JObject>(result);
                    if (jo != null)
                    {
                        int status = (int)(jo["status"] ?? -1);
                        return status == 0;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static List<string>? QueryServer()
        {
            try
            {
                string? csrftoken = ConfigHelper.Get("csrftoken");
                string? sessionid = ConfigHelper.Get("sessionid");

                if (string.IsNullOrEmpty(csrftoken) || string.IsNullOrEmpty(sessionid))
                    return null;

                string res = Get("https://sql-out.sdcreditech.com/group/user_all_instances/?tag_codes%5B%5D=can_read", csrftoken, sessionid);
                JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
                
                if (jo == null)
                    return null;

                JArray? ja = jo["data"] as JArray;
                List<string> list = new List<string>();
                
                if (ja != null)
                {
                    foreach (JObject item in ja)
                    {
                        string? instanceName = item["instance_name"]?.ToString();
                        if (instanceName != null)
                            list.Add(instanceName);
                    }
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        public static List<string> QueryDb(string instanceName)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");

            string res = Get($"https://sql-out.sdcreditech.com/instance/instance_resource/?instance_name={instanceName}&resource_type=database", csrftoken ?? "", sessionid ?? "");
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            JArray? ja = jo?["data"] as JArray;
            
            List<string> list = new List<string>();
            if (ja != null)
            {
                foreach (var item in ja)
                {
                    string? itemStr = item?.ToString();
                    if (itemStr != null)
                        list.Add(itemStr);
                }
            }
            return list;
        }

        public static List<string> QueryTable(string dbName)
        {
            string? instanceName = ConfigHelper.Get("instance_name");
            return QueryTable(instanceName ?? "", dbName);
        }

        public static List<string> QueryTable(string instanceName, string dbName)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");

            string res = Get($"https://sql-out.sdcreditech.com/instance/instance_resource/?instance_name={instanceName}&db_name={dbName}&resource_type=table", csrftoken ?? "", sessionid ?? "");
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            JArray? ja = jo?["data"] as JArray;
            
            List<string> list = new List<string>();
            if (ja != null)
            {
                foreach (var item in ja)
                {
                    string? itemStr = item?.ToString();
                    if (itemStr != null)
                        list.Add(itemStr);
                }
            }
            return list;
        }

        public static List<string> QueryColumn(string instanceName, string dbName, string tableName)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");

            string res = Get($"https://sql-out.sdcreditech.com/instance/instance_resource/?instance_name={instanceName}&db_name={dbName}&tb_name={tableName}&resource_type=column", csrftoken ?? "", sessionid ?? "");
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            JArray? ja = jo?["data"] as JArray;
            
            List<string> list = new List<string>();
            if (ja != null)
            {
                foreach (var item in ja)
                {
                    string? itemStr = item?.ToString();
                    if (itemStr != null)
                        list.Add(itemStr);
                }
            }
            return list;
        }

        public static string ShowTable(string instanceName, string dbName, string tableName)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");
            
            var formData = new Dictionary<string, string>
            {
                ["instance_name"] = instanceName,
                ["db_name"] = dbName,
                ["schema_name"] = "",
                ["tb_name"] = tableName
            };
            
            string res = Post("https://sql-out.sdcreditech.com/instance/describetable/", formData, csrftoken ?? "", sessionid ?? "");
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            string? table = jo?["data"]?["rows"]?[0]?[1]?.ToString();
            return table ?? "";
        }

        public static JObject QuerySql(string instanceName, string dbName, string tableName, string sql)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");
            string? pageSize = ConfigHelper.Get("pageSize");
            
            var formData = new Dictionary<string, string>
            {
                ["instance_name"] = instanceName,
                ["db_name"] = dbName,
                ["schema_name"] = "",
                ["tb_name"] = tableName,
                ["sql_content"] = sql,
                ["limit_num"] = pageSize ?? "100"
            };
            
            string res = Post("https://sql-out.sdcreditech.com/query/", formData, csrftoken ?? "", sessionid ?? "");
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            return jo ?? new JObject();
        }

        public static int CountBySql(string instanceName, string dbName, string tableName)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");
            string? pageSize = ConfigHelper.Get("pageSize");
            
            var formData = new Dictionary<string, string>
            {
                ["instance_name"] = instanceName,
                ["db_name"] = dbName,
                ["schema_name"] = "",
                ["tb_name"] = tableName,
                ["sql_content"] = $"select count(1) from {tableName}",
                ["limit_num"] = pageSize ?? "100"
            };
            
            string res = Post("https://sql-out.sdcreditech.com/query/", formData, csrftoken ?? "", sessionid ?? "");
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            int count = (int)(jo?["data"]?["rows"]?[0]?[0] ?? 0);
            return count;
        }

        private static string Get(string url, string csrftoken, string sessionid)
        {
            var httpClient = GetHttpClient();
            var handler = httpClient as HttpClientHandler;
            var cookieContainer = (handler as HttpClientHandler)?.CookieContainer ?? new CookieContainer();
            
            var uri = new Uri(url);
            cookieContainer.Add(uri, new Cookie("csrftoken", csrftoken));
            cookieContainer.Add(uri, new Cookie("sessionid", sessionid));

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-csrftoken", csrftoken);

            var response = httpClient.Send(request);
            return response.Content.ReadAsStringAsync().Result;
        }

        private static string Post(string url, Dictionary<string, string> formData, string csrftoken, string sessionid)
        {
            var httpClient = GetHttpClient();
            var handler = httpClient as HttpClientHandler;
            var cookieContainer = (handler as HttpClientHandler)?.CookieContainer ?? new CookieContainer();
            
            var uri = new Uri(url);
            cookieContainer.Add(uri, new Cookie("csrftoken", csrftoken));
            cookieContainer.Add(uri, new Cookie("sessionid", sessionid));

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-csrftoken", csrftoken);
            request.Content = new FormUrlEncodedContent(formData);

            var response = httpClient.Send(request);
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
