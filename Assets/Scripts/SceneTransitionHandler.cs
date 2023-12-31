using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using DilmerGames.Core.Singletons;

public class SceneTransitionHandler : Singleton<SceneTransitionHandler>
{
    static public SceneTransitionHandler sceneTransitionHandler { get; internal set; }

    [SerializeField]
    public string DefaultMainMenu = "XRMultiplayerMain";

    [HideInInspector]
    public delegate void ClientLoadedSceneDelegateHandler(ulong clientId);
    [HideInInspector]
    public event ClientLoadedSceneDelegateHandler OnClientLoadedScene;

    [HideInInspector]
    public delegate void SceneStateChangedDelegateHandler(SceneStates newState);
    [HideInInspector]
    public event SceneStateChangedDelegateHandler OnSceneStateChanged;

    private int m_numberOfClientLoaded;


    [SerializeField] private bool testHost = false;

    [SerializeField] private bool testClient = false;
    public bool InitializeAsHost { get; set; }

    private void Update()
    {
        // for testing only
        if(testHost) //Input.GetKeyDown(KeyCode.H) || 
        {
            testHost = false;
            LaunchMP(true);
        }
        if(testClient) //Input.GetKeyDown(KeyCode.C) || 
        {
            testClient = false;
            LaunchMP(false);
        }
    }

    /// <summary>
    /// Example scene states
    /// </summary>
    public enum SceneStates
    {
        Init,
        Start,
        Lobby,
        Ingame
    }

    [SerializeField] private SceneStates m_SceneState;

    /// <summary>
    /// Awake
    /// If another version exists, destroy it and use the current version
    /// Set our scene state to INIT
    /// </summary>
    private void Awake()
    {
        if(sceneTransitionHandler != this && sceneTransitionHandler != null)
        {
            Debug.Log("Another scene transition handler existed, removing");
            GameObject.Destroy(sceneTransitionHandler.gameObject);
        }
        sceneTransitionHandler = this;
        SetSceneState(SceneStates.Init);
    }

    
    /// <summary>
    /// SetSceneState
    /// Sets the current scene state to help with transitioning.
    /// </summary>
    /// <param name="sceneState"></param>
    public void SetSceneState(SceneStates sceneState)
    {
        m_SceneState = sceneState;
        if(OnSceneStateChanged != null)
        {
            OnSceneStateChanged.Invoke(m_SceneState);
        }
    }

    /// <summary>
    /// GetCurrentSceneState
    /// Returns the current scene state
    /// </summary>
    /// <returns>current scene state</returns>
    public SceneStates GetCurrentSceneState()
    {
        return m_SceneState;
    }

    /// <summary>
    /// Initialize
    /// Loads the default main menu when started (this should always be a component added to the networking manager)
    /// </summary>
    public void Initialize()
    {
        if(m_SceneState == SceneStates.Init)
        {
            Debug.Log("Loading " + DefaultMainMenu);
            SceneManager.LoadScene(DefaultMainMenu);
        }
    }

    /// <summary>
    /// Switches to a new scene
    /// </summary>
    /// <param name="scenename"></param>
    public void SwitchScene(string scenename)
    {
        Debug.Log("Switching to " + scenename);
        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
            NetworkManager.Singleton.SceneManager.LoadScene(scenename, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadSceneAsync(scenename);
        }
    }
    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        //We are only interested by Client Loaded Scene events
        if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;

        m_numberOfClientLoaded += 1;
        OnClientLoadedScene?.Invoke(sceneEvent.ClientId);
    }

    public bool AllClientsAreLoaded()
    {
        return m_numberOfClientLoaded == NetworkManager.Singleton.ConnectedClients.Count;
    }

    /// <summary>
    /// ExitAndLoadStartMenu
    /// This should be invoked upon a user exiting a multiplayer game session.
    /// </summary>
    public void ExitAndLoadStartMenu()
    {
        OnClientLoadedScene = null;
        SetSceneState(SceneStates.Start);
        SceneManager.LoadScene(1);
    }

    public void LaunchMP(bool asHost)
    {
        Debug.Log("Clicked at " + Time.time);
        InitializeAsHost = asHost;
        Initialize();
    }
}