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
            var count = 0;
            Console.WriteLine("File Contents");
            
            using (var reader = new StructuredFileReader<DataRecordStruct>(".\\DataFiles\\DATALOG.100"))
            {
                foreach (var entry in reader)
                {
                    Console.WriteLine(entry.ToString());
                    Console.WriteLine();
                    count++;
                }
            }

            Console.WriteLine("Total of {0} entries", count);
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

        Struct Layout 
        ============
        tttttttt tttttttt tttttttt tttttttt
        01010101 01101010 11110110 11011111
        5   5    6   A    F   6    D   F

        rrTTTTTT TTTTtttt ttttttee eeeeeeee
        00001101 01010011 00100000 11010000
        0   D    5   3    2   0    D   0 

        rrHHHHHH HHHHhhhh hhhhhhuu uuuuuuuu
        00101101 01011010 01101010 10111100
        2   D    5   A    6   A    B   C  
 
        rrrrrrrr rrrrIIII IIIIIIPP PPPPPPPP
        00000000 00000101 01001000 11110101
        0   0    0   5    4   8    F   5
    */

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DataRecordStruct
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
        
        public uint InternalTemp { get { return (_raw1 >> 0) & 0x03FF; } }

        public uint MinInternalTemp { get { return (_raw1 >> 10) & 0x03FF; } }

        public uint MaxInternalTemp { get { return (_raw1 >> 20) & 0x03FF; } }

        public uint InternalRh { get { return (_raw2 >> 0) & 0x03FF; } }

        public uint MinInternalRh { get { return (_raw2 >> 10) & 0x03FF; } }

        public uint MaxInternalRh { get { return (_raw2 >> 20) & 0x03FF; } }

        public uint ExternalTemp { get { return (_raw3 >> 0) & 0x03FF; } }

        public uint ExternalRh { get { return (_raw3 >> 10) & 0x03FF; } }


        public override string ToString()
        {
            return string.Format("Time:{0}\r\nIntT:{1}\r\nMinIntT:{2}\r\nMaxIntT:{3}\r\nIntRH:{4}\r\nMinRH:{5}\r\nMaxRH:{6}\r\nExtTemp:{7}\r\nExtRH:{8}", 
                                    Timestamp, 
                                    InternalTemp, MinInternalTemp, MaxInternalTemp, 
                                    InternalRh, MinInternalRh, MaxInternalRh, 
                                    ExternalTemp, ExternalRh);
        }
    }

    internal class StructuredFileReader<T> : IEnumerable<T>, IEnumerator<T> where T : struct
    {
        private readonly string _filePath;
        private int _fileOffset;
        private FileStream _fileStream;
        private T _currentEntry;
        private int _structSize;


        public StructuredFileReader(string filePath)
        {
            _fileOffset = 0;
            _structSize = Marshal.SizeOf(typeof(T)); 
            _filePath = filePath;
            _fileStream = null;
            _currentEntry = default(T);

            Initialize();
        }

        private void Initialize()
        {
            if (string.IsNullOrWhiteSpace(_filePath))
            {
                throw new ArgumentNullException("filePath","file path can not be null or empty");
            }

            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException("file was not found in the specified location", _filePath);
            }

            _fileStream = File.OpenRead(_filePath);

            Reset();
        }

        //public IEnumerable<T> GetEntries()
        //{
        //    using (var s = File.OpenRead(_filePath))
        //    {
        //        var entry = ReadStruct(s);
        //        yield return entry;
        //    }
        //}

        private T ReadStruct(Stream stream) 
        {
            var buffer = new byte[_structSize];

            var bytesRead = stream.Read(buffer, 0, _structSize);

            if (bytesRead < _structSize)
            {
                return default(T);
            }

            _fileOffset += bytesRead;

            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var structure = (T)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(T));
            pinnedBuffer.Free();

            return structure;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (_fileStream != null)
            {
                _fileStream.Dispose();
            }
        }

        public bool MoveNext()
        {
            _currentEntry = ReadStruct(_fileStream);

            return !_currentEntry.Equals(default(T));
        }

        public void Reset()
        {
            if (_fileStream == null)
            {
                return;
            }

            _fileOffset = 0;
            _fileStream.Seek(_fileOffset, SeekOrigin.Begin);
            _currentEntry = default(T);
        }

        public T Current { get { return _currentEntry; } }

        object IEnumerator.Current
        {
            get { return Current; }
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
