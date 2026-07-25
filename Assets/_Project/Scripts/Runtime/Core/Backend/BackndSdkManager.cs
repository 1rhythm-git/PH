using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using BackndApi = BackEnd.Backend;

namespace LootUp.Core.Backend
{
    public static class BackndSdkManager
    {
        private const string RuntimeObjectName = "[BackndSdkRuntime]";

        private static BackndSdkRuntime runtime;
        private static TaskCompletionSource<BackndInitializationResult>
            initializationSource;

        public static event Action<BackndInitializationState> StateChanged;

        public static BackndInitializationState State { get; private set; } =
            BackndInitializationState.NotStarted;

        public static Task<BackndInitializationResult> InitializeAsync()
        {
            EnsureRuntime();
            return runtime.InitializeAsync();
        }

        internal static void PostToMainThread(Action action)
        {
            if (action == null)
            {
                return;
            }

            EnsureRuntime();
            runtime.Enqueue(action);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            runtime = null;
            initializationSource = null;
            State = BackndInitializationState.NotStarted;
            StateChanged = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            _ = InitializeAsync();
        }

        private static void EnsureRuntime()
        {
            if (runtime != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeObjectName);
            runtime = runtimeObject.AddComponent<BackndSdkRuntime>();
            UnityEngine.Object.DontDestroyOnLoad(runtimeObject);
        }

        private static void SetState(BackndInitializationState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }

        private sealed class BackndSdkRuntime : MonoBehaviour
        {
            private readonly ConcurrentQueue<Action> mainThreadActions = new();

            public void Enqueue(Action action)
            {
                if (action != null)
                {
                    mainThreadActions.Enqueue(action);
                }
            }

            public Task<BackndInitializationResult> InitializeAsync()
            {
                if (State == BackndInitializationState.Initialized)
                {
                    return Task.FromResult(
                        BackndInitializationResult.Success());
                }

                if (initializationSource != null)
                {
                    return initializationSource.Task;
                }

                initializationSource =
                    new TaskCompletionSource<BackndInitializationResult>();
                SetState(BackndInitializationState.Initializing);

                try
                {
                    BackndApi.InitializeAsync(backendReturnObject =>
                    {
                        if (backendReturnObject.IsSuccess())
                        {
                            mainThreadActions.Enqueue(() =>
                            {
                                SetState(BackndInitializationState.Initialized);
                                Debug.Log("[BackND] SDK 초기화 성공");
                                initializationSource.TrySetResult(
                                    BackndInitializationResult.Success());
                            });
                            return;
                        }

                        string message = backendReturnObject.ToString();
                        mainThreadActions.Enqueue(() =>
                        {
                            SetState(BackndInitializationState.Failed);
                            Debug.LogError(
                                $"[BackND] SDK 초기화 실패: {message}");
                            initializationSource.TrySetResult(
                                BackndInitializationResult.Fail(message));
                        });
                    });
                }
                catch (Exception exception)
                {
                    SetState(BackndInitializationState.Failed);
                    Debug.LogException(exception);
                    initializationSource.TrySetResult(
                        BackndInitializationResult.Fail(exception.Message));
                }

                return initializationSource.Task;
            }

            private void Update()
            {
                while (mainThreadActions.TryDequeue(out Action action))
                {
                    action.Invoke();
                }
            }
        }
    }
}
