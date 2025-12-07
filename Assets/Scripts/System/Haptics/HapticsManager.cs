using UnityEngine;

public static class HapticsManager
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject vibrator;          // Vibrator
    private static AndroidJavaObject vibrationEffect;   // VibrationEffect (API>=26에서 사용)
    private static bool hasAmplitudeControl = false;
    private static int apiLevel = 0;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                apiLevel = version.GetStatic<int>("SDK_INT");
            }

            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
            {
                vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator");
            }

            if (vibrator != null && apiLevel >= 26)
            {
                // hasAmplitudeControl() 지원 여부 체크
                hasAmplitudeControl = vibrator.Call<bool>("hasAmplitudeControl");
            }
        }
        catch { /* 장치가 없거나 권한 이슈 등 */ }
#endif
    }

    public static bool IsSupported()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return vibrator != null;
#else
        return false;
#endif
    }

    // 가벼운 탭 (UI 클릭 등)
    public static void Light() => VibrateMs(12, 80);

    // 중간 강도 (데미지, 아이템 획득 등)
    public static void Medium() => VibrateMs(28, 160);

    // 강한 진동 (경고)
    public static void Heavy() => VibrateMs(55, 255);

    // 선택(Selection) 피드백 느낌
    public static void Selection() => VibrateMs(8, 100);

    // 성공 패턴
    public static void Success()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (vibrator == null) return;
        if (apiLevel >= 26)
        {
            long[] timings = new long[] { 0, 15, 40, 30 };   // 대기, on, off, on
            int[] amps     = new int[]  { 0, 130, 0, 200 };
            var pattern = CreateWaveform(timings, amps);
            vibrator.Call("vibrate", pattern);
        }
        else
        {
            vibrator.Call("vibrate", 35L);
        }
#endif
    }

    // 경고/주의 패턴
    public static void Warning()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (vibrator == null) return;
        if (apiLevel >= 26)
        {
            long[] timings = new long[] { 0, 40, 30, 40 };
            int[] amps     = new int[]  { 0, 180, 0, 180 };
            var pattern = CreateWaveform(timings, amps);
            vibrator.Call("vibrate", pattern);
        }
        else
        {
            vibrator.Call("vibrate", 70L);
        }
#endif
    }

    // 실패/게임오버 패턴
    public static void Failure()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (vibrator == null) return;
        if (apiLevel >= 26)
        {
            long[] timings = new long[] { 0, 30, 40, 30, 40, 45 };
            int[] amps     = new int[]  { 0, 220, 0, 220, 0, 255 };
            var pattern = CreateWaveform(timings, amps);
            vibrator.Call("vibrate", pattern);
        }
        else
        {
            vibrator.Call("vibrate", 120L);
        }
#endif
    }

    // ─────────────────────────── 내부 구현 ───────────────────────────

    private static void VibrateMs(long ms, int amplitude)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (vibrator == null) return;

        try
        {
            if (apiLevel >= 26)
            {
                // VibrationEffect.createOneShot(duration, amplitude)
                var effect = CreateOneShot(ms, amplitude);
                vibrator.Call("vibrate", effect);
            }
            else
            {
                // 구형 API는 단순 시간 기반
                vibrator.Call("vibrate", ms);
            }
        }
        catch
        {
            // 일부 기기에서 실패 시, 마지막 안전망
            try { vibrator.Call("vibrate", ms); } catch { }
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject CreateOneShot(long ms, int amplitude)
    {
        var vClass = new AndroidJavaClass("android.os.VibrationEffect");
        if (hasAmplitudeControl)
        {
            return vClass.CallStatic<AndroidJavaObject>("createOneShot", ms, Mathf.Clamp(amplitude, 1, 255));
        }
        else
        {
            // DEFAULT_AMPLITUDE 사용
            int DEFAULT_AMPLITUDE = vClass.GetStatic<int>("DEFAULT_AMPLITUDE");
            return vClass.CallStatic<AndroidJavaObject>("createOneShot", ms, DEFAULT_AMPLITUDE);
        }
    }

    private static AndroidJavaObject CreateWaveform(long[] timings, int[] amplitudes)
    {
        var vClass = new AndroidJavaClass("android.os.VibrationEffect");
        if (!hasAmplitudeControl) amplitudes = null; // DEFAULT_AMPLITUDE 사용
        return vClass.CallStatic<AndroidJavaObject>("createWaveform", timings, amplitudes, -1);
    }
#endif
}
