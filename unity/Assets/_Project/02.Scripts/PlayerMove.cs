using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 100f; // 이동 속도

    void Update()
    {
        // 1. 키보드 입력 받기 (수평, 수직)
        float h = Input.GetAxis("Horizontal"); // A, D / 왼쪽, 오른쪽 화살표
        float v = Input.GetAxis("Vertical");   // W, S / 위, 아래 화살표
        
        // 2. 이동 방향 계산
        Vector3 dir = new Vector3(h, 0, v);

        // 3. 실제 위치 이동 (공식: 방향 * 속도 * 시간)
        transform.position += dir * moveSpeed * Time.deltaTime;
    }
}
