using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;

namespace Sync.Theater.Utils
{
    class MimeType
    {

        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        static extern int FindMimeFromData(IntPtr pBC,[MarshalAs(UnmanagedType.LPWStr)] string pwzUrl, 
            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I1, SizeParamIndex=3)] byte[] pBuffer, int cbSize, 
            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed, int dwMimeFlags, out IntPtr ppwzMimeOut, int dwReserved);

        /// <summary>
        /// Ensures that file exists and retrieves the content type 
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Returns for instance "images/jpeg" </returns>
        public static string getMimeFromFile(string file)
        {
            IntPtr mimeout;

            int MaxContent = 4096;

            byte[] buf = new byte[MaxContent];

            WebRequest request = WebRequest.Create(file);
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            dataStream.Read(buf, 0, MaxContent);
            dataStream.Close();

            int result = FindMimeFromData(IntPtr.Zero, file, buf, MaxContent, null, 0, out mimeout, 0);

            if (result != 0)
                throw Marshal.GetExceptionForHR(result);
            string mime = Marshal.PtrToStringUni(mimeout);
            Marshal.FreeCoTaskMem(mimeout);
            return mime;
        }
    }
}
