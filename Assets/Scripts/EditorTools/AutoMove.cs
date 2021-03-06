// Move the gameObject at a fixed speed;

using UnityEngine;
using UnityEngine.EventSystems;

public class AutoMove : MonoBehaviour
{
    public Vector3 speed;
    public Vector3 rotateSpeed;
    [Tooltip("speed when holding down the shift key")]
    public Vector3 shiftSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool shiftPressed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
            EventSystem.current.currentSelectedGameObject == null;

        if (shiftPressed) {
            transform.Translate(shiftSpeed * Time.deltaTime);
        } else {
            transform.Translate(speed * Time.deltaTime);
        }
        transform.Rotate(rotateSpeed * Time.deltaTime);
    }
}
