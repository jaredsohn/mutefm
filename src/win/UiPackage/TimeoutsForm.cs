using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MuteFm
{
    public partial class mTimeoutsForm : Form
    {
        private MuteFmConfig _config = null;

        public mTimeoutsForm()
        {
            InitializeComponent();
        }

        public void Init(MuteFmConfig muteTunesConfig)
        {
            _config = muteTunesConfig;

            this.mAutokillCheckBox.Checked = (muteTunesConfig.GeneralSettings.AutokillMutedTime != 0);
            if (this.mAutokillCheckBox.Checked)
                this.mAutokillUpDown.Value = (decimal)muteTunesConfig.GeneralSettings.AutokillMutedTime;
            else
                this.mAutokillUpDown.Value = 0m;

            this.mFadeInUpDown.Value = (decimal)muteTunesConfig.GeneralSettings.FadeInTime;
            this.mFadeOutUpDown.Value = (decimal)muteTunesConfig.GeneralSettings.FadeOutTime;
            this.mPrefadeinUpDown.Value = (decimal)muteTunesConfig.GeneralSettings.SilentDuration;
            this.mPrefadeoutUpDown.Value = (decimal)muteTunesConfig.GeneralSettings.ActiveOverDurationInterval;
            this.mPollingIntervalUpDown.Value = (decimal)muteTunesConfig.GeneralSettings.SoundPollIntervalInS;
        }

        public void Save()
        {
            _config.GeneralSettings.AutokillMutedTime = this.mAutokillCheckBox.Checked ? (float)this.mAutokillUpDown.Value : 0;
            _config.GeneralSettings.FadeInTime = (float)this.mFadeInUpDown.Value;            
            _config.GeneralSettings.FadeOutTime = (float)this.mFadeOutUpDown.Value;
            _config.GeneralSettings.SilentDuration = (float)this.mPrefadeinUpDown.Value;
            _config.GeneralSettings.ActiveOverDurationInterval = (float)this.mPrefadeoutUpDown.Value;
            _config.GeneralSettings.SoundPollIntervalInS = (float)this.mPollingIntervalUpDown.Value;
            MuteFmConfigUtil.Save(_config);
            SmartVolManagerPackage.BgMusicManager.InitConstants();
        }

        private void mCancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mOkButton_Click(object sender, EventArgs e)
        {
            Save();
            this.Close(); 
        }

        private void mResetButton_Click(object sender, EventArgs e)
        {
            this.mPrefadeoutUpDown.Value = (decimal)MuteFmConfig.ActiveOverDurationIntervalDefault;
            this.mFadeOutUpDown.Value = (decimal)MuteFmConfig.FadeoutTimeoutDefault;

            this.mPrefadeinUpDown.Value = (decimal)MuteFmConfig.SilentDurationDefault;
            this.mFadeInUpDown.Value = (decimal)MuteFmConfig.FadeinTimeoutDefault;

            this.mPollingIntervalUpDown.Value = (decimal)MuteFmConfig.SoundPollIntervalDefault;

            this.mAutokillUpDown.Value = 1800;
            mAutokillCheckBox.Checked = true;
        }

        private void mAutokillUpDown_ValueChanged(object sender, EventArgs e)
        {
            this.mAutokillCheckBox.Checked = true;
        }
    }
}
