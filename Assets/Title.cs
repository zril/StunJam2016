using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        GameObject source = GameObject.FindGameObjectWithTag("AudioSource");
        DontDestroyOnLoad(source);

        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }

        if (Input.GetButtonDown("Start"))
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }
}
