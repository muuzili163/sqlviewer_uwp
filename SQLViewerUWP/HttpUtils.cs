using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SQLViewerUWP
{
    class HttpUtils
    {
        private static readonly HttpClient _httpClient;
        private static readonly CookieContainer _cookieContainer;
        private static readonly string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36";

        static HttpUtils()
        {
            _cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                UseCookies = true
            };
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
        }

        public static async Task<bool> LoginAsync(string username, string password)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            if (string.IsNullOrEmpty(csrftoken))
            {
                csrftoken = "0w8mYnqK82gNrkNmgs9CIn3UaaHpmaxY";
            }

            var formData = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };

            string url = "https://sql-out.sdcreditech.com/authenticate/";

            try
            {
                var uri = new Uri(url);
                _cookieContainer.Add(uri, new Cookie("csrftoken", csrftoken));

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("x-csrftoken", csrftoken);
                request.Content = new FormUrlEncodedContent(formData);

                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    // Get cookies from response
                    var cookies = _cookieContainer.GetCookies(uri);
                    foreach (Cookie cookie in cookies)
                    {
                        if (cookie.Name.Equals("csrftoken", StringComparison.OrdinalIgnoreCase))
                            ConfigHelper.Set("csrftoken", cookie.Value);
                        else if (cookie.Name.Equals("sessionid", StringComparison.OrdinalIgnoreCase))
                            ConfigHelper.Set("sessionid", cookie.Value);
                    }

                    string result = await response.Content.ReadAsStringAsync();
                    JObject? jo = JsonConvert.DeserializeObject<JObject>(result);
                    int status = jo?["status"]?.Value<int>() ?? -1;
                    return status == 0;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<List<string>?> QueryServerAsync()
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");

            if (string.IsNullOrEmpty(csrftoken) || string.IsNullOrEmpty(sessionid))
                return null;

            try
            {
                string res = await GetAsync("https://sql-out.sdcreditech.com/group/user_all_instances/?tag_codes%5B%5D=can_read", csrftoken, sessionid);
                JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
                JArray? ja = jo?["data"] as JArray;
                
                if (ja == null)
                    return null;

                List<string> list = new List<string>();
                foreach (JObject item in ja)
                {
                    string? instanceName = item["instance_name"]?.ToString();
                    if (instanceName != null)
                        list.Add(instanceName);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<string>> QueryDbAsync(string instanceName)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");

            string res = await GetAsync($"https://sql-out.sdcreditech.com/instance/instance_resource/?instance_name={instanceName}&resource_type=database", csrftoken!, sessionid!);
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            JArray? ja = jo?["data"] as JArray;
            
            List<string> list = new List<string>();
            if (ja != null)
            {
                foreach (var item in ja)
                {
                    string? value = item?.ToString();
                    if (value != null)
                        list.Add(value);
                }
            }
            return list;
        }

        public static async Task<List<string>> QueryTableAsync(string dbName)
        {
            string? instanceName = ConfigHelper.Get("instance_name");
            return await QueryTableAsync(instanceName!, dbName);
        }

        public static async Task<List<string>> QueryTableAsync(string instanceName, string dbName)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");

            string res = await GetAsync($"https://sql-out.sdcreditech.com/instance/instance_resource/?instance_name={instanceName}&db_name={dbName}&resource_type=table", csrftoken!, sessionid!);
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            JArray? ja = jo?["data"] as JArray;
            
            List<string> list = new List<string>();
            if (ja != null)
            {
                foreach (var item in ja)
                {
                    string? value = item?.ToString();
                    if (value != null)
                        list.Add(value);
                }
            }
            return list;
        }

        public static async Task<List<string>> QueryColumnAsync(string instanceName, string dbName, string tableName)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");

            string res = await GetAsync($"https://sql-out.sdcreditech.com/instance/instance_resource/?instance_name={instanceName}&db_name={dbName}&tb_name={tableName}&resource_type=column", csrftoken!, sessionid!);
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            JArray? ja = jo?["data"] as JArray;
            
            List<string> list = new List<string>();
            if (ja != null)
            {
                foreach (var item in ja)
                {
                    string? value = item?.ToString();
                    if (value != null)
                        list.Add(value);
                }
            }
            return list;
        }

        public static async Task<string> ShowTableAsync(string instanceName, string dbName, string tableName)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");
            
            var formData = new Dictionary<string, string>
            {
                { "instance_name", instanceName },
                { "db_name", dbName },
                { "schema_name", "" },
                { "tb_name", tableName }
            };

            string res = await PostAsync("https://sql-out.sdcreditech.com/instance/describetable/", formData, csrftoken!, sessionid!);
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            string? table = jo?["data"]?["rows"]?[0]?[1]?.ToString();
            return table ?? "";
        }

        public static async Task<JObject> QuerySqlAsync(string instanceName, string dbName, string tableName, string sql)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");
            string? pageSize = ConfigHelper.Get("pageSize");
            
            var formData = new Dictionary<string, string>
            {
                { "instance_name", instanceName },
                { "db_name", dbName },
                { "schema_name", "" },
                { "tb_name", tableName },
                { "sql_content", sql },
                { "limit_num", pageSize ?? "100" }
            };

            string res = await PostAsync("https://sql-out.sdcreditech.com/query/", formData, csrftoken!, sessionid!);
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            return jo ?? new JObject();
        }

        public static async Task<int> CountBySqlAsync(string instanceName, string dbName, string tableName)
        {
            string? csrftoken = ConfigHelper.Get("csrftoken");
            string? sessionid = ConfigHelper.Get("sessionid");
            string? pageSize = ConfigHelper.Get("pageSize");
            
            var formData = new Dictionary<string, string>
            {
                { "instance_name", instanceName },
                { "db_name", dbName },
                { "schema_name", "" },
                { "tb_name", tableName },
                { "sql_content", $"select count(1) from {tableName}" },
                { "limit_num", pageSize ?? "100" }
            };

            string res = await PostAsync("https://sql-out.sdcreditech.com/query/", formData, csrftoken!, sessionid!);
            JObject? jo = JsonConvert.DeserializeObject<JObject>(res);
            int count = jo?["data"]?["rows"]?[0]?[0]?.Value<int>() ?? 0;
            return count;
        }

        private static async Task<string> GetAsync(string url, string csrftoken, string sessionid)
        {
            var uri = new Uri(url);
            _cookieContainer.Add(uri, new Cookie("csrftoken", csrftoken));
            _cookieContainer.Add(uri, new Cookie("sessionid", sessionid));

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-csrftoken", csrftoken);

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> PostAsync(string url, Dictionary<string, string> formData, string csrftoken, string sessionid)
        {
            var uri = new Uri(url);
            _cookieContainer.Add(uri, new Cookie("csrftoken", csrftoken));
            _cookieContainer.Add(uri, new Cookie("sessionid", sessionid));

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-csrftoken", csrftoken);
            request.Content = new FormUrlEncodedContent(formData);

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        // Synchronous wrappers for backwards compatibility
        public static bool login(string username, string password) => LoginAsync(username, password).Result;
        public static List<string>? queryServer() => QueryServerAsync().Result;
        public static List<string> queryDb(string instanceName) => QueryDbAsync(instanceName).Result;
        public static List<string> queryTable(string dbName) => QueryTableAsync(dbName).Result;
        public static List<string> queryTable(string instanceName, string dbName) => QueryTableAsync(instanceName, dbName).Result;
        public static List<string> queryColumn(string instanceName, string dbName, string tableName) => QueryColumnAsync(instanceName, dbName, tableName).Result;
        public static string showTable(string instanceName, string dbName, string tableName) => ShowTableAsync(instanceName, dbName, tableName).Result;
        public static JObject querySql(string instanceName, string dbName, string tableName, string sql) => QuerySqlAsync(instanceName, dbName, tableName, sql).Result;
        public static int countBySql(string instanceName, string dbName, string tableName) => CountBySqlAsync(instanceName, dbName, tableName).Result;
    }
}
