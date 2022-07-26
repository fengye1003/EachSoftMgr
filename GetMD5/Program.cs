using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography;

namespace GetMD5
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.SaveLog("EachSoft Runtime Installer System v.1.0.0.0");
            Log.SaveLog("Installer tools : GetMD5");
            Console.Write("\nInput the path of target file:");
            string? path = Console.ReadLine();
            if (path == null) 
            {
                Log.SaveLog("Unexpected input...");
                Main(args);
            }
            else
            {
                try
                {
                    Log.SaveLog($"The MD5 of \"{path}\" is \"{GetMD5Hash(path)}\" .");
                    Main(args);
                }
                catch (Exception ex)
                {
                    Log.SaveLog(ex.ToString());
                    Main(args);
                }
            }
        }



        //计算文件的MD5码
        public static string? GetMD5Hash(string pathName)
        {
            string strResult;
            string strHashData;
            byte[] arrbytHashValue;
            FileStream? oFileStream;
            MD5 oMD5Hasher = MD5.Create();
            try
            {

                oFileStream = new FileStream(pathName, FileMode.Open,

                      FileAccess.Read, FileShare.ReadWrite);

                arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);//计算指定Stream 对象的哈希值

                oFileStream.Close();

                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”

                strHashData = BitConverter.ToString(arrbytHashValue);

                //替换-

                strHashData = strHashData.Replace("-", "");

                strResult = strHashData;

            }
            catch (Exception ex)
            {
                Log.SaveLog(ex.ToString());
                throw;
            }
            return strResult;

        }
    }
}