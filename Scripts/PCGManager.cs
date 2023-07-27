using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCGManager : MonoBehaviour
{
    public int currentFloor = 0;
    public WFCGenerator generator;
    public Teleporter teleporter;

    int waves;
    int encounterFinishBlows = 0;
    int encountersCompleted = 0;

    void Start()
    {
        currentFloor = -1;
        SetUpNextLevel();
    }

    public void SetUpNextLevel()
    {
        currentFloor++;
        generator.difficulty = (Difficulty)Mathf.Clamp(currentFloor / 4, 0, 3);
        generator.size.x = Mathf.Clamp(9 + currentFloor / 4, 9, 12);
        generator.size.z = Mathf.Clamp(9 + currentFloor / 4, 9, 12);

        generator.size.y = 3 + Mathf.Clamp(currentFloor / 4, 0, 4);
        generator.airHeight = 1 + Mathf.Clamp((currentFloor / 4) - 1, 0, 2);
        
        waves = 2 + Mathf.Clamp(currentFloor / 4, 0, 2);
        encounterFinishBlows = 0;
        encountersCompleted = 0;

        StartCoroutine(SetUpPCGLevel());
    }

    public IEnumerator SetUpPCGLevel()
    {
        generator.Generate();
        yield return null;

        generator.UpdatePathfindingGrids();
        generator.UpdateEnemyPlacement(waves);

        GameObject[] teleporterPositions = GameObject.FindGameObjectsWithTag("ValidPosition");
        teleporter.transform.position = teleporterPositions[Random.Range(0, teleporterPositions.Length)].transform.position;
        for(int i = teleporterPositions.Length - 1; i > 0; i--) Destroy(teleporterPositions[i]);

        Vector3 firstEnemyPos = generator.enemyPlacer.encounter1.waves[0].GetChild(0).transform.position;

        teleporter.transform.LookAt(new Vector3(firstEnemyPos.x, teleporter.transform.position.y, firstEnemyPos.z), Vector3.up);

        teleporter.Arrive();
        teleporter.SetTeleporterActive(false);

        yield return new WaitForSeconds(1.5f);
        generator.enemyPlacer.StartGroups();
    }

    public void OnEncounterFinishBlow(Vector3 position)
    {
        encounterFinishBlows++;

        if(encounterFinishBlows == 2 && (currentFloor+1) % 4 == 0)
        {
            GameManager.cameraScript.SetBossFinishFocus(position);
            GameManager.manager.DoBossFinishCutscene();
        }
    }

    public void OnEncounterCompleted()
    {
        encountersCompleted++;

        if (encountersCompleted == 2)
        {
            teleporter.SetTeleporterActive(true);
        }
    }
}
