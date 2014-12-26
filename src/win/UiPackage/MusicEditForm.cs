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
    public partial class MusicInfoEditForm : Form
    {
        private SoundPlayerInfo _musicInfo = null;
        Dictionary<string, string> _commands = new Dictionary<string, string>();

        public MusicInfoEditForm()
        {
            InitializeComponent();
            this.Text = Constants.ProgramName + " - " + "Edit Sound Player Info";
        }
        public MusicInfoEditForm(SoundPlayerInfo playerInfo, bool editable)
        {
            InitializeComponent();
            this.Text = Constants.ProgramName + " - " + "Edit Sound Player Info";

#if NOAWE
            mWebsiteRadioButton.Text += " (Requires mute.fm+)";
#endif

            if (playerInfo == null)
            {
                this.Text = Constants.ProgramName + " - Add Sound Info";
                _musicInfo = new SoundPlayerInfo();
#if !NOAWE
                _musicInfo.IsWeb = true;
                mWebsiteRadioButton.Checked = true;
                mProgramRadioButton.Enabled = true;
                mWebsiteRadioButton.Enabled = true;
#else
                _musicInfo.IsWeb = false;
                mWebsiteRadioButton.Enabled = false;
                mProgramRadioButton.Enabled = true;
                mProgramRadioButton.Checked = true;
                mWebsiteRadioButton_CheckedChanged_1(null, null);
#endif
                this.mIgnoreAutomuteCheckbox.Checked = false;
                this.mIgnoreAutomuteCheckbox.Visible = !_musicInfo.IsWeb;
                mStopIfMutedTooLongCheckbox.Checked = true;

                _commands["OnLoad"] = "";
                _commands["Play"] = "";
                _commands["Pause"] = "";
                _commands["PrevTrack"] = "";
                _commands["NextTrack"] = "";
                _commands["Like"] = "";
                _commands["Dislike"] = "";
                _commands["Stop"] = "";
            }
            else
            {
                _musicInfo = playerInfo;
                mWebsiteRadioButton.Checked = _musicInfo.IsWeb;
                mProgramRadioButton.Checked = !_musicInfo.IsWeb;
                mIgnoreAutomuteCheckbox.Visible = !_musicInfo.IsWeb;
                mWebsiteRadioButton_CheckedChanged_1(null, null);

                bool ignoreForAutomute = false;
                SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict.TryGetValue(GetProcName(_musicInfo.UrlOrCommandLine), out ignoreForAutomute);
                this.mIgnoreAutomuteCheckbox.Checked = ignoreForAutomute;
                mStopIfMutedTooLongCheckbox.Checked = playerInfo.KillAfterAutoMute;

                _commands["OnLoad"] = _musicInfo.OnLoadCommand;
                _commands["Play"] = _musicInfo.PlayCommand;
                _commands["Pause"] = _musicInfo.PauseCommand;
                _commands["PrevTrack"] = _musicInfo.PrevSongCommand;
                _commands["NextTrack"] = _musicInfo.NextSongCommand;
                _commands["Like"] = _musicInfo.LikeCommand;
                _commands["Dislike"] = _musicInfo.DislikeCommand;
                _commands["Stop"] = _musicInfo.StopCommand;

                mIcon.Image = MuteFmConfigUtil.GetImage(_musicInfo.Id, 16);
            }

            if (!editable)
            {
                foreach (Control control in this.Controls)
                {
                    if ((string)control.Tag != "+")
                        control.Enabled = false;
                }
            }
        }

        private void mCancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mOkButton_Click(object sender, EventArgs e)
        {
            if (mMusicInfoNameTextBox.Text.Trim() == "")
            {
                MessageBox.Show(this, "Error: Name not set.");
                return;
            }
            if ((!mWebsiteRadioButton.Checked) && (!System.IO.File.Exists(mMusicInfoFileNameTextBox.Text.Trim())))
            {
                MessageBox.Show(this, "File not found.  Please enter full path and do not include quotes.", Constants.ProgramName);
                return;
            }

            // Update ignoreforautomute
            if (!mWebsiteRadioButton.Checked)
            {
                bool wasIgnoreForAutomute;
                SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict.TryGetValue(GetProcName(_musicInfo.UrlOrCommandLine), out wasIgnoreForAutomute);
                if ((wasIgnoreForAutomute) && (!mIgnoreAutomuteCheckbox.Checked))
                {
                    // Remove it
                    string procname = GetProcName(_musicInfo.UrlOrCommandLine);
                    SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict.Remove(procname);
                    MuteFmConfigUtil.InitIgnoreForAutoMute(SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict, SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                }
                else if (((!wasIgnoreForAutomute) && (mIgnoreAutomuteCheckbox.Checked)) && (mIgnoreAutomuteCheckbox.Visible = true))
                {
                    // Add it
                    string procname = GetProcName(mMusicInfoFileNameTextBox.Text);
                    SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict[procname] = true;
                    MuteFmConfigUtil.InitIgnoreForAutoMute(SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict, SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                }
            }

            _musicInfo.IsWeb = mWebsiteRadioButton.Checked;
            _musicInfo.Name = this.mMusicInfoNameTextBox.Text;

            if (_musicInfo.IsWeb)
            {
                _musicInfo.UrlOrCommandLine = this.mWebsiteUrlTextBox.Text.Trim();
                _musicInfo.CommandLineArgs = "";
            }
            else
            {
                _musicInfo.UrlOrCommandLine = this.mMusicInfoFileNameTextBox.Text.Trim();
                _musicInfo.CommandLineArgs = this.mProgramArgumentsTextBox.Text;
            }

            if (this.Text == Constants.ProgramName + " - Add Sound Info")
                MuteFmConfigUtil.AddSoundPlayerInfo(_musicInfo, SmartVolManagerPackage.BgMusicManager.MuteFmConfig);

            _musicInfo.OnLoadCommand = _commands["OnLoad"];
            _musicInfo.PlayCommand = _commands["Play"];
            _musicInfo.PauseCommand = _commands["Pause"];
            _musicInfo.PrevSongCommand = _commands["PrevTrack"];
            _musicInfo.NextSongCommand = _commands["NextTrack"];
            _musicInfo.LikeCommand = _commands["Like"];
            _musicInfo.DislikeCommand = _commands["Dislike"];
            _musicInfo.StopCommand = _commands["Stop"];

            _musicInfo.KillAfterAutoMute = mStopIfMutedTooLongCheckbox.Checked;

            MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);

            MuteFmConfigUtil.GenerateIconImage(_musicInfo, true);

            this.Close();
        }

        private void mWebsiteRadioButton_CheckedChanged_1(object sender, EventArgs e)
        {
            mWebsiteGroupBox.Visible = mWebsiteRadioButton.Checked;
            mProgramGroupBox.Visible = !mWebsiteRadioButton.Checked;
            mIgnoreAutomuteCheckbox.Visible = !mWebsiteRadioButton.Checked;

            if (mWebsiteRadioButton.Checked)
            {
                mWebsiteUrlTextBox_TextChanged(null, null);
            } else
            {
                mMusicInfoFileNameTextBox_TextChanged(null, null);
            }

            this.mCommandsListbox.Items.Clear();
            if (mWebsiteRadioButton.Checked == true)
                this.mCommandsListbox.Items.Add("OnLoad");

            this.mCommandsListbox.Items.Add("Play");
            this.mCommandsListbox.Items.Add("Pause");
            this.mCommandsListbox.Items.Add("PrevTrack");
            this.mCommandsListbox.Items.Add("NextTrack");
            this.mCommandsListbox.Items.Add("Like");
            this.mCommandsListbox.Items.Add("Dislike");
            if (mWebsiteRadioButton.Checked == false)
                this.mCommandsListbox.Items.Add("Stop");
        }

        private void MusicInfoEditForm_Load(object sender, EventArgs e)
        {
            if (_musicInfo == null)
                return;

            this.mMusicInfoNameTextBox.Text = _musicInfo.GetName();

            if (_musicInfo.IsWeb)
            {
                mWebsiteRadioButton.Checked = true;
                mWebsiteUrlTextBox.Text = _musicInfo.UrlOrCommandLine;
                mMusicInfoFileNameTextBox.Text = "";
                mProgramArgumentsTextBox.Text = "";
            }
            else
            {
                mProgramRadioButton.Checked = true;
                mWebsiteUrlTextBox.Text = "";
                mMusicInfoFileNameTextBox.Text = _musicInfo.UrlOrCommandLine;
                mProgramArgumentsTextBox.Text = _musicInfo.CommandLineArgs;
            }
        }

        private void mMusicInfoEditButton_Click(object sender, EventArgs e)
        {
            if (this.mCommandsListbox.Text == "")
                return;

            string commandText = _commands[this.mCommandsListbox.Text];
            bool isChecked = false;

            string promptText = "";
            if (this.mWebsiteRadioButton.Checked)
            {
                promptText = "Enter a JavaScript command that should be run when this operation occurs.\n\nYou can most easily debug it by going to the same page in your browser.";
            }
            else
            {
                promptText = "Enter a process name with args that should be run when this operation occurs.\n\nYou can include the process name by entering '$procname$'.";
            }

            DialogResult result = InputBoxWithCheck.Show(this, Constants.ProgramName + " - " + this.mCommandsListbox.Text, promptText, null, ref commandText, ref isChecked);
            if (result != System.Windows.Forms.DialogResult.Cancel)
            {
                _commands[this.mCommandsListbox.Text] = commandText;

                mCommandsListbox_SelectedIndexChanged(null, null);
            }
        }

        private void mCommandsListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.mCommandsListbox.Text == "")
                return;

            string commandText = _commands[this.mCommandsListbox.Text];

            if (commandText.Trim() == "")
                commandText = "(none)";
            mCustomCommandTextBox.Text = "Command: " + commandText;
        }

        private void mBrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog  = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "Applications (*.exe)|*.exe";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
               this.mMusicInfoFileNameTextBox.Text = dialog.FileName;
            }
        }

        private void mTestButton_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(mMusicInfoFileNameTextBox.Text.Trim()))
            {
                try
                {
                    System.Diagnostics.Process.Start(mMusicInfoFileNameTextBox.Text.Trim(), mProgramArgumentsTextBox.Text.Trim());
                }
                catch 
                { 
                }
            }
            else
            {
                MessageBox.Show(this, "File does not exist.  Please include full path and no quotes.", Constants.ProgramName);
            }
        }

        private void mCommandsListbox_DoubleClick(object sender, EventArgs e)
        {
            mMusicInfoEditButton_Click(null, null);
        }

        private void mTestWebButton_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(mWebsiteUrlTextBox.Text.Trim(), mProgramArgumentsTextBox.Text.Trim());
            }
            catch
            {

            }
        }

        private static string GetProcName(string fullPath)
        {
            string fullPathNotNull = fullPath;
            if (fullPath == null)
                fullPathNotNull = "";

            string procname = "";
            string filename = System.IO.Path.GetFileName(fullPathNotNull).ToLower();
            if (filename != "")
            {
                string[] parts = filename.Split('.');
                procname = parts[0];
            }

            return procname;
        }

        private void mDefaultsButton_Click(object sender, EventArgs e)
        {
            string procname = "";
            if (mWebsiteRadioButton.Checked)
                procname = this.mWebsiteUrlTextBox.Text.Trim().TrimEnd('/');
            else
                procname = GetProcName(this.mMusicInfoFileNameTextBox.Text);
            if (MuteFmConfigUtil.hasDefaults(procname))
            {
                SoundPlayerInfo tempMusicInfo = new SoundPlayerInfo();
                if (mWebsiteRadioButton.Checked)
                    tempMusicInfo.UrlOrCommandLine = procname;
                else
                    tempMusicInfo.UrlOrCommandLine = mMusicInfoFileNameTextBox.Text;

                if (this.mWebsiteRadioButton.Checked)
                    MuteFmConfigUtil.InitDefaultsWeb(tempMusicInfo);
                else
                    MuteFmConfigUtil.InitDefaultsProcess(tempMusicInfo, procname);
                _commands["OnLoad"] = tempMusicInfo.OnLoadCommand;
                _commands["Play"] = tempMusicInfo.PlayCommand;
                _commands["Pause"] = tempMusicInfo.PauseCommand;
                _commands["PrevTrack"] = tempMusicInfo.PrevSongCommand;
                _commands["NextTrack"] = tempMusicInfo.NextSongCommand;
                _commands["Like"] = tempMusicInfo.LikeCommand;
                _commands["Dislike"] = tempMusicInfo.DislikeCommand;
                _commands["Stop"] = tempMusicInfo.StopCommand;

                MessageBox.Show(this, "Defaults loaded.", Constants.ProgramName);
                mCommandsListbox_SelectedIndexChanged(null, null);  
            }
            else
            {
                MessageBox.Show(this, "No defaults", Constants.ProgramName); // should never see this
            }
        }

        private void mMusicInfoFileNameTextBox_TextChanged(object sender, EventArgs e)
        {
            string procname = GetProcName(this.mMusicInfoFileNameTextBox.Text);
            mDefaultsButton.Enabled = MuteFmConfigUtil.hasDefaults(procname);

            //this.mNoIgnoreAutomuteCheckbox.Checked = 
        }

        private void mWebsiteUrlTextBox_TextChanged(object sender, EventArgs e)
        {
            mDefaultsButton.Enabled = MuteFmConfigUtil.hasDefaults(mWebsiteUrlTextBox.Text);
        }

        private void mIcon_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("TODO: change source for icon here.");
        }

        private void mExportButton_Click(object sender, EventArgs e)
        {
            SoundExportData data = new SoundExportData();

            data.PlayCommand = _commands["Play"];
            data.PauseCommand = _commands["Pause"];
            data.StopCommand = _commands["Stop"];
            data.NextTrackCommand = _commands["NextTrack"];
            data.PrevTrackCommand = _commands["PrevTrack"];
            data.OnLoadCommand = _commands["OnLoad"];
            data.LikeCommand = _commands["Like"];
            data.DislikeCommand = _commands["Dislike"];

            string jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            string tempFileName = System.IO.Path.GetTempFileName();

            System.IO.File.WriteAllText(tempFileName, jsonText);

            MessageBox.Show(this, "To share with the community, please e-mail to mutefmapp@gmail.com.", Constants.ProgramName);
            System.Diagnostics.Process.Start("notepad.exe", tempFileName);
        }

        [Serializable]
        private class SoundExportData
        {
            public SoundExportData()
            {

            }

            public string PlayCommand = "";
            public string PauseCommand = "";
            public string StopCommand = "";
            public string NextTrackCommand = "";
            public string PrevTrackCommand = "";
            public string OnLoadCommand = "";
            public string LikeCommand = "";
            public string DislikeCommand = "";
        }
    }
}
