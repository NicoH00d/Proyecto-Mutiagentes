using System.IO;
using UnityEngine;

public class TableroBuilder : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;

    public string fileName = "tablero"; 
    private string[] lines;

    // Variables para guardar la última posición calculada
    private Vector3 lastWallPosition;
    private Vector3 lastDoorPosition;

    void Start()
    {
        CargarArchivo();
        CrearTablero();
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
                // Instanciar celda en el plano XZ
                Vector3 cellPosition = new Vector3(j, 0, -i); // Usamos XZ, Y=0
                GameObject cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);

                // Leer las paredes
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
        // walls es una cadena de 4 dígitos
        if (walls[0] == '1') // Pared atrás (en la posición Z-)
        {
            Vector3 wallPos = cellPosition + new Vector3(0, 0.3f, 0.5f); 
            Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0)); // Rotar 90 grados para que sea horizontal en XZ
            lastWallPosition = wallPos; // Guardar la posición para visualizar con gizmos
        }
        if (walls[1] == '1') // Pared derecha (en la posición X+)
        {
            Vector3 wallPos = cellPosition + new Vector3(0.5f, 0.3f, 0); 
            Instantiate(wallPrefab, wallPos, Quaternion.identity); // Sin rotación, ya que es vertical en XZ
            lastWallPosition = wallPos; // Guardar la posición para visualizar con gizmos
        }
        if (walls[2] == '1') // Pared adelante (en la posición Z+)
        {
            Vector3 wallPos = cellPosition + new Vector3(0, 0.3f, -0.5f); 
            Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0)); // Rotar 90 grados para que sea horizontal en XZ
            lastWallPosition = wallPos; // Guardar la posición para visualizar con gizmos
        }
        if (walls[3] == '1') // Pared izquierda (en la posición X-)
        {
            Vector3 wallPos = cellPosition + new Vector3(-0.5f, 0.3f, 0); 
            Instantiate(wallPrefab, wallPos, Quaternion.identity); // Sin rotación, ya que es vertical en XZ
            lastWallPosition = wallPos; // Guardar la posición para visualizar con gizmos
        }
    }

    void CrearPuerta(Vector3 pos1, Vector3 pos2)
    {
        Vector3 doorPosition = (pos1 + pos2) / 2;

        // Instanciar la puerta en la posición calculada
        Instantiate(doorPrefab, doorPosition, Quaternion.identity);
        lastDoorPosition = doorPosition; // Guardar la posición para visualizar con gizmos
    }

    // Visualizar los gizmos para debug
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Dibujar una esfera en la última posición calculada para una pared
        Gizmos.DrawSphere(lastWallPosition, 0.1f);

        // Dibujar una esfera en la última posición calculada para una puerta
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(lastDoorPosition, 0.1f);
    }
}
