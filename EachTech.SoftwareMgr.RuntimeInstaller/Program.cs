using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography;

namespace RuntimeInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.SaveLog("EachSoft Runtime Installer v.1.0.0.0");
            Log.SaveLog("Initializing installer, please wait...");
            Log.SaveLog("Getting config from local file...");
            Hashtable htStandard = new Hashtable
            {
                { "type", "RuntimeInstaller" },
                { "enableOutputing", "true" },
                { "checkFile", "none" },
                { "runtimePath", "fromURL" },
                { "runtimeURL", "none" },
                { "runtimeArgs", "-I" },
                { "afterArgs", "-I" },
                { "doPackageChecking", "true" },
                { "doRuntimeChecking", "true" },
                { "runtimeSHA", "none" },
                { "packageSHA", "none" },
                { "latestRuntimeSHA-URL", "none" },
                { "latestPackageSHA-URL", "none" },
                { "waitUntilExit", "true" },
                { "openAfterDone", "none" }
            };
            Directory.CreateDirectory("./config/");
            Hashtable config = PropertiesHelper.AutoCheck(htStandard, "./config/runtime.properties");

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
                return null;
            }
            return strResult;

        }
    }
}