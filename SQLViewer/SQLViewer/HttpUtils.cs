using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Drawing.Printing;

namespace SQLViewer
{
    class HttpUtils
    {

        private static string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36";

        public static bool login(string username, string password)
        {
            string csrftoken = ConfigHelper.Get("csrftoken");
            if (csrftoken == null)
            {
                csrftoken = "0w8mYnqK82gNrkNmgs9CIn3UaaHpmaxY";
            }
            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("username", username);
            formData.Add("password", password);

            string url = "https://sql-out.sdcreditech.com/authenticate/";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            req.Headers.Add("x-csrftoken", csrftoken);
            req.UserAgent = userAgent;


            CookieContainer cookies = new CookieContainer();
            Uri uri = new Uri(url);
            cookies.Add(new Cookie("csrftoken", csrftoken) { Domain = uri.Host });
            
            req.CookieContainer = cookies;

            string postData = string.Join("&", formData.Select(
                kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"
            ));
            byte[] dataBytes = Encoding.UTF8.GetBytes(postData);
            req.ContentLength = dataBytes.Length;

            // 发送请求
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(dataBytes, 0, dataBytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                // 先判断状态码是否成功
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // 从响应头的 Set-Cookie 中获取 csrftoken 和 sessionid
                    foreach (Cookie cookie in response.Cookies)
                    {
                        if (cookie.Name.Equals("csrftoken", StringComparison.OrdinalIgnoreCase))
                            ConfigHelper.Set("csrftoken", cookie.Value);
                        else if (cookie.Name.Equals("sessionid", StringComparison.OrdinalIgnoreCase))
                            ConfigHelper.Set("sessionid", cookie.Value);
                    }

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        String ret = reader.ReadToEnd();
                        string result = ret.ToString();
                        JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                        int status = (int)jo["status"];
                        return status == 0;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public static List<string> queryServer()
        {
            string csrftoken = ConfigHelper.Get("csrftoken");
            string sessionid = ConfigHelper.Get("sessionid");

            string res = HttpUtils.Get("https://sql-out.sdcreditech.com/group/user_all_instances/?tag_codes%5B%5D=can_read", csrftoken, sessionid);
            JObject jo = null;
            try
            {
                jo = (JObject)JsonConvert.DeserializeObject(res);
            }
            catch(Exception e) { 
                return null;
            }
            
            JArray ja = (JArray)jo["data"];
            List<string> list = new List<string>();
            foreach (JObject item in ja)
            {
                list.Add(item["instance_name"].ToString());
            }
            return list;
        }

        public static List<string> queryDb(string instanceName)
        {
            string csrftoken = ConfigHelper.Get("csrftoken");
            string sessionid = ConfigHelper.Get("sessionid");

            string res = HttpUtils.Get("https://sql-out.sdcreditech.com/instance/instance_resource/?instance_name="+ instanceName + "&resource_type=database", csrftoken, sessionid);
            JObject jo = (JObject)JsonConvert.DeserializeObject(res);
            JArray ja = (JArray)jo["data"];
            List<string> list = new List<string>();
            foreach (object item in ja)
            {
                list.Add(item.ToString());
            }
            return list;
        }

        public static List<string> queryTable(string dbName)
        {
            string instanceName = ConfigHelper.Get("instance_name");
            return queryTable(instanceName, dbName);
        }

        public static List<string> queryTable(string instanceName, string dbName)
        {
            string csrftoken = ConfigHelper.Get("csrftoken");
            string sessionid = ConfigHelper.Get("sessionid");

            string res = HttpUtils.Get("https://sql-out.sdcreditech.com/instance/instance_resource/?instance_name=" + instanceName + "&db_name=" + dbName + "&resource_type=table", csrftoken, sessionid);
            JObject jo = (JObject)JsonConvert.DeserializeObject(res);
            JArray ja = (JArray)jo["data"];
            List<string> list = new List<string>();
            foreach (object item in ja)
            {
                list.Add(item.ToString());
            }
            return list;
        }

        public static List<string> queryColumn(string instanceName, string dbName, string tableName)
        {
            string csrftoken = ConfigHelper.Get("csrftoken");
            string sessionid = ConfigHelper.Get("sessionid");

            string res = HttpUtils.Get("https://sql-out.sdcreditech.com/instance/instance_resource/?instance_name=" + instanceName + "&db_name=" + dbName + "&tb_name=" + tableName + "&resource_type=column", csrftoken, sessionid);
            JObject jo = (JObject)JsonConvert.DeserializeObject(res);
            JArray ja = (JArray)jo["data"];
            List<string> list = new List<string>();
            foreach (object item in ja)
            {
                list.Add(item.ToString());
            }
            return list;
        }

        public static string showTable(string instanceName, string dbName, string tableName)
        {
            string csrftoken = ConfigHelper.Get("csrftoken");
            string sessionid = ConfigHelper.Get("sessionid");
            //string instanceName = ConfigHelper.Get("instance_name");
            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("instance_name", instanceName);
            formData.Add("db_name", dbName);
            formData.Add("schema_name", "");
            formData.Add("tb_name", tableName);
            string res = HttpUtils.Post("https://sql-out.sdcreditech.com/instance/describetable/", formData, csrftoken, sessionid);
            JObject jo = (JObject)JsonConvert.DeserializeObject(res);
            string table = (string)jo["data"]["rows"][0][1];
            return table;
        }

        public static JObject querySql(string instanceName, string dbName,string tableName,string sql)
        {
            string csrftoken = ConfigHelper.Get("csrftoken");
            string sessionid = ConfigHelper.Get("sessionid");
            //string instanceName = ConfigHelper.Get("instance_name");
            string pageSize = ConfigHelper.Get("pageSize");
            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("instance_name", instanceName);
            formData.Add("db_name", dbName);
            formData.Add("schema_name", "");
            formData.Add("tb_name", tableName);
            //"select id,channel_source from isd_product_apply_flow_tg order by id"
            formData.Add("sql_content", sql);
            formData.Add("limit_num", pageSize);
            string res = HttpUtils.Post("https://sql-out.sdcreditech.com/query/", formData, csrftoken, sessionid);
            JObject jo = (JObject)JsonConvert.DeserializeObject(res);
            return jo;
        }

        public static int countBySql(string instanceName, string dbName, string tableName)
        {
            string csrftoken = ConfigHelper.Get("csrftoken");
            string sessionid = ConfigHelper.Get("sessionid");
            //string instanceName = ConfigHelper.Get("instance_name");
            string pageSize=ConfigHelper.Get("pageSize");
            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("instance_name", instanceName);
            formData.Add("db_name", dbName);
            formData.Add("schema_name", "");
            formData.Add("tb_name", tableName);
            //"select id,channel_source from isd_product_apply_flow_tg order by id"
            formData.Add("sql_content", "select count(1) from "+tableName);
            formData.Add("limit_num", pageSize);
            string res = HttpUtils.Post("https://sql-out.sdcreditech.com/query/", formData, csrftoken, sessionid);
            JObject jo = (JObject)JsonConvert.DeserializeObject(res);
            int count = (int)jo["data"]["rows"][0][0];
            return count;
        }


        public static string Get(string url, string csrftoken, string sessionid)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Headers.Add("x-csrftoken", csrftoken);
            req.UserAgent = userAgent;

            CookieContainer cookies = new CookieContainer();
            Uri uri = new Uri(url);
            cookies.Add(new Cookie("csrftoken", csrftoken) { Domain = uri.Host });
            cookies.Add(new Cookie("sessionid", sessionid) { Domain = uri.Host });
            req.CookieContainer = cookies;

            
            HttpWebResponse myResponse = (HttpWebResponse)req.GetResponse();
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            string returnXml = reader.ReadToEnd();//利用StreamReader就可以从响应内容从头读到尾
            reader.Close();
            myResponse.Close();
            return returnXml;
        }

        public static string Post(string url, Dictionary<string, string> formData, string csrftoken,string sessionid)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            req.Headers.Add("x-csrftoken", csrftoken);
            req.UserAgent = userAgent;

            CookieContainer cookies = new CookieContainer();
            Uri uri = new Uri(url);
            cookies.Add(new Cookie("csrftoken", csrftoken) { Domain = uri.Host });
            cookies.Add(new Cookie("sessionid", sessionid) { Domain = uri.Host });
            req.CookieContainer = cookies;

            string postData = string.Join("&", formData.Select(
                kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"
            ));
            byte[] dataBytes = Encoding.UTF8.GetBytes(postData);
            req.ContentLength = dataBytes.Length;

            Stream stream;
            stream = req.GetRequestStream();
            stream.Write(dataBytes, 0, dataBytes.Length);
            stream.Close();
            using (var httpWebResponse = req.GetResponse())
            using (StreamReader responseStream = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                String ret = responseStream.ReadToEnd();
                string result = ret.ToString();
                return result;
            }
        }
    }
}
