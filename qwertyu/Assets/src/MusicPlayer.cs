using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MusicPlayer {
  string musicFilePath;
  AudioSource musicPlayerObjectSource;

  // publicMethots
  
  public MusicPlayer(string path) {
    this.musicFilePath = path;

    // Get MusicPlayer Object. 
    GameObject musicPlayerObject = GameObject.FindGameObjectWithTag("MusicPlayerObjectTag");
    this.musicPlayerObjectSource = musicPlayerObject.GetComponent<AudioSource>();

    load_music();
  }
  public void playMusic() {
    check_object_valid_check();
    this.musicPlayerObjectSource.Play();
  }
  public void stopMusic() {
    check_object_valid_check();
    this.musicPlayerObjectSource.Stop();
  }

  // private_methots
  private void load_music() {
    AudioClip audioClip = (AudioClip)Resources.Load(@"scores/bpm_rt/00-04-BPM=RT");
    this.musicPlayerObjectSource.clip = audioClip;
  }
  private void check_object_valid_check() {
    if(musicPlayerObjectSource == null) {
      Debug.LogError("MusicPlayerObject must be initialized.");
    }
    if(musicPlayerObjectSource.clip == null) {
      Debug.LogError("Audio clip in MusicPlayerObject must be initialized.");
    }
  }

}
