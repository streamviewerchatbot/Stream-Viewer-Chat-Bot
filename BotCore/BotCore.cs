﻿using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace BotCore
{
    public class Core
    {
        public static string zipDirectory = "";
        public static string proxyListDirectory = "";
        public static string streamUrl = "";
        public static bool headless = false;
        public static bool first = true;
        public static List<ChromeDriver> chromes = new List<ChromeDriver>();
        public static List<Thread> threads = new List<Thread>();
        public void Start(string proxyListDirectory, string stream, bool headless)
        {
            first = true;
            int i = 0;
            streamUrl = stream;
            System.IO.DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\zipSource\\");
            foreach (FileInfo res in di.GetFiles())
            {
                res.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            zipDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\zipSource\\background";
            string line = string.Empty;

            System.IO.StreamReader file = new System.IO.StreamReader(proxyListDirectory);
            while ((line = file.ReadLine()) != null)
            {
                var array = line.ToString().Split(':');
                string text = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\zipDirectory\\backgroundTemplate.js");
                text = text.Replace("{ip}", array[0]).Replace("{port}", array[1]).Replace("{username}", array[2]).Replace("{password}", array[3]);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\zipDirectory\\background.js", text);

                ZipFile.CreateFromDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\zipDirectory", AppDomain.CurrentDomain.BaseDirectory + "\\zipSource\\background" + i + ".zip");

                Thread thr = new Thread(Request);
                threads.Add(thr);
                Random r = new Random();
                int rInt = r.Next(0, 10000);
                if (!first)
                    Thread.Sleep(rInt);

                first = false;
                thr.Start(new Item { url = line, count = i });
                i++;
            }

            file.Close();
        }

        public void Stop()
        {
            foreach (var item in chromes)
            {
                item.Quit();
            }

            foreach (var item in threads)
            {
                item.Abort();
            }
        }

        private static void Request(object obj)
        {
            try
            {
                Random r = new Random();
                Item itm = (Item)obj;
                var array = itm.url.ToString().Split(':');
                var proxy = new Proxy();
                proxy.HttpProxy = array[0] + ':' + array[1];
                proxy.SslProxy = array[0] + ':' + array[1];
                var chrome_options = new ChromeOptions();
                chrome_options.Proxy = proxy;
                chrome_options.AcceptInsecureCertificates = true;

                if (headless)
                    chrome_options.AddArgument("headless");

                string[] resolutions = { "1152,864", "1080,720", "1400,1050", "1280,800", "1280,720", "1024,600", "1024,768", "800,600" };
                chrome_options.AddArgument("window-size=" + resolutions[r.Next(0, resolutions.Length - 1)]);
                chrome_options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36");
                chrome_options.AddExcludedArgument("enable-automation");
                chrome_options.AddAdditionalCapability("useAutomationExtension", false);
                chrome_options.AddExtension(zipDirectory + itm.count + ".zip");
                var driver = new ChromeDriver(chrome_options);

                //driver.Url = "https://15d7a43b4075c3068ed719ff0b3a5937.m.pipedream.net";
                //driver.Url = "https://whatismyipaddress.com/";
                driver.Url = streamUrl;
                chromes.Add(driver);
                driver.Navigate();
                
                bool mute = false;
                while (true)
                {
                    var mature = driver.FindElementsByClassName("tw-flex-grow-0");

                    foreach (var btn in mature)
                    {
                        try
                        {
                            if (btn.Text != null && btn.Text == "Start Watching")
                            {
                                btn.Click();

                                if (!mute)
                                {
                                    mute = true;
                                    new OpenQA.Selenium.Interactions.Actions(driver).SendKeys("m").Perform();
                                }
                            }
                        }
                        catch
                        {
                        }
                    }

                    var cache = driver.FindElementsByClassName("tw-pd-x-1");

                    foreach (var btn in cache)
                    {
                        try
                        {
                            if (btn.Text != null && btn.Text == "Accept" || btn.Text == "Kabul")
                            {
                                btn.Click();
                            }
                        }
                        catch
                        {
                        }
                    }

                    if (!mute)
                    {
                        mute = true;
                        new OpenQA.Selenium.Interactions.Actions(driver).SendKeys("m").Perform();
                    }

                    var item = driver.FindElementByClassName("simplebar-scrollbar");

                    if (item != null)
                    {

                        int rInt = r.Next(0, 5000);
                        item.SendKeys(Keys.Down);
                        Thread.Sleep(rInt);
                        item.SendKeys(Keys.Up);
                        Thread.Sleep(rInt);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error" + ex);
            }
            return;
        }
    }
    public class Item
    {

        public string url { get; set; }

        public int count { get; set; }
    }
}