using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 튜토리얼 전용 플레이어 컨트롤러.
/// WASD 이동 + 마우스 방향 회전. 공격은 TutorialBat이 담당.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TutorialPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private Transform _body;          // 회전할 자식 오브젝트
    [SerializeField] private float _rotationSpeed = 15f;

    private Rigidbody2D _rb;
    private Camera _cam;

    private void Awake()
    {
        _rb  = GetComponent<Rigidbody2D>();
        _cam = Camera.main;

        if (_body == null && transform.childCount > 0)
            _body = transform.GetChild(0);
    }

    private void Update()
    {
        RotateTowardsMouse();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        _rb.linearVelocity = new Vector2(h, v).normalized * _moveSpeed;
    }

    private void RotateTowardsMouse()
    {
        if (_body == null || _cam == null || Mouse.current == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld  = _cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -_cam.transform.position.z));

        Vector2 dir = (Vector2)(mouseWorld - transform.position);
        if (dir.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _body.rotation = Quaternion.Slerp(_body.rotation, Quaternion.Euler(0f, 0f, angle), _rotationSpeed * Time.deltaTime);
    }
}
