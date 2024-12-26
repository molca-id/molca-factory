using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace Molca
{
    public class LogManager : RuntimeSubsystem
    {
        private static LogManager instance;

        [SerializeField]
        private bool saveToStreamingAssets = true;

        public Action<string> onLogInfo;
        public Action<string> onLogWarning;
        public Action<string> onLogError;

        private static ILogger logger = Debug.unityLogger;

        private LogHandler _logHandler;
        private string _filePath = Application.streamingAssetsPath + "/runtime-log.txt";
        
        internal List<string> _logMessages;


        public override void Initialize(Action<IRuntimeSubsystem> finishCallback)
        {
            instance = this;

            if(!File.Exists(_filePath))
                File.Create(_filePath);

            _logHandler = new LogHandler(this); ;
            _logMessages = new List<string>();
            Activate();

            finishCallback?.Invoke(this);
        }

        private void OnDestroy()
        {
            _logHandler?.Dispose();
            if (saveToStreamingAssets)
                WriteLogToStreamingAssets();
        }

        private void WriteLogToStreamingAssets()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _logMessages.Count; i++)
            {
                sb.Append($"{_logMessages[i]}\r\n");
            }
            using (StreamWriter writer = new StreamWriter(_filePath, false)) // Open in overwrite mode
            {
                writer.WriteLine(sb.ToString());
            }
        }
    }
}