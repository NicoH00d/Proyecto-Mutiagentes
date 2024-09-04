using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class WebClient : MonoBehaviour
{
    public GameObject cellPrefab;   // Prefab para las celdas
    public GameObject wallPrefab;   // Prefab para las paredes
    public GameObject firePrefab;   // Prefab para el fuego
    public GameObject poiPrefab;    // Prefab para los puntos de interés (POI)
    public GameObject victimPrefab; // Prefab para las víctimas
    public GameObject doorPrefab;
    public GameObject entrancePrefab;
    public GameObject firefighterPrefab;
    


    private Dictionary<Vector3, GameObject> cells = new Dictionary<Vector3, GameObject>();
    private Dictionary<int, GameObject> bomberos = new Dictionary<int, GameObject>(); // Almacenar las instancias de los bomberos

    private string previousJson = ""; // Variable para almacenar el JSON anterior
    
    IEnumerator SendData()
    {
        string url = "http://127.0.0.1:8585";
    
        while (true)  // Esto hará que las solicitudes se repitan indefinidamente
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
    
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error: {www.error}, Status Code: {www.responseCode}");
                }
                else
                {
                    string newJson = www.downloadHandler.text;
                    Debug.Log("Received JSON: " + newJson);
    
                    // Verifica si el JSON ha cambiado
                    if (!newJson.Equals(previousJson))
                    {
                        // Deserializar el nuevo JSON
                        var cellDictionary = JsonConvert.DeserializeObject<Dictionary<string, Cell>>(newJson);
    
                        // Crear el escenario basado en los datos deserializados
                        CrearEscenario(cellDictionary);
    
                        // Deserializar los datos de los bomberos
                        JObject jsonObject = JObject.Parse(newJson);
                        InstanciarBomberos(jsonObject);
    
                        // Actualizar el JSON anterior
                        previousJson = newJson;
                    }
                    else
                    {
                        Debug.Log("No changes detected in the JSON.");
                    }
                }
            }
    
            // Esperar un tiempo antes de volver a hacer la solicitud (por ejemplo, cada 1 segundo)
            yield return new WaitForSeconds(1.0f);
        }
    }



    void CrearEscenario(Dictionary<string, Cell> cellDictionary)
    {
        int maxColumn = 8; // Número máximo de columnas (ancho)
        int currentRow = 0;  // Índice para la fila actual
        int currentColumn = 0;  // Índice para la columna actual

        Vector3 cellSize = GetPrefabSize(cellPrefab);

        foreach (var entry in cellDictionary)
        {
            string cellId = entry.Key;
            Cell cell = entry.Value;

            Vector3 cellPosition = new Vector3(currentColumn * cellSize.x, 0f, currentRow * cellSize.z);

            InstanciarCelda(cellPosition);
            InstanciarMuros(cell, cellPosition, cellSize);
            InstanciarPuertas(cell, cellPosition);
            InstanciarEntradas( cell, cellPosition);
            
            if (cell.coordenadas_fuego != null && cell.coordenadas_fuego.Count > 0)
            {
                InstanciarFuego(cell, cellPosition);
            }
            if (cell.coordenadas_poi != null && cell.coordenadas_poi.Count > 0)
            {
                InstanciarPOI(cell, cellPosition);
            }

            currentColumn++;
            if (currentColumn >= maxColumn)
            {
                currentColumn = 0;
                currentRow++;
            }
        }
    }
      // Método para instanciar o actualizar bomberos
    void InstanciarBomberos(JObject jsonObject)
    {
        foreach (var firefighter in jsonObject)
        {
            if (firefighter.Key.StartsWith("Firefighter_"))
            {
                int id = int.Parse(firefighter.Key.Replace("Firefighter_", ""));
                int x = firefighter.Value["posicion_x"].ToObject<int>();
                int y = firefighter.Value["posicion_y"].ToObject<int>();
                bool carryingVictim = firefighter.Value["carrying_victim"].ToObject<bool>();

                Vector3 position = new Vector3(x, 0.45f, y);

                // Si el bombero ya está en la escena, actualiza su posición
                if (bomberos.ContainsKey(id))
                {
                    bomberos[id].transform.position = position;
                }
                else
                {
                    // Si no existe, crea una nueva instancia del bombero
                    GameObject newFirefighter = Instantiate(firefighterPrefab, position, Quaternion.Euler(-90,0,0));
                    bomberos.Add(id, newFirefighter);
                }
            }
        }
    }

    void InstanciarMuros(Cell cell, Vector3 cellPosition, Vector3 cellSize)
    {
        float wallHeight = 0.3f; // Ajusta la altura de las paredes si es necesario.

        // Pared arriba (en la posición Z+)
        if (cell.muro_arriba)
        {
            Vector3 wallPos = cellPosition + new Vector3(0, wallHeight, -cellSize.z / 2);
            InstantiateWall(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0));
        }

        // Pared derecha (en la posición X+)
        if (cell.muro_derecha)
        {
            Vector3 wallPos = cellPosition + new Vector3(-cellSize.x / 2, wallHeight, 0);
            InstantiateWall(wallPrefab, wallPos, Quaternion.identity);
        }

        // Pared abajo (en la posición Z-)
        if (cell.muro_abajo)
        {
            Vector3 wallPos = cellPosition + new Vector3(0, wallHeight, cellSize.z / 2);
            InstantiateWall(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0));
        }

        // Pared izquierda (en la posición X-)
        if (cell.muro_izquierda)
        {
            Vector3 wallPos = cellPosition + new Vector3(cellSize.x / 2, wallHeight, 0);
            InstantiateWall(wallPrefab, wallPos, Quaternion.identity);
        }
    }

    void InstantiateWall(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject newWall = Instantiate(prefab, position, rotation);
        // No se asigna a un objeto padre como 'floor', ya que cada muro pertenece a una celda
    }

    void InstanciarCelda(Vector3 cellPosition)
    {
        GameObject newCell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
    
    }

    void InstanciarFuego(Cell cell, Vector3 position)
    {
        Vector3 firePosition = position + new Vector3(0f, 0.2f, 0f); 
        GameObject newFire = Instantiate(firePrefab, firePosition, Quaternion.Euler(-90,0,0));
    }

    void InstanciarPOI(Cell cell, Vector3 position)
    {
        Vector3 poiPosition = position + new Vector3(0f, 0.4f, 0f); 
        GameObject newPOI = Instantiate(poiPrefab, poiPosition, Quaternion.Euler(-90,0,0));
    }

    void InstanciarPuertas(Cell cell, Vector3 position)
    {
        if (cell.puerta != null && cell.puerta.Count == 2) // Verificar que haya dos coordenadas para la puerta
        {
            int puertaX = cell.puerta[0]; // La coordenada X de la otra celda conectada
            int puertaZ = cell.puerta[1]; // La coordenada Y de la otra celda conectada

            // Definir la posición de la puerta
            Vector3 doorPosition;

            // Verificar si la puerta está en una celda horizontal (misma fila) o vertical (misma columna)
            if (puertaX == cell.posicion_x) // Mismo X, puerta horizontal
            {
                doorPosition = position + new Vector3(0.5f, 0.2f, 0f); // Ajustar Z para puertas en horizontal
                Instantiate(doorPrefab, doorPosition, Quaternion.Euler(0, 90, 0)); // Rotar 90 grados para puertas en horizontal
            }
            else if (puertaZ == cell.posicion_y) // Mismo Z, puerta vertical
            {
                doorPosition = position + new Vector3(0f, 0.2f, 0.5f); // Ajustar X para puertas en vertical
                Instantiate(doorPrefab, doorPosition, Quaternion.identity); // No rotar para puertas en vertical
            }
        }
    }

    void InstanciarEntradas(Cell cell, Vector3 position)
    {
        if (cell.coordenadas_entradas != null && cell.coordenadas_entradas.Count > 0)
        {
            foreach (var entrada in cell.coordenadas_entradas)
            {
                int entradaX = entrada[0];
                int entradaZ = entrada[1];

                Vector3 entradaPosition = position;

                // Ajuste de la posición de la puerta abierta basada en la ubicación de la entrada
                if (entradaZ == 1) // Entrada en el borde izquierdo
                {
                    entradaPosition += new Vector3(-0.5f, 0.3f, 0); // Ajuste para borde izquierdo
                }
                else if (entradaZ == 8) // Entrada en el borde derecho
                {
                    entradaPosition += new Vector3(0.5f, 0.3f, 0); // Ajuste para borde derecho
                }
                else if (entradaX == 1) // Entrada en el borde superior
                {
                    entradaPosition += new Vector3(0, 0.3f, -0.5f); // Ajuste para borde superior
                }
                else if (entradaX == 6) // Entrada en el borde inferior
                {
                    entradaPosition += new Vector3(0, 0.3f, 0.5f); // Ajuste para borde inferior
                }

                Instantiate(entrancePrefab, entradaPosition, Quaternion.identity); // Instancia la puerta abierta

                // Para depuración, verificar la posición de la puerta
                Debug.Log("Instanciando puerta abierta en: " + entradaPosition);
            }
        }
    }

    Vector3 GetPrefabSize(GameObject prefab)
    {
        Renderer renderer = prefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size;
        }
        else
        {
            Debug.LogWarning("Prefab no tiene un componente Renderer. Se usará tamaño predeterminado.");
            return new Vector3(1, 1, 1); // Tamaño predeterminado si no hay un componente Renderer
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SendData());
    }

    // Update is called once per frame
    void Update()
    {
        // Aquí puedes actualizar el estado del mapa si es necesario.
    }
}

// Clase Cell que contiene la información de cada celda.
public class Cell
{
    public int posicion_x;
    public int posicion_y;
    public bool muro_arriba;
    public bool muro_izquierda;
    public bool muro_abajo;
    public bool muro_derecha;
    public int punto_interes;
    public int fuego;
    public List<int> puerta;
    public bool entrada;
    public List<List<int>> coordenadas_fuego;
    public List<List<int>> coordenadas_victimas;
    public List<List<int>> coordenadas_poi;
    public List<List<int>> coordenadas_entradas;
}

public class Bombero
{
    public int posicion_x;
    public int posicion_y;
    bool carrying_victim;

    public Bombero(int x, int y, bool carrying)
    {
        posicion_x = x;
        posicion_y = y;
        carrying_victim = carrying;
    }

    // Método para obtener la posición como Vector3 (necesario para Unity)
    public Vector3 GetPosition()
    {
        return new Vector3(posicion_x, 0f, posicion_y); // Puedes ajustar la altura Y si es necesario
    }
}