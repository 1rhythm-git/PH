using UnityEngine;

namespace LootUp.Core.Feedback
{
    public enum HapticFeedbackPattern
    {
        Pivot,
        Damage
    }

    public static class HapticFeedback
    {
        private const float MinIntervalSeconds = 0.06f;

        private static float lastPlayedRealtime = -999f;
        private static bool isEnabled = true;

        public static bool IsEnabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }

        public static void Play(HapticFeedbackPattern pattern)
        {
            if (!isEnabled)
            {
                return;
            }

            bool isDamage = pattern == HapticFeedbackPattern.Damage;
            if (!isDamage && Time.realtimeSinceStartup - lastPlayedRealtime < MinIntervalSeconds)
            {
                return;
            }

            lastPlayedRealtime = Time.realtimeSinceStartup;
            HapticSpec spec = HapticSpec.FromPattern(pattern);
            PlayAndroid(spec);
        }

        private static void PlayAndroid(HapticSpec spec)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (vibrator == null)
                {
                    Handheld.Vibrate();
                    return;
                }

                bool hasVibrator = vibrator.Call<bool>("hasVibrator");
                if (!hasVibrator)
                {
                    return;
                }

                using AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION");
                int sdkInt = version.GetStatic<int>("SDK_INT");
                if (sdkInt >= 26)
                {
                    using AndroidJavaClass vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                    using AndroidJavaObject effect = vibrationEffect.CallStatic<AndroidJavaObject>(
                        "createOneShot",
                        spec.DurationMilliseconds,
                        spec.Amplitude);
                    vibrator.Call("vibrate", effect);
                    return;
                }

                vibrator.Call("vibrate", spec.DurationMilliseconds);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Android haptic failed. Falling back to Handheld.Vibrate(). {exception.Message}");
                Handheld.Vibrate();
            }
#else
            _ = spec;
#endif
        }

        private readonly struct HapticSpec
        {
            public HapticSpec(long durationMilliseconds, int amplitude)
            {
                DurationMilliseconds = durationMilliseconds;
                Amplitude = Mathf.Clamp(amplitude, 1, 255);
            }

            public long DurationMilliseconds { get; }
            public int Amplitude { get; }

            public static HapticSpec FromPattern(HapticFeedbackPattern pattern)
            {
                switch (pattern)
                {
                    case HapticFeedbackPattern.Damage:
                        return new HapticSpec(65L, 235);
                    default:
                        return new HapticSpec(12L, 65);
                }
            }
        }
    }
}
