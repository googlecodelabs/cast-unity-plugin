/*
 * Copyright 2015 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using System.Collections;
using Google.Cast.RemoteDisplay;

namespace CompleteProject {
  public class UIController : MonoBehaviour {

    public PlayerHealth playerHealth;
    public GameObject pauseButton;
    public GameObject pausePanel;

    private GameObject castUIController;
    private bool gameOver = false;
    private CastRemoteDisplayManager displayManager;

    void Start () {
      displayManager = CastRemoteDisplayManager.GetInstance();
      displayManager.RemoteDisplaySessionStartEvent
        .AddListener(OnRemoteDisplaySessionStart);
      displayManager.RemoteDisplaySessionEndEvent
        .AddListener(OnRemoteDisplaySessionEnd);
      displayManager.RemoteDisplayErrorEvent
        .AddListener(OnRemoteDisplayError);
      castUIController = GameObject.Find("CastDefaultUI");
      pausePanel.SetActive(false);
      pauseButton.SetActive(false);
      if (displayManager.IsCasting()) {
        pauseButton.SetActive(true);
      }
    }

    public void OnRemoteDisplaySessionStart(CastRemoteDisplayManager manager) {
      castUIController.SetActive(false);
      pauseButton.SetActive(true);
    }

    public void OnRemoteDisplaySessionEnd(CastRemoteDisplayManager manager) {
      castUIController.SetActive(true);
      pauseButton.SetActive(false);
    }

    public void OnRemoteDisplayError(CastRemoteDisplayManager manager) {
      castUIController.SetActive(true);
      pauseButton.SetActive(false);
    }

    void Update () {
      if (!gameOver && playerHealth.currentHealth <= 0) {
        gameOver = true;
        EndGame();
      }
    }

    public void StartGame() {
      Time.timeScale = 1f;
      castUIController.SetActive(false);
    }

    public void PauseGame() {
      pauseButton.SetActive(false);
      pausePanel.SetActive(true);
      castUIController.SetActive(true);
      Time.timeScale = 0f;
    }

    public void UnpauseGame() {
      pauseButton.SetActive(true);
      pausePanel.SetActive(false);
      castUIController.SetActive(false);
      Time.timeScale = 1f;
    }

    public void EndGame() {
      pauseButton.SetActive(false);
    }

    public void RestartLevel() {
      castUIController.SetActive(true);
      Application.LoadLevel(Application.loadedLevel);
    }
  }
}