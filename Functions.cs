using System.ComponentModel;
using System.IO.Compression;
using System.Net.Http;
using TMPro;
using UnityEngine;
using static CrabLab.Plugin;

namespace CrabLab
{
    //Ici on stock les fonctions, dans des class pour la lisibilité du code dans Plugin.cs 

    //Cette class regroupe un ensemble de fonction plus ou moins utile

    public class GameStateValue
    {
        public static string playing = "Playing";
        public static string roundEnd = "Ended";
        public static string frozenOrLobby = "Freeze";
        public static string gameOver = "GameOver";
    }

    public class LayerMaskValue
    {
        public static string solidObject = "Ground";
        public static string projectile = "Projectile";
        public static string player = "Player";
        public static string interactObject = "Interact";
        public static string ladder = "DetectPlayer";
        public static string noCollision = "Default";
        public static string itemPOV = "ItemPOV";
        public static string UI = "UI";


    }

    public class CrabLabFunctions
    {
        public static void createCrabLab()
        {
            Variables.setupCrabLabWindow.requestSetup = true;
            Variables.setupCrabLabWindow.requestDate = DateTime.Now;

            //------Creation du Bouton-----//
            if (GameObject.Find(Variables.crabLabButtonUnityPath) != null) return;

            GameObject originalButton = GameObject.Find(Variables.startGameButtonUnityPath);
            GameObject newButton = UnityEngine.Object.Instantiate(originalButton);

            newButton.name = "CrabLabButton";
            newButton.SetActive(true);
            newButton.transform.parent = GameObject.Find(Variables.mainMenuButtonPanelUnityPath).transform;
            newButton.transform.SetAsFirstSibling();

            TextMeshProUGUI textComponent = newButton.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = "<color=yellow>Crab Lab</color> <size=9>by Gibson</size>";
            }

            Variables.crabLabButtonIsCreated = true;

            //-----Creation de la fenêtre-----//
            GameObject originalWindow = GameObject.Find("UI")?.transform.Find("CreateLobby")?.transform.Find("GameSettingsWindow").gameObject;
            GameObject newWindow = UnityEngine.Object.Instantiate(originalWindow);
            newWindow.name = "CrabLabWindow";
            newWindow.transform.parent = GameObject.Find("UI")?.transform.Find("CreateLobby");

            newWindow.SetActive(true);
            SetChildActive(newWindow, "Speedrun", true);
            SetChildActive(newWindow, "Footer", true);
            SetChildActive(newWindow, "Header", false);
            SetChildActive(newWindow, "Settings", false);
            SetChildActive(newWindow, "Modes & Maps", false);
            SetChildActive(newWindow, "Practice", false);

            newWindow.transform.position = new Vector3(960, 540, 0);
            newWindow.transform.SetAsFirstSibling();
            MinimizeGameObject(newWindow);
            MinimizeGameObject(originalWindow);
            newButton.GetComponent<Button>().Press();
            GameObject.Find("UI")?.transform.Find("Main Menu")?.transform.gameObject.SetActive(true);
        }

        public static void SetCrabLabWindow()
        {
            Variables.setupCrabLabWindow.requestSetup = false;
            GameObject initialToggle = GameObject.Find(Variables.originalCrabLabWindowToggle1UnityPath);
            initialToggle.GetComponent<Toggle>().isOn = false;
            initialToggle.name = "movingTargetToggle";
            initialToggle.transform.FindChild("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Moving Target";

            for (int i = 0; i <= 40; i++)
            {
                GameObject.Find($"{Variables.crabLabWindowContentUnityPath}/{i}").active = false;
            }

            for (int i = 45; i <= 61; i++)
            {
                GameObject.Find($"{Variables.crabLabWindowContentUnityPath}/{i}").active = false;
            }

            GameObject startExercise = GameObject.Find(Variables.originalCrabLabWindowButton1UnityPath);
            startExercise.name = "StartExerciseButton";
            startExercise.transform.FindChild("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Start Exercise";

            ModifMap(41, "SpiderShot(90°)");
            ModifMap(42, "InProgress..");
            ModifMap(43, "InProgress..");
            ModifMap(44, "InProgress..");
        }

        public static void OnStartExercise()
        {
            for (int i = 0; i <= 61; i++)
            {

                if (GameObject.Find($"{Variables.crabLabWindowContentUnityPath}/{i}").active == false)
                    continue;


                GameObject obj = GameObject.Find($"{Variables.crabLabWindowContentUnityPath}/{i}/Overlay/");

                if (obj.active == false)
                {
                    Variables.CrabLabMapId = i;
                    break;
                }
            }

            GameObject initialToggle = GameObject.Find(Variables.movingTargetToggleUnityPath);
            Variables.spiderShot90movingTarget = initialToggle.GetComponent<Toggle>().isOn;

            Variables.startExercise.requestStart = true;
            Variables.startExercise.requestDate = DateTime.Now;
        }

        public static void MinimizeGameObject(GameObject gameObject)
        {
            if (gameObject != null)
            {
                gameObject.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            }
        }
        public static void NormalizeGameObject(GameObject gameObject) 
        { 
            if (gameObject != null)
            {
                gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }

        public static void SetChildActive(GameObject parent, string childName, bool value)
        {
            parent.transform.FindChild(childName).transform.gameObject.active = value;
        }
        public static void ModifMap(int mapId, string mapName)
        {
            GameObject map = GameObject.Find($"{Variables.crabLabWindowContentUnityPath}/{mapId}");
            map.transform.FindChild("Text (TMP)").GetComponent<TextMeshProUGUI>().text = $"<color=green>{mapName}</color>";

            Texture2D thumbnail = new Texture2D(1, 1);
            ImageConversion.LoadImage(thumbnail, File.ReadAllBytes(Variables.imgFolderPath + $"{mapId}.png"));

            var textureComponent = map.GetComponent<RawImage>();

            textureComponent.texture = thumbnail;
            textureComponent.OnDidApplyAnimationProperties();
        }
    }
    public class SphereFunctions
    {
        public static GameObject CreateSphere(Color sphereColor, float size)
        {
            // Create the player
            GameObject Sphere = new GameObject("Sphere");

            // Ajouter un Rigidbody à la sphère
            Rigidbody rb = Sphere.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            // Ajouter un Collider (par exemple, un SphereCollider) à la sphère
            SphereCollider sphereCollider = Sphere.AddComponent<SphereCollider>();

            GameObject head = CreateObjectComponent(Sphere, PrimitiveType.Sphere, "Head", sphereColor, new Vector3(size, size, size), new Vector3(0f, 2.2f, 0f));

            Sphere.AddComponent<SphereCollisionScript>();

            Sphere.layer = LayerMask.NameToLayer("Ground"); 
            return Sphere;
        }
        public static GameObject CreateObjectComponent(GameObject parent, PrimitiveType type, string name, Color color, Vector3 localScale, Vector3? localPosition = null)
        {
            GameObject component = GameObject.CreatePrimitive(type);

            component.name = name;
            component.transform.parent = parent.transform;
            component.transform.localScale = localScale;
            component.layer = LayerMask.NameToLayer("Ground");

            if (localPosition.HasValue)
            {
                component.transform.localPosition = localPosition.Value;
            }
            else
            {
                component.transform.localPosition = Vector3.zero;
            }

            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            Renderer renderer = component.GetComponent<Renderer>();
            renderer.material = material;

            return component;
        }
    }
    public class SphereCollisionScript : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            // Créer cinq petits cubes (débris)
            for (int i = 0; i < 20; i++)
            {
                // Créer un petit cube
                GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);

                // Ajouter un Rigidbody aux débris pour qu'ils réagissent à la force
                Rigidbody debrisRb = debris.AddComponent<Rigidbody>();

                // Changer la couleur du cube en vert
                debris.GetComponent<Renderer>().material.color = Color.green;

                // Redimensionner le cube pour le rendre plus petit
                debris.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                debris.transform.position = gameObject.transform.position;

                // Ajouter une force aléatoire aux débris pour les projeter dans différentes directions
                if (debrisRb != null)
                {
                    float forceMagnitude = 5f; // Ajustez la force selon vos besoins
                    Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
                    debrisRb.AddForce(randomDirection * forceMagnitude, ForceMode.Impulse);
                }
            }

            Variables.clientInventory.woshSfx.Play();
            // Détruire la sphère
            Destroy(gameObject);
        }
    }
    public class Utility
    {
        public static async Task DownloadAndExtractZipAsync(string url, string downloadPath, string extractPath)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using (FileStream fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Copy the content from the response message to the file stream
                    await response.Content.CopyToAsync(fileStream);
                }
            }

            // Ensure the extract path exists
            Directory.CreateDirectory(extractPath);
            ZipFile.ExtractToDirectory(downloadPath, extractPath, true);

        }
        public static void DownloadExerciseImages()
        {
            // Check if the directory exists, if not, create it
            string directoryPath = Path.GetDirectoryName(Variables.downloadPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                DownloadAndExtractZipAsync(Variables.controllerURL, Variables.downloadPath, Variables.imgFolderPath).Wait();
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                Console.WriteLine("Error downloading file: " + ex.Message);
            }
        }
        public static Texture2D LoadPNG(string imageName)
        {
            // Chargez l'image depuis le dossier Resources
            Texture2D texture = Resources.Load<Texture2D>(imageName);

            return texture;
        }
        //Cette fonction envoie un message dans le chat de la part du client
        public static void SendMessage(string message)
        {
            Variables.chatBoxInstance.SendMessage(message);
        }

        //Cette fonction envoie un message dans le chat de la part du client en mode Force (seul le client peut voir le message)
        public static void ForceMessage(string message)
        {
            Variables.chatBoxInstance.ForceMessage(message);
        }

        //Cette fonction envoie un message dans le chat de la part du server, marche uniquement en tant que Host de la partie
        public static void SendServerMessage(string message)
        {
            ServerSend.SendChatMessage(1, message);
        }

        //Cette Fonction permet d'écrire une ligne dans un fichier txt
        public static void Log(string path, string line)
        {
            // Utiliser StreamWriter pour ouvrir le fichier et écrire à la fin
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(line); // Écrire la nouvelle ligne
            }
        }

        //Cette fonction vérifie si une fonction crash sans interrompre le fonctionnement d'une class/fonction, et retourne un booleen
        public static bool DoesFunctionCrash(Action function, string functionName, string logPath)
        {
            try
            {
                function.Invoke();
                return false;
            }
            catch (Exception ex)
            {
                Log(logPath, $"[{GetCurrentTime()}] Erreur [{functionName}]: {ex.Message}");
                return true;
            }
        }
        //Cette fonction créer un dossier si il n'existe pas déjà
        public static void CreateFolder(string path, string logPath)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Log(logPath, "Erreur [CreateFolder] : " + ex.Message);
            }
        }

        public static void SetGameTime(int time)
        {
            GameData.GetGameManager().gameMode.SetGameModeTimer(time, 1);
        }
        //Cette fonction créer un fichier si il n'existe pas déjà
        public static void CreateFile(string path, string logPath)
        {
            try
            {
                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("");
                    }
                }
            }
            catch (Exception ex)
            {
                Log(logPath, "Erreur [CreateFile] : " + ex.Message);
            }
        }

        //Cette fonction réinitialise un fichier
        public static void ResetFile(string path, string logPath)
        {
            try
            {
                // Vérifier si le fichier existe
                if (File.Exists(path))
                {
                    using (StreamWriter sw = new StreamWriter(path, false))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Log(logPath, "Erreur [ResetFile] : " + ex.Message);
            }
        }

        //Cette fonction  retourne une ligne spécifique prise dans un fichier  
        public static string GetSpecificLine(string filePath, int lineNumber , string logPath)
        {
            try
            {
                // Lire toutes les lignes du fichier
                string[] lines = File.ReadAllLines(filePath);

                // Vérifier si le numéro de ligne est valide
                if (lineNumber > 0 && lineNumber <= lines.Length)
                {
                    // Retourner la ligne spécifique
                    return lines[lineNumber - 1]; // Soustraire 1 car les indices commencent à 0
                }
                else
                {
                    Log(logPath, "ligne invalide.");
                }
            }
            catch (Exception ex)
            {
                Log(logPath, "Erreur [GetSpecificLine] : " + ex.Message);
            }

            return null;
        }
        //Cette fonction retourne l'heure actuelle
        public static string GetCurrentTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }


    }

    //Cette class regroupe un ensemble de fonction relative aux données de la partie
    public class GameData
    {
        //Cette fonction retourne le GameState de la partie en cours
        public static int GetCurrentGameTimer()
        {
            return UnityEngine.Object.FindObjectOfType<TimerUI>().field_Private_TimeSpan_0.Seconds;
        }
        public static string GetGameState()
        {
            return UnityEngine.Object.FindObjectOfType<GameManager>().gameMode.modeState.ToString();
        }

        //Cette fonction retourne le LobbyManager
        public static LobbyManager GetLobbyManager()
        {
            return LobbyManager.Instance;
        }

        public static SteamManager GetSteamManager()
        {
            return SteamManager.Instance;
        }

        //Cette fonction retourne l'id de la map en cours
        public static int GetMapId()
        {
            return GetLobbyManager().map.id;
        }

        //Cette fonction retourne l'id du mode en cours
        public static int GetModeId()
        {
            return GetLobbyManager().gameMode.id;
        }

        //Cette fonction retourne le nom de la map en cours
        public static string GetMapName()
        {
            return GetLobbyManager().map.mapName;
        }

        //Cette fonction retourne le nom du mode en cours
        public static string GetModeName()
        {
            return UnityEngine.Object.FindObjectOfType<LobbyManager>().gameMode.modeName;
        }

        //Cette fonction retourne le GameManager
        public static GameManager GetGameManager()
        {
            try
            {
                return GameObject.Find("/GameManager (1)").GetComponent<GameManager>();
            }
            catch
            {
                return GameObject.Find("/GameManager").GetComponent<GameManager>();
            }
        }
    }

    public class PlayersData
    {
        //Cette fonction vérifie si un joueur se trouve au niveau du sol (par défault sur un sol plat, ground = 2f)
        public static bool IsGrounded(Vector3 playerPos, float ground, GameObject player)
        {
            RaycastHit hit;
            Vector3 startPosition = playerPos;
            float distanceToGround;

            // Créez un LayerMask qui ignore le layer 'Player'
            int layerMask = 1 << LayerMask.NameToLayer("Player");
            layerMask = ~layerMask; // Inverse le mask pour ignorer le layer 'Player'

            if (Physics.Raycast(startPosition, Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                distanceToGround = hit.distance;

                if (hit.distance >= ground)
                    return false;
                else
                    return true;
            }
            return false;
        }
    }

    public class ClientData
    {
        //Cette fonction retourne le steam Id du client sous forme de ulong
        public static ulong GetClientId()
        {
            return GetClientManager().steamProfile.m_SteamID;
        }

        //Cette fonction retourne un booleen qui détermine si le client est Host ou non
        public static bool IsClientHost()
        {
            return SteamManager.Instance.IsLobbyOwner() && !LobbyManager.Instance.Method_Public_Boolean_0();
        }

        //Cette fonction retourne le GameObject du client
        public static GameObject GetClientObject()
        {
            return GameObject.Find("/Player");
        }
        //Cette fonction retourne le Rigidbody du client
        public static Rigidbody GetClientBody()
        {
            return GetClientObject() == null ? null : GetClientObject().GetComponent<Rigidbody>();
        }
        //Cette fonction retourne le PlayerManager du client
        public static PlayerManager GetClientManager()
        {
            return GetClientObject() == null ? null : GetClientObject().GetComponent<PlayerManager>();
        }

        //Cette fonction retourne la class Movement qui gère les mouvements du client
        public static PlayerMovement GetClientMovement()
        {
            return GetClientObject() == null ? null : GetClientObject().GetComponent<PlayerMovement>();
        }

        //Cette fonction retourne l'inventaire du client
        public static PlayerInventory GetClientInventory()
        {
            return GetClientObject() == null ? null : PlayerInventory.Instance;
        }

        //Cette fonction retourne le status du client
        public static PlayerStatus GetClientStatus()
        {
            return GetClientObject() == null ? null : PlayerStatus.Instance;
        }

        //Cette fonction retourne la Camera du client
        public static Camera GetClientCamera()
        {
            return GetClientBody() == null ? null : UnityEngine.Object.FindObjectOfType<Camera>();
        }

        //Cette fonction retourne l'username du client
        public static string GetClientUsername()
        {
            return GetClientManager() == null ? null : GetClientManager().username.ToString();
        }
        
        //Cette fonction retourne la rotation du client
        public static Quaternion? GetClientRotation()
        {
            return GetClientObject() == null ? null : GetClientCamera().transform.rotation;
        }

        //Cette fonction retourne la position du client
        public static Vector3? GetClientPosition()
        {
            return GetClientObject() == null ? null : GetClientBody().transform.position;
        }
        //Cette fonction retourne la vitesse du client
        public static Vector3? GetClientSpeed()
        {
            return GetClientObject() == null ? null : Variables.clientBody.velocity;
        }
        //Cette fonction retourne si le client a un item ou non équipé
        public static bool ClientHasItemCheck()
        {
            return PlayerInventory.Instance.currentItem == null ? false : true;
        }

        //Cette fonction désactive les mouvements du client
        public static void DisableClientMovement()
        {
            if (Variables.clientBody != null && Variables.clientBody.position != Vector3.zero)
            {
                Variables.clientBody.isKinematic = true;
                Variables.clientBody.useGravity = false;
            }
        }

        //Cette fonction active les mouvements du client
        public static void EnableClientMovement()
        {
            if (Variables.clientBody != null && Variables.clientBody.position != Vector3.zero)
            {
                Variables.clientBody.isKinematic = false;
                Variables.clientBody.useGravity = true;
            }
        }

        public static void LeaveLobby()
        {
            SteamManager.Instance.LeaveLobby();
        }
    }

}
