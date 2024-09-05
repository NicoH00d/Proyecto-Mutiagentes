using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Mapa : MonoBehaviour
{
    public GameObject wallPrefab;  // Prefab para las paredes
    private Dictionary<string, CellData> cells;

    // Clase para los datos de cada celda
    [System.Serializable]
    public class CellData
    {
        public int posicion_x;
        public int posicion_y;
        public bool muro_arriba;
        public bool muro_izquierda;
        public bool muro_abajo;
        public bool muro_derecha;
    }

    void Start()
    {
        CargarJson();
    }

    void CargarJson()
    {
        // Construir la ruta al archivo JSON en StreamingAssets
        string filePath = Path.Combine(Application.streamingAssetsPath, "simulation_state.json");
        
        // Verificar si el archivo existe
        if (File.Exists(filePath))
        {
            // Leer el contenido del archivo
            string json = File.ReadAllText(filePath);
            Debug.Log("JSON cargado: " + json);

            // Deserializar el JSON en el diccionario
            cells = JsonConvert.DeserializeObject<Dictionary<string, CellData>>(json);

            // Crear las paredes basadas en los datos de las celdas
            CrearParedes();
        }
        else
        {
            Debug.LogError("El archivo JSON no fue encontrado en: " + filePath);
        }
    }

    void CrearParedes()
    {
        foreach (var cell in cells)
        {
            CellData cellData = cell.Value;

            Vector3 cellPosition = new Vector3(cellData.posicion_x, 0, cellData.posicion_y);

            // Instanciar las paredes si existen
            if (cellData.muro_arriba)
            {
                Vector3 wallPos = cellPosition + new Vector3(0, 0.5f, 0.5f);  // Ajusta la posición según sea necesario
                Instantiate(wallPrefab, wallPos, Quaternion.identity);
            }
            if (cellData.muro_izquierda)
            {
                Vector3 wallPos = cellPosition + new Vector3(-0.5f, 0.5f, 0);
                Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0));
            }
            if (cellData.muro_abajo)
            {
                Vector3 wallPos = cellPosition + new Vector3(0, 0.5f, -0.5f);
                Instantiate(wallPrefab, wallPos, Quaternion.identity);
            }
            if (cellData.muro_derecha)
            {
                Vector3 wallPos = cellPosition + new Vector3(0.5f, 0.5f, 0);
                Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0));
            }
        }
    }
}
