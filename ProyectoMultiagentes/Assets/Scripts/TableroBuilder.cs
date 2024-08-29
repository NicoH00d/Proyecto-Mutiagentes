using System.IO;
using UnityEngine;

public class TableroBuilder : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject pointInterestPrefab;
    public GameObject fireMarkerPrefab;  // Prefab para marcadores de fuego

    public string fileName = "tablero"; 
    private string[] lines;

    // Variables para guardar la última posición calculada
    private Vector3 lastWallPosition;
    private Vector3 lastDoorPosition;
    private Vector3 lastPointInterestPosition;
    private Vector3 lastFireMarkerPosition;

    void Start()
    {
        CargarArchivo();
        CrearTablero();
        CrearPuntosDeInteres();
        CrearMarcadoresDeFuego();
    }

    void CargarArchivo()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);
        if (textAsset != null)
        {
            lines = textAsset.text.Split('\n');
        }
        else
        {
            Debug.LogError("Archivo no encontrado en Resources: " + fileName);
        }
    }

    void CrearTablero()
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("No se pudo cargar el archivo o está vacío.");
            return;
        }

        // Leer las celdas y paredes
        for (int i = 0; i < 6; i++)
        {
            string[] cells = lines[i].Split(' ');
            for (int j = 0; j < 8; j++)
            {
                Vector3 cellPosition = new Vector3(j, 0, -i); // Usamos XZ, Y=0
                GameObject cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                string walls = cells[j];
                CrearParedes(cellPosition, walls);
            }
        }

        // Leer y crear puertas
        for (int i = 19; i < 27; i++)
        {
            string[] doorData = lines[i].Split(' ');
            int r1 = int.Parse(doorData[0]) - 1;
            int c1 = int.Parse(doorData[1]) - 1;
            int r2 = int.Parse(doorData[2]) - 1;
            int c2 = int.Parse(doorData[3]) - 1;
            CrearPuerta(new Vector3(c1, 0, -r1), new Vector3(c2, 0, -r2));
        }
    }

    void CrearParedes(Vector3 cellPosition, string walls)
    {
        if (walls[0] == '1') // Pared atrás (en la posición Z-)
        {
            Vector3 wallPos = cellPosition + new Vector3(0, 0.3f, 0.5f); 
            Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0)); 
            lastWallPosition = wallPos; 
        }
        if (walls[1] == '1') // Pared derecha (en la posición X+)
        {
            Vector3 wallPos = cellPosition + new Vector3(0.5f, 0.3f, 0); 
            Instantiate(wallPrefab, wallPos, Quaternion.identity); 
            lastWallPosition = wallPos; 
        }
        if (walls[2] == '1') // Pared adelante (en la posición Z+)
        {
            Vector3 wallPos = cellPosition + new Vector3(0, 0.3f, -0.5f); 
            Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0)); 
            lastWallPosition = wallPos; 
        }
        if (walls[3] == '1') // Pared izquierda (en la posición X-)
        {
            Vector3 wallPos = cellPosition + new Vector3(-0.5f, 0.3f, 0); 
            Instantiate(wallPrefab, wallPos, Quaternion.identity); 
            lastWallPosition = wallPos; 
        }
    }

    void CrearPuerta(Vector3 pos1, Vector3 pos2)
    {
        Vector3 doorPosition = (pos1 + pos2) / 2;
        doorPosition.y = 0.20f;
        Instantiate(doorPrefab, doorPosition, Quaternion.identity);
        lastDoorPosition = doorPosition; 
    }

    void CrearPuntosDeInteres()
    {
        for (int i = 6; i < 9; i++)
        {
            string[] pointData = lines[i].Split(' ');
            int r = int.Parse(pointData[0]) - 1;
            int c = int.Parse(pointData[1]) - 1;
            char type = pointData[2][0]; 
            Vector3 pointPosition = new Vector3(c, 0.5f, -r);
            GameObject pointInterest = Instantiate(pointInterestPrefab, pointPosition, Quaternion.Euler(-90, 0 , -105));

            if (type == 'v')
            {
                pointInterest.GetComponent<Renderer>().material.color = Color.red;
            }
            else if (type == 'f')
            {
                pointInterest.GetComponent<Renderer>().material.color = Color.yellow;
            }

            lastPointInterestPosition = pointPosition; 
        }
    }

    void CrearMarcadoresDeFuego()
    {
        for (int i = 9; i < 19; i++) // Marcadores de fuego están en las líneas 10 a 19
        {
            string[] fireData = lines[i].Split(' ');
            int r = int.Parse(fireData[0]) - 1;
            int c = int.Parse(fireData[1]) - 1;

            Vector3 firePosition = new Vector3(c, 0.5f, -r);
            GameObject fireMarker = Instantiate(fireMarkerPrefab, firePosition, Quaternion.Euler(-90, 0 , -105));

            lastFireMarkerPosition = firePosition; 
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lastWallPosition, 0.1f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(lastDoorPosition, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(lastPointInterestPosition, 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(lastFireMarkerPosition, 0.1f);
    }
}
