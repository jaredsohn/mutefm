using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperWebSocket;
using System.Threading;
using SuperWebSocket.SubProtocol;
using System.Reflection;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;
using MuteApp.SmartVolManagerPackage;
namespace MuteApp
{
    //Subcommands
    public class AUTH : SuperWebSocket.SubProtocol.JsonSubCommand<AuthReceiveData>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, AuthReceiveData commandInfo)
        {
            //TODO
        }
    }

    public class GETBGMUSICSITES : SuperWebSocket.SubProtocol.JsonSubCommand<GetBgMusicSiteReceiveData>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, GetBgMusicSiteReceiveData commandInfo)
        {
            MixerWebSocketServerHelper.SendCommand("BGMUSICSITES", new GetBgMusicSiteSendData());
        }
    }

    public class SHOWSITE: SuperWebSocket.SubProtocol.JsonSubCommand<ShowSiteReceiveData>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, ShowSiteReceiveData commandInfo)
        {
            //TODO: first check if bgmusic already exists instead of creating a new entry
            if (commandInfo.AlwaysBgMusic == true)
            {
                MuteApp.BackgroundMusic bgMusic = MuteApp.MuteTunesConfigUtil.FindBgMusic(commandInfo.TabUrl, MuteApp.SmartVolManagerPackage.BgMusicManager.MuteTunesConfig);
                if (bgMusic == null)
                {
                    bgMusic = MuteApp.MuteTunesConfigUtil.CreateWeb(commandInfo.TabTitle, commandInfo.TabUrl);
                    MuteApp.MuteTunesConfigUtil.AddBgMusic(bgMusic, BgMusicManager.MuteTunesConfig);
                }
                MixerWebSocketServerHelper.SendCommand("BGMUSICSITES", new GetBgMusicSiteSendData());
                MuteApp.MuteTunesConfigUtil.Save(BgMusicManager.MuteTunesConfig);
                UiPackage.UiCommands.OnOperation(bgMusic.Id, Operation.ChangeMusic, "");
                UiPackage.UiCommands.OnOperation(Operation.Show);
            }
            else
            {
                MuteApp.BackgroundMusic bgMusic = MuteApp.MuteTunesConfigUtil.FindBgMusic(commandInfo.TabUrl, MuteApp.SmartVolManagerPackage.BgMusicManager.MuteTunesConfig);
                if (bgMusic != null)
                {
                    UiPackage.UiCommands.OnOperation(bgMusic.Id, Operation.ChangeMusic, "");
                    UiPackage.UiCommands.ShowWebBgMusic();
                }
                else
                {
                    UiPackage.UiCommands.ShowSite(commandInfo.TabTitle, commandInfo.TabUrl);
                }
            }
        }
    }

    public class PERFORMOPERATION : SuperWebSocket.SubProtocol.JsonSubCommand<PerformOperationReceiveData>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, PerformOperationReceiveData commandInfo)
        {
            try
            {
                long musicId;
                musicId = commandInfo.MusicId;
                if (commandInfo.MusicId == MuteTunesConfig.UseActiveId)
                    musicId = BgMusicManager.ActiveBgMusic.Id;

                Operation op = (Operation)(Enum.Parse(typeof(Operation), commandInfo.Operation, true));
                UiPackage.UiCommands.OnOperation(musicId, op, commandInfo.Param1);
            }
            catch (Exception ex)
            {
                MuteApp.SmartVolManagerPackage.SoundEventLogger.LogMsg(ex);
            }
        }
    }

    public class GETPLAYERHTML : SuperWebSocket.SubProtocol.JsonSubCommand<PlayerHtmlReceiveData>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, PlayerHtmlReceiveData commandInfo)
        {
            //Read in player html and send it back
            string html = MuteApp.WebServer.GetExtensionPlayerHtml();
            PlayerHtmlSendData request = new PlayerHtmlSendData();
            request.Html = html;
            MixerWebSocketServerHelper.SendCommand("PLAYERHTML", request);
        }
    }
    public class GETPLAYERSTATE : SuperWebSocket.SubProtocol.JsonSubCommand<PlayerStateReceiveData>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, PlayerStateReceiveData commandInfo)
        {
            //TODO
        }
    }

    public class GETSETTINGS : SuperWebSocket.SubProtocol.JsonSubCommand<SettingsRequestReceiveData>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, SettingsRequestReceiveData commandInfo)
        {
            SettingsSendData request = new SettingsSendData(BgMusicManager.MuteTunesConfig);
            MixerWebSocketServerHelper.SendCommand("SETTINGS", request);
        }
    }

    public class UPDATESETTINGS : SuperWebSocket.SubProtocol.JsonSubCommand<SettingsUpdatedReceiveData>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, SettingsUpdatedReceiveData commandInfo)
        {
            // TODO: update mutetunesconfig


            MuteTunesConfigUtil.Save(BgMusicManager.MuteTunesConfig);
        }
    }


    // Old ones (maybe will add back for mutetab Pro)
    /*
    public class GETWEBSTATUS : SuperWebSocket.SubProtocol.JsonSubCommand<GetWebStatusResponse>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, GetWebStatusResponse commandInfo)
        {
            BrowserSoundInfos.UpdateBrowserSoundInfo(session.SessionID, commandInfo.TabStatuses);
        }
    }
    public class PERFORMOPERATION : SuperWebSocket.SubProtocol.JsonSubCommand<PerformOperationResponse>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, PerformOperationResponse commandInfo)
        {
            // just do nothing for now (maybe will want to call a delegate and update the UI)
        }
    }
    public class WEBSTATUSCHANGED : SuperWebSocket.SubProtocol.JsonSubCommand<WebStatusChangedResponse>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, WebStatusChangedResponse commandInfo)
        {
            //TODO: alert needs to support add, remove, change and indicate appropriate param

            // just do nothing for now (should perform logic similar to pid changing)
        }
    }
    public class TABCHANGED : SuperWebSocket.SubProtocol.JsonSubCommand<CurrentTabChangedResponse>
    {
        protected override void ExecuteJsonCommand(SuperWebSocket.WebSocketSession session, CurrentTabChangedResponse commandInfo)
        {
            BrowserSoundInfos.SetCurrentTabId(session.SessionID, commandInfo.TabId);
        }
    }
    */
    //TODO: try to combine this and the above class
    public class MixerWebSocketServerHelper
    {
        protected static WebSocketServer m_WebSocketServer;

        //protected static MixerWebSocketServer m_WebSocketServer;
        private static AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        private static AutoResetEvent m_OpenEvent = new AutoResetEvent(false);
        private static AutoResetEvent m_CloseEvent = new AutoResetEvent(false);
        private static string m_CurrentMessage = string.Empty;
        private static List<WebSocketSession> _sessions = new List<WebSocketSession>();

        private static readonly Type ThisType = typeof(MixerWebSocketServerHelper);

        public static void Init()
        {
            //LogUtil.Setup(new ConsoleLogger());

            //m_WebSocketServer = new MixerWebSocketServer(new BasicSubProtocol("Basic", new List<Assembly> { ThisType.Assembly }));
            m_WebSocketServer = new WebSocketServer(new BasicSubProtocol("Basic", new List<Assembly> { ThisType.Assembly }));
            m_WebSocketServer.Setup(new RootConfig(), new ServerConfig
            {
                Port = 7450,
                Ip = "127.0.0.1",
                MaxConnectionNumber = 100,
                Mode = SocketMode.Sync,
                Name = MuteApp.Constants.ProgramName,
                ReceiveBufferSize = 100000
            }, SocketServerFactory.Instance);
            m_WebSocketServer.NewSessionConnected += new SessionEventHandler<WebSocketSession>(m_WebSocketServer_NewSessionConnected);
            m_WebSocketServer.SessionClosed += new SessionEventHandler<WebSocketSession, CloseReason>(m_WebSocketServer_SessionClosed);

            m_WebSocketServer.Start();
        }

        public static bool SendCommand(string commandName, object request)
        {
            if (Program.LicenseExpired == true)
            {
                commandName = "LICENSEEXPIRED";
                request = new Object();
            }

            try
            {
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerHtmlSendData>(jsonString); //TODO-jared
                for (int i = 0; i < _sessions.Count; i++)
                {
                    _sessions[i].SendResponse((commandName + " " + jsonString).Trim());
                }
				if (_sessions.Count > 0)
					return true;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            return false;
        }

        public static void StopServer()
        {
            m_WebSocketServer.Stop();
        }

        private static void m_WebSocketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            _sessions.Remove(session);
        }

        private static void m_WebSocketServer_NewSessionConnected(WebSocketSession session)
        {
            _sessions.Add(session);
            //WinSoundServer.OsIntegrationPackage.PidManager.AddSocketServerProcName(""); could get it from first received data (if so, don't do this here)

            //Read in player html and send it back
            string html = MuteApp.WebServer.GetExtensionPlayerHtml();
            PlayerHtmlSendData request = new PlayerHtmlSendData();
            request.Html = html;
            MixerWebSocketServerHelper.SendCommand("PLAYERHTML", request);
            UiPackage.UiCommands.UpdateUiForState(); // sends playerstate            
            MixerWebSocketServerHelper.SendCommand("BGMUSICSITES", new GetBgMusicSiteSendData());
            int x = 0;
            x++;
        }
    }
}
