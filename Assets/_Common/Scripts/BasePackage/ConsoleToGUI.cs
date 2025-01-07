using UnityEngine;

namespace DebugStuff
{
    public class ConsoleToGUI : MonoBehaviour
    {
        [SerializeField] private bool showInEditor = false;

        static string consoleContent = "";

        void OnEnable()
        {
            Application.logMessageReceived += Log;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= Log;
        }

        public void Log(string logString, string stackTrace, LogType type)
        {
            consoleContent = logString + "\n" + consoleContent;
            consoleContent = stackTrace + "\n" + consoleContent;

            if (consoleContent.Length > 5000)
                consoleContent = consoleContent[..4000];
        }

        void OnGUI()
        {
            if (showInEditor || !Application.isEditor)
                consoleContent = GUI.TextArea(new Rect(10, Screen.height * 0.75f, Screen.width - 10, Screen.height * 0.25f - 10), consoleContent);
        }
    }
}