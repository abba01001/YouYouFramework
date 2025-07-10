using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Joystick joystick;
    public CharacterController controller;
    public Animator anim;

    public float speed;
    public float gravity;

    Vector3 moveDirection;

    void Update()
    {
        Vector2 direction = joystick.direction;

        if (controller.isGrounded)
            moveDirection = new Vector3(direction.x, 0, direction.y);

        moveDirection.y += gravity * Time.deltaTime;
        controller.Move(moveDirection * speed * Time.deltaTime);

        Vector3 rotDirection = new Vector3(direction.x, 0, direction.y);

        Quaternion targetRotation = rotDirection != Vector3.zero ? Quaternion.LookRotation(rotDirection) : transform.rotation;
        transform.rotation = targetRotation;

        if (direction != Vector2.zero)
            anim.SetBool("Run", true);
        else
            anim.SetBool("Run", false);
    }

    public void SidePos()
    {
        controller.Move(Vector3.right * 2.5f);
    }
}
