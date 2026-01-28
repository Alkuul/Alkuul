using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRoot : MonoBehaviour
{
    [SerializeField] private string firstScene = "OrderScene";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.LoadScene(firstScene);
    }
}
