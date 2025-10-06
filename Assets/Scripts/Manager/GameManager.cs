using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game References")]
    public CameraController cameraController;
    public PlaneController planeController;
    
    [Header("Win condition")]
    [SerializeField]
    private List<GameObject> collectors;
    
    [SerializeField]
    private GameObject endMenuCanvas;
    
    [SerializeField]
    private GameObject lightHouse;
    private MoveLightHouse _moveLightHouse;
    
    [Header("Debug")]
    public List<GameObject> collected;
    public bool isGameFinished = false;
    public bool canFinishGame = false;

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

    void Start()
    {
        _moveLightHouse = lightHouse.GetComponent<MoveLightHouse>();
    }

    void Update()
    {
        if (canFinishGame) return;
        if (collectors.Count == collected.Count)
        {
            Debug.Log("All collected!");
            canFinishGame = true;
            _moveLightHouse.MoveLightHouseToYZero();
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
        // yield return new WaitForSeconds(2f);
        // endMenuCanvas.GetComponent<CreditsScroller>().StartScrolling();
    }
    

    public void RespawnPlane()
    {
        planeController.RespawnPlaneOnExit();
    }
    
}
