using UnityEngine;

public class GameplayZone : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Le joueur a quitté la zone de jeu !");
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.canFinishGame)
            {
                GameManager.Instance.EndGame();
            }
            else
            {
                GameManager.Instance.RespawnPlane();
            }
        }
    }
}
