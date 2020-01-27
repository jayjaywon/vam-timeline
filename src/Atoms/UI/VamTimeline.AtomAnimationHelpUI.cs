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
Timeline allows creating keyframe animations. In short, each controllers gets assigned a position and rotation at a specific point in time, and they get interpolated between keyframes following a specific curve (bezier curve). If this all sounds like gibberish, please go read a quick overview about keyframe animations, or try Source FilmMaker.

Since there is no curve visualizer yet, it may be hard to get nice animations at first, but with a little bit of practice you should be able to create simple animations in a few minutes!

Check out the Getting Started page for a small walkthrough to get you started, or check out https://github.com/acidbubbles/vam-timeline for more information.
".Trim();
                        break;

                    case Page_GettingStarted:
                        _helpJSON.val = $@"
Building your first animation:

1. In {AtomAnimationSettingsUI.ScreenName} (in the top-left Tab menu), select the controllers you want to animate and press Add Controller for each of them. They will show up in a list underneath, uncheck them to remove them.
2. Go to the Controllers screen; you will see that there is a Scrubber component. This allows you to control the animation time. Move it in the middle, and move the controllers you have enabled in the previous step.
3. Now play with the Scrubber. You will see your animation! You can also use Play to start the animation. By default it will loop and run for {AtomAnimationClip.DefaultAnimationLength}s, but this can be configured.
4. If you want to modify your keyframes, use the Next/Previous Frame buttons. This will move the scrubber directly at a frame. See the top-right text box? There you can see which frame is selected, and what controllers have keyframes.

There you go! You can now add keyframes and create simpler animations! Check out the other help pages for more information on what is possible.
".Trim();
                        break;

                    case Page_Concepts_Animations:
                        _helpJSON.val = $@"
Each Atom can have one or multiple animations. Each animation is completely independent.

When you play an animation, if a 'Next Animation' is setup in the {AtomAnimationSettingsUI.ScreenName} screen, it will automatically switch to that animation after the configured time.

When the animation is playing, switching animation will blend for the Blend Duration configured in {AtomAnimationSettingsUI.ScreenName}.

When the animation is not playing, the plugin controls will update accordingly.

Note that each animation specify it's own controllers and targets. When changing to an animation that doesn't animate a controller, it will stay at it's current position.
".Trim();
                        break;

                    case Page_Concepts_Playback:
                        _helpJSON.val = $@"
{StorableNames.Scrubber}: Displays and controls the animation time. It will snap to the value defined in the {AtomAnimationSettingsUI.ScreenName} screen, by default

{StorableNames.Play}: Starts the animation from zero.

{StorableNames.Stop}: When playing, pauses the animation at the current time. When stopped, resets the time to zero.

{StorableNames.FilterAnimationTarget}: Filters which frames are affected by {StorableNames.PreviousFrame}, {StorableNames.NextFrame}, {StorableNames.ChangeCurve}, Cut/Delete, Copy and the Display text.

{StorableNames.PreviousFrame} / {StorableNames.NextFrame}: Navigates between all frames or the frames of the filtered animation target.

Note that if you have setup the VamTimeline.Controller, all linked atoms will play, stop and scrub together, even when within a single atom's playback controls.
".Trim();
                        break;

                    case Page_Concepts_AnimateMultipleAtoms:
                        _helpJSON.val = $@"
".Trim();
                        break;

                    case Page_Screen_AnimationSettings:
                        _helpJSON.val = $@"
".Trim();
                        break;

                    case Page_Screen_Controllers:
                        _helpJSON.val = $@"
".Trim();
                        break;

                    case Page_Screen_FloatParams:
                        _helpJSON.val = $@"
".Trim();
                        break;

                    case Page_Screen_Advanced:
                        _helpJSON.val = $@"
".Trim();
                        break;

                    case Page_Screen_Performance:
                        _helpJSON.val = $@"
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

