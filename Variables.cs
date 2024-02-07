namespace CrabLab
{
    //Ici on stock les variables "globale" pour la lisibilité du code dans Plugin.cs 
    internal class Variables
    {
        //folder
        public static string assemblyFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string defaultFolderPath = assemblyFolderPath.Replace("\\BepInEx\\plugins", "\\");
        public static string mainFolderPath = defaultFolderPath + @"BepInEx\plugins\CrabLab\";
        public static string imgFolderPath = mainFolderPath + @"img\";

        //file
        public static string logFilePath = mainFolderPath + "log.txt";
        public static string controllerURL = "https://github.com/GibsonFR/CrabLab/raw/main/src/imgSrc.zip";
        public static string downloadPath = Path.Combine(mainFolderPath + "bin\\", "imgSrc.zip");

        //Manager
        public static GameManager gameManager;
        public static PlayerMovement clientMovement;
        public static PlayerInventory clientInventory;
        public static PlayerStatus clientStatus;
        public static LobbyManager lobbyManager;
        public static SteamManager steamManager;

        //UnityPath
        public static string crabLabButtonUnityPath = "UI/Main Menu/BtnsPanel/CrabLabButton";
        public static string startGameButtonUnityPath = "UI/Main Menu/BtnsPanel/StartGame";
        public static string mainMenuButtonPanelUnityPath = "UI/Main Menu/BtnsPanel";
        public static string roundTimerFreezeGUIUnityPath = "RoundTimer/Canvas/FreezeTime";
        public static string goodPlayerLeftGUIUnityPath = "GameUI/Status/TopRight/PlayersLeft/Good/Text (TMP)";
        public static string gameSettingsWindowUnityPath = "UI/CreateLobby/GameSettingsWindow";
        public static string crabLabWindowUnityPath = "UI/CreateLobby/CrabLabWindow";
        public static string createLobbyUIUnityPath = "UI/CreateLobby";
        public static string crabLabWindowContentUnityPath = "UI/CreateLobby/CrabLabWindow/Speedrun/ViewPort/Container/Content";
        public static string originalCrabLabWindowToggle1UnityPath = "UI/CreateLobby/CrabLabWindow/Speedrun/Modes/Container/GameModeToggle";
        public static string originalCrabLabWindowButton1UnityPath = "UI/CreateLobby/CrabLabWindow/Footer/Tabs/StartSingle";
        public static string movingTargetToggleUnityPath = "UI/CreateLobby/CrabLabWindow/Speedrun/Modes/Container/movingTargetToggle";
        public static string mainMenuUIUnityPath = "UI/Main Menu";


        //TextBox
        public static ChatBox chatBoxInstance;

        //Dictionary
        public static Il2CppSystem.Collections.Generic.Dictionary<ulong, PlayerManager> activePlayers;

        //KeyValuePair
        public static (bool requestSetup, DateTime requestDate) setupCrabLabWindow;
        public static (bool requestStart, DateTime requestDate) startExercise;

        //Rigidbody
        public static Rigidbody clientBody;

        //GameObject
        public static GameObject clientObject;

        //int
        public static int mapId, modeId, CrabLabMapId;

        //ulong
        public static ulong clientId, clientIdSafe;

        //string
        public static string gameState, lastGameState;

        //bool
        public static bool shouldEnterCrabLab, spiderShot90movingTarget, crabLabButtonIsCreated;

        //float
        public static float elapsedsetupCrabLabWindow, elapsedStartExercise, elapsedCreateCrabLabButton = 0f, spiderShot90Score;
        
    }
}


