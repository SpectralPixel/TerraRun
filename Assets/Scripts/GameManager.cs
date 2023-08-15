using System;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public static event Action<GameState> OnGameStateChanged;

    public GameState State;
    private GameState nextState;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        UpdateGameState(GameState.GeneratingTerrain);

        OnGameStateChanged += GameStateChangeCompleted;
    }

    private void GameStateChangeCompleted(GameState newState)
    {
        if (nextState != newState) UpdateGameState(nextState);
    }

    private void OnDestroy()
    {
        OnGameStateChanged -= GameStateChangeCompleted;
    }

    public async void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (State)
        {
            case GameState.GeneratingTerrain:
                GameUtilities.InitializeUtilities();

                if (WorldGenerator.Instance == null) FindObjectOfType<WorldGenerator>().EnsureSingleton();
                if (GridManager.Instance == null) FindObjectOfType<GridManager>().EnsureSingleton();

                Task[] tasks = new Task[2];
                tasks[0] = WorldGenerator.Instance.InitializeWorld();
                tasks[1] = GridManager.Instance.InitializeGrid();

                await Task.WhenAll(tasks);

                nextState = GameState.GameStart;

                break;
            case GameState.GameStart:

                // Set the player's position to the floor procedurally
                Vector2 _startPosition = transform.position;
                WorldGenerator.Instance.GenerateFloorHeight((int)_startPosition.x);
                _startPosition.y = WorldGenerator.Instance.FloorHeights[(int)transform.position.x] + transform.localScale.y * 2;
                _startPosition.x = Mathf.RoundToInt(_startPosition.x) + 0.5f;

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
}

public enum GameState
{
    GeneratingTerrain,
    GameStart,
    GameEnd
}