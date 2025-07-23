using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Joystick joystick;
    public CharacterController controller;
    public Animator anim;

    public float speed;
    public float gravity;

    Vector3 moveDirection;
    bool usingJoystick = false; // 标记当前是否使用遥感控制
    bool usingKeyboard = false;
    void Update()
    {
        // 检查是否有遥感输入，若有则使用遥感控制
        if (joystick != null && joystick.direction != Vector2.zero)
        {
            usingJoystick = true;
            usingKeyboard = false;
        }
        // 检查是否有键盘输入，若有则使用WSAD控制
        else if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            usingJoystick = false;
            usingKeyboard = true;
        }

        // 根据当前的控制方式进行移动
        if (usingJoystick)
        {
            JoystickControl();
        }
        if(usingKeyboard)
        {
            KeyboardControl();
        }
    }

    // 遥感控制方法
    void JoystickControl()
    {
        Vector2 direction = joystick.direction;

        if (controller.isGrounded)
            moveDirection = new Vector3(direction.x, 0, direction.y);

        moveDirection.y += gravity * Time.deltaTime;
        controller.Move(moveDirection * speed * Time.deltaTime);

        // 旋转玩家朝向
        Vector3 rotDirection = new Vector3(direction.x, 0, direction.y);

        Quaternion targetRotation = rotDirection != Vector3.zero ? Quaternion.LookRotation(rotDirection) : transform.rotation;
        transform.rotation = targetRotation;

        // 动画控制
        if (direction != Vector2.zero)
            anim.SetBool("Run", true);
        else
            anim.SetBool("Run", false);
    }

    // WSAD控制方法
    void KeyboardControl()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D键或者左/右箭头
        float vertical = Input.GetAxis("Vertical");     // W/S键或者上/下箭头

        if (controller.isGrounded)
            moveDirection = new Vector3(horizontal, 0, vertical);

        moveDirection.y += gravity * Time.deltaTime;
        controller.Move(moveDirection * speed * Time.deltaTime);

        // 旋转玩家朝向
        Vector3 rotDirection = new Vector3(horizontal, 0, vertical);

        Quaternion targetRotation = rotDirection != Vector3.zero ? Quaternion.LookRotation(rotDirection) : transform.rotation;
        transform.rotation = targetRotation;

        // 动画控制
        if (rotDirection != Vector3.zero)
            anim.SetBool("Run", true);
        else
            anim.SetBool("Run", false);
    }

    public void SidePos()
    {
        controller.Move(Vector3.right * 2.5f);
    }
}