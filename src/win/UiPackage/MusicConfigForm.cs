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
    public partial class MusicConfigForm : Form
    {
        private TreeNode _activeSoundsNode, _favoritesNode;

        public MusicConfigForm()
        {
            InitializeComponent();
            this.Text = Constants.ProgramName + " - " + "Sound Players";

            Init(SmartVolManagerPackage.BgMusicManager.MuteFmConfig, SmartVolManagerPackage.BgMusicManager.GetRecentMusics()); // TODO: don't do it this way
        }

        private void UpdateBgMusicUI()
        {
            SoundPlayerInfo playerInfo = SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GetActiveBgMusic();
            mBackgroundMusicName.Text = playerInfo.Name;

            this.mBgMusicIcon.Image = MuteFmConfigUtil.GetImage(playerInfo.Id, 16);
        }

        public void Init(MuteFmConfig muteTunesConfig, SoundPlayerInfo[] fgMusics)
        {
            TreeNode node;
            Object currentTag = null;

            UpdateBgMusicUI();

            if (mSoundTree.SelectedNode != null)
                currentTag = mSoundTree.SelectedNode.Tag;


            ImageList imageList = new ImageList();
            mSoundTree.ImageList = imageList;
            TreeNode[] children = new TreeNode[0];

            imageList.Images.Add("0", UiCommands.mPlayerForm.Icon.ToBitmap());

            mSoundTree.Nodes.Clear();

            //TreeNode supportedWebNode = mSoundTree.Nodes.Add("Supported Web");
            _activeSoundsNode = new TreeNode("Recent Sounds", 0, 0, children);
            mSoundTree.Nodes.Add(_activeSoundsNode);
            _favoritesNode = new TreeNode("Favorites", 0, 0, children);
            mSoundTree.Nodes.Add(_favoritesNode);

            for (int i = 0; i < fgMusics.Length; i++)
            {
                string fgMusicName = fgMusics[i].ShortProcessName;
                if (fgMusicName.Trim() == "")
                    fgMusicName = "System Sounds";
                node = new TreeNode(fgMusicName, i+1, i+1, children);
                _activeSoundsNode.Nodes.Add(node);
                node.Tag = fgMusics[i].ShortProcessName;
                imageList.Images.Add((string)(node.Tag), MuteFmConfigUtil.GetImage(fgMusics[i].Id, 16)); 
                if ((string)node.Tag == (string)currentTag)
                    mSoundTree.SelectedNode = node;
            }

            int j = 0;
            for (int i = 0; i < muteTunesConfig.BgMusics.Length; i++)
            {
#if NOAWE
                // Don't show web-based music in the editor
                if (muteTunesConfig.BgMusics[i].IsWeb)
                    continue;
#endif

                node = new TreeNode(muteTunesConfig.BgMusics[i].Name, j + 1 + fgMusics.Length, j + 1 + fgMusics.Length, children);
                node.Tag = muteTunesConfig.BgMusics[i].Id.ToString();
                _favoritesNode.Nodes.Add(node);
                Image image = MuteFmConfigUtil.GetImage(muteTunesConfig.BgMusics[i].Id, 16);
                if (image != null)
                    imageList.Images.Add((string)(node.Tag), image); 

                if ((string)node.Tag == (string)currentTag)
                    mSoundTree.SelectedNode = node;
                j++;
            }
            _activeSoundsNode.ExpandAll();
            _favoritesNode.ExpandAll();
        }

        private void mOkButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mChooseButton_Click(object sender, EventArgs e)
        {
            chooseSelected();
        }

        private bool chooseSelected()
        {
            try
            {
                if (mSoundTree.SelectedNode == null)
                    return false;

                Cursor.Current = Cursors.WaitCursor;
                if ((mSoundTree.SelectedNode.Parent != null) && (mSoundTree.SelectedNode.Parent.Text == "Recent Sounds"))
                {
                    SoundPlayerInfo soundInfo = getRecentByName((string)mSoundTree.SelectedNode.Tag);
                    if (soundInfo.Name == "System Sounds")
                    {
                        MessageBox.Show("You cannot set this sound as your background music.");
                        Cursor.Current = Cursors.Default;
                        return false;
                    }
                    soundInfo = makeSelectedAFavorite();
                    UiCommands.OnOperation(soundInfo.Id, Operation.ChangeMusic, "", false, true);
                    UpdateBgMusicUI();
                    refresh();
                }
                else if ((mSoundTree.SelectedNode.Parent != null) && (mSoundTree.SelectedNode.Parent.Text == "Favorites"))
                {
                    long musicId = long.Parse((string)mSoundTree.SelectedNode.Tag);
                    SoundPlayerInfo soundInfo = SmartVolManagerPackage.BgMusicManager.FindPlayerInfo(musicId);
                    if (soundInfo.Name == "System Sounds")
                    {
                        MessageBox.Show("You cannot set this sound as your background music.");
                        Cursor.Current = Cursors.Default;
                        return false;
                    }
                    UiCommands.OnOperation(musicId, Operation.ChangeMusic, "", false, true);
                    UpdateBgMusicUI();
                }
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
            Cursor.Current = Cursors.Default;

            return true;
        }

        private void mEditBgButton_Click(object sender, EventArgs e)
        {
            long musicId = SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id;
            SoundPlayerInfo playerInfo = SmartVolManagerPackage.BgMusicManager.FindPlayerInfo(musicId);
            showEditDialog(playerInfo, true);
        }
        private void showEditDialog(SoundPlayerInfo playerInfo, bool editable)
        {
            if (playerInfo != null)
            {
                MusicInfoEditForm form = new MusicInfoEditForm(playerInfo, editable);
                form.ShowDialog();
                if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                    MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
            }
        }

        private void mEditAvailableButton_Click(object sender, EventArgs e)
        {
            if ((mSoundTree.SelectedNode != null) && (mSoundTree.SelectedNode.Tag != null))
            {
                SoundPlayerInfo playerInfo = null;
                bool editable = false;

                if (mSoundTree.SelectedNode.Parent.Text == "Recent Sounds")
                {
                    playerInfo = getRecentByName((string)mSoundTree.SelectedNode.Tag);
                }
                else
                {
                    long musicId = long.Parse((string)mSoundTree.SelectedNode.Tag);
                    playerInfo = SmartVolManagerPackage.BgMusicManager.FindPlayerInfo(musicId);
                    editable = true;
                }
                
                showEditDialog(playerInfo, editable);
                refresh();
            }
        }

        private void mAddButton_Click(object sender, EventArgs e)
        {
            MusicInfoEditForm form = new MusicInfoEditForm(null, true);
            form.ShowDialog();
            form.DialogResult = System.Windows.Forms.DialogResult.None;
            this.DialogResult = System.Windows.Forms.DialogResult.None;

            refresh();
        }

        private void mDeleteButton_Click(object sender, EventArgs e)
        {
            if ((mSoundTree.SelectedNode != null) && (mSoundTree.SelectedNode.Tag != null))
            {
                if (long.Parse((string)mSoundTree.SelectedNode.Tag) == SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GetActiveBgMusic().Id)
                {
                    MessageBox.Show(this, "Cannot delete current background music.", Constants.ProgramName);
                    return;
                }

                MuteFmConfigUtil.RemoveBgMusic(long.Parse((string)mSoundTree.SelectedNode.Tag), SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);

                refresh();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            refresh();
        }

        private void refresh()
        {
            Init(SmartVolManagerPackage.BgMusicManager.MuteFmConfig, SmartVolManagerPackage.BgMusicManager.GetRecentMusics());
        }

        private void mSoundTree_DoubleClick(object sender, EventArgs e)
        {
            if ((mSoundTree.SelectedNode != null) && (mSoundTree.SelectedNode.Tag != null))
            {
                if (chooseSelected())
                    this.Close();
            }            
        }

        private void mSoundTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.mFavoriteButton.Enabled = ((mSoundTree.SelectedNode.Parent != null) && (mSoundTree.SelectedNode.Parent.Text == "Recent Sounds"));
            this.mEditAvailableButton.Enabled = ((mSoundTree.SelectedNode.Parent != null)); // && (mSoundTree.SelectedNode.Parent.Text == "Favorites"));
            this.mDeleteButton.Enabled = ((mSoundTree.SelectedNode.Parent != null) && (mSoundTree.SelectedNode.Parent.Text == "Favorites"));
            this.mChooseButton.Enabled = (mSoundTree.SelectedNode.Parent != null);
        }

        private SoundPlayerInfo getRecentByName(string name)
        {
            SoundPlayerInfo soundPlayerInfo = null;
            KeyValuePair<SoundPlayerInfo, DateTime> kvp;
            if (SmartVolManagerPackage.BgMusicManager.RecentMusics.TryGetValue(name, out kvp))
            {
                soundPlayerInfo = kvp.Key;
            }
            return soundPlayerInfo;

            /*
            SoundPlayerInfo soundInfo = null;

            SoundPlayerInfo[] fgMusics = SmartVolManagerPackage.BgMusicManager.FgMusics;

            for (int i = 0; i < fgMusics.Length; i++)  // Assumes all names are unique. If not, user cannot distinguish via UI anyway so not a bug.
            {
                if ((string)fgMusics[i].ShortProcessName == (string)mSoundTree.SelectedNode.Tag)
                {
                    soundInfo = fgMusics[i];
                    break;
                }
            }
            return soundInfo;*/
        }

        private SoundPlayerInfo makeSelectedAFavorite()
        {
            bool found = false;            
            SoundPlayerInfo selectedSoundInfo = getRecentByName((string)mSoundTree.SelectedNode.Tag);
            SoundPlayerInfo playerInfo = selectedSoundInfo;

            if (selectedSoundInfo != null)
            {
                // Determine if a favorite already exists for this and if so select it instead of adding a new favorite.
                int len = _favoritesNode.Nodes.Count;
                for (int i = 0; i < len; i++)
                {
                    long musicId = long.Parse((string)_favoritesNode.Nodes[i].Tag);
                    playerInfo = SmartVolManagerPackage.BgMusicManager.FindPlayerInfo(musicId);
                    if (playerInfo != null)
                    {
                        if (playerInfo.UrlOrCommandLine.ToLower() == selectedSoundInfo.UrlOrCommandLine.ToLower())
                        {
                            this.mSoundTree.SelectedNode = _favoritesNode.Nodes[i];
                            found = true;
                            break;
                        }                    }
                }

                if (found)
                {
                    //MessageBox.Show("Already exists as a favorite.");
                }
                else
                {
                    selectedSoundInfo.Name = mSoundTree.SelectedNode.Text;
                    MuteFmConfigUtil.AddSoundPlayerInfo(selectedSoundInfo, SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                }
            }
            return playerInfo;
        }

        private void mFavoriteButton_Click(object sender, EventArgs e)
        {
            makeSelectedAFavorite();
            refresh();
        }
    }
}