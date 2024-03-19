using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public float sensitivity = 2.0f;

    private Rigidbody rb;

    private float moveFB;
    private float moveLR;
    private float rotX;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        // Movimento avanti/indietro e sinistra/destra
        moveFB = Input.GetAxis("Vertical") * speed;
        moveLR = Input.GetAxis("Horizontal") * speed;

        // Rotazione del player con il mouse
        rotX = Input.GetAxis("Mouse X") * sensitivity;
        Vector3 movement = new Vector3(moveLR, 0, moveFB);
        transform.Rotate(0, rotX, 0);

        // Applica il movimento
        Vector3 velocity = transform.TransformDirection(movement) * Time.deltaTime;
        rb.MovePosition(rb.position + velocity);
    }
}
