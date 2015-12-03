using UnityEngine;
using Google.Cast.RemoteDisplay;

namespace CompleteProject {
  public class GameOverManager : MonoBehaviour {
    public PlayerHealth playerHealth;
    public GameObject gameOverButtonCanvas;

    void Start() {
      gameOverButtonCanvas.SetActive(false);
    }

    void Update () {
      if(playerHealth.currentHealth <= 0) {
        GetComponent<Animator>().SetTrigger("GameOver");
        if (CastRemoteDisplayManager.GetInstance().IsCasting()) {
          gameOverButtonCanvas.SetActive(true);
          gameOverButtonCanvas.GetComponent<Animator>()
              .SetTrigger("GameOver");
        }
      }
    }
  }
}