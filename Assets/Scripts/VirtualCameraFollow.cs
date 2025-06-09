using UnityEngine;
using Cinemachine;

public class VirtualCameraWinningCutscene : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera; // Ссылка на виртуальную камеру
    private Transform playerTransform; // Ссылка на трансформ игрока
    public GameObject winningCutscene; // Объект WinningCutscene
    public float zoomSpeed = 5f; // Скорость отдаления камеры
    public float maxHeight = 100f; // Максимальная высота камеры для остановки

    private bool isZooming = false;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        if (virtualCamera == null)
        {
            Debug.LogError("CinemachineVirtualCamera не найдена на объекте!");
        }
    }

    private void Start()
    {
        FindPlayer();

        
    }

    private void Update()
    {
        if (winningCutscene != null && winningCutscene.activeSelf)
        {
            ActivateWinningCutsceneMode();
        }
        else if (isZooming)
        {
            PerformZoomOut();
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Игрок не найден в сцене! Убедитесь, что объект игрока имеет правильный тег.");
        }
    }

    private void ActivateWinningCutsceneMode()
    {
        if (playerTransform != null)
        {
            virtualCamera.Follow = playerTransform; // Устанавливаем Follow на игрока по умолчанию
            virtualCamera.LookAt = playerTransform; // Устанавливаем LookAt на игрока
            virtualCamera.Follow = null; // Убираем Follow
            isZooming = true; // Начинаем процесс отдаления
        }
    }

    private void PerformZoomOut()
    {
        // Проверяем текущую высоту камеры
        Vector3 currentPosition = virtualCamera.transform.position;

        if (currentPosition.y < maxHeight)
        {
            // Постепенно поднимаем камеру вверх
            virtualCamera.transform.position += Vector3.up * zoomSpeed * Time.deltaTime;
        }
        else
        {
            // Останавливаемся, когда достигли максимальной высоты
            isZooming = false;
            virtualCamera.Follow = null; // Отключаем Follow после завершения
        }
    }
}