//was: 330, 484

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MuteFm.UiPackage
{
    public partial class InitWizForm : Form
    {
        public InitWizForm()
        {
            InitializeComponent();
            this.Text = MuteFm.Constants.ProgramName + " - Getting Started Wizard";
            this.TopMost = true;
        }

        public void UpdateUI()
        {
            InitWizard();
            //LoadSite("player", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)  + @"player\internalmixer.html");
        }

        private void InitWizard()
        {
            webControl1.DocumentReady += new Awesomium.Core.UrlEventHandler(InitWizForm_DocumentReady);
            webControl1.Source = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\mixer\wizard.html");
        }

        void InitWizForm_DocumentReady(object sender, Awesomium.Core.UrlEventArgs e)
        {
            webControl1.DocumentReady -= new Awesomium.Core.UrlEventHandler(InitWizForm_DocumentReady);

            // Make sure the view is alive.
            if (!webControl1.IsLive)
                return;

            //Awesomium.Core.JSObject window = webControl1.ExecuteJavascriptWithResult("window");

            //string play = " if ($('.playing').length === 0) { $('.play_pause').click(); }";
            //Awesomium.Core.JSObject window = webControl1.ExecuteJavascriptWithResult(play);
            //Awesomium.Core.Error err = webControl1.GetLastError();


            // NOTE: Below code (and comments) are modified Awesomium sample code
            //
            // This sample demonstrates creating and acquiring a Global Javascript object.
            // These object persist for the lifetime of the web-view.  
            using (Awesomium.Core.JSObject myGlobalObject = webControl1.CreateGlobalJavascriptObject("mutefm"))
            {
                // 'Bind' is the method of the regular API, that needs to be used to create
                // a custom method on our global object and bind it to a handler.
                // The handler is of type JavascriptMethodEventHandler. Here we define it
                // using a lambda expression.
                myGlobalObject.Bind("launchWebSite", false, (s, e2) =>
                {
                    // We need to call this asynchronously because the code of 'ChangeHTML'
                    // includes synchronous calls. In this case, 'ExecuteJavascriptWithResult'.
                    // Synchronous Javascript interoperation API calls, cannot be made from
                    // inside Javascript method handlers.
                    BeginInvoke((Action<String>)LaunchWebSite, (string)e2.Arguments[0]);
                });
                myGlobalObject.Bind("closebrowser", false, (s, e2) =>
                {
                    BeginInvoke((Action)CloseBrowser, null);
                });
            }
        }

        private static void LaunchWebSite(string url)
        {
            switch (url)
            {
                case "http://www.mutefm.com/":
                case "http://www.mutefm.com/documentation.html":
                case "http://www.youtube.com/watch?v=60og9gwKh1o":
                    System.Diagnostics.Process.Start(url);
                    break;
            }
        }

        private static void CloseBrowser()
        {
            UiCommands.CloseGettingStartedWizard();
        }

        private void InitWizForm_Resize(object sender, EventArgs e)
        {
        }

        private void InitWizForm_Shown(object sender, EventArgs e)
        {
        }

        private void InitWizForm_Activated(object sender, EventArgs e)
        {
        }

        private void InitWizForm_Deactivate(object sender, EventArgs e)
        {
        }
    }
}
