using FMOD.Studio;
using Sxer.Plugin.AudioSystem.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace Sxer.Plugin.AudioSystem.FMODAudio
{
    public class FMODAudioPlayer : IAudioPlayer
    {
        private EventInstance _instance;
        public bool IsPlaying => _instance.isValid() && GetState() == PLAYBACK_STATE.PLAYING;

        public void Play(string eventPath, bool loop = false)
        {
            throw new System.NotImplementedException();
        }

        public void SetParameter(string name, float value)
        {
            throw new System.NotImplementedException();
        }

        public void Stop(bool immediate = false)
        {
            throw new System.NotImplementedException();
        }


        private PLAYBACK_STATE GetState()
        {
            _instance.getPlaybackState(out PLAYBACK_STATE state);
            return state;
        }

    }

}
