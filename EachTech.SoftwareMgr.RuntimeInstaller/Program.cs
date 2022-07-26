using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using System.Net;

namespace RuntimeInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.SaveLog("EachSoft Runtime Installer v.1.0.0.0");
            Log.SaveLog("Initializing installer, please wait...");
            string module;
            #region ConfigReader
            module = "ConfigLoader";
            Log.SaveLog("Getting config from local file...");
            Hashtable htStandard = new Hashtable
            {
                { "type", "RuntimeInstaller" },//配置类型
                { "enableOutputing", "true" },//是否允许输出
                { "runtimePath", "fromURL" },//运行库路径,若为fromURL就从指定链接获取
                { "runtimeURL", "none" },//运行时直链下载地址
                { "runtimeArgs", "-I" },//运行时安装参数
                { "runtimeType", "exe" },//运行时后缀
                { "redownloadRuntimeIfRuntimeMD5IsIncorrect", "true" },//如果运行时MD5不正确是否重新下载
                { "afterArgs", "-I" },//安装包安装参数
                { "doPackageChecking", "true" },//是否校验运行时
                { "doRuntimeChecking", "true" },//是否校验安装包
                { "runtimeMD5", "none" },//运行时MD5校验值
                { "packageMD5", "none" },//安装包MD5校验值
                { "packagePath", "fromURL" },//安装包路径,若为fromURL就从指定链接获取
                { "packageURL", "none" },//安装包直链下载地址
                { "packageType", "exe" },//安装包文件后缀
                { "redownloadRuntimeIfPackageMD5IsIncorrect", "true" },//如果安装包MD5不正确是否重新下载
                { "latestRuntimeMD5-URL", "none" },//运行时MD5链接(可用于检查更新)
                { "latestPackageMD5-URL", "none" },//安装包MD5链接(可用于检查更新)
                { "waitUntilExit", "true" },//是否等待安装进程退出
                { "openAfterDone", "none" }//在完成安装后打开的文件
            };
            Directory.CreateDirectory("./config/");
            Hashtable config = PropertiesHelper.AutoCheck(htStandard, "./config/runtime.properties");
            bool output = false;
            if (config["enableOutputing"] as string == "true")
            {
                output = true;
            }
            Log.SaveLog("Config file loaded.", module, output);
            //配置文件读取成功
            #endregion
            #region RuntimeExistingChecker
            module = "RuntimeExistingChecker";
            Log.SaveLog("Locating runtime package...");
            //定位运行时
            if (config["runtimePath"] as string == "fromURL") 
            {
                Log.SaveLog($"Downloading runtime package from \"{config["runtimeURL"]}\"...");
                if (((string)config["runtimeURL"]).Contains("://"))
                {

                }
                else
                {
                    Log.SaveLog("Invaild URL", "Downloader:" + module, output);
                    Console.WriteLine("Failed to install.View log file for details.");
                    Console.WriteLine("Press any key to exit...");
                    return;
                }
                try
                {
                    HttpDownload(config["runtimeURL"] as string, $"./runtime.{config["runtimeType"]}");
                    //开始下载
                    if (File.Exists($"./runtime.{config["runtimeType"]}")) 
                    {
                        Log.SaveLog("Runtime package downloading successful.", "HttpDownload", output);
                        //下载成功
                    }
                    else
                    {
                        Log.SaveLog("Unable to download runtime package : File does not exist.", module, output);
                        //下载没有出现异常,但是文件不存在
                        Console.WriteLine("Failed to install.View log file for details.");
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log.SaveLog(ex.ToString(), "HttpDownload", output);
                    Log.SaveLog("Unable to download runtime package : Unexpected exception.", "HttpDownload", output);
                    //下载出现异常
                    Console.WriteLine("Failed to install.View log file for details.");
                    Console.WriteLine("Press any key to exit...");
                    return;
                }
            }
            else
            {
                if (File.Exists(config["runtimePath"] as string)) 
                {
                    Log.SaveLog("Runtime package located.", output);
                    //成功定位本地运行时
                }
                else
                {
                    Log.SaveLog("Unable to locate runtime package : File does not exist.", "Locating runtime", output);
                    Log.SaveLog("Preparing for package downloading...", output);
                    //本地运行时不存在,尝试从网络下载
                    if (((string)config["runtimeURL"]).Contains("://"))
                    {
                        try
                        {
                            HttpDownload(config["runtimeURL"] as string, $"./runtime.{config["runtimeType"]}");
                            //开始下载
                            if (File.Exists($"./runtime.{config["runtimeType"]}"))
                            {
                                Log.SaveLog("Runtime package downloading successful.", "HttpDownload", output);
                                //下载成功
                            }
                            else
                            {
                                Log.SaveLog("Unable to download runtime package : File does not exist.", "HttpDownload", output);
                                //下载没有出现异常,但是文件不存在
                                Console.WriteLine("Failed to install.View log file for details.");
                                Console.WriteLine("Press any key to exit...");
                                Console.ReadKey();
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.SaveLog(ex.ToString(), "HttpDownload", output);
                            Log.SaveLog("Unable to download runtime package : Unexpected exception.", "HttpDownload", output);
                            //下载出现异常
                            Console.WriteLine("Failed to install.View log file for details.");
                            Console.WriteLine("Press any key to exit...");
                            return;
                        }
                    }
                    else
                    {
                        Log.SaveLog("Invaild URL", "Downloader", output);
                        //URL未指定或出现错误
                        Console.WriteLine("Failed to install.View log file for details.");
                        Console.WriteLine("Press any key to exit...");
                        return;
                    }
                    
                }
            }
            #endregion
            #region RuntimeMD5Checker
            module = "RuntimeMD5Checker";
            if (config["doRuntimeChecking"] as string == "true")
            {
                Log.SaveLog("Checking the MD5 of runtime package...", module, output);
                //开始检查运行库MD5
                string runtimeMD5;
                try
                {
                    runtimeMD5 = GetMD5Hash(config["runtimePath"] as string);
                    Log.SaveLog($"The MD5 of runtime is \"{runtimeMD5}\".", module, output);
                    //输出运行库MD5，方便检查
                }
                catch (Exception ex)
                {
                    Log.SaveLog("Unable to check the MD5 of runtime. ", module, output);
                    Log.SaveLog(ex.ToString(), module, output);
                    Console.WriteLine("Failed to install.View log file for details.");
                    Console.WriteLine("Press any key to exit...");
                    return;
                }
                if (config["runtimeMD5"] as string == runtimeMD5) 
                {
                    Log.SaveLog("Runtime package's MD5 is correct.", module, output);
                }

            }
            #endregion
        }



        /// <summary>
        /// http下载文件
        /// </summary>
        /// <param name="url">下载文件地址</param>
        /// <param name="path">文件存放地址，包含文件名</param>
        public static void HttpDownload(string url, string path)
        {
            string tempPath = Path.GetDirectoryName(path) + @"\temp";
            Directory.CreateDirectory(tempPath);  //创建临时文件目录
            string tempFile = tempPath + @"\" + Path.GetFileName(path) + ".temp"; //临时文件
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);    //存在则删除
            }
            try
            {
                FileStream fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                // 设置参数
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();
                //创建本地文件写入流
                //Stream stream = new FileStream(tempFile, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    //stream.Write(bArr, 0, size);
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                //stream.Close();
                fs.Close();
                responseStream.Close();
                File.Move(tempFile, path);
                
            }
            catch
            {
                throw;
            }
        }

        //计算文件的MD5码
        public static string GetMD5Hash(string pathName)
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