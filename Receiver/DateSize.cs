using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace API2
{
    public class DateSize
    {
        string jsonFilePath = "periods.json";
        List<PeriodJsonModel> PeriodList;

        public DateSize() {
            ParseFromFile();
        }

        public void ParseFromFile()
        {
            try
            {
                string jsonString = File.ReadAllText(jsonFilePath);
                PeriodList = JsonConvert.DeserializeObject<List<PeriodJsonModel>>(jsonString);
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.ToString());
            }

        }

        public int GetCurrentPeriod()
        {
            int id = 0;
            //Console.WriteLine(DateTime.Now);
            for (int i = 0; i < PeriodList.Count; i++)
            {
                if( (DateTime.Now > PeriodList[i].From) && (DateTime.Now < PeriodList[i].To) ){
                    id = i;
                }
            }

            //DateTime dateTime = PeriodList[15].From;
            //Console.WriteLine(dateTime);
            //id = GetCurrentPeriod(dateTime.AddSeconds(2));

            return id;
        }
        public int GetCurrentPeriod(DateTime dateTime)
        {
            int id = 0;
 
            for (int i = 0; i < PeriodList.Count; i++)
            {
                if ( (dateTime > PeriodList[i].From) && (dateTime < PeriodList[i].To))
                {
                    id = i;
                }
            }


            return id;
        }
        public double CalculateMaxSeconds(int id)
        {
            double seconds = 0;
            if ((id >= 0) && (id < PeriodList.Count))
            {
                seconds = PeriodList[id].To.Subtract(PeriodList[id].From).TotalSeconds;
            }

            return seconds;
        }

        public double CalculateMaxSeconds(DateTime dateTime) {

            int id = GetCurrentPeriod(dateTime);

            double seconds = (dateTime.Subtract(PeriodList[id].From)).TotalSeconds;
            return seconds;
        }
        public double ConvertKbitsToBits(double Kbits)
        {
            double Bits = Kbits * 1000;
            return Bits;
        }
        public double ConvertMbitsToBits(double Mbits)
        {
            double Bits = Mbits * 1000000;
            return Bits;
        }
        public double ConvertBitsToBytes(double Bits)
        {
            double Bytes = Bits / 8;
            return Bytes;
        }
        public double ConvertBytesToKBytes(double Bytes)
        {
            double Kbytes = Bytes / 1000;
            return Kbytes;
        }
        public double ConvertBytesToMbytes(double Bytes)
        {
            double Mbytes = Bytes / 1000000;
            return Mbytes;
        }
        public double CalculateMaxBytes(int id)
        {

            double seconds = CalculateMaxSeconds(id);
            double bytes = ConvertBitsToBytes(seconds * ConvertMbitsToBits(PeriodList[id].Speed) );

            return bytes;
        }
        public double CalculateMaxBytes(DateTime dateTime)
        {
            int id = GetCurrentPeriod(dateTime);
            double seconds = CalculateMaxSeconds(dateTime);
            double bytes = ConvertBitsToBytes(seconds * ConvertMbitsToBits(PeriodList[id].Speed) );

            return bytes;
        }
        public bool CompareSize(double fileSizeInBytes, int id)
        {
            bool result = false;
            double maxbytes = CalculateMaxBytes(id);
            result = (maxbytes >= fileSizeInBytes);

            return result;
        }
        public bool CompareSize(double fileSizeInBytes, DateTime dateTime)
        {
            bool result = false;
            double maxbytes = CalculateMaxBytes(dateTime);
            result = (maxbytes >= fileSizeInBytes);

            return result;
        }
        public bool CompareSize(double fileSizeInBytes, int id, double DelayInSeconds)
        {
            bool result = false;
            double maxbytes = CalculateMaxBytes(id);
            result = (maxbytes >= (fileSizeInBytes+DelayInSeconds));

            return result;
        }
        public bool CompareSize(double fileSizeInBytes, DateTime dateTime, double DelayInSeconds)
        {
            bool result = false;
            double maxbytes = CalculateMaxBytes(dateTime);
            result = (maxbytes >= (fileSizeInBytes + DelayInSeconds));

            return result;
        }

    }
}
