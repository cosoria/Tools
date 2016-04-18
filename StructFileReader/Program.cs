using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace StructFileReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("File Contents");


            var reader = new StructuredFileReader<FileStruct>(".\\DataFiles\\DATALOG.100");

            foreach (var entry in reader.GetEntries())
            {
                Console.WriteLine(entry.ToString());
                Console.WriteLine();
            }

            Console.WriteLine("============");
            Console.WriteLine("That is all folks!!");
            
            Console.ReadKey();
        }
    }


    /*  // DataRec 16 Bytes Total 
        typedef struct {
          time_t   TimeStamp; // +4 bytes = 4 bytes
          uint32_t IntTemp	: 10;
          uint32_t MinIntTemp : 10;
          uint32_t MaxIntTemp : 10;
          uint32_t Res1		: 2;  // +4 bytes = 8 bytes 
          uint32_t IntRH	: 10;
          uint32_t MinIntRH	: 10;
          uint32_t MaxIntRH : 10;
          uint32_t Res2		: 2;  // +4 bytes = 12 bytes 
          uint32_t ExtTemp	: 10;
          uint32_t ExtRH	: 10;
          uint32_t Res3		: 12; // +4 bytes = 16 bytes 
        } DataRec;
    */

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct FileStruct
    {
        [FieldOffset(0)]
        public uint _raw0;

        [FieldOffset(4)]
        public uint _raw1;

        [FieldOffset(8)]
        public uint _raw2;

        [FieldOffset(12)]
        public uint _raw3;

        
        public uint Timestamp { get { return _raw0; } }
        
        public uint InternalTemp { get { return (_raw1 >> 22) & 0x03FF; } }

        public uint MinInternalTemp { get { return (_raw1 >> 12) & 0x03FF; } }

        public uint MaxInternalTemp { get { return (_raw1 >> 2) & 0x03FF; } }

        public uint internal_rh { get { return (_raw1 >> 2) & 0x03FF; } }

        public uint min_internal_rh { get { return 0; } }

        public uint max_internal_rh { get { return 0; } }
        
        public uint external_temp { get { return 0; } }

        public uint external_rh { get { return 0; } }


        public override string ToString()
        {
            return string.Format("T:{0}\r\nIntT:{1}\r\nMinIntT:{2}\r\nMaxIntT:{3}", Timestamp.FromUnixTimestamp(), InternalTemp, MinInternalTemp, MaxInternalTemp);
        }
    }

    internal class StructuredFileReader<T> where T : struct
    {
        private readonly string _filePath;


        public StructuredFileReader(string filePath)
        {
            
            _filePath = filePath;
        }

        public IEnumerable<T> GetEntries()
        {
            using (var s = File.OpenRead(_filePath))
            {
                var entry = ReadStruct(s);
                yield return entry;
            }
        }

        public T ReadStruct(Stream stream) 
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];
            stream.Read(buffer, 0, sz);
            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var structure = (T)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(T));
            pinnedBuffer.Free();
            return structure;
        }

    }

    public static class DateTimeExtensions
    {
        private static DateTime _unixEpoch = new DateTime(1970,1,1,0,0,0);

        public static DateTime FromUnixTimestamp(this uint timestamp)
        {
            return _unixEpoch.AddSeconds(timestamp);
        }
    }
}
