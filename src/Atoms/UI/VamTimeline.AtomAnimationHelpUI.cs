using System.Collections.Generic;
using System.Text;

namespace VamTimeline
{
    /// <summary>
    /// VaM Timeline
    /// By Acidbubbles
    /// Animation timeline with keyframes
    /// Source: https://github.com/acidbubbles/vam-timeline
    /// </summary>
    public class AtomAnimationHelpUI : AtomAnimationBaseUI
    {
        public const string ScreenName = "Help";
        public override string Name => ScreenName;

        private const string Page_Intro = "Intro";
        private const string Page_GettingStarted = "Getting Started";
        private const string Page_Concepts_Animations = "Concepts: Animations";
        private const string Page_Concepts_Playback = "Concepts: Playback";
        private const string Page_Concepts_AnimateMultipleAtoms = "Concepts: Animate Multiple Atoms";
        private const string Page_Screen_AnimationSettings = "Screen: " + AtomAnimationSettingsUI.ScreenName;
        private const string Page_Screen_Controllers = "Screen: " + AtomAnimationControllersUI.ScreenName;
        private const string Page_Screen_FloatParams = "Screen: " + AtomAnimationFloatParamsUI.ScreenName;
        private const string Page_Screen_Advanced = "Screen: " + AtomAnimationAdvancedUI.ScreenName;
        private const string Page_Screen_Performance = "Screen: " + AtomAnimationLockedUI.ScreenName;

        private JSONStorableString _helpJSON;
        private JSONStorableStringChooser _pagesJSON;

        public AtomAnimationHelpUI(IAtomPlugin plugin)
            : base(plugin)
        {

        }
        public override void Init()
        {
            base.Init();

            _helpJSON = new JSONStorableString("Page", "");
            _pagesJSON = new JSONStorableStringChooser(
                "Pages", new List<string>{
                Page_Intro,
                Page_Screen_AnimationSettings
            },
            "",
            "Pages",
            (string val) =>
            {
                switch (val)
                {
                    case Page_Intro:
                        _helpJSON.val = @"
It is expected that you have some basic knowledge of how Virt-A-Mate works before getting started. Basic knowledge of keyframe based animation is also useful. In a nutshell, you specify some positions at certain times, and all positions in between will be interpolated using curves (linear, smooth, etc.).

You can find out more on the project site: https://github.com/acidbubbles/vam-timeline

Building your first animation:

1. Add the VamTimeline.AtomAnimation.cslist plugin on atoms you want to animate, and open the plugin settings (Open Custom UI in the Atom's Plugin section).
2. In Animation Settings screen, select a controller you want to animate in the Animate Controller drop down, and select Add Controller to include it. This will turn on the ""position"" and ""rotation"" controls for that controller if that's not already done.
3. You can now select the Controllers tab by using the top-left drop-down. Your controller is checked, that means there is a keyframe at this time in the timeline.
4. To add a keyframe, move the Time slider to where you want to create a keyframe, and move the controller you have included before. This will create a new keyframe. You can also check the controller's toggle. Unchecking the toggle will delete that keyframe for that controller. Try navigating using the Next Frame and Previous Frame buttons, and try your animation using the Play button.
5. There is a text box on the top right; this shows all frames, and for the current frame (the current frame is shown using square brackets), the list of affected controllers. This is not as good as an actual curve, but you can at least visualize your timeline.
".Trim();
                        break;

                    case Page_GettingStarted:
                        _helpJSON.val = @"
".Trim();
                        break;

                    case Page_Concepts_Animations:
                        _helpJSON.val = @"
".Trim();
                        break;

                    case Page_Concepts_Playback:
                        _helpJSON.val = @"
".Trim();
                        break;

                    case Page_Concepts_AnimateMultipleAtoms:
                        _helpJSON.val = @"
".Trim();
                        break;

                    case Page_Screen_AnimationSettings:
                        _helpJSON.val = @"
".Trim();
                        break;

                    case Page_Screen_Controllers:
                        _helpJSON.val = @"
".Trim();
                        break;

                    case Page_Screen_FloatParams:
                        _helpJSON.val = @"
".Trim();
                        break;

                    case Page_Screen_Advanced:
                        _helpJSON.val = @"
".Trim();
                        break;

                    case Page_Screen_Performance:
                        _helpJSON.val = @"
".Trim();
                        break;

                    default:
                        _helpJSON.val = "Page Not Found";
                        break;
                }
            });

            var pagesUI = Plugin.CreateScrollablePopup(_pagesJSON);
            pagesUI.popupPanelHeight = 800;
            _linkedStorables.Add(_pagesJSON);

            var helpUI = Plugin.CreateTextField(_helpJSON, true);
            helpUI.height = 1200;
            _linkedStorables.Add(_helpJSON);

            _pagesJSON.val = "Basic setup";
        }
    }
}

