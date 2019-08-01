using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GameFramework
{
    public class SoundMgr : SingletonComp<SoundMgr>
    {
        public const string STR_BGM = "bgm";
        public const string STR_SOUND = "sound";

        //TODO:是否播放和音量设置的相关处理
        public bool IsPlayBgm 
        { 
            get { return mPlayBgm; } 
            set 
            { 
                mPlayBgm = value;
                GameConfig.IsPlayBgm = value;
                if(!value)
                {
                    StopBgm();
                }
            } 
        }
        public bool IsPlaySound 
        { 
            get { return mPlaySound; } 
            set 
            { 
                GameConfig.IsPlaySound = value;
                mPlaySound = value; 
            } 
        }
        public float BGMVolume 
        { 
            get { return mBgmVolume; } 
            set 
            { 
                mBgmVolume = value; 
                GameConfig.BGMVolume = value;
                if (mBgmSource.isPlaying)
                {
                    mBgmSource.volume = value * mCurBgmFade;
                }
            } 
        }
        public float SoundVolume 
        { 
            get { return mSoundVolume; } 
            set 
            { 
                mSoundVolume = value; 
                GameConfig.SoundVolume = value;
            } 
        }

        /// <summary>
        /// source对象池
        /// </summary>
        private ObjPool<AudioSource> mSourcePool;
        /// <summary>
        /// 正播放音效的AudioSource列表
        /// </summary>
        private List<AudioSource> mPlayingSources;
        //private GameResHandler<AudioClip> 
        /// <summary>
        /// 播放bgm的AudioSource
        /// </summary>
        private AudioSource mBgmSource;
        /// <summary>
        /// 是否播放BGM
        /// </summary>
        private bool mPlayBgm;
        /// <summary>
        /// 是否播放音乐
        /// </summary>
        private bool mPlaySound;
        /// <summary>
        /// Bgm音量
        /// </summary>
        private float mBgmVolume;
        /// <summary>
        /// 音效音量
        /// </summary>
        private float mSoundVolume;
        /// <summary>
        /// bgm淡入淡出的Tween
        /// </summary>
        private Tween mBgmTween;
        /// <summary>
        /// AudioClip管理
        /// </summary>
        private GameResHandler<AudioClip> mAudios;
        /// <summary>
        /// bgm淡入淡出时的比例
        /// </summary>
        private float mCurBgmFade = 1f;

        #region MonoBehavior
        void Start()
        {
            //从GameConfig初始化音乐相关的配置
            mPlayBgm = GameConfig.IsPlayBgm;
            mPlaySound = GameConfig.IsPlaySound;
            mBgmVolume = GameConfig.BGMVolume;
            mSoundVolume = GameConfig.SoundVolume;

            mAudios = new GameResHandler<AudioClip>("common");
            mAudios.OnReleaseCallback = delegate(ref AudioClip audioClip) 
            {
                audioClip.UnloadAudioData();
            };
            mPlayingSources = new List<AudioSource>();
            mAudios.OnLoadCallbcak = _onAudioClipLoad;
            mSourcePool = new ObjPool<AudioSource>(HandleOnGetDelegate, HandleOnRecoverDelegate, HandleOnDisposeDelegate);

            mBgmSource = gameObject.AddComponent<AudioSource>();
        }

        void LateUpdate()
        {
            if(mPlayingSources.Count > 0)
            {

                for (int i = mPlayingSources.Count - 1; i >= 0; --i)
                {
                    AudioSource audioSource = mPlayingSources[i];
                    if(!audioSource.isPlaying)
                    {
                        mPlayingSources.Remove(audioSource);
                        mSourcePool.Recover(audioSource);
                    }
                }
            }
        }
        #endregion

        #region objPool 处理
        bool HandleOnGetDelegate(ref AudioSource obj)
        {
            if (null == obj)
            {
                GameObject gobj = new GameObject("AudioSource"+Time.time);
                gobj.transform.SetParent(transform);
                //gobj.SetActive(true);
                obj = gobj.AddComponent<AudioSource>();
            }
            obj.playOnAwake = false;
            obj.gameObject.SetActive(true);
            return true;
        }

        bool HandleOnRecoverDelegate(AudioSource obj)
        {
            obj.gameObject.SetActive(false);
            obj.clip = null;
            return true;
        }

        bool HandleOnDisposeDelegate(ref AudioSource obj)
        {
            Destroy(obj.gameObject);
            obj = null;
            return true;
        }
        #endregion

        public void LoadAudios(string asbName, string[] audioNames)
        {
            mAudios.Load(asbName, audioNames);
        }

        public void PlayBgm(string asbName, string audioName, float fadeOutTime = 0f)
        {
            if(!mPlayBgm)
            {
                return;
            }
            AudioClip audioClip = mAudios.Get(asbName, audioName);
            if(null != audioClip)
            {
                _playBgm(audioClip, fadeOutTime);
            }else
            {
                //BGM不添加到队列里面顺序执行
                mAudios.Load(asbName, audioName, STR_BGM);
            }
        }

        public void StopBgm(float fadeOutTime = 0f)
        {
            if (null != mBgmTween && mBgmTween.IsPlaying())
            {
                if(null != mBgmTween && mBgmTween.IsPlaying())
                {
                    mBgmTween.Kill();
                    mBgmTween = null;
                }
                mBgmTween = DOTween.Sequence().Append(DOTween.To(() => mBgmSource.volume, (x) => mBgmSource.volume = x, 0f, fadeOutTime))
                                   .AppendCallback(() => { mBgmSource.Stop(); });
                mBgmTween.Play();
            }

        }

        public void PauseBgm()
        {
            mBgmSource.Pause();
        }

        public void ResumeBgm()
        {
            if(mPlayBgm && null != mBgmSource.clip)
            {
                mBgmSource.Play();
            }
        }

        public void PlaySound(string asbName, string audioName)
        {
            if(!mPlaySound)
            {
                return;
            }
            AudioClip audioClip = mAudios.Get(asbName, audioName);
            if (null != audioClip)
            {
                _playSound(audioClip);
            }
            else
            {
                //音效需要添加到队列里顺序执行
                mAudios.Load(asbName, audioName, STR_SOUND, true);
            }

        }

        public void StopAllSound()
        {
            for (int i = 0; i < mPlayingSources.Count; ++i)
            {
                AudioSource audioSource = mPlayingSources[i];
                audioSource.Stop();
            }
        }

        public void SetCurGroup(string group)
        {
            mAudios.CurGroup = group;
        }

        public void ClearGroup(string group)
        {
            mAudios.ClearGroup(group);
        }

        #region private 
        private bool _playBgm(AudioClip audioClip, float fadeOutTime)
        {
            if(null != audioClip)
            {
                if(audioClip.LoadAudioData())
                {
                    if (null != mBgmTween && mBgmTween.IsPlaying())
                    {
                        mBgmTween.Kill();
                        mBgmTween = null;
                    }
                    if (mBgmSource.isPlaying)
                    {
                        //如果正在播放，先关闭之前播放的
                        mBgmTween = DOTween.Sequence().Append(DOTween.To(() => mCurBgmFade, (x) => {
                            mCurBgmFade = x;
                            mBgmSource.volume = mBgmVolume * mCurBgmFade;
                        }, 0f, fadeOutTime / 2))
                       .AppendCallback(() =>
                       {
                           mBgmSource.Stop();
                           mBgmSource.clip = audioClip;
                           mBgmSource.loop = true;
                           mBgmSource.Play();
                       })
                       .Append(DOTween.To(() => mCurBgmFade, (x) => {
                           mCurBgmFade = x;
                           mBgmSource.volume = mBgmVolume * mCurBgmFade;
                       }, 1f, fadeOutTime / 2))
                       .Play();

                    }
                    else
                    {
                        //如果没有播放，直接播放
                        mBgmSource.clip = audioClip;
                        mBgmSource.volume = 0f;
                        mBgmSource.loop = true;
                        mBgmSource.Play();
                        mBgmTween = DOTween.To(() => mCurBgmFade, (x) =>
                        {
                            mCurBgmFade = x;
                            mBgmSource.volume = mBgmVolume * mCurBgmFade;
                        }, 1f, fadeOutTime / 2)
                        .Play();
                    }
                    return true;
                }
                else
                {
                    LogFile.Warn("AudioClip LoadAudioData 失败，请检查。");
                }
            }
            return false;
        }

        private bool _playSound(AudioClip audioClip)
        {
            AudioSource source = mSourcePool.Get();
            source.clip = audioClip;
            source.Play();
            source.volume = mSoundVolume;
            //lateupdate检测到停止播放则回收
            mPlayingSources.Add(source);
            return true;
        }

        /// <summary>
        /// 当有AudioClip被加载的回调
        /// </summary>
        /// <param name="audioClip">Arg1.</param>
        /// <param name="info">Arg2.</param>
        private void _onAudioClipLoad(AudioClip audioClip, AsbInfo info)
        {
            if(info.extral.Equals(STR_BGM))
            {
                //异步的快速播放，想要淡入淡出可以后期做优化
                _playBgm(audioClip, 0f);
            }else if(info.extral.Equals(STR_SOUND))
            {
                _playSound(audioClip);
            }
        }
        #endregion
    }
}