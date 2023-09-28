using UnityEngine;
using UnityEngine.UI;

public class SimpleLoading : MonoBehaviour {

    private RectTransform rectComponent;
    public float rotateSpeed = 200f;

    // Use this for initialization
    void Start () {
        TryGetComponent(out rectComponent);
    }
	
	// Update is called once per frame
	void Update () {
        rectComponent.Rotate(0f, 0f, -(rotateSpeed * Time.deltaTime));
    }
}
