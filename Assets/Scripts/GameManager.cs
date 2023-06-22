using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    public GameState state;

    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UpdateGameState(GameState.GeneratingTerrain);
    }

    public void UpdateGameState(GameState newState)
    {
        state = newState;

        switch (state)
        {
            case GameState.GeneratingTerrain:
                break;
            case GameState.GameStart:

                // Set the player's position to the floor procedurally
                Vector2 _startPosition = transform.position;
                _startPosition.y = GridManager.FloorHeights[(int)transform.position.x] + 5;
                _startPosition.x = Mathf.RoundToInt(_startPosition.x);

                GameObject.FindWithTag("Player").transform.position = _startPosition;

                break;
            case GameState.GameEnd:
                break;
            default:
                Debug.LogError($"{newState} is not a valid GameState.");
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    public enum GameState
{
    GeneratingTerrain,
    GameStart,
    GameEnd
}
}