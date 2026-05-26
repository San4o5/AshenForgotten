using System.Collections;
using UnityEngine;

namespace AshenForgotten.Combat
{
    public static class Hitstop
    {
        private static HitstopRunner _runner;

        public static void Freeze(float duration)
        {
            if (duration <= 0f) return;
            EnsureRunner();
            _runner.Run(duration);
        }

        private static void EnsureRunner()
        {
            if (_runner != null) return;
            var go = new GameObject("[HitstopRunner]");
            Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            _runner = go.AddComponent<HitstopRunner>();
        }

        private class HitstopRunner : MonoBehaviour
        {
            private Coroutine _current;
            private float _savedTimeScale = 1f;

            public void Run(float duration)
            {
                if (_current != null)
                {
                    StopCoroutine(_current);
                    Time.timeScale = _savedTimeScale;
                }
                _current = StartCoroutine(FreezeRoutine(duration));
            }

            private IEnumerator FreezeRoutine(float duration)
            {
                _savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                yield return new WaitForSecondsRealtime(duration);
                Time.timeScale = _savedTimeScale;
                _current = null;
            }
        }
    }
}
