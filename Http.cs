using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace ExamTask
{
    internal class Http
    {
        private Thread _serverThread;
        private string _siteDirectory;

        private HttpListener _listener;

        private int _port;

        public Http(string path, int port)

        {

            this.Initialize(path, port);

        }
        private void Initialize(string path, int port)

        {

            _siteDirectory = path;

            _port = port;

            _serverThread = new Thread(Listen);

            _serverThread.Start();

            Console.WriteLine($"Сервер запущен на порту: {port}");

            Console.WriteLine($"Файлы сайта лежат в папке: {path}");

        }
        public void Stop()

        {

            _serverThread.Abort();

            _listener.Stop();

        }
        private void Listen()

        {

            _listener = new HttpListener();

            _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");

            _listener.Start();

            while (true)

            {

                try

                {

                    // Пришел запрос получаем так называемый контекст запроса

                    HttpListenerContext context = _listener.GetContext();


                    // И передаем этот контекст на обработку

                    Process(context);

                }

                catch (Exception e)

                {

                    Console.WriteLine(e.Message);

                }

            }

        }
        private void Process(HttpListenerContext context)

        {

            string filename = context.Request.Url.AbsolutePath;

            Console.WriteLine(filename);


            // Убираем слэш в начале имени

            filename = filename.Substring(1);


            // Формируем полный путь к файлу

            filename = Path.Combine(_siteDirectory, filename);

            string content = "";

            string query = context.Request.Url.Query;
           
            if (filename.Contains("html"))
               
            {
                content = BuildHtml(filename, query);
                if (context.Request.HttpMethod == "POST" && filename.Contains("index"))
                {
                    StreamReader reader = new StreamReader(context.Request.InputStream);
                    string c = reader.ReadToEnd();
                    
                }


            }

            else

            {

                content = File.ReadAllText(filename);

            }

            if (File.Exists(filename))

            {

                try

                {

                    byte[] htmlBytes = System.Text.Encoding.UTF8.GetBytes(content);

                    Stream fileStream = new MemoryStream(htmlBytes);



                    context.Response.ContentType = GetContentType(filename);



                    context.Response.ContentLength64 = fileStream.Length;


                    byte[] buffer = new byte[16 * 1024];


                    int dataLength;
                  
                    
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                    do

                    {


                        dataLength = fileStream.Read(buffer, 0, buffer.Length);


                        context.Response.OutputStream.Write(buffer, 0, dataLength);

                    } while (dataLength > 0);


                    fileStream.Close();

                    context.Response.OutputStream.Flush();

                }

                catch (Exception e)

                {

                    Console.WriteLine(e.Message);

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                }



            }

            else

            {

                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            }


            context.Response.OutputStream.Close();

        }
        private string GetContentType(string filename)

        {

            var dictionary = new Dictionary<string, string> {

            {".css",  "text/css"},

            {".html", "text/html"},

            {".ico",  "image/x-icon"},

            {".js",   "application/x-javascript"},

            {".json", "application/json"},

            {".png",  "image/png"}

        }; string contentType = "";

            string fileExtension = Path.GetExtension(filename);

            dictionary.TryGetValue(fileExtension, out contentType);

            return contentType;

        }
        private string BuildHtml(string filename, object result)

        {

            string html = "";

            string layoutPath = Path.Combine(_siteDirectory, "layout.html");

            string filePath = Path.Combine(_siteDirectory, filename);


            var razorService = Engine.Razor; // Подключаем движок


            if (!razorService.IsTemplateCached("layout", null)) // Проверяем наличие базового шаблона в кэше

                razorService.AddTemplate("layout", File.ReadAllText(layoutPath)); //Добавляем его если отсутствует




            if (!razorService.IsTemplateCached(filename, null))//Находим шаблон страницы который будет вложен в базовый

            {

                razorService.AddTemplate(filename, File.ReadAllText(filePath));

                razorService.Compile(filename);

            }


            html = razorService.Run(filename, null, new
            {
                Result = result
            });

            return html;

        }
    }
}

