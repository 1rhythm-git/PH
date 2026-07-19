using System;
using System.IO;
using UnityEditor.Android;

namespace PH.Editor.Android
{
    public sealed class HapticPermissionGradlePostprocessor : IPostGenerateGradleAndroidProject
    {
        private const string PermissionModuleName = "PHHapticPermission.androidlib";
        private const string NamespaceLine = "    namespace 'com.lafgames.lootup.haptics'";

        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string gradlePath = Path.Combine(path, PermissionModuleName, "build.gradle");
            if (!File.Exists(gradlePath))
            {
                return;
            }

            string gradleText = File.ReadAllText(gradlePath);
            if (gradleText.IndexOf("namespace ", StringComparison.Ordinal) >= 0)
            {
                return;
            }

            const string androidBlock = "android {";
            int androidBlockIndex = gradleText.IndexOf(androidBlock, StringComparison.Ordinal);
            if (androidBlockIndex < 0)
            {
                return;
            }

            // (추가) Unity 6.3의 Android Gradle Plugin 8+가 요구하는 namespace를 생성된 권한 모듈에 보정한다.
            int insertIndex = androidBlockIndex + androidBlock.Length;
            gradleText = gradleText.Insert(insertIndex, $"{Environment.NewLine}{NamespaceLine}");
            File.WriteAllText(gradlePath, gradleText);
        }
    }
}
