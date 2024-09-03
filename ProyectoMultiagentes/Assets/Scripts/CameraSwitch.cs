using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;

    void Start()
    {
        // Asegurarse de que la cámara 1 esté activa al inicio y la cámara 2 esté desactivada
        camera1.gameObject.SetActive(true);
        camera2.gameObject.SetActive(false);
    }

    void Update()
    {
        // Cambiar a la cámara 2 mientras se mantenga presionada la tecla C
        if (Input.GetKey(KeyCode.C))
        {
            camera1.gameObject.SetActive(false);
            camera2.gameObject.SetActive(true);
        }
        else
        {
            // Regresar a la cámara 1 cuando se suelta la tecla C
            camera1.gameObject.SetActive(true);
            camera2.gameObject.SetActive(false);
        }
    }
}
