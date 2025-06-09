using UnityEngine;

public class CarController : MonoBehaviour
{
    public float maxSpeed = 50f;  // Максимальная скорость автомобиля
    public float reverseSpeed = 25f;  // Скорость при движении назад
    public float accelerationTime = 2f;  // Время разгона до максимальной скорости
    public float decelerationTime = 0.5f;  // Время торможения до полной остановки
    public float rotSpeed = 5f;  // Скорость поворота автомобиля

    private float currentSpeed = 0f;  // Текущая скорость автомобиля
    private Vector3 desiredMoveDirection = Vector3.zero;  // Направление движения
    private float accelerationRate;  // Темп разгона
    private float decelerationRate;  // Темп торможения

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Рассчитываем темпы ускорения и торможения
        accelerationRate = maxSpeed / accelerationTime;
        decelerationRate = maxSpeed / decelerationTime;
    }

    void Update()
    {
        HandleInput();
        MoveAndRotate();
    }

    // Функция для обработки ввода с клавиатуры
    void HandleInput()
    {
        float inputVertical = SimpleInput.GetAxis("Vertical");   // W/S или стрелки вверх/вниз для управления движением вперед/назад
        float inputHorizontal = SimpleInput.GetAxis("Horizontal");  // A/D или стрелки влево/вправо для поворота

        // Если мы жмем вперед или назад
        if (inputVertical != 0)
        {
            // Если едем вперед, увеличиваем скорость до maxSpeed
            if (inputVertical > 0)
            {
                currentSpeed += accelerationRate * Time.deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, maxSpeed);  // Не превышаем maxSpeed
            }
            // Если едем назад, увеличиваем скорость до reverseSpeed
            else if (inputVertical < 0)
            {
                currentSpeed += accelerationRate * Time.deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, reverseSpeed);  // Ограничиваем заднюю скорость
            }

            // Направление движения вперед/назад
            desiredMoveDirection = transform.forward * inputVertical;
        }
        else
        {
            // Торможение при отсутствии ввода
            currentSpeed -= decelerationRate * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f);  // Не опускаемся ниже 0 скорости
        }

        // Повороты: если машина движется вперед или назад
        if (currentSpeed > 0)
        {
            transform.Rotate(0, inputHorizontal * rotSpeed * Time.deltaTime, 0);  // Поворачиваемся при движении
        }
    }

    // Функция для перемещения автомобиля
    void MoveAndRotate()
    {
        // Если есть направление движения
        if (desiredMoveDirection != Vector3.zero)
        {
            // Двигаем автомобиль вперед/назад
            controller.Move(desiredMoveDirection.normalized * currentSpeed * Time.deltaTime);
        }
    }
}