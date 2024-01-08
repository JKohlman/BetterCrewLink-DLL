using BepInEx;
using BepInEx.Unity.IL2CPP;
using InnerNet;
using System;
using System.Collections.Generic;

namespace BCLDLL.Model
{
    public enum GameState { 
        LOBBY = 0,
        TASKS,
        DISCUSSION,
        MENU,
        UNKNOWN,
    }

    public class Player
    {
        public int id { get; set; }
        public int clientId { get; set; }
        public string name { get; set; }
        public int nameHash { get; set; }
        public int colorId { get; set; }
        public string hatId { get; set; }
        public int petId { get; set; }
        public string skinId { get; set; }
        public string visorId { get; set; }
        public bool disconnected { get; set; }
        public bool isImpostor { get; set; }
        public bool isDead { get; set; }
        public bool isLocal { get; set; }
        public int shiftedColor { get; set; }
        public bool bugged { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public bool inVent { get; set; }
        public bool isDummy { get; set; }
    }
    public enum CameraLocation
    {
        East, //ENGINE ROOM
        Central, // VAULT
        Northeast, // RECORDS
        South, // SECURITY
        SouthWest, // CARGO BAY
        NorthWest, // MEETING ROOM
        Skeld,
        NONE,
    }
    public enum MapType
    {
        THE_SKELD,
        MIRA_HQ,
        POLUS,
        THE_SKELD_APRIL,
        AIRSHIP,
        SUBMERGED,
        UNKNOWN,
    }

    public static class ModTypes
    {
        public static string NONE { get; set; } = "NONE";
        public static string TOWN_OF_US { get; set; } = "TOWN_OF_US"; // DONE
        public static string TOWN_OF_HOSTS { get; set; } = "TOWN_OF_HOSTS"; // DONE
        public static string THE_OTHER_ROLES { get; set; } = "THE_OTHER_ROLES"; // DONE
        public static string LAS_MONJAS { get; set; } = "LAS_MONJAS"; // DONE
        public static string OTHER { get; set; } = "OTHER";
    }

    public class AmongUsState
    {
        public GameState gameState { get; set; }
        public GameState oldGameState { get; set; }
        public string lobbyCode { get; set; }
        public Player[] players { get; set; } = Array.Empty<Player>();
        public bool isHost { get; set; }
        public int clientId { get; set; }
        public int hostId { get; set; }
        public bool comsSabotaged { get; set; } = false;
        public CameraLocation currentCamera { get; set; } = CameraLocation.NONE;
        public MapType map { get; set; } = MapType.UNKNOWN;
        public float lightRadius { get; set; } = 1;
        public bool lightRadiusChanged { get; set; } = true;
        public int[] closedDoors { get; set; } = Array.Empty<int>();
        public string currentServer { get; set; } = "";
        public int maxPlayers { get; set; } = 10;
        public string mod { get; set; } = ModTypes.NONE;
        public bool oldMeetingHud { get; set; }

        private static float _lastLightRadius = 1;
        private static GameState _oldGameState = GameState.UNKNOWN;

        public AmongUsState()
        {
            oldGameState = _oldGameState;
            gameState = GetGameState();
            _oldGameState = gameState;

            hostId = AmongUsClient.Instance.HostId;
            clientId = AmongUsClient.Instance.ClientId;
            isHost = hostId == clientId;
            mod = GetMod();
            currentServer = ServerManager.Instance.CurrentRegion.Name;
            oldMeetingHud = false;

            // Overridden by local games from player later
            int lobbyCodeInt = AmongUsClient.Instance.GameId;
            if (gameState == GameState.MENU)
                lobbyCode = "MENU";
            else // IN GAME/LOBBY values
            {
                maxPlayers = PlayerControl.GameOptions.MaxPlayers;

                GameData.PlayerInfo localPlayer = null;
                int playerCount = GameData.Instance.AllPlayers.Count;
                players = new Player[playerCount];
                for (int i = 0; i < playerCount; i++)
                {
                    var player = GameData.Instance.AllPlayers[i];
                    players[i] = GetPlayer(player);
                    if (players[i].isLocal)
                        localPlayer = player;
                }
                // Override for local games
                if (lobbyCodeInt == 32)
                    lobbyCode = (HashCode(localPlayer.PlayerName) % 99999).ToString();
                else
                    lobbyCode = GameCode.IntToGameName(AmongUsClient.Instance.GameId);

                map = (MapType)PlayerControl.GameOptions.MapId;
                if (gameState == GameState.TASKS)
                {
                    if (map != MapType.MIRA_HQ)
                        comsSabotaged = ShipStatus.Instance.Systems[SystemTypes.Comms].TryCast<HudOverrideSystemType>().IsActive;
                    else
                        comsSabotaged = ShipStatus.Instance.Systems[SystemTypes.Comms].TryCast<HqHudSystemType>().ActiveConsoles.Count < 2;
                    currentCamera = GetCamera(map, localPlayer);
                    List<int> doors = new();
                    for (int i = 0; i < ShipStatus.Instance.AllDoors.Count; i++)
                    {
                        if (!ShipStatus.Instance.AllDoors[i].Open)
                            doors.Add(i);
                    }
                    closedDoors = doors.ToArray();
                }

                lightRadius = localPlayer.Object.myLight.LightRadius;
                lightRadiusChanged = lightRadius != _lastLightRadius;
                _lastLightRadius = lightRadius;
            }
        }

        private static GameState GetGameState()
        {
            switch (AmongUsClient.Instance.GameState)
            {
                case InnerNetClient.GameStates.NotJoined:
                    return GameState.MENU;
                case InnerNetClient.GameStates.Joined:
                case InnerNetClient.GameStates.Ended:
                    return GameState.LOBBY;
                case InnerNetClient.GameStates.Started:
                    BetterCrewLink.Logger.LogInfo("Meeting hud: " + MeetingHud.Instance);
                    if (MeetingHud.Instance && MeetingHud.Instance.CurrentState != MeetingHud.VoteStates.Animating)
                        return GameState.DISCUSSION;
                    else
                        return GameState.TASKS;
                default:
                    return GameState.UNKNOWN;
            }
        }

        // TODO: Make this return an array of mods
        private static string GetMod()
        {
            var mods = IL2CPPChainloader.Instance.Plugins;
            foreach (var item in mods)
            {
                string modName = item.Value.Metadata.GUID;
                switch (modName)
                {
                    case "com.slushiegoose.townofus":
                        return ModTypes.TOWN_OF_US;
                    case "me.eisbison.theotherroles":
                        return ModTypes.THE_OTHER_ROLES;
                    case "me.allul.lasmonjas":
                        return ModTypes.LAS_MONJAS;
                    case "com.emptybottle.townofhost":
                        return ModTypes.TOWN_OF_HOSTS;
                }
            }
            return ModTypes.NONE;
        }

        private static Player GetPlayer(GameData.PlayerInfo player)
        {
            bool local = player.Object.OwnerId == AmongUsClient.Instance.ClientId;
            return new Player
            {
                id = player.PlayerId,
                clientId = local ? AmongUsClient.Instance.ClientId : player.Object.OwnerId,
                name = player.PlayerName,
                nameHash = HashCode(player.PlayerName),
                colorId = player.Object.CurrentOutfit.ColorId,
                hatId = player.Object.CurrentOutfit.HatId,
                petId = -1, // player.Object.CurrentOutfit.PetId, // PetId is a string? // TODO
                skinId = player.Object.CurrentOutfit.SkinId,
                visorId = player.Object.CurrentOutfit.VisorId,
                disconnected = player.Disconnected,
                isImpostor = player.Role.TeamType == RoleTeamTypes.Impostor,
                isDead = player.IsDead,
                isLocal = local,
                shiftedColor = (player.Object.CurrentOutfitType == PlayerOutfitType.Shapeshifted) ? player.Outfits[PlayerOutfitType.Shapeshifted].ColorId : -1,
                // bugged // TODO
                x = player.Object.MyPhysics.transform.position.x,
                y = player.Object.MyPhysics.transform.position.y,
                inVent = player.Object.inVent,
                isDummy = player.Object.isDummy
            };
        }

        private static CameraLocation GetCamera(MapType map, GameData.PlayerInfo localPlayer)
        {
            BetterCrewLink.Logger.LogInfo("Getting cams for map: " + map);
            if (map == MapType.POLUS || map == MapType.AIRSHIP)
            {
                PlanetSurveillanceMinigame cams = Minigame.Instance.TryCast<PlanetSurveillanceMinigame>();
                if (cams && cams.currentCamera >= 0 && cams.currentCamera <= 5 && cams.survCameras.Count == 6)
                    return (CameraLocation)cams.currentCamera;
            }
            else if (map == MapType.THE_SKELD)
            {
                int roomCount = Minigame.Instance.TryCast<SurveillanceMinigame>().FilteredRooms.Length;
                if (roomCount == 4)
                {
                    // Why does this dist exist? // TODO
                    double dist = Math.Sqrt(Math.Pow(localPlayer.Object.MyPhysics.transform.position.x + 12.9364d, 2) + Math.Pow(localPlayer.Object.MyPhysics.transform.position.y + 2.7928, 2));
                    if (dist < 0.6)
                        return CameraLocation.Skeld;
                }
            }
            else if (map == MapType.SUBMERGED)
            {
                BetterCrewLink.Logger.LogInfo("GETTING SUBMERGED CAM");
                int cam = SubmergedComptability.getCamera();
                BetterCrewLink.Logger.LogInfo("GOT SUBMERGED CAM: " + cam);
                return (CameraLocation)cam;
            }
            return CameraLocation.NONE;
        }

        private static int HashCode(string s)
        {
            int h = 0;
            for (int i = 0; i < s.Length; i++)
                h = ((31 * h) + s[i]) | 0;
            return h;
        } 
    }  
}