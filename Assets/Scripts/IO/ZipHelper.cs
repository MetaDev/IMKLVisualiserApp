using UnityEngine;
using Ionic.Zip;
using System.IO;
using System;
using System.Collections.Generic;
using UniRx;

namespace IO
{
    /// <summary>
    /// A simple zip file helper class.
    /// </summary>
    public class ZipHelper
    {
        /// <summary>
        ///
        /// </summary>
        public static IEnumerable<byte[]> ExtractFilesFromRecursiveZipData(byte[] zipData,string fileType=null)
        {
            //use byte stream
            using (MemoryStream zipstream = new MemoryStream(zipData))
            using (ZipFile zout = ZipFile.Read(zipstream))
            {
                var list = new List<byte[]>();
                foreach (ZipEntry e in zout)
                {
                    var d = e.OpenReader();
                    var bytes = ReadToEnd(d);
                    if (!e.IsDirectory){
                        //if another zip unpack it
                        if (Path.GetExtension(e.FileName)==".zip"){
                            list.AddRange(ExtractFilesFromRecursiveZipData(bytes,fileType));
                        }
                        //if file and matching the filter
                        else if (fileType==null || fileType == Path.GetExtension(e.FileName)){
                            list.Add(bytes);
                        }
                    }
                }
                return list;
            }
        }
        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }



}
