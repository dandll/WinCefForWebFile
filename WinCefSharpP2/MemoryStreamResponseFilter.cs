using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCefSharpP2
{
    //数据处理器，仅将 数据保存到内存。
    public class MemoryStreamResponseFilter : IResponseFilter
    {
        private MemoryStream memoryStream;

        bool IResponseFilter.InitFilter()
        {
            //NOTE: We could initialize this earlier, just one possible use of InitFilter
            memoryStream = new MemoryStream();

            return true;
        }

        FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
        {
            if (dataIn == null)
            {
                dataInRead = 0;
                dataOutWritten = 0;

                return FilterStatus.Done;
            }

            dataInRead = dataIn.Length;
            dataOutWritten = Math.Min(dataInRead, dataOut.Length);
            if (dataOut.Length >= dataIn.Length)
            {
                //Important we copy dataIn to dataOut
                dataIn.CopyTo(dataOut);
            }
            else
            {
                //这个地方图片长可能会报错 暂没解决
            }
            //Copy data to stream
            dataIn.Position = 0;
            dataIn.CopyTo(memoryStream);

            return FilterStatus.Done;
        }

        void IDisposable.Dispose()
        {
            memoryStream.Dispose();
            memoryStream = null;
        }

        public byte[] Data
        {
            get { return memoryStream.ToArray(); }
        }


    }
}
