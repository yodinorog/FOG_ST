using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class DynamicRenderTexture : MonoBehaviour
{
    public RawImage rawImage;  // UI элемент RawImage, который показывает Render Texture
    private RenderTexture renderTexture;  // Render Texture для камеры
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        CreateRenderTexture(Screen.width, Screen.height);
    }

    void Update()
    {
        // Проверка на изменение размеров экрана
        if (renderTexture.width != Screen.width || renderTexture.height != Screen.height)
        {
            CreateRenderTexture(Screen.width, Screen.height);
        }
    }

    private void CreateRenderTexture(int width, int height)
    {
        // Создаем новый Render Texture, если текущий не подходит
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        renderTexture = new RenderTexture(width, height, 24);
        cam.targetTexture = renderTexture;


        //var rawImage = GetComponentInChildren<UnityEngine.UI.RawImage>();
        // Назначаем Render Texture для отображения в UI
        if (rawImage != null)
        {
            rawImage.texture = renderTexture;
        }
    }

    void OnPreRender()
    {
        // Обновляем Render Texture, если размер экрана изменился
        if (renderTexture.width != Screen.width || renderTexture.height != Screen.height)
        {
            CreateRenderTexture(Screen.width, Screen.height);
        }
    }
}