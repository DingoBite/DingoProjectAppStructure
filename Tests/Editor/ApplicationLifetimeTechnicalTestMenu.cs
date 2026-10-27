#if UNITY_EDITOR && UNITY_INCLUDE_TESTS

using System;
using System.IO;
using DingoProjectAppStructure.Tests.PlayMode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using Object = UnityEngine.Object;
using TestMode = UnityEditor.TestTools.TestRunner.Api.TestMode;

namespace DingoProjectAppStructure.Tests.Editor
{
    [InitializeOnLoad]
    public static class ApplicationLifetimeTechnicalTestMenu
    {
        public const string MENU = "Dingo SDK/Technical Tests/Run Application Lifetime";
        public const string ACTIVE_KEY = "DingoProjectAppStructure.ApplicationLifetimeTests.Active";
        public const string RESULT_DIRECTORY = "Temp/DingoProjectAppStructure";
        public const string RESULT_PATH = RESULT_DIRECTORY + "/ApplicationLifetime.xml";
        private static ApplicationLifetimeTechnicalTestCallbacks _callbacks;

        static ApplicationLifetimeTechnicalTestMenu()
        {
            if (SessionState.GetBool(ACTIVE_KEY, false))
            {
                _callbacks = new ApplicationLifetimeTechnicalTestCallbacks();
                TestRunnerApi.RegisterTestCallback(_callbacks);
            }
        }

        [MenuItem(MENU, true)]
        public static bool CanRun() => !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && !SessionState.GetBool(ACTIVE_KEY, false);

        [MenuItem(MENU)]
        public static void Run()
        {
            if (!CanRun() || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            try
            {
                _callbacks = new ApplicationLifetimeTechnicalTestCallbacks();
                ApplicationLifetimeTechnicalTestOwner.instance.Attach(runner);
                SessionState.SetBool(ACTIVE_KEY, true);
                TestRunnerApi.RegisterTestCallback(_callbacks);
                if (File.Exists(RESULT_PATH))
                    File.Delete(RESULT_PATH);
                // The fixture enters and exits Play itself; the Editor runner restores its scene setup.
                runner.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, testNames = new[] { typeof(GAsyncLifetimePlayModeTests).FullName } }) { runSynchronously = false });
            }
            catch (Exception error)
            {
                Release(_callbacks);
                Debug.LogException(error);
            }
        }

        public static bool Owns(ApplicationLifetimeTechnicalTestCallbacks callbacks) => ReferenceEquals(_callbacks, callbacks);

        public static void Release(ApplicationLifetimeTechnicalTestCallbacks callbacks)
        {
            if (callbacks != null)
                TestRunnerApi.UnregisterTestCallback(callbacks);
            if (!Owns(callbacks))
                return;
            _callbacks = null;
            SessionState.SetBool(ACTIVE_KEY, false);
            ApplicationLifetimeTechnicalTestOwner.instance.Release();
        }
    }

    // Keep the exact runner across the fixture's Play transitions without writing an Editor asset.
    public class ApplicationLifetimeTechnicalTestOwner : ScriptableSingleton<ApplicationLifetimeTechnicalTestOwner>
    {
        [SerializeField] private TestRunnerApi _runner;

        public void Attach(TestRunnerApi runner) => _runner = runner;

        public void Release()
        {
            var runner = _runner;
            _runner = null;
            if (runner != null)
                Object.DestroyImmediate(runner);
        }
    }

    public class ApplicationLifetimeTechnicalTestCallbacks : IErrorCallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun) { }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }

        public void RunFinished(ITestResultAdaptor result)
        {
            if (!ApplicationLifetimeTechnicalTestMenu.Owns(this))
                return;
            try
            {
                Directory.CreateDirectory(ApplicationLifetimeTechnicalTestMenu.RESULT_DIRECTORY);
                TestRunnerApi.SaveResultToFile(result, ApplicationLifetimeTechnicalTestMenu.RESULT_PATH);
                var summary = $"Application lifetime: {result.PassCount} passed, {result.FailCount} failed, {result.SkipCount} skipped, {result.InconclusiveCount} inconclusive. Results: {ApplicationLifetimeTechnicalTestMenu.RESULT_PATH}";
                if (result.FailCount > 0 || result.InconclusiveCount > 0 || result.PassCount == 0)
                    Debug.LogWarning(summary);
                else
                    Debug.Log(summary);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
            }
            finally
            {
                ApplicationLifetimeTechnicalTestMenu.Release(this);
            }
        }

        public void OnError(string message)
        {
            if (!ApplicationLifetimeTechnicalTestMenu.Owns(this))
                return;
            Debug.LogError("Application lifetime tests could not complete: " + message);
            ApplicationLifetimeTechnicalTestMenu.Release(this);
        }
    }
}

#endif
