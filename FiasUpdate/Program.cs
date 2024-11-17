using System;

using System.Threading.Tasks;
using System.Net;
using System.IO.Compression;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using FIAS.Core.API;

using FiasUpdate.Models;
using FiasUpdate.Readers;


using System.Collections.Generic;

using System.IO;
using System.Linq;

using System.Threading;


using System.Text;




//using FiasUpdate.Properties;
using JANL;
//using System;
using System.ComponentModel;
//using System.Windows.Forms;

using System.Data;


namespace FiasUpdate
{
    class Program
    {
        FIASClient Client = new FIASClient();
        private static readonly ImportDeltaOptions Options = new ImportDeltaOptions();

        static string path1 = @"C:\SystemTasks\WPF_Projects\UpdateFias";
        static string subpath = @"UpdateFiasLog";
        static DirectoryInfo dirInfo;

        static Microsoft.SqlServer.Management.Smo.Database DB;

        private static void SBC_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            SqlBulkCopy SBC = (SqlBulkCopy)sender;
            var SBCCount = (int)e.RowsCopied;
            //SP.Report(new TaskProgress(SBCCount, SBCCount));
            if (SBCCount >= 10000 && SBC.NotifyAfter != 1000) { SBC.NotifyAfter = 1000; }
        }

        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 20 * 60 * 1000;
                return w;
            }
        }
        static async Task Main(string[] args)
        {
            string connectionString = FIASProperties.SQLConnection; //"Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;";
            //string connectionString2 = FIASProperties.SQLConnection2; //"Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;";
            string dirName = @"./gar_delta_xml";
            //FIASArchive Archive = new FIASArchive();
            //ImportDeltaOptions Options;
            string ArchivePath = @"./gar_delta_xml.zip";


            string ExtractPath = $@"./gar_delta_xml";

            StringBuilder stringBuilder = new StringBuilder();
            dirInfo = new DirectoryInfo(path1);
            DateTime now = DateTime.Now;
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            dirInfo.CreateSubdirectory(subpath);
            string pathLog = "C:\\SystemTasks\\WPF_Projects\\UpdateFias\\UpdateFiasLog\\UpdateFiasLog." + now.ToString("dd.MM.yyyy") + ".txt";
            string dirNameDel = "C:\\SystemTasks\\WPF_Projects\\UpdateFias\\UpdateFiasLog";
            string[] files = Directory.GetFiles(dirNameDel);

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime < DateTime.Now.AddDays(-3))
                    fi.Delete();
            }


            try
            {
                Console.WriteLine("Скачивание архивов");
             
                stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Скачивание архивов \r\n");
                System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                stringBuilder.Clear();

                var archivePath = "https://fias.nalog.ru/Public/Downloads/Actual/gar_delta_xml.zip";
                using (var client = new MyWebClient())
                {
 
                    client.DownloadFile(archivePath, "./gar_delta_xml.zip");


                    DirectoryInfo dirInfo = new DirectoryInfo(dirName);
                    if (dirInfo.Exists)
                    {
                        dirInfo.Delete(true);
                        Console.WriteLine("Каталог удалён");
                    }
                    //ZipFile.ExtractToDirectory("./fias.zip", dirName);
                }

                Console.WriteLine("Скачивание завершено");

                stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Скачивание завершено \r\n");
                System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                stringBuilder.Clear();

                //Extract();

                Console.WriteLine("Разархивирование архивов в xml");

                var path = ArchivePath;

                ZipFile.ExtractToDirectory(ArchivePath, ExtractPath);


                Console.WriteLine("Разархивирование архивов в xml завершено");

            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("Скачивание отменено");
                Console.WriteLine(ex.Message);
               
                stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Скачивание отменено: \r\n" + ex.Message);
                System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                stringBuilder.Clear();
            }




            // Создание подключения
            SqlConnection connection = new SqlConnection(connectionString);
           
            try
            {
                // Открываем подключение
                //await connection.OpenAsync();

                Server Server = new Server(new ServerConnection(connection));


                List<FIASTable> Tables = new List<FIASTable>();
                Console.WriteLine("Cканирование xml");

              
                stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Cканирование xml \r\n");
                System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                stringBuilder.Clear();

                Tables.Clear();
                var rootFiles = Directory.EnumerateFiles(dirName, "*.xml")
                    .Select(F => new FIASFile(F));

                var subjectFiles = Directory.EnumerateDirectories(dirName)
                    .SelectMany(D => Directory.EnumerateFiles(D))
                    .Select(F => new FIASFile(F)
                    {
                        Region = Path.GetFileName(Path.GetDirectoryName(F))
                    });

                var tables = Enumerable.Concat(rootFiles, subjectFiles)
                   .ToLookup(F => F.Name.Contains("PARAMS") ? "PARAMS" : F.Name)
                   .Select(L => new FIASTable(L.Key, L.ToList()));

                Tables.AddRange(tables);
                Console.WriteLine("Cканирование xml завершено");
                Console.WriteLine("Импорт");
                stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Cканирование xml завершено \r\n");
                stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Импорт \r\n");
                System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                stringBuilder.Clear();
                foreach (var item in Tables)
                {
                    // Проверка существования
                    //Server Server = new Server(new ServerConnection(Connection));
                    DB = Server.Databases[FIASProperties.DBName];

                    var target = DB.Tables[item.Name];
                    if (target == null) { continue; }
                    // Проверка настроек импорта
                    target.Refresh();
                    //if (!Store.GetCanImport(table.Name)) { continue; }

                    // Импорт
                    //DBImportDelta.ImportTable(target, item);            

                    var temporaryTable = $"_{target.Name}";
                    var table = DB.Tables[temporaryTable];
                    table?.Drop();
                    table = new Table(DB, temporaryTable);
                    foreach (Column column in target.Columns)
                    { column.CloneTo(table); }
                    table.Create();

                    // Импортировать данные
                    var columns = target.Columns.Cast<Column>();
                    Console.WriteLine("Подключение открыто");

                    stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Подключение открыто \r\n");
                    System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                    stringBuilder.Clear();

                    //using (var connection = DBClient.NewConnection())
                    using (var SBC = new SqlBulkCopy(connection) { DestinationTableName = table.Name, BulkCopyTimeout = 3600, NotifyAfter = 100 })
                    {
                        SBC.SqlRowsCopied += SBC_SqlRowsCopied;
                        SBC.EnableStreaming = true;
                        var names = target.Columns.Cast<Column>().Select(C => C.Name);
                        foreach (var File in item.Files)
                        {
                            //Token.ThrowIfCancellationRequested();
                            Console.WriteLine($"Импорт файла: {File.FullName}", 0, 0); //SP?.Report(new TaskProgress($"Импорт файла: {File.FullName}", 0, 0));
                            stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Cканирование xml \r\n" + File.FullName);
                            System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                            stringBuilder.Clear();
                            using (var FR = new FIASReader(File.Path, names))
                            {
                                SBC.WriteToServer(FR);
                            }
                            SBC.NotifyAfter = 100;
                            var Count = SBC.RowsCopied;
                            Console.WriteLine($"Импорт файла завершён: {File.FullName}", 0, 0);
                            stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Cканирование xml \r\n" + File.FullName);
                            System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                            stringBuilder.Clear();
                            //SP.Report(new TaskProgress($"Импорт файла завершён: {File.FullName}", Count, Count));
                            Thread.Sleep(200);
                        }
                    }

                    // Объединить таблицы
                    var key = columns.First().Name;
                    var insert = columns.Select(C => $"[{C.Name}]");
                    var values = columns.Select(C => $"[S].[{C.Name}]");
                    var update = columns.Skip(1).Select(C => $"[{C.Name}] = [S].[{C.Name}]");

                    var query = new StringBuilder();
                    query.AppendLine($"MERGE INTO [{target.Name}] AS [T]");
                    query.AppendLine($"USING [{temporaryTable}] AS [S]");
                    query.AppendLine($"ON([T].[{key}] = [S].[{key}])");
                    query.AppendLine("WHEN NOT MATCHED BY TARGET THEN");
                    query.AppendLine($"INSERT ({string.Join(",", insert)})");
                    query.AppendLine($"VALUES ({string.Join(",", values)})");
                    query.AppendLine("WHEN MATCHED THEN");
                    query.AppendLine($"UPDATE SET { string.Join(",", update)};");
                    DB.ExecuteNonQuery(query.ToString());

                    table.Drop();
                    Console.WriteLine($"Импорт в таблицу завершён: {target.Name}", 0, 0);
                    stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Импорт в таблицу завершён: \r\n" + target.Name);
                    System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                    stringBuilder.Clear();
                    //SP.Report(new TaskProgress($"Импорт в таблицу завершён: {target.Name}", 0, 0));
                    target.Refresh();



                    //return target.RowCount;






                    //Store.SetLastImport(item.Name, item.Date);
                    Thread.Sleep(500);
                }
                //Store.SetVersion(Archive.Date);
                //if (Options.ShrinkDatabase) { ShrinkDatabase(); }
                Console.WriteLine("Импорт завершен");

                stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Импорт завершен \r\n");
                System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                stringBuilder.Clear();

            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);

               
                stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Ошибка в импорте файла: \r\n" + ex.Message);
                System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                stringBuilder.Clear();
            }
            finally
            {
                ////если подключение открыто
                //if (connection.State == ConnectionState.Open)
                //{
                //    // закрываем подключение
                //    //await connection.CloseAsync();
                    
                //}
                Console.WriteLine("Подключение закрыто...");
                
                stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Подключение закрыто... \r\n" );
                System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
                stringBuilder.Clear();
            }
            Console.WriteLine("Программа завершила работу.");
            
            stringBuilder.Append("\r\n--------------------\r\n-->" + now.ToString() + "Ошибка в импорте файла: \r\n");
            System.IO.File.AppendAllText(pathLog, stringBuilder.ToString());
            stringBuilder.Clear();
        }
    }
}
