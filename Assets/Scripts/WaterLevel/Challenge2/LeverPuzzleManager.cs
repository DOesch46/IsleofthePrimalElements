using UnityEngine;

public class LeverPuzzleManager : MonoBehaviour
{
    public int[] leverStates = new int[4];

    [SerializeField] private GameObject bridgeObject;
    [SerializeField] private GameObject gapBlocker;

    private void Start()
    {
        CheckSolution();
    }

    public void SetLeverState(int index, int state)
    {
        leverStates[index] = state;
        CheckSolution();
    }

    private void CheckSolution()
    {
        // Correct combination: 0 1 0 1
        bool solved =
            leverStates[0] == 0 &&
            leverStates[1] == 1 &&
            leverStates[2] == 0 &&
            leverStates[3] == 1;

        if (bridgeObject != null)
            bridgeObject.SetActive(solved);

        if (gapBlocker != null)
            gapBlocker.SetActive(!solved);
    }
}