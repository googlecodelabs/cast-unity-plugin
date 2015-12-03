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
using UnityEngine.UI;

namespace Google.Cast.RemoteDisplay.UI {
  /**
   * Contains the functionality for the cast button and corresponding frame.
   */
  public class CastButtonFrame : MonoBehaviour {

    /**
     * Start/stop casting button.
     */
    public Button castButton;

    /**
     * The callback for tapping the cast button.
     */
    public UICallback castButtonTappedCallback;

    /**
     * Tracks whether the frame is currently casting.
     */
    private bool isCasting = false;

    /**
     * A private copy of the UI sprites, so they can be used locally.
     */
    public CastUISprites UiSprites {
      set {
        uiSprites = value;
      }
    }
    private CastUISprites uiSprites;

    /**
     * A private copy of the UI cast connecting animation, so they can be used locally.
     */
    public Animator ConnectingAnimator {
      set {
        connectingAnimator = value;
      }
    }
    private Animator connectingAnimator;

    /**
     * Shows the "casting" state for the cast button.
     */
    public void ShowCasting() {
      isCasting = true;
      connectingAnimator.enabled = false;
      castButton.image.sprite = uiSprites.casting;
      Show();
    }

    /**
     * Shows the "connecting" animation for the cast button.
     */
    public void ShowConnecting() {
      // The state is already casting - do nothing.
      if (isCasting) {
        return;
      }
      Show();
      connectingAnimator.enabled = true;
      connectingAnimator.Play("CastButtonConnecting");
    }

    /**
     * Shows the "not casting" state for the cast button.
     */
    public void ShowNotCasting() {
      isCasting = false;
      connectingAnimator.enabled = false;
      castButton.image.sprite = uiSprites.notCasting;
      Show();
    }

    /**
     * Shows the cast button.
     */
    public void Show() {
      connectingAnimator.enabled = false;
      gameObject.SetActive(true);
    }

    /**
     * Hides the cast button.
     */
    public void Hide() {
      connectingAnimator.enabled = false;
      gameObject.SetActive(false);
    }

    /**
     * Triggers the callback for tapping the cast button.  Set as the OnClick function for
     * CastButton.
     */
    public void OnCastButtonTapped() {
      castButtonTappedCallback();
    }
  }
}
