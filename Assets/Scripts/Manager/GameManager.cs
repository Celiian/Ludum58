using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game References")]
    public CameraController cameraController;
    public PlaneController planeController;
    
    public bool isGameFinished = false;
    public bool canFinishGame = false;
    
    [Header("Win condition")]
    [SerializeField]
    private List<GameObject> collectors;
    public List<GameObject> collected;
    
    [SerializeField]
    private GameObject endMenuCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (canFinishGame) return;
        if (collectors.Count == collected.Count)
        {
            Debug.Log("All collected!");
            canFinishGame = true;
        }
    }

    public void EndGame()
    {
        Debug.Log("Game finished!");
        isGameFinished = true;
        cameraController.SwitchEndGameCamera();
        StartCoroutine(EndScreen(5f));
    }

    private IEnumerator EndScreen(float duration)
    {
        yield return planeController.FadeToBlack(duration);
        endMenuCanvas.SetActive(true);
    }
    

    public void RespawnPlane()
    {
        planeController.RespawnPlaneOnExit();
    }
    
}
