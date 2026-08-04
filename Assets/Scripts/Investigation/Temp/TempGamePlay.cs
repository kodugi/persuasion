using UnityEngine;

public class TempGamePlay : MonoBehaviour
{
    ChiefManager chiefManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chiefManager = FindFirstObjectByType<ChiefManager>();
    }
    public void StageCleared()
    {
        chiefManager.StartInvestigation();
    }
}
