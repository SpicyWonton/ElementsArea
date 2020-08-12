using GrpcLibrary;
using UnityEngine;
using UnityEngine.EventSystems;

public class ThrowJoystick : MonoBehaviour, IPointerUpHandler
{
    public Transform playerTrans;
    public bool banThrow = false;

    private Vector3 startPoint, endPoint;               // 投掷时的显示的直线的两点
    private LineRenderer lineRenderer;                  // 投掷时的显示的直线
    private float moveX, moveZ;                         // 决定投掷方向
    private Vector3 dir;                                // 直线方向
    private Vector3 atkDir;                             // 攻击方向

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        moveX = GetComponent<Joystick>().Horizontal;
        moveZ = GetComponent<Joystick>().Vertical;

        if (moveX == 0 && moveZ == 0)
            return;

        // 只有当玩家有方块时
        if (playerTrans.GetComponent<PlayerControl>().haveWeapon)
        {
            // 直线起点的位置就是玩家的位置
            startPoint = playerTrans.position;

            // 根据拖拽的方向决定直线的方向
            dir = new Vector3(moveX, 0, moveZ).normalized;
            endPoint = startPoint + 5 * dir;

            DrawLine();
        }
    }

    // 画直线
    private void DrawLine()
    {
        lineRenderer.enabled = true;
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    // 停止画线
    private void StopDrawLine()
    {
        lineRenderer.enabled = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopDrawLine();
        // 调用玩家类的攻击函数
        atkDir = (dir + new Vector3(0, 0.1f, 0)).normalized;
        playerTrans.GetComponent<PlayerControl>().AttackAction(atkDir);
        // 发送给服务器，进行同步
        if (!User.isSingle)
            SendAttackDir();
    }

    // 松开摇杆时调用，给服务器发送消息
    private void SendAttackDir()
    {
        Vector attackDir = new Vector
        {
            X = atkDir.x,
            Y = atkDir.y,
            Z = atkDir.z
        };
        Client.SendMsg((uint)MSGID.Atk, attackDir);
    }
}