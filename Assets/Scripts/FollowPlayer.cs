using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;

    
    private float x;
    private float y;
    public float sensitivity = -1f;
    private Vector3 rotate;
    // Update is called once per frame
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        //Debug.Log(player.position); 
        transform.position = player.position + offset;
        y = Input.GetAxis("Mouse X");
        x = Input.GetAxis("Mouse Y");
        rotate = new Vector3(x, y * sensitivity, 0);
        transform.eulerAngles = transform.eulerAngles - rotate;
    }
}