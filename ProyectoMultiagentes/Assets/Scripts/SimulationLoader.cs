using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SimulationLoader : MonoBehaviour
{
    public GameObject agentPrefab;
    public GameObject firePrefab;
    public GameObject victimPrefab;

    private string filePath;
    private Dictionary<int, GameObject> agents = new Dictionary<int, GameObject>();
    private Dictionary<Vector3, GameObject> fires = new Dictionary<Vector3, GameObject>();
    private Dictionary<Vector3, GameObject> victims = new Dictionary<Vector3, GameObject>();

    void Start()
    {
        // Define la ruta al archivo JSON
        filePath = Path.Combine(Application.dataPath, "../simulation_data/simulation_state.json");
        StartCoroutine(UpdateSimulationRoutine());
    }

    IEnumerator UpdateSimulationRoutine()
    {
        while (true)
        {
            LoadSimulationData();
            yield return new WaitForSeconds(1f); // Espera 1 segundo antes de recargar los datos
        }
    }

    void LoadSimulationData()
    {
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            SimulationState simulationState = JsonUtility.FromJson<SimulationState>(jsonContent);
            UpdateSimulation(simulationState);
        }
        else
        {
            Debug.LogError("Archivo no encontrado: " + filePath);
        }
    }

    void UpdateSimulation(SimulationState state)
    {
        // Actualizar o crear agentes
        foreach (var agent in state.firefighter_positions)
        {
            Vector3 position = new Vector3(agent.position[0], 0, agent.position[1]);
            if (!agents.ContainsKey(agent.id))
            {
                GameObject newAgent = Instantiate(agentPrefab, position, Quaternion.identity);
                agents.Add(agent.id, newAgent);
            }
            else
            {
                agents[agent.id].transform.position = position;
            }
        }

        // Actualizar o crear fuegos
        foreach (var firePos in state.fire_locations)
        {
            Vector3 position = new Vector3(firePos[0], 0, firePos[1]);
            if (!fires.ContainsKey(position))
            {
                GameObject newFire = Instantiate(firePrefab, position, Quaternion.identity);
                fires.Add(position, newFire);
            }
        }

        // Actualizar o crear víctimas
        foreach (var poi in state.poi_locations)
        {
            Vector3 position = new Vector3(poi.position[0], 0, poi.position[1]);
            if (!victims.ContainsKey(position))
            {
                GameObject newVictim = Instantiate(victimPrefab, position, Quaternion.identity);
                victims.Add(position, newVictim);
            }
        }

        // Opcional: Verificar si la simulación ha terminado y mostrar un mensaje
        if (!state.running)
        {
            Debug.Log("La simulación ha terminado.");
        }
    }
}

[System.Serializable]
public class FirefighterPosition
{
    public int id;
    public int[] position;
    public bool carrying_victim;
}

[System.Serializable]
public class POILocation
{
    public int[] position;
    public bool revealed;
}

[System.Serializable]
public class SimulationState
{
    public int step;
    public int damage_markers;
    public int rescued_victims;
    public int lost_victims;
    public bool running;
    public List<int[]> fire_locations;
    public List<int[]> smoke_locations;
    public List<POILocation> poi_locations;
    public List<FirefighterPosition> firefighter_positions;
}
