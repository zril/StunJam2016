using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Cancel"))
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }

        if (Input.GetButtonDown("Start"))
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }
}
