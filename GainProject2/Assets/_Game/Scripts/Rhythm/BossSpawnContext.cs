using System;

namespace _Game.Scripts.Rhythm
{
    public static class BossSpawnContext
    {
        public static bool IsStageManagerSpawning { get; private set; }
        public static string LastReason { get; private set; }

        public static IDisposable Enter(string reason)
        {
            IsStageManagerSpawning = true;
            LastReason = reason;
            return new Scope();
        }

        private sealed class Scope : IDisposable
        {
            public void Dispose()
            {
                IsStageManagerSpawning = false;
                LastReason = null;
            }
        }
    }
}