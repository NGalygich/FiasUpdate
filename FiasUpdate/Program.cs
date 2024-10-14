using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.IO.Compression;

namespace FiasUpdate
{
    class Program
    {
        //static void Main(string[] args)
        //{

        //}
        static HttpClient httpClient = new HttpClient();

        static async Task Main()
        {
            //// определяем данные запроса
            //using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com");

            //// получаем ответ
            //using HttpResponseMessage response = await httpClient.SendAsync(request);

            //// просматриваем данные ответа
            //// статус
            //Console.WriteLine($"Status: {response.StatusCode}\n");
            ////заголовки
            //Console.WriteLine("Headers");
            //foreach (var header in response.Headers)
            //{
            //    Console.Write($"{header.Key}:");
            //    foreach (var headerValue in header.Value)
            //    {
            //        Console.WriteLine(headerValue);
            //    }
            //}
            //// содержимое ответа
            //Console.WriteLine("\nContent");
            //string content = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(content);

            //using (var client = new WebClient())
            //{
            //    client.DownloadFile("https://fias.nalog.ru/Public/Downloads/Actual/gar_delta_xml.zip", @"D:\Downloads\1.zip");
            //}

            var archivePath = "https://fias.nalog.ru/Public/Downloads/Actual/gar_delta_xml.zip";
            using (var client = new WebClient())
            {
                //client.DownloadFile(archivePath, "./fias.zip");
                //ZipFile.ExtractToDirectory("./fias.zip", "./extract");

            }

        }
    }
}
