using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuteFm.SmartVolManagerPackage
{
    #region SiteVol
    public class BrowserVolRule
    {
        public string Selector;  // Can be URL or tabid; specify   url:[website] or tabid:[tabid]; website can include wildcards and maybe regexp
        public float MaxVolume;
        public BgMusicBehavior BgMusicBehavior;
    }

    public class BrowserSoundManager
    {
        public Dictionary<int, string> TabIdToUrlDict;

        /* websocket commands:          
            -- add website
            -- change website
            -- remove website
            -- get all websites
            -- set all websites

            -- add rule
            -- remove rule
            -- send rule list to extension
            -- send applicable rules to extension
*/

        // TODO: store active rules
        public static bool MuteByDefault = false;
        //public Dictionary<int, float> tabIdToMaxVolDict = new Dictionary<int, float>();
        //public Dictionary<string, float> urlExprToMaxVolDict = new Dictionary<string, float>();

        public static void OnUpdateSoundSourceInfos(SoundSourceInfo[] soundSourceInfos)
        {
            // TODO: send a message via socket indicating if there is sound coming from the browser or not
        }

        /*        public static void OnBrowserChange(TabInfo[] tabInfos) // TODO: maybe make this smart so that it doesn't have to look at every rule every time the user opens/closes a tab
                {
                    //NOTE: We do this only for the browser that the tabinfos belong to
            
                    bool matchingRuleFound = false;
                    float maxVolume = 1.0f;

                    for (int i = 0; i < tabInfos.Length; i++)
                    {
                        //TODO: do a lookup in each dict to see if there is a rule for this tabinfo.
                        //If so, set matchingRuleFound to true. If rule indicates lower volume, update maxVolume.
                    }

                    if ((matchingRuleFound == false) && (MuteByDefault == true))
                        maxVolume = 0.0f;

                    // TODO: set the browser's volume to maxVolume
                }*/
    }
    #endregion
}
