using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

namespace Syy.Tools
{
    public class AnimatorPreviewSyncParticle : EditorWindow
    {
        [MenuItem("Window/AnimatorPreviewSyncParticle")]
        public static void Open()
        {
            GetWindow<AnimatorPreviewSyncParticle>("AnimatorPreview");
        }

        Animator _animTarget;
        AnimationClip[] _clips = new AnimationClip[0];
        int _clipIndex = -1;
        bool _isPreviewPlaying;

        void OnEnable()
        {
            EditorApplication.update += Update;
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
            if (AnimationMode.InAnimationMode())
            {
                AnimationMode.StopAnimationMode();
                _isPreviewPlaying = false;
            }
        }

        void OnGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                _animTarget = (Animator)EditorGUILayout.ObjectField(_animTarget, typeof(Animator), true);
                if (check.changed)
                {
                    _clipIndex = 0;
                    if (_animTarget != null)
                    {
                        var controller = _animTarget.runtimeAnimatorController as AnimatorController;
                        _clips = controller.animationClips;
                    }
                    else
                    {
                        _clips = new AnimationClip[0];
                    }
                }
            }

            if (_clips.Length != 0)
            {
                _clipIndex = EditorGUILayout.Popup(_clipIndex, _clips.Select(c => c.name).ToArray());
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                _isPreviewPlaying = EditorGUILayout.ToggleLeft("Preview Sync", _isPreviewPlaying);
                if (check.changed)
                {
                    if (_isPreviewPlaying)
                    {
                        AnimationMode.StartAnimationMode();
                    }
                    else
                    {
                        AnimationMode.StopAnimationMode();
                    }
                }
            }
        }

        void Update()
        {
            if (_isPreviewPlaying && AnimationMode.InAnimationMode())
            {
                if (_animTarget != null && _clips.Length != 0)
                {
                    var clip = _clips[_clipIndex];
                    AnimationMode.SampleAnimationClip(_animTarget.gameObject, clip, ParticleSystemEditorHelper.I.PlaybackTime);
                }
            }
        }
    }

    public class ParticleSystemEditorHelper
    {
        private static ParticleSystemEditorHelper _instance;
        public static ParticleSystemEditorHelper I
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ParticleSystemEditorHelper();
                }

                return _instance;
            }
        }

        private static PropertyInfo _playbackTimePI;
        private static Func<float> _playbackTimeGetFunc;

        private ParticleSystemEditorHelper()
        {
            var assembly = Assembly.GetAssembly(typeof(Editor));
            var type = assembly.GetType("UnityEditor.ParticleSystemEditorUtils");
            _playbackTimePI = type.GetProperty("playbackTime", BindingFlags.Static | BindingFlags.NonPublic);
            _playbackTimeGetFunc = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), _playbackTimePI.GetGetMethod(true));
        }

        public float PlaybackTime { get { return _playbackTimeGetFunc(); } set { _playbackTimePI.SetValue(null, value, null); } }
    }
}
