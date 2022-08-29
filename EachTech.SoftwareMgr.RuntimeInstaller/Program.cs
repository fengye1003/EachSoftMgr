using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using System.Net;
using System.Text;
using System.Diagnostics;

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
                { "allowRedownloadRuntime", "true" },//如果运行时MD5不正确是否重新下载
                { "allowSkipOnlineRuntimeChecking", "true" },//如果运行时MD5不是URL指定的最新版且无法下载时是否允许跳过
                { "packageArgs", "-I" },//安装包安装参数
                { "doPackageChecking", "true" },//是否校验运行时
                { "doRuntimeChecking", "true" },//是否校验安装包
                { "runtimeMD5", "none" },//运行时MD5校验值
                { "packageMD5", "none" },//安装包MD5校验值
                { "packagePath", "fromURL" },//安装包路径,若为fromURL就从指定链接获取
                { "packageURL", "none" },//安装包直链下载地址
                { "packageType", "exe" },//安装包文件后缀
                { "allowRedownloadPackage", "true" },//如果安装包MD5不正确是否重新下载
                { "allowSkipOnlinePackageChecking", "true" },//如果安装包MD5不是URL指定的最新版且无法下载时是否允许跳过
                { "latestRuntimeMD5-URL", "none" },//运行时MD5链接(可用于检查更新)
                { "latestPackageMD5-URL", "none" },//安装包MD5链接(可用于检查更新)
                { "waitForExit", "true" },//是否等待安装进程退出
                { "openAfterDone", "none" },//在完成安装后打开的文件
                { "tip", "If you want to use equal sign, use \"\\e\" instead." }//提示
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
                    Console.ReadKey();
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
                        config["runtimePath"] = $"./runtime.{config["runtimeType"]}";
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
                    Console.ReadKey();
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
                    if ((config["runtimeURL"] as string).Contains("://"))
                    {
                        try
                        {
                            HttpDownload(config["runtimeURL"] as string, $"./runtime.{config["runtimeType"]}");
                            //开始下载
                            if (File.Exists($"./runtime.{config["runtimeType"]}"))
                            {
                                Log.SaveLog("Runtime package downloading successful.", "HttpDownload", output);
                                //下载成功
                                config["runtimePath"] = $"./runtime.{config["runtimeType"]}";
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
                            Console.ReadKey();
                            return;
                        }
                    }
                    else
                    {
                        Log.SaveLog("Invaild URL", "Downloader", output);
                        //URL未指定或出现错误
                        Console.WriteLine("Failed to install.View log file for details.");
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                    
                }
            }
            #endregion
            #region RuntimeMD5Checker
            module = "RuntimeMD5Checker";
            string runtimeMD5;
            if (config["doRuntimeChecking"] as string == "true")
            {
                Log.SaveLog("Checking the MD5 of runtime package...", module, output);
                //开始检查运行库MD5
                try
                {
                    runtimeMD5 = GetMD5Hash(config["runtimePath"] as string);
                    Log.SaveLog($"The MD5 of runtime is \"{runtimeMD5}\".", module, output);
                    //输出运行库MD5，方便检查
                }
                catch (Exception ex)
                {
                    Log.SaveLog("Unable to check the MD5 of runtime. ", module, output);
                    //文件MD5未能正确运算 可能原因:权限不足
                    Log.SaveLog(ex.ToString(), module, output);
                    Console.WriteLine("Failed to install.View log file for details.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
                if (config["runtimeMD5"] as string == runtimeMD5) 
                {
                    Log.SaveLog("Runtime package's MD5 is correct.", module, output);
                    //运行库完整
                }
                else
                {
                    //MD5不正确,程序包可能存在破损
                    //若允许,重新下载运行库
                    if (config["allowRedownloadRuntime"] as string == "true")
                    {
                        try
                        {
                            if(File.Exists($"./runtime.{config["runtimeType"]}"))
                                File.Delete($"./runtime.{config["runtimeType"]}");
                            HttpDownload(config["runtimeURL"] as string, $"./runtime.{config["runtimeType"]}");
                            //开始下载
                            if (File.Exists($"./runtime.{config["runtimeType"]}"))
                            {
                                Log.SaveLog("Runtime package downloading successful.", "HttpDownload", output);
                                //下载成功
                                config["runtimePath"] = $"./runtime.{config["runtimeType"]}";
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
                            Console.ReadKey();
                            return;
                        }
                    }
                    else
                    {
                        Log.SaveLog("Runtime's MD5 is incorrect. Damaged package.", module, output);
                        Console.WriteLine("Failed to install.View log file for details.");
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                }

                if (((string)config["latestRuntimeMD5-URL"]).Contains("://"))
                {
                    try
                    {
                        if (HttpGet(config["latestRuntimeMD5-URL"] as string) == runtimeMD5) 
                        {
                            Log.SaveLog("Runtime checked with Internet.", "OnlineMD5", output);
                        }
                        else
                        {
                            Log.SaveLog("Runtime is not the version from Internet.Trying to download...", "OnlineMD5", output);

                            try
                            {
                                if (File.Exists($"./runtime.{config["runtimeType"]}"))
                                    File.Delete($"./runtime.{config["runtimeType"]}");
                                HttpDownload(config["runtimeURL"] as string, $"./runtime.{config["runtimeType"]}");
                                //开始下载
                                if (File.Exists($"./runtime.{config["runtimeType"]}"))
                                {
                                    Log.SaveLog("Runtime package downloading successful.", "HttpDownload", output);
                                    //下载成功
                                    config["runtimePath"] = $"./runtime.{config["runtimeType"]}";
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
                                Console.WriteLine("Failed to install the newest version from Internet. View log file for details.");
                                //是否继续安装视情况而定
                                if (config["allowSkipOnlineRuntimeChecking"] as string != "true") 
                                {
                                    Console.WriteLine("Press any key to exit...");
                                    Console.ReadKey();
                                }
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        Log.SaveLog(ex.ToString(), "OnlineMD5", output);
                        Log.SaveLog("Unable to download runtime package : Unexpected exception.", "OnlineMD5", output);
                        //GET出现异常
                        Console.WriteLine("Failed to check the MD5 from Internet. View log file for details.");
                        //是否继续安装视情况而定
                        if (config["allowSkipOnlineRuntimeChecking"] as string != "true")
                        {
                            Console.WriteLine("Press any key to exit...");
                            Console.ReadKey();
                        }
                    }
                }
                else
                {
                    Log.SaveLog("Warning : Invaild latestRuntimeMD5-URL. Program will skip online checking.", module, output);
                }
            }
            #endregion
            #region PackageExistingChecker
            module = "PackageExistingChecker";
            Log.SaveLog("Locating Package...");
            //定位安装包
            if (config["packagePath"] as string == "fromURL")
            {
                Log.SaveLog($"Downloading package from \"{config["packageURL"]}\"...");
                if (((string)config["packageURL"]).Contains("://"))
                {

                }
                else
                {
                    Log.SaveLog("Invaild URL", "Downloader:" + module, output);
                    Console.WriteLine("Failed to install.View log file for details.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
                try
                {
                    HttpDownload(config["packageURL"] as string, $"./package.{config["packageType"]}");
                    //开始下载
                    if (File.Exists($"./package.{config["packageType"]}"))
                    {
                        Log.SaveLog("Package downloading successful.", "HttpDownload", output);
                        //下载成功
                        config["packagePath"] = $"./package.{config["packageType"]}";
                    }
                    else
                    {
                        Log.SaveLog("Unable to download package : File does not exist.", module, output);
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
                    Log.SaveLog("Unable to runtime package : Unexpected exception.", "HttpDownload", output);
                    //下载出现异常
                    Console.WriteLine("Failed to install.View log file for details.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                if (File.Exists(config["packagePath"] as string))
                {
                    Log.SaveLog("Package located.", output);
                    //成功定位本安装包
                }
                else
                {
                    Log.SaveLog("Unable to locate package : File does not exist.", "Locating package", output);
                    Log.SaveLog("Preparing for package downloading...", output);
                    //本地安装包不存在,尝试从网络下载
                    if (((string)config["packageURL"]).Contains("://"))
                    {
                        try
                        {
                            HttpDownload(config["packageURL"] as string, $"./package. {config["packageType"]}");
                            //开始下载
                            if (File.Exists($"./package.{config["packageType"]}"))
                            {
                                Log.SaveLog("Package downloading successful.", "HttpDownload", output);
                                //下载成功
                                config["packagePath"] = $"./package.{config["packageType"]}";
                            }
                            else
                            {
                                Log.SaveLog("Unable to download package : File does not exist.", "HttpDownload", output);
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
                            Log.SaveLog("Unable to download package : Unexpected exception.", "HttpDownload", output);
                            //下载出现异常
                            Console.WriteLine("Failed to install.View log file for details.");
                            Console.WriteLine("Press any key to exit...");
                            Console.ReadKey();
                            return;
                        }
                    }
                    else
                    {
                        Log.SaveLog("Invaild URL", "Downloader", output);
                        //URL未指定或出现错误
                        Console.WriteLine("Failed to install.View log file for details.");
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        return;
                    }

                }
            }
            #endregion
            #region PackageMD5Checker
            module = "PackageMD5Checker";
            string packageMD5;
            if (config["doPackageChecking"] as string == "true")
            {
                Log.SaveLog("Checking the MD5 of package...", module, output);
                //开始检查运行库MD5
                
                try
                {
                    packageMD5 = GetMD5Hash(config["packagePath"] as string);
                    Log.SaveLog($"The MD5 of package is \"{packageMD5}\".", module, output);
                    //输出运行库MD5，方便检查
                }
                catch (Exception ex)
                {
                    Log.SaveLog("Unable to check the MD5 of package. ", module, output);
                    //文件MD5未能正确运算 可能原因:权限不足
                    Log.SaveLog(ex.ToString(), module, output);
                    Console.WriteLine("Failed to install.View log file for details.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
                if (config["runtimeMD5"] as string == packageMD5)
                {
                    Log.SaveLog("Package's MD5 is correct.", module, output);
                    //运行库完整
                }
                else
                {
                    //MD5不正确,程序包可能存在破损
                    //若允许,重新下载运行库
                    if (config["allowRedownloadPackage"] as string == "true")
                    {
                        try
                        {
                            if(File.Exists($"./package.{config["packageType"]}"))
                                File.Delete($"./package.{config["packageType"]}");
                            HttpDownload(config["packageURL"] as string, $"./package.{config["packageType"]}");
                            //开始下载
                            if (File.Exists($"./package.{config["packageType"]}"))
                            {
                                Log.SaveLog("Package downloading successful.", "HttpDownload", output);
                                //下载成功
                                config["packagePath"] = $"./package.{config["packageType"]}";
                            }
                            else
                            {
                                Log.SaveLog("Unable to download package : File does not exist.", module, output);
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
                            Log.SaveLog("Unable to download package : Unexpected exception.", "HttpDownload", output);
                            //下载出现异常
                            Console.WriteLine("Failed to install.View log file for details.");
                            Console.WriteLine("Press any key to exit...");
                            Console.ReadKey();
                            return;
                        }
                    }
                    else
                    {
                        Log.SaveLog("Runtime's MD5 is incorrect. Damaged package.", module, output);
                        Console.WriteLine("Failed to install.View log file for details.");
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                }

                if ((config["latestPackageMD5-URL"] as string).Contains("://"))
                {
                    try
                    {
                        if (HttpGet(config["latestPackageMD5-URL"] as string) == packageMD5)
                        {
                            Log.SaveLog("Package checked with Internet.", "OnlineMD5", output);
                        }
                        else
                        {
                            Log.SaveLog("Package is not the version from Internet.Trying to download...", "OnlineMD5", output);

                            try
                            {
                                if (File.Exists($"./package.{config["packageType"]}"))
                                    File.Delete($"./package.{config["packageType"]}");
                                HttpDownload(config["packageURL"] as string, $"./package.{config["packageType"]}");
                                //开始下载
                                if (File.Exists($"./package.{config["packageType"]}"))
                                {
                                    Log.SaveLog("Package downloading successful.", "HttpDownload", output);
                                    //下载成功
                                    config["packagePath"] = $"./package.{config["packageType"]}";
                                }
                                else
                                {
                                    Log.SaveLog("Unable to download package : File does not exist.", module, output);
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
                                Log.SaveLog("Unable to download package : Unexpected exception.", "HttpDownload", output);
                                //下载出现异常
                                Console.WriteLine("Failed to install the newest version from Internet. View log file for details.");
                                //是否继续安装视情况而定
                                if (config["allowSkipOnlinePackageChecking"] as string != "true")
                                {
                                    Console.WriteLine("Press any key to exit...");
                                    Console.ReadKey();
                                }
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        Log.SaveLog(ex.ToString(), "OnlineMD5", output);
                        Log.SaveLog("Unable to download package : Unexpected exception.", "OnlineMD5", output);
                        //GET出现异常
                        Console.WriteLine("Failed to check the MD5 from Internet. View log file for details.");
                        //是否继续安装视情况而定
                        if (config["allowSkipOnlinePackageChecking"] as string != "true")
                        {
                            Console.WriteLine("Press any key to exit...");
                            Console.ReadKey();
                        }
                    }
                }


            }
            #endregion

            Log.SaveLog("Packages' MD5 checked.", "PackageChecker", output);

            #region RuntimeInstaller
            module = "RuntimeInstaller";
            Log.SaveLog("Preparing for runtime installation...", module, output);
            Process runtimeInstaller = new();
            runtimeInstaller.StartInfo.FileName = config["runtimePath"] as string;
            runtimeInstaller.StartInfo.Arguments = config["runtimeArgs"] as string;
            runtimeInstaller.Start();
            runtimeInstaller.WaitForExit();
            Log.SaveLog("Runtime installer finished.", module, output);
            #endregion
            #region PackageInstaller
            module = "PackageInstaller";
            Log.SaveLog("Preparing for package installation...", module, output);
            Process packageInstaller = new();
            packageInstaller.StartInfo.FileName = config["packagePath"] as string;
            packageInstaller.StartInfo.Arguments = config["packageArgs"] as string;
            packageInstaller.Start();
            if (config["waitForExit"] as string == "true") 
            {
                packageInstaller.WaitForExit();
                Log.SaveLog("Package installer finished(exited).", module, output);
            }
            else
            {
                Log.SaveLog("Package installer started.");
            }
            if (config["openAfterDone"] as string != "none") 
            {
                Process.Start(config["openAfterDone"] as string);
            }
            Console.WriteLine("Installation finished. Press any key to exit.");
            Console.ReadKey();
            #endregion
        }

        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <param name="Url"></param>
        /// <returns>请求结果</returns>
        public static string HttpGet(string Url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        /// <summary>
        /// http下载文件
        /// </summary>
        /// <param name="url">下载文件地址</param>
        /// <param name="path">文件存放地址，包含文件名</param>
        public static void HttpDownload(string url, string path)
        {
            string tempPath = Path.GetDirectoryName(path) + @"/temp";
            Directory.CreateDirectory(tempPath);  //创建临时文件目录
            string tempFile = tempPath + @"/" + Path.GetFileName(path) + ".temp"; //临时文件
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
                File.Delete(tempFile);
                
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 计算文件MD5
        /// </summary>
        /// <param name="pathName">文件路径</param>
        /// <returns>文件的MD5校验码</returns>
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