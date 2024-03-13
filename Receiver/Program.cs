using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using API2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    public static async Task Main(string[] args)
    {
        //var serviceProvider = new ServiceCollection()
            //.AddDbContext<ApplicationContext>(options => options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=applicationdb;"))
            //.AddDbContext<ApplicationContext>(options => options.UseMySql("Server=localhost;Database=API2;User=api2admin;Password=12345atomdev;", new MySqlServerVersion(new Version(8, 0, 36))))
            //.BuildServiceProvider();
        var tcpListener = new TcpListener(IPAddress.Any, 8888);
        try
        {
            tcpListener.Start();    // запускаем сервер
            Console.WriteLine("Сервер запущен. Ожидание подключений... ");

            while (true)
            {
                // получаем подключение в виде TcpClient
                using var tcpClient = await tcpListener.AcceptTcpClientAsync();
                // получаем объект NetworkStream для взаимодействия с клиентом
                var stream = tcpClient.GetStream();

                // создаем BinaryReader для чтения данных
                using var binaryReader = new BinaryReader(stream);
                // создаем BinaryWriter для отправки данных
                using var binaryWriter = new BinaryWriter(stream);

                double canSave = await HowManyMbCanSave();
                int periodNow = await PeriodNow(0);
                int period4MinutesFromNow = await PeriodNow(4);

                ulong canSaveBytes = (ulong)(canSave * 125000);
                Console.WriteLine($"Доступно до закрытия канала: {canSaveBytes}");

                //для теста
                //canSaveBytes = 1000000000;

                var needSpace = binaryReader.ReadDouble();
                int status = binaryReader.ReadInt32();
                if (needSpace <= canSaveBytes && periodNow== period4MinutesFromNow && periodNow!=0)
                {
                    if (status==0)
                    {

                        await ReceiveReport(stream, binaryReader, binaryWriter, (long)needSpace);

                    } else
                    {
                        await ReceiveReportNextPart(stream, binaryReader, binaryWriter, (long)needSpace);
                    }
                }
                else
                {
                    binaryWriter.Write(false);
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        Report? report = db.Reports.Where(p => p.Status == 2).FirstOrDefault();
                        if (report != null)
                        {
                            db.Reports.Remove(report);
                            await db.SaveChangesAsync();
                        }
                    }
                    Console.WriteLine("Bad time gap. Try again later");
                }



            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            tcpListener.Stop();
        }
    }

    public async static Task<int> PeriodNow(int minutes_from)
    {
        int res = 0;
        using (ApplicationContext db = new ApplicationContext())
        {
            List<PeriodTableModel> periods = db.Periods.ToList();
            DateTime time = DateTime.Now;
            if (minutes_from > 0)
            {
                time.AddMinutes(minutes_from);
            }
            int ourPeriod = periods.Count - 1;
            for (int i = 0; i < periods.Count; i++)
            {
                if (periods[i].From > time)
                {
                    if (i == 0)
                    {
                        return res;
                    }
                    else
                    {
                        ourPeriod = i - 1;
                    }
                    break;
                }
            }
            DateTime periodTo = periods[ourPeriod].To;
            if (periods[ourPeriod].To > time)
            {
                res = ourPeriod;
            }

        }
        return res;
    }

    public async static Task<double> HowManyMbCanSave()
    {
        double res = 0;
        using (ApplicationContext db = new ApplicationContext())
        {
            List<PeriodTableModel> periods = db.Periods.ToList();
            DateTime time = DateTime.Now.AddMinutes(4);
            int ourPeriod = periods.Count-1;
            for (int i = 0; i < periods.Count; i++)
            {
                if (periods[i].From > time)
                {
                    if (i == 0)
                    {
                        return res;
                    }
                    else
                    {
                        ourPeriod = i - 1;
                    }
                    break;
                }
            }
                DateTime periodTo = periods[ourPeriod].To;
               if (periods[ourPeriod].To > time)
                {
                    TimeSpan diff= periodTo - time;
                    res = diff.TotalSeconds * periods[ourPeriod].Speed;
                }
            
        }
        return res;
    }
    public async static Task ReceiveReport(NetworkStream stream, BinaryReader binaryReader, BinaryWriter binaryWriter, long bytes)
    {
        await Task.Delay(TimeSpan.FromSeconds(240));
        string SenderName= binaryReader.ReadString();
        string ReportName = binaryReader.ReadString();
        string Filename = binaryReader.ReadString();
        int Description = binaryReader.ReadInt32();
        string TextContent = binaryReader.ReadString();
        DateTime dateCreated = DateTime.Parse(binaryReader.ReadString());
        DateTime dateSent = DateTime.Parse(binaryReader.ReadString());
        long fileSize = 0;

        if (Filename!= "" && Filename != null)
        {
            fileSize = binaryReader.ReadInt64();
            Console.WriteLine($"Receiving report and file of size: {fileSize}");
            string path = "../api/wwwroot/files/" + Filename;

            using (FileStream fileStream = File.Create(path))
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                long bytesReceived = 0;

                while (bytesReceived < fileSize &&
                       (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                    bytesReceived += bytesRead;
                }
            }

        }
        else
        {
            Filename = "";
        }
        string dateReceived;
        using (ApplicationContext db = new ApplicationContext())
        {
            Report report = new Report();
            report.SenderName = SenderName;
            report.ReportName = ReportName;
            report.Filename = Filename;
            report.Status = 1;
            report.Filesize = bytes;
            report.BytesSend = fileSize;
            report.DateCreated = dateCreated;
            report.DateSent = dateSent;
            report.DateReceived = DateTime.Now;
            report.ReportDescription_Id = Description;
            report.Filesize= fileSize;
            ReportTextContent content = new ReportTextContent();
            content.TextContent = TextContent;
            db.Reports.Add(report);
            await db.SaveChangesAsync();
            content.ReportId = report.Id;
            db.ReportTextContents.Add(content);
            await db.SaveChangesAsync();
            report.TextContent_Id = content.Id;
            await db.SaveChangesAsync();
            dateReceived = report.DateReceived.ToString();
            Console.WriteLine($"New report: {report}");
        }
        Console.WriteLine("File received successfully.");
        await Task.Delay(TimeSpan.FromSeconds(240));
        binaryWriter.Write(true);
        binaryWriter.Write(dateReceived);
        binaryWriter.Flush();

    }
    public async static Task ReceiveReportNextPart(NetworkStream stream, BinaryReader binaryReader, BinaryWriter binaryWriter, long bytes)
    {
        await Task.Delay(TimeSpan.FromSeconds(240));
        string SenderName = binaryReader.ReadString();
        string ReportName = binaryReader.ReadString();
        string Filename = binaryReader.ReadString();
        int Description = binaryReader.ReadInt32();
        string TextContent = binaryReader.ReadString();
        DateTime dateCreated = DateTime.Parse(binaryReader.ReadString());
        DateTime dateSent = DateTime.Parse(binaryReader.ReadString());

        long fileSize = binaryReader.ReadInt64();
        Console.WriteLine($"Receiving file part of size: {fileSize}");
        string path = "../api/wwwroot/files/" + Filename;
        // отправляем клиенту сгенерированный id
        using (FileStream fileStream = new FileStream(path, FileMode.Append, FileAccess.Write))
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            long bytesReceived = 0;

            while (bytesReceived < fileSize &&
                   (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
                bytesReceived += bytesRead;
            }
        }
        string dateReceived;
        using (ApplicationContext db = new ApplicationContext())
        {
            Report report = db.Reports.Where(p => p.ReportName == ReportName).FirstOrDefault();
            report.DateReceived = DateTime.Now;
            report.DateSent = dateSent;
            report.Filesize += fileSize;
            report.BytesSend += fileSize;
            await db.SaveChangesAsync();
            dateReceived = report.DateReceived.ToString();
        }
        await Task.Delay(TimeSpan.FromSeconds(240));
        binaryWriter.Write(true);
        binaryWriter.Write(dateReceived);
        Console.WriteLine("Part received successfully.");
        binaryWriter.Flush();

    }
}