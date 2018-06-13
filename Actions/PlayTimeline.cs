using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ActionGraph
{
    public class PlayTimeline : Action
    {
        // -------------------------------------------------------------------------------

        public Variable<PlayableDirector>   PlayableDirector = null;
        public TimelineAsset                TimelineOverride = null;
        public List<string>                 TrackNames = new List<string>();
        public List<Object>                 TrackBindings = new List<Object>();
        public bool                         WaitUntilComplete = true;


        public override string DisplayName
        {
            get
            {
                return (TimelineOverride == null) ? "Play " + PlayableDirector : "Play " + TimelineOverride.name;
            }
        }

        // -------------------------------------------------------------------------------

        protected override void OnStart()
        {
            if (PlayableDirector.Value != null)
            {
                // Override the timeline to play, if specified
                if (TimelineOverride != null)
                {
                    PlayableDirector.Value.playableAsset = TimelineOverride;
                }

                // Override the track bindings, if required
                if (TrackNames != null && TrackBindings != null)
                {
                    if (TrackNames.Count == TrackBindings.Count)
                    {
                        for (int i = 0; i < TrackNames.Count; i++)
                        {
                            SetGenericBinding(TrackNames[i], TrackBindings[i]);
                        }
                    }
                    else
                    {
                        Debug.LogError("PlayTimeline task has non-matching number of track names and bindings!");
                    }
                }

                // Play the timeline!
                PlayableDirector.Value.Play();
            }

            if (WaitUntilComplete)
            {
                PlayableDirector.Value.stopped += OnPlayableStopped;
            }
            else
            {
                FinishAction();
            }
        }

        // -------------------------------------------------------------------------------

        private void OnPlayableStopped(PlayableDirector playableDirector)
        {
            PlayableDirector.Value.stopped -= OnPlayableStopped;
            FinishAction();
        }

        // -------------------------------------------------------------------------------

        private void SetGenericBinding(string trackName, Object trackBinding)
        {
            foreach (var playableAssetOutput in PlayableDirector.Value.playableAsset.outputs)
            {
                if (playableAssetOutput.streamName == trackName)
                {
                    PlayableDirector.Value.SetGenericBinding(playableAssetOutput.sourceObject, trackBinding);
                }
            }
        }

        // -------------------------------------------------------------------------------
    }
}