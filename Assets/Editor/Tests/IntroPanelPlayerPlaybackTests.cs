using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Video;

namespace IncrementalRPG.Tests
{
    public sealed class IntroPanelPlayerPlaybackTests
    {
        private GameObject _root;
        private GameObject _display;
        private IntroPanelPlayer _intro;
        private VideoPlayer _video;
        private AudioSource _audio;
        private RenderTexture _texture;
        private bool _originalRunInBackground;
        private int _completions;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return new EnterPlayMode();

            _originalRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
            _root = new GameObject("Intro playback test");
            _root.SetActive(false);
            _display = new GameObject("Video display");
            _display.transform.SetParent(_root.transform);
            _display.SetActive(false);
            _texture = new RenderTexture(1920, 1080, 0);
            _texture.Create();
            _video = _root.AddComponent<VideoPlayer>();
            _video.playOnAwake = false;
            _video.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
            _video.renderMode = VideoRenderMode.RenderTexture;
            _video.targetTexture = _texture;
            _video.clip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Video/Full_version/full_intro.webm");
            _audio = _root.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.volume = 0f;
            _audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Video/Full_version/full_intro.ogg");
            _intro = _root.AddComponent<IntroPanelPlayer>();
            Set("_panel", _root);
            Set("_videoDisplay", _display);
            Set("_videoPlayer", _video);
            Set("_audioSource", _audio);
            Set("_allowInputSkip", false);
            Set("_hidePanelOnComplete", true);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);
            if (_texture != null)
            {
                _texture.Release();
                Object.DestroyImmediate(_texture);
            }
            Application.runInBackground = _originalRunInBackground;
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator ImportedVp8IntroProducesFramesCanSkipReplayAndReachEnd()
        {
            Assert.That(_video.clip, Is.Not.Null);
            Assert.That(_audio.clip, Is.Not.Null);
            _intro.Play(() => _completions++);
            Assert.That(_display.activeSelf, Is.False);
            Assert.That(_audio.isPlaying, Is.False);
            yield return WaitForFrames();
            Assert.That(_audio.isPlaying, Is.True);

            _intro.Skip();
            Assert.That(_completions, Is.EqualTo(1));
            Assert.That(_intro.IsPlaying, Is.False);
            Assert.That(_video.isPlaying, Is.False);
            Assert.That(_audio.isPlaying, Is.False);
            Assert.That(_display.activeSelf, Is.False);

            _intro.Play(() => _completions++);
            yield return WaitForFrames();
            Assert.That(_audio.isPlaying, Is.True);
            var deadline = Time.realtimeSinceStartupAsDouble + _video.clip.length + 5d;
            while (_intro.IsPlaying && Time.realtimeSinceStartupAsDouble < deadline)
                yield return null;

            Assert.That(_intro.IsPlaying, Is.False, "The intro must finish without user input.");
            Assert.That(_completions, Is.EqualTo(2));
            Assert.That(_video.isPlaying, Is.False);
            Assert.That(_audio.isPlaying, Is.False);
            Assert.That(_display.activeSelf, Is.False);
        }

        private IEnumerator WaitForFrames()
        {
            var deadline = Time.realtimeSinceStartupAsDouble + 10d;
            while (_intro.IsPlaying && (!_display.activeSelf || _video.frame < 15) &&
                   Time.realtimeSinceStartupAsDouble < deadline)
                yield return null;

            Assert.That(_intro.IsPlaying, Is.True, "The decoder must not fail before displaying frames.");
            Assert.That(_display.activeSelf, Is.True);
            Assert.That(_video.frame, Is.GreaterThanOrEqualTo(15));
        }

        private void Set(string name, object value) =>
            typeof(IntroPanelPlayer).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_intro, value);
    }
}
