using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuteApp
{
    public class WebServer
    {
        private static HttpListener _listener = null;
        public static void Init()
        {
            _listener = new System.Net.HttpListener();
            _listener.Prefixes.Add("http://*:1234/");
            _listener.Start();

            Process();
        }


        #region // From http://www.west-wind.com/weblog/posts/2009/Feb/05/Html-and-Uri-String-Encoding-without-SystemWeb
        /// <summary>
        /// UrlEncodes a string without the requirement for System.Web
        /// </summary>
        /// <param name="String"></param>
        /// <returns></returns>
        // [Obsolete("Use System.Uri.EscapeDataString instead")]
        public static string UrlEncode(string text)
        {
            // Sytem.Uri provides reliable parsing
            return System.Uri.EscapeDataString(text);
        }

        /// <summary>
        /// UrlDecodes a string without requiring System.Web
        /// </summary>
        /// <param name="text">String to decode.</param>
        /// <returns>decoded string</returns>
        public static string UrlDecode(string text)
        {
            // pre-process for + sign space formatting since System.Uri doesn't handle it
            // plus literals are encoded as %2b normally so this should be safe
            text = text.Replace("+", " ");
            return System.Uri.UnescapeDataString(text);
        }
        /// <summary>
        /// Retrieves a value by key from a UrlEncoded string.
        /// </summary>
        /// <param name="urlEncoded">UrlEncoded String</param>
        /// <param name="key">Key to retrieve value for</param>
        /// <returns>returns the value or "" if the key is not found or the value is blank</returns>
        public static string GetUrlEncodedKey(string urlEncoded, string key)
        {
            urlEncoded = "&" + urlEncoded + "&";

            int Index = urlEncoded.IndexOf("&" + key + "=", StringComparison.OrdinalIgnoreCase);
            if (Index < 0)
                return "";

            int lnStart = Index + 2 + key.Length;

            int Index2 = urlEncoded.IndexOf("&", lnStart);
            if (Index2 < 0)
                return "";

            return UrlDecode(urlEncoded.Substring(lnStart, Index2 - lnStart));
        }
        public static string GetBaseUrl(string urlEncoded)
        {
            string[] ss = urlEncoded.Split(new char[] { '&', '?' });
            if (ss.Length > 0)
                return ss[0];
            else
                return "";
        }

        #endregion

        public static string GetPlayerHtml()
        {
            // TODO: also cache it.  not useful until file ends up static
            System.IO.StreamReader sr = new System.IO.StreamReader("player.html");
            string html = sr.ReadToEnd();
            sr.Close();
            return html;
            // for images: have an images folder and have it automatically read in files if url matches.  cache file contents, too.

            MuteApp.BackgroundMusic bgMusic;
            string str = "<html><head><title>MuteTunes</title><style type='text/css'>a:link { color:white; } a:visited { color:white; } body {  background-color:black; margin-top: 1px; font-size: 12px; margin-bottom: 1px;}</style></head>";
            str += "<body>";
            str += "<select id='music_combo' onchange='getComboVal(this)'  style='font-family: monospace; font-size: 6pt;'>";

            bgMusic = SmartVolManagerPackage.BgMusicManager.ActiveBgMusic;
            str += "   <option value='" + bgMusic.Id + "'>" + bgMusic.GetName() + "</option>";
            for (int i = 0; i < SmartVolManagerPackage.BgMusicManager.MuteTunesConfig.BgMusics.Length; i++)
            {
                bgMusic = SmartVolManagerPackage.BgMusicManager.MuteTunesConfig.BgMusics[i];
                if (bgMusic.Id == SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id)
                    continue;
                str += "   <option value='" + bgMusic.Id + "'>" + bgMusic.GetName() + "</option>";
            }
            str += "</select>";
            str += "&nbsp;&nbsp;&nbsp;<a href='javascript:document.location = document.location;'>Refresh</a>&nbsp;&nbsp;";
            str += "<a href='Play'>" + "Play" + "</a>  <a href='Pause'>Pause</a>  <a href='Stop'>Stop</a> <a href='Mute'>Mute</a> <a href='Unmute'>Unmute</a> <a href='Show'>Show</a> <a href='Hide'>Hide</a> <a href='Settings'>Settings</a> <a href='Exit'>Exit</a>";

            str += "<script type='text/javascript'>";

            str += "function getComboVal(sel) { window.open('/ChangeMusic&id=' + sel.options[sel.selectedIndex].value); };";
            str += "</script>";
            str += "</body></html>";

            return str;
        }

        private static string GetMediaImageHtml(string operation)
        {
            return "<img src='" + operation + ".png" + "' height=32 width=32 title='" + operation + "'>";
        }
        private static string GetPathForLocalFile(string fileName)
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + fileName;
        }

        private static void Process()
        {
            Dictionary<string, string> fileCache = new Dictionary<string, string>();

            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    byte[] output;

                    //TODO: shouldn't allow just having a URL like this since people could create pages that mess with your background music.  Need to add auth.
                    //TODO: Have commands run in separate thread; make player look nicer (i.e. use icons)
                    if (context.Request.Url.AbsolutePath.StartsWith("/Open"))
                    {
                        string url = GetUrlEncodedKey(context.Request.RawUrl, "url");
                        WinSoundServerSysTray.ShowSite(url);
                    }

                    string str = GetPlayerHtml(); // default output

                    switch (GetBaseUrl(context.Request.Url.AbsolutePath))
                    {
                        case "/":
                            break;
                        case "/Play":
                            WinSoundServerSysTray.OnOperation("Play");
                            continue;
                        case "/Pause":
                            WinSoundServerSysTray.OnOperation("Pause");
                            continue;
                        case "/Mute":
                            WinSoundServerSysTray.OnOperation("Mute");
                            continue;
                        case "/Unmute":
                            WinSoundServerSysTray.OnOperation("Unmute");
                            continue;
                        case "/Show":
                            WinSoundServerSysTray.OnOperation("Show");
                            continue;
                        case "/Hide":
                            WinSoundServerSysTray.OnOperation("Hide");
                            continue;
                        case "/ChangeMusic":
                            try
                            {
                                string id = GetUrlEncodedKey(context.Request.RawUrl, "id");
                                WinSoundServerSysTray.ChangeMusic(int.Parse(id));
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.Write(ex);
                            }
                            continue;
                        case "/Stop":
                            WinSoundServerSysTray.OnOperation("Stop");
                            continue;
                        case "/Settings": //TODO: if interactive, then show it as a new window
                            //                        output = Encoding.ASCII.GetBytes("Settings");
                            break;
                        case "/Exit":
                            WinSoundServerSysTray.Exit();
                            continue;
                        default:
                            string filePath = GetBaseUrl(context.Request.Url.AbsolutePath);
                            string fileContents;
                            if (fileCache.TryGetValue(filePath, out fileContents))
                                str = fileContents;
                            else
                            {
                                try
                                {
                                    string pathOnDisk = GetPathForLocalFile(filePath);
                                    System.IO.StreamReader sr = new System.IO.StreamReader(pathOnDisk);
                                    str = sr.ReadToEnd();
                                    fileCache[filePath] = str;
                                    sr.Close();
                                }
                                catch (Exception ex)
                                {
                                    context.Response.StatusCode = 404;
                                    str = "404 - File not found";
                                    System.Diagnostics.Debug.Write(ex);
                                }
                            }
                            break;
                    }
                    output = Encoding.ASCII.GetBytes(str);
                    context.Response.ContentEncoding = Encoding.UTF8;
                    context.Response.ContentLength64 = output.Length;
                    context.Response.OutputStream.Write(output, 0, output.Length);
                    context.Response.OutputStream.Flush();
                    context.Response.OutputStream.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                }
            }

            /*
            System.Net.HttpListenerContext context = _listener.GetContext();
            System.Net.HttpListenerRequest request = context.Request;
            // Obtain a response object.
            System.Net.HttpListenerResponse response = context.Response;
            // Construct a response.
            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();*/
        }
    }
}
