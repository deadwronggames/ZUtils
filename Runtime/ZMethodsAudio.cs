using System.Collections.Generic;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsAudio
    {
        public static AudioClip GetRandomClipAvoidingRecent(this AudioClip[] audioClips, List<int> audioClipIndicesOrderedByRecentlyPlayed)
        {
            int numberOfAudioClips = audioClips.Length;
            if (numberOfAudioClips == 0) return null;
            if (numberOfAudioClips == 1) return audioClips[0];

            // basically take random audio clip from first half of the list of all audio clips
            int randomIndexFirstHalf = Random.Range(0, numberOfAudioClips / 2);
            int audioClipIndex = audioClipIndicesOrderedByRecentlyPlayed[randomIndexFirstHalf];
            
            // and then put that clip to the back of the list
            audioClipIndicesOrderedByRecentlyPlayed.RemoveAt(randomIndexFirstHalf);
            audioClipIndicesOrderedByRecentlyPlayed.Add(audioClipIndex);
            
            return audioClips[audioClipIndex];
        }
        
        public static float GetRemainingAudioClipTime(this AudioSource audioSource)
        {
            return (audioSource.clip == null) ? 0f : audioSource.clip.length - audioSource.time;
        }
        
        // TODO maybe fading
    }
}