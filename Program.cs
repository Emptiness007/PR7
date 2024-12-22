using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpNewsPAT
{
    internal class Program
    {
        private static string logFilePath = "debug_log.txt";
        private static HttpClient httpClient = new HttpClient(new HttpClientHandler() { UseCookies = true});
        static async Task Main(string[] args)
        {
            var cookies = await SignIn();
            await GetContent();

            Console.WriteLine("Введите имя записи:");
            string name = Console.ReadLine();

            Console.WriteLine("Введите описание записи:");
            string description = Console.ReadLine();

            Console.WriteLine("Введите URL изображения:");
            string imageUrl = Console.ReadLine();

            await AddRecord(name, description, imageUrl);

            Console.Read();
        }
        public static async Task<CookieContainer> SignIn()
        {
            string url = "http://localhost/ajax/login.php";
            Console.WriteLine($"Выполняем запрос: {url}");
            var postData = new StringContent("login=admin&password=admin", Encoding.ASCII, "application/x-www-form-urlencoded");

            HttpResponseMessage response = await httpClient.PostAsync(url, postData);

            Console.WriteLine($"Статус выполнения: {response.StatusCode}");
            string responseFromServer = await response.Content.ReadAsStringAsync();
            Log(responseFromServer);

            var cookies = response.Headers.GetValues("Set-Cookie");
            Console.WriteLine("Полученные печеньки: ");
            foreach (var cookie in cookies)
            {
                Console.WriteLine(cookie);
            }
            return null;
        }
        public static async Task GetContent()
        {
            string url = "http://localhost/main";
            
            HttpResponseMessage response = await httpClient.GetAsync(url);
            Console.WriteLine($"Статус выполнения: {response.StatusCode}");
            string responseFromServer = await response.Content.ReadAsStringAsync();
            ParsingHtml(responseFromServer);
        }
        public static void ParsingHtml(string htmlCode)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlCode);
            var Document = html.DocumentNode;
            var DivsNews = Document.Descendants(0).Where(n => n.HasClass("news"));
            foreach (HtmlNode DivNews in DivsNews)
            {
                var src = DivNews.ChildNodes[1].GetAttributeValue("src", "none");
                var name = DivNews.ChildNodes[3].InnerText;
                var description = DivNews.ChildNodes[5].InnerText;
                Log(name + "\n" + "Изображение: " + src + "\n" + "Описание: " + description + "\n");
            }
        }
        public static async Task AddRecord(string name, string description, string imageUrl)
        {
            string url = "http://localhost/ajax/add.php";
            Trace.WriteLine($"Выполняем запрос: {url}");
            var formData = new Dictionary<string, string>
            {
                { "name", name },
                { "description", description },
                { "src", imageUrl }
            };
            var content = new FormUrlEncodedContent(formData);
            HttpResponseMessage response = await httpClient.PostAsync(url, content);
            Trace.WriteLine($"Статус выполнения: {response.StatusCode}");
            string responseFromServer = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ответ сервера: {responseFromServer}");
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Запись успешно добавлена.");
            }
            else
            {
                Console.WriteLine($"Ошибка при добавлении записи: {response.StatusCode}, сообщение: {responseFromServer}");
            }
        }
        private static void Log(string message)
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
                Debug.WriteLine(message);
            }
        }
    }
}
