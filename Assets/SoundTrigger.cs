using UnityEngine;
using System.Collections;

public class SoundTrigger : MonoBehaviour {

    public AudioClip sound;
    public bool playOnce = true;
    private bool played = false;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public bool Play()
    {
        bool play = !played;
        if (playOnce)
        {
            played = true;
        }
        return play;
    }
}
