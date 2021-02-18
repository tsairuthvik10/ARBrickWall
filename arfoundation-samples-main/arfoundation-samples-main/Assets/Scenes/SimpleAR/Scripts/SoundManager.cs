using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance; // instance for singleton

    public AudioClip[] bgmClips; 
    public AudioClip[] sfxClips;
    public List<AudioSource> current_PlayingSFX;
    public AudioSource current_PlayingBGM;
    public float sfxVolume = 1;
    public float bgmVolume = 0.1f;
    public delegate void SFXStarted(AudioSource snd);
    public static event SFXStarted OnSFXStarted;
    public delegate void SFXFinished(AudioSource snd);
    public static event SFXFinished OnSFXFinished;
    
    float preSFXVolume;
    float preBGMVolume;

    private void Update()
    {
        SetSFXVolume(sfxVolume);
        SetBGMVolume(bgmVolume);
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject); //for singleton
        
        PlayBGM(SoundManager.instance.bgmClips[0]); //start bgm
        SetBGMVolume(bgmVolume); //set volume
        preSFXVolume = sfxVolume;
        preBGMVolume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        this.sfxVolume = volume;
        if (current_PlayingSFX.Count > 0)
        {
            foreach (AudioSource playingSFX in current_PlayingSFX)
                playingSFX.volume = volume;
        } //if there are playing sfx, change their volumes too
    }

    public void playSFXByID(int ID)
    {
        PlaySFX2D(SoundManager.instance.sfxClips[ID]); //play SFX by array index
    }

    public void SetBGMVolume(float volume)
    {
        this.bgmVolume = volume;
        current_PlayingBGM.volume = volume; //set bgm volume
    }

    public void PlayBGM(AudioClip bgmClip, bool looping = true, float pitch = 1.0f, float pan = 0.0f)
    {
        Destroy(current_PlayingBGM);
        AudioSource newBGM = this.gameObject.AddComponent<AudioSource>();
        if (bgmClip != null)
            newBGM.clip = bgmClip;
        newBGM.loop = looping;
        newBGM.pitch = pitch;
        newBGM.panStereo = pan;
        newBGM.Play();
        current_PlayingBGM = newBGM; 
        //TODO: fade in fade out effect
    } //function for playing BGM by parameter

    public void PlayBGM()
    {
        if (current_PlayingBGM != null && !current_PlayingBGM.isPlaying)
            current_PlayingBGM.Play();
    } //function for playing BGM without parameter

    public void PlaySFX2D(AudioClip sfxClip, bool looping = false, float pitch = 1.0f, float pan = 0.0f)
    {
        AudioSource newSFX = this.gameObject.AddComponent<AudioSource>();

        newSFX.clip = sfxClip;
        newSFX.loop = looping;
        newSFX.pitch = pitch;
        newSFX.panStereo = pan;
        newSFX.Play();
        current_PlayingSFX.Add(newSFX);
        if (OnSFXStarted != null)
            OnSFXStarted(newSFX);
        StartCoroutine(destroy_Audiosource(newSFX));
        //add to current play sfx
    } //function for playing SFX in 2D by parameter

    public void PlaySFX3D(Vector3 position, AudioClip sfx)
    {
        GameObject audioObj = new GameObject();
        audioObj.transform.position = position;
        audioObj.AddComponent<AudioSource>();
        audioObj.GetComponent<AudioSource>().clip = sfx;
        audioObj.GetComponent<AudioSource>().loop = false;
        audioObj.GetComponent<AudioSource>().Play();
        current_PlayingSFX.Add(audioObj.GetComponent<AudioSource>());
        if (OnSFXStarted != null)
            OnSFXStarted(audioObj.GetComponent<AudioSource>());
        StartCoroutine(destroy_Audiosource(audioObj.GetComponent<AudioSource>()));
    } //function for playing SFX in 3D

    public void PlaySFX3D(Vector3 position, AudioClip sfx, bool looping = false, float pitch = 1.0f, float pan = 0.0f)
    {
        GameObject audioObj = new GameObject();
        audioObj.transform.position = position;
        audioObj.AddComponent<AudioSource>();
        audioObj.GetComponent<AudioSource>().pitch = pitch;
        audioObj.GetComponent<AudioSource>().panStereo = pan;
        audioObj.GetComponent<AudioSource>().clip = sfx;
        audioObj.GetComponent<AudioSource>().loop = looping;
        audioObj.GetComponent<AudioSource>().Play();
        current_PlayingSFX.Add(audioObj.GetComponent<AudioSource>());
        if (OnSFXStarted != null)
            OnSFXStarted(audioObj.GetComponent<AudioSource>());
        StartCoroutine(destroy_Audiosource(audioObj.GetComponent<AudioSource>()));
    }//function for playing SFX in 3D

    public void PauseBGM()
    {
        if (current_PlayingBGM != null)
            current_PlayingBGM.Pause();
    } //pause the BGM that is currently playing

    public void StopBGM()
    {
        if (current_PlayingBGM != null)
            current_PlayingBGM.Stop();
    } //Stop the BGM

    IEnumerator destroy_Audiosource(AudioSource this_Source)
    {
        if (this_Source.isPlaying)
        {
            yield return new WaitForSeconds(this_Source.clip.length);
        }
        if (OnSFXFinished != null)
            OnSFXFinished(this_Source);
        current_PlayingSFX.Remove(this_Source);
        Destroy(this_Source);
        yield return null;
    } //Destroy the audiosource after it finished.
}

