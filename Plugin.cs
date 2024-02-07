//Using (ici on importe des bibliothèques utiles)
global using BepInEx;
global using BepInEx.IL2CPP;
global using HarmonyLib;
global using UnityEngine;
global using System;
global using System.IO;
global using System.Collections.Generic;
global using UnhollowerRuntimeLib;
global using System.Linq;
global using System.IO.Compression;
global using System.Net.Http;
global using System.Threading.Tasks;
global using UnityEngine.UI;
global using TMPro;

namespace CrabLab
{
    [BepInPlugin("6D4555CF-4B94-42E2-A525-1684E08B17C5", "CrabLab", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<Basics>();

            //Ajouter ici toute vos class MonoBehaviour pour quelle soit active dans le jeu
            //Format: ClassInjector.RegisterTypeInIl2Cpp<NomDeLaClass>(); 
            ClassInjector.RegisterTypeInIl2Cpp<SpiderShot90>();
            ClassInjector.RegisterTypeInIl2Cpp<SphereCollisionScript>();



            Harmony.CreateAndPatchAll(typeof(Plugin));

            //Ici on créer un fichier log.txt situé dans le dossier GibsonTemplateMod
            Utility.CreateFolder(Variables.mainFolderPath, Variables.logFilePath);
            Utility.CreateFolder(Variables.imgFolderPath, Variables.logFilePath);


            Utility.CreateFile(Variables.logFilePath, Variables.logFilePath);
            Utility.ResetFile(Variables.logFilePath, Variables.logFilePath);

            //Download depuis GitHub les vignettes des exercices
            Utility.DownloadExerciseImages();
        }

        //Cette class permet de récupérer des variables de base ne pas toucher sauf pour rajouter d'autres variables a Update
        public class Basics : MonoBehaviour
        {
            float elapsedServerUpdate, elapsedClientUpdate;
            void Update()
            {
                float elapsedTime = Time.deltaTime;
                elapsedServerUpdate += elapsedTime;
                elapsedClientUpdate += elapsedTime;

                if (elapsedServerUpdate > 1f)
                {
                    BasicUpdateServer();
                    elapsedServerUpdate = 0f;
                }
                    
                if (elapsedClientUpdate > 1f)
                {
                    BasicUpdateClient();
                    elapsedClientUpdate = 0f;
                }

            }

            //Ceci mets a jour les données relative au Client(fonctionne uniquement si le client a un Rigidbody (en vie))
            void BasicUpdateClient()
            {
                Variables.clientBody = ClientData.GetClientBody();
                if (Variables.clientBody == null) return;

                Variables.clientObject = ClientData.GetClientObject();
                Variables.clientId = ClientData.GetClientId();
                Variables.clientMovement = ClientData.GetClientMovement();
                Variables.clientInventory = ClientData.GetClientInventory();
                Variables.clientStatus = PlayerStatus.Instance;
                if (Variables.clientIdSafe == 0)
                    Variables.clientIdSafe = Variables.clientId;
            }

            //Ceci mets a jour les données relative au Server
            void BasicUpdateServer()
            {
                Variables.chatBoxInstance = ChatBox.Instance;
                Variables.gameManager = GameData.GetGameManager();
                Variables.lobbyManager = GameData.GetLobbyManager();
                Variables.steamManager = GameData.GetSteamManager();
                Variables.mapId = GameData.GetMapId();
                Variables.modeId = GameData.GetModeId();
                Variables.gameState = GameData.GetGameState();
                Variables.activePlayers = Variables.gameManager.activePlayers;
                if (Variables.gameState != Variables.lastGameState)
                    Variables.lastGameState = Variables.gameState;
            }
        }

        //Cette classe gère la logique derrière l'exercice SpiderShot90
        public class SpiderShot90 : MonoBehaviour
        {
            public static GameObject currentSpiderShot90Sphere;
            public static bool exerciseHasStarted, exerciseSpiderShot90Init, spiderShot90Trigger, spiderShot90MapTrigger;

            private Vector2 targetDirection;
            private float targetSpeed;
            private List<SnowballPile> SnowballPiles = null;
            private SnowballPile nearestSnowballPile = null;
            private TextMeshProUGUI freezeTimerGUI, playersLeftGUI;

            void Update()
            {
                // Vérifier si nous sommes en mode Practice
                if (Variables.modeId != 13) return;

                // Initialiser l'exercice SpiderShot90
                if (spiderShot90Trigger && !exerciseHasStarted && !exerciseSpiderShot90Init)
                {
                    InitSnowballPiles();
                    CreateSpiderShot90Map();

                    freezeTimerGUI = GameObject.Find(Variables.roundTimerFreezeGUIUnityPath).GetComponent<TextMeshProUGUI>();
                    playersLeftGUI = GameObject.Find(Variables.goodPlayerLeftGUIUnityPath).GetComponent<TextMeshProUGUI>();

                    Variables.spiderShot90Score = 0;

                    CreateNewSphere(ref currentSpiderShot90Sphere);

                    InitClientPositionAndRotation();
                    nearestSnowballPile.transform.position = new Vector3(0, -18, -16);

                    Utility.SetGameTime(60);

                    spiderShot90Trigger = false;
                    exerciseSpiderShot90Init = true;
                    exerciseHasStarted = true;
                }

                //Lorsque la sphère actuelle est détruite on mets a jour le score, et on créer une nouvelle sphère
                if (exerciseHasStarted && currentSpiderShot90Sphere == null)
                {
                    Variables.spiderShot90Score += 1.25f;
                    CreateNewSphere(ref currentSpiderShot90Sphere);
                }

                // Mettre à jour les GUI pendant l'exercice
                if (exerciseHasStarted)
                {
                    freezeTimerGUI.text = $"Score: {Variables.spiderShot90Score}";
                    playersLeftGUI.text = $"{Variables.spiderShot90Score.ToString("F1")}";
                }

                //A la fin de l'exercice on réinitialise les variables et on envoie le score au joueur
                if (Variables.gameState == GameStateValue.roundEnd && exerciseHasStarted)
                {
                    exerciseHasStarted = false;
                    spiderShot90MapTrigger = false;
                    currentSpiderShot90Sphere = null;
                    Destroy(currentSpiderShot90Sphere);

                    //Ici le .ToString("F2") permet de formater le float avec uniquement 2 chiffres après la virgules
                    Utility.ForceMessage($"<color=green>[CrabLab]</color> Your Score : {Variables.spiderShot90Score.ToString("F2")}");

                    Variables.spiderShot90Score = 0;
                }

                //Lorsque le timer atteint 3 secondes on renvoie le joueur vers le CrabLab 
                if (Variables.gameState == GameStateValue.roundEnd && GameData.GetCurrentGameTimer() == 3)
                {
                    ClientData.LeaveLobby();
                }

                //Si le client a choisi l'option MovingTarget, on utilise la fonction MoveSphere pour déplacer la sphère
                if (Variables.spiderShot90movingTarget && currentSpiderShot90Sphere != null)
                {
                    MoveSphere(currentSpiderShot90Sphere);
                }
            }
            #region SpiderShot90Functions
            private void InitClientPositionAndRotation()
            {
                Vector3 exerciseInitialPosition = new Vector3(0, -17, -16);

                Variables.clientBody.transform.position = exerciseInitialPosition;
                Variables.clientMovement.playerCam.rotation = Quaternion.identity;
                ClientData.DisableClientMovement();
            }
            private void ReplaceAndResizeRoundTimer()
            {
                Transform roundTimerTransform = GameObject.Find("RoundTimer").transform;

                roundTimerTransform.position = new Vector3(0, 37, 16);
                roundTimerTransform.localScale = new Vector3(9, 9, 9);
                roundTimerTransform.Rotate(0, 0, 90);
            }
            private void DestroySomeMapObject()
            {
                GameObject[] rocks = GameObject.FindObjectsOfType<GameObject>()
                    .Where(go => go.name.StartsWith("Rock"))
                    .ToArray();

                GameObject[] crates = GameObject.FindObjectsOfType<GameObject>()
                    .Where(go => go.name.StartsWith("Crate"))
                    .ToArray();

                GameObject[] tires = GameObject.FindObjectsOfType<GameObject>()
                    .Where(go => go.name.StartsWith("Tire"))
                    .ToArray();
                GameObject[] snowballPiles = GameObject.FindObjectsOfType<GameObject>()
                    .Where(go => go.name.StartsWith("SnowballPile"))
                    .ToArray();

                DestroyGameObjects(rocks);
                DestroyGameObjects(crates);
                DestroyGameObjects(tires);
                Destroy(GameObject.Find("Snow"));
                Destroy(GameObject.Find("Map"));
                ResizeSnowballPiles(snowballPiles);
            }
            private void InitSnowballPiles()
            {
                bool init = false;

                SnowballPiles = new List<MonoBehaviour1PublicBoInSiUnique>();
                GameObject[] snowballPilesObjects = GameObject.FindObjectsOfType<GameObject>()
                    .Where(go => go.name.StartsWith("SnowballPile"))
                    .ToArray();

                foreach (GameObject snowballPileObject in snowballPilesObjects)
                {
                    var pileComponent = snowballPileObject.GetComponent<MonoBehaviour1PublicBoInSiUnique>();
                    if (pileComponent != null)
                    {
                        SnowballPiles.Add(pileComponent);
                    }
                }

                init = SnowballPiles.Count > 0;
                if (init)
                {
                    nearestSnowballPile = GetNearestSnowballPile();
                }
                else
                {
                    nearestSnowballPile = null;
                }
            }
            private SnowballPile GetNearestSnowballPile()
            {
                float shortestDistance = float.MaxValue;
                MonoBehaviour1PublicBoInSiUnique nearestPile = null;

                if (SnowballPiles == null || SnowballPiles.Count == 0)
                    return null;

                if (Variables.clientBody == null || Variables.clientBody.transform == null)
                    return null;

                foreach (var pile in SnowballPiles)
                {
                    if (pile == null || pile.transform == null)
                        continue;

                    GameObject rootPileGameObject = pile.transform.root.gameObject;
                    float distance = Vector3.Distance(Variables.clientBody.transform.position, rootPileGameObject.transform.position);

                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearestPile = pile;
                    }
                }
                return nearestPile;
            }
            
            private void CreateSpiderShot90Map()
            {
                DestroySomeMapObject();
                ReplaceAndResizeRoundTimer();
                CreateMapBox();
                CreateGround();
                CreateSphereZone();
            }
            private void CreateGround()
            {
                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Rigidbody rb = ground.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
                Collider Collider = ground.AddComponent<Collider>();
                ground.transform.position = new Vector3(0, -17, -16);
                ground.layer = LayerMask.NameToLayer(LayerMaskValue.solidObject);
                ground.transform.localScale = new Vector3(60f, 0.1f, 100f); // Ajustez la taille selon vos besoins

                Material material = new Material(Shader.Find("Standard"));
                material.color = Color.gray;
                ground.GetComponent<Renderer>().material = material;

                // Parentez le sol au personnage si vous souhaitez qu'il se déplace avec lui
                ground.transform.parent = transform;
            }
            private void CreateMapBox()
            {
                // Créer un mur pour représenter le sol
                CreateWall(new Vector3(35f, 0f, 0f), new Vector3(1f, 500f, 500f), Color.black);
                CreateWall(new Vector3(-35f, 0f, 0f), new Vector3(1f, 500f, 500f), Color.black);
                CreateWall(new Vector3(0f, 0f, 35f), new Vector3(500f, 500f, 1f), Color.black);
                CreateWall(new Vector3(0f, 0f, -35f), new Vector3(500f, 500f, 1f), Color.black);
            }
            private void CreateSphereZone()
            {
                float spawnAreaWidth = 62f;
                float spawnAreaHeight = 37f;

                // Créer le rectangle derrière le rectangle des snowballs
                CreateWall(new Vector3(0, 6f, 17f), new Vector3(spawnAreaWidth, spawnAreaHeight, 1f), Color.black); // Ajustez la couleur si nécessaire
            }
            private void ResizeSnowballPiles(GameObject[] snowballPiles)
            {
                foreach (GameObject obj in snowballPiles)
                {
                    obj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
            private void CreateWall(Vector3 position, Vector3 scale, Color color)
            {
                // Créer un cube pour représenter le mur
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = position;
                wall.transform.localScale = scale;

                Material material = new Material(Shader.Find("Standard"));
                material.color = color;
                wall.GetComponent<Renderer>().material = material;
            }
            private void DestroyGameObjects(GameObject[] gameObjects)
            {
                foreach (GameObject obj in gameObjects)
                {
                    Destroy(obj);
                }
            }

            private void CreateNewSphere(ref GameObject refGameObject)
            {
                refGameObject = SphereFunctions.CreateSphere(Color.green, 1f);
                float z = 16f;
                Vector2 spherePos;
                if (Variables.spiderShot90movingTarget)
                    spherePos = Vector2.zero;
                else
                    spherePos = GetRandomPositionInRectangle(-30, 30, -15, 20);

                refGameObject.transform.position = new Vector3(spherePos.x, spherePos.y, z);

                // Choisissez une direction unique au moment de la création
                targetDirection = GetRandomDirection();

                // Choisissez une vitesse aléatoire entre 2 valeurs (à définir)
                float minSpeed = 1f; // Vitesse minimale
                float maxSpeed = 10f; // Vitesse maximale
                targetSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
            }
            private void MoveSphere(GameObject sphere)
            {
                // Déplacez la sphère dans la direction choisie avec une vitesse aléatoire
                Vector3 newPosition = sphere.transform.position + new Vector3(targetDirection.x, targetDirection.y, 0) * targetSpeed * Time.deltaTime;

                // Vérifiez si la nouvelle position est toujours dans le rectangle
                if (IsPositionInRectangle(newPosition.x, newPosition.y, -30, 30, -15, 20))
                {
                    sphere.transform.position = newPosition;
                }
                else
                {
                    Variables.spiderShot90Score -= 2.5f;
                    Destroy(sphere);
                }
            }

            private Vector2 GetRandomDirection()
            {
                float randomAngle = UnityEngine.Random.Range(0f, 360f);
                Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
                return randomDirection;
            }
            private bool IsPositionInRectangle(float x, float y, float minX, float maxX, float minY, float maxY)
            {
                return x >= minX && x <= maxX && y >= minY && y <= maxY;
            }
            private Vector2 GetRandomPositionInRectangle(float minX, float maxX, float minY, float maxY)
            {
                float randomX = UnityEngine.Random.Range(minX, maxX);
                float randomY = UnityEngine.Random.Range(minY, maxY);

                Vector2 randomPosition = new Vector2(randomX, randomY);
                return randomPosition;
            }
            #endregion SpiderShot90Functions
        }

        //Ici on intercepte la method appelé lors de l'utilisation de snowball pour injecter notre propre code a ce moment
        [HarmonyPatch(typeof(ItemGun), nameof(ItemGun.AllUse))]
        [HarmonyPostfix]
        public static void OnSnowballUse(ItemGun __instance)
        {
            switch (Variables.mapId)
            {
                case 41: //SpiderShot90
                    Variables.spiderShot90Score -= 0.25f;
                    break;
            }
        }

        //Ici on intercepte la method appelé lorsque l'on quitte un lobby pour injecter notre propre code a ce moment
        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.LeaveLobby))]
        [HarmonyPrefix]
        public static void OnLeaveLobby()
        {
            switch (Variables.mapId)
            {
                case 41: //SpiderShot90
                    Variables.elapsedCreateCrabLabButton = -0.01f;
                    Variables.shouldEnterCrabLab = true;
                    Variables.CrabLabMapId = 0;
                    SpiderShot90.exerciseHasStarted = false;
                    SpiderShot90.spiderShot90MapTrigger = false;
                    SpiderShot90.spiderShot90Trigger = false;
                    SpiderShot90.currentSpiderShot90Sphere = null;
                    SpiderShot90.exerciseSpiderShot90Init = false;
                    
                    break;
            }
            Variables.crabLabButtonIsCreated = false;

        }

        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Update))]
        [HarmonyPostfix]
        public static void OnSteamManagerUpdate(SteamManager __instance)
        {
            var now = DateTime.Now;
            Variables.elapsedCreateCrabLabButton += Time.deltaTime;

            //Créer le bouton Crab Lab si il n'existe pas encore
            if (!Variables.crabLabButtonIsCreated && Variables.elapsedCreateCrabLabButton > 0)
                CrabLabFunctions.createCrabLab();

            //Setup la fenêtre de CrabLab après la création du CrabLab
            if (Variables.setupCrabLabWindow.requestSetup == true && (now - Variables.setupCrabLabWindow.requestDate).TotalMilliseconds > 1)
            {
                CrabLabFunctions.SetCrabLabWindow();
                Variables.setupCrabLabWindow.requestSetup = false;
            }

            //Après la fin d'un exercice ramener dans le CrabLab
            if (Variables.shouldEnterCrabLab && Variables.setupCrabLabWindow.requestSetup == false)
            {
                GameObject.Find(Variables.crabLabButtonUnityPath).GetComponent<Button>().Press();
                Variables.shouldEnterCrabLab = false;
            }

            if (Variables.startExercise.requestStart && (now - Variables.startExercise.requestDate).TotalMilliseconds > 400)
            {
                ServerSend.LoadMap(Variables.CrabLabMapId, 13);
                Variables.startExercise.requestStart = false;
            }

            if (Variables.clientBody == null)
            {
                switch (Variables.CrabLabMapId)
                {
                    case 41:
                        string param;
                        string description;
                        if (Variables.spiderShot90movingTarget)
                        {
                            param = "Dynamic";
                            description = "• Shoot the sphere before it reaches the edge of the map as quickly as possible\n\n• The sphere moves at random speeds\n\n• Use a minimum of snowball";
                        }
                        else
                        {
                            param = "Static";
                            description = "• Shoot the sphere as fast as you can\n\n• The sphere appears in a random location\n\n• Use a minimum of snowball";
                        }

                        ModifMapDisplay(41, "SpiderShot 90°", param, description);
                        break;
                }
            }
            else
            {
                switch (Variables.CrabLabMapId)
                {
                    case 41:
                        if (!SpiderShot90.spiderShot90MapTrigger)
                        {
                            SpiderShot90.spiderShot90Trigger = true;
                            SpiderShot90.spiderShot90MapTrigger = false;


                        }
                        break;
                }
            }
            void ModifMapDisplay(int mapId, string mapName, string paramName, string description)
            {
                GameObject mapThumbnail = GameObject.Find($"UI (1)/MapThumbnail");

                if (mapThumbnail != null)
                {
                    Texture2D thumbnail = new Texture2D(1, 1);
                    ImageConversion.LoadImage(thumbnail, System.IO.File.ReadAllBytes(Variables.imgFolderPath + $"{mapId}.png"));

                    var textureComponent = mapThumbnail.GetComponent<UnityEngine.UI.RawImage>();

                    if (textureComponent != null)
                    {
                        textureComponent.texture = thumbnail;
                        textureComponent.OnDidApplyAnimationProperties();
                    }

                    GameObject nameText = GameObject.Find("UI (1)/Meta/Overlay/Name/Name");
                    if (nameText != null)
                    {
                        nameText.GetComponent<TMPro.TextMeshProUGUI>().text = $"<color=green>{mapName}</color>";
                    }

                    GameObject modeText = GameObject.Find("UI (1)/Meta/Overlay/Gamemode/Mode");
                    if (modeText != null)
                    {
                        modeText.GetComponent<TMPro.TextMeshProUGUI>().text = $"{paramName}";
                    }

                    GameObject descriptionText = GameObject.Find("UI (1)/Meta/Overlay/Mode_description/Mode_description");
                    if (descriptionText != null)
                    {
                        descriptionText.GetComponent<TMPro.TextMeshProUGUI>().text = $"{description}";
                    }
                }
            }


        }
        [HarmonyPatch(typeof(Button), nameof(Button.Press))]
        [HarmonyPostfix]
        public static void onButtonPress(UnityEngine.UI.Button __instance)
        {
            if (Variables.setupCrabLabWindow.requestSetup == true) return;

            if (__instance.name == "StartGame")
            {
                StartGameButtonPress();
            }
            if (__instance.name == "CrabLabButton")
            {
                CrabLabButtonPress();
            }

            if (__instance.name == "StartExerciseButton")
            {
                StartExercisePress();
            }

            void StartExercisePress()
            {
                CrabLabFunctions.OnStartExercise();
            }

            void CrabLabButtonPress()
            {
                CrabLabFunctions.MinimizeGameObject(GameObject.Find(Variables.gameSettingsWindowUnityPath));
                CrabLabFunctions.NormalizeGameObject(GameObject.Find(Variables.crabLabWindowUnityPath));
            }
            void StartGameButtonPress()
            {
                CrabLabFunctions.MinimizeGameObject(GameObject.Find(Variables.crabLabWindowUnityPath));
                CrabLabFunctions.NormalizeGameObject(GameObject.Find(Variables.gameSettingsWindowUnityPath));
            }
        }

        



        [HarmonyPatch(typeof(GameUI), "Awake")]
        [HarmonyPostfix]
        public static void UIAwakePatch(GameUI __instance)
        {
            GameObject menuObject = new GameObject();
            Basics basics = menuObject.AddComponent<Basics>();

            //Ici aussi ajouter toute vos class MonoBehaviour pour quelle soit active dans le jeu
            //Format: NomDeLaClass nomDeLaClass = menuObject.AddComponent<NomDeLaClass>();
            SpiderShot90 exemple = menuObject.AddComponent<SpiderShot90>();
            SphereCollisionScript sphereCollisionHandler = menuObject.AddComponent<SphereCollisionScript>();

            menuObject.transform.SetParent(__instance.transform);
        }

        #region AntiCheat ByPass
        [HarmonyPatch(typeof(EffectManager), "Method_Private_Void_GameObject_Boolean_Vector3_Quaternion_0")]
        [HarmonyPatch(typeof(LobbyManager), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(MonoBehaviourPublicVesnUnique), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(LobbySettings), "Method_Public_Void_PDM_2")]
        [HarmonyPatch(typeof(MonoBehaviourPublicTeplUnique), "Method_Private_Void_PDM_32")]
        [HarmonyPrefix]
        public static bool Prefix(System.Reflection.MethodBase __originalMethod)
        {
            return false;
        }
        #endregion AntiCheat ByPass
    }
}