using BepInEx.Logging;
using DiscordRPC.Logging;

using LogLevel = DiscordRPC.Logging.LogLevel;

namespace H3MP
{
    // This interface implementation is a great example of the need for flaggable enums (which LogLevel isn't):
    internal class DiscordLog : ILogger
    {
        private const string PREFIX = "DiscordRichPresence: ";

        private readonly ManualLogSource _log;

        public LogLevel Level { get; set; }

        public DiscordLog(ManualLogSource log)
        {
            _log = log;
        }

        public void Error(string message, params object[] args)
        {
            if (Level == LogLevel.None)
            {
                return;
            }

            _log.LogError(PREFIX + string.Format(message, args));
        }

        public void Info(string message, params object[] args)
        {
            if (Level != LogLevel.Trace && Level != LogLevel.Info)
            {
                return;
            }

            _log.LogInfo(PREFIX + string.Format(message, args));
        }

        public void Trace(string message, params object[] args)
        {
            if (Level != LogLevel.Trace)
            {
                return;
            }

            _log.LogInfo(PREFIX + string.Format(message, args));
        }

        public void Warning(string message, params object[] args)
        {
            if (Level == LogLevel.Error || Level == LogLevel.None)
            {
                return;
            }

            _log.LogInfo(PREFIX + string.Format(message, args));
        }
    }
}