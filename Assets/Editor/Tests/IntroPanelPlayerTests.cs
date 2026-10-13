using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Video;

namespace IncrementalRPG.Tests
{
    public sealed class IntroPanelPlayerTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private GameObject _root;
        private GameObject _display;
        private IntroPanelPlayer _intro;
        private VideoPlayer _video;
        private AudioSource _audio;
        private int _completions;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("IntroPanelPlayer test");
            _display = new GameObject("Video display");
            _display.transform.SetParent(_root.transform);
            _display.SetActive(false);
            _video = _root.AddComponent<VideoPlayer>();
            _video.playOnAwake = false;
            _audio = _root.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _intro = _root.AddComponent<IntroPanelPlayer>();
            Set("_panel", _root);
            Set("_videoDisplay", _display);
            Set("_videoPlayer", _video);
            Set("_audioSource", _audio);
            Set("_hidePanelOnComplete", true);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_root);
        }

        [Test]
        public void PreparationTimeoutCompletesOnceAndKeepsVideoAndAudioStopped()
        {
            Arm("Preparing");
            Tick(4f);
            Assert.That(_intro.IsPlaying, Is.True);
            ExpectFailure("prepare timed out");
            Tick(1f);
            _intro.Skip();
            Tick(10f);
            AssertCompleted();
        }

        [Test]
        public void MissingFirstFrameTimesOutWithoutRevealingVideo()
        {
            Arm("WaitingForFirstFrame");
            Tick(4f);
            Assert.That(_display.activeSelf, Is.False);
            Assert.That(_audio.isPlaying, Is.False);
            ExpectFailure("first frame timed out");
            Tick(1f);
            AssertCompleted();
        }

        [Test]
        public void FirstDecodedFrameRevealsVideoAndStartsPlaybackWatchdog()
        {
            Arm("WaitingForFirstFrame");
            Tick(4f);
            Invoke("HandleFrameReady", _video, 0L);
            Tick(1f);
            Assert.That(_intro.IsPlaying, Is.True);
            Assert.That(_display.activeSelf, Is.True);
            Assert.That(_completions, Is.Zero);
            Assert.That(Get("_secondsWithoutFrame"), Is.EqualTo(0f));
        }

        [Test]
        public void FrameDuringPreparationCannotStartPresentation()
        {
            Arm("Preparing");
            Invoke("HandleFrameReady", _video, 0L);
            Assert.That(Get("_hasFirstFrame"), Is.False);
            Assert.That(_display.activeSelf, Is.False);
        }

        [TestCase("Preparing")]
        [TestCase("WaitingForFirstFrame")]
        [TestCase("Playing")]
        public void DecoderErrorCompletesOnceInEveryPlaybackPhase(string phase)
        {
            Arm(phase);
            Invoke("HandleVideoError", _video, "Decoder unavailable");
            ExpectFailure("Decoder unavailable");
            Tick(0f);
            Invoke("HandleVideoFinished", _video);
            _intro.Skip();
            AssertCompleted();
        }

        [Test]
        public void NewFramesResetStallTimerButRepeatingTheSameFrameDoesNot()
        {
            Arm("Playing");
            Invoke("HandleFrameReady", _video, 0L);
            Tick(4f);
            Invoke("HandleFrameReady", _video, 1L);
            Tick(4f);
            Assert.That(_intro.IsPlaying, Is.True);
            Invoke("HandleFrameReady", _video, 1L);
            ExpectFailure("playback stalled");
            Tick(1f);
            AssertCompleted();
        }

        [TestCase("Preparing")]
        [TestCase("WaitingForFirstFrame")]
        [TestCase("Playing")]
        public void SkipIgnoresLateDecoderEventsAndCompletesOnce(string phase)
        {
            Arm(phase);
            _intro.Skip();
            Invoke("HandleFrameReady", _video, 10L);
            Invoke("HandleVideoError", _video, "Late error");
            Invoke("HandleVideoFinished", _video);
            Tick(10f);
            AssertCompleted();
            Assert.That(_video.sendFrameReadyEvents, Is.False);
        }

        [Test]
        public void DisablingPlayerCancelsMediaWithoutInvokingSceneTransition()
        {
            Arm("Playing");
            Invoke("OnDisable");
            Invoke("HandleVideoFinished", _video);
            Assert.That(_intro.IsPlaying, Is.False);
            Assert.That(_completions, Is.Zero);
            Assert.That(_video.sendFrameReadyEvents, Is.False);
            Assert.That(_audio.isPlaying, Is.False);
        }

        private void Arm(string phase)
        {
            _completions = 0;
            Set("_isPlaying", true);
            Set("_onComplete", (Action)(() => _completions++));
            var field = typeof(IntroPanelPlayer).GetField("_phase", PrivateInstance);
            field.SetValue(_intro, Enum.Parse(field.FieldType, phase));
            Set("_phaseElapsed", 0f);
            Set("_secondsWithoutFrame", 0f);
            Set("_lastReceivedFrame", -1L);
            _video.sendFrameReadyEvents = true;
        }

        private void Tick(float deltaTime) => Invoke("TickPlayback", deltaTime);

        private void AssertCompleted()
        {
            Assert.That(_intro.IsPlaying, Is.False);
            Assert.That(_completions, Is.EqualTo(1));
            Assert.That(_display.activeSelf, Is.False);
            Assert.That(_audio.isPlaying, Is.False);
            Assert.That(_video.isPlaying, Is.False);
        }

        private static void ExpectFailure(string reason)
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"\[IntroPanelPlayer\].*" + Regex.Escape(reason)));
        }

        private void Set(string name, object value) =>
            typeof(IntroPanelPlayer).GetField(name, PrivateInstance).SetValue(_intro, value);

        private object Get(string name) =>
            typeof(IntroPanelPlayer).GetField(name, PrivateInstance).GetValue(_intro);

        private void Invoke(string name, params object[] arguments) =>
            typeof(IntroPanelPlayer).GetMethod(name, PrivateInstance).Invoke(_intro, arguments);
    }
}
