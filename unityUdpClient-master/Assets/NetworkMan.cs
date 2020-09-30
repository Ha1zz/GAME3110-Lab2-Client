using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Security.Cryptography.X509Certificates;
using System.CodeDom;
using System.Collections.Specialized;
using System.Security.Cryptography;

public class NetworkMan : MonoBehaviour
{
    public GameObject cube;
    public List<GameObject> cubeList;
    public List<string> droppedPlayers;

    public UdpClient udp;
    // Start is called before the first frame update
    void Start()
    {

        cubeList = new List<GameObject>();

        cloneTag = new GameObject();

        udp = new UdpClient();

        udp.Connect("3.91.228.18", 12345);

        //udp.Connect("localhost", 12345);

        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");

        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);
    }

    void OnDestroy() {
        udp.Dispose();
    }


    public enum commands {
        NEW_CLIENT,
        UPDATE,
        EXIT,
        LIST,
        NONE
    };

    [Serializable]
    public class Message {
        public commands cmd;
    }

    [Serializable]
    public class Player {
        [Serializable]
        public struct receivedColor {
            public float R;
            public float G;
            public float B;
        }
        public string id;
        public receivedColor color;
    }

    [Serializable]
    public class NewPlayer {
        //
        public string id;
    }

    [Serializable]
    public class GameState {
        //
        public Message message;
        public List<Player> players;
    }

    public class NewGameState
    {
        public Message message;
        public NewPlayer player;
        //public NewGameState()
        //{
        //    player.id = "";
        //}
    }

    public class SuperDropGameState
    {
        public Message message;
        //public List<Player> exit_player;
        public Player player;
    }

    //public class DropGameState
    //{
    //    public Message message;
    //    public NewPlayer player;
    //}

    public class DropGameState
    {
        public Message message;
        public List<NewPlayer> player;
    }

    [Serializable]
    public class ListOfDroppedPlayers
    {
        public string[] droppedPlayers;
    }

    //public Player[] allPlayers;
    public List<Player> allPlayers;
    GameObject cloneTag;

    public List<Player> deletedPlayers;

    public NewGameState newGameState;
    //public DropGameState dropGameState;

    public GameState listGameState;
    public GameState dropGameState;
    public SuperDropGameState superDropGameState;
    private float cubePosition = -4.0f;

    public Message latestMessage;
    public GameState lastestGameState;

    void OnReceived(IAsyncResult result) {
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;

        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);

        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);

        //dropGameState = new DropGameState();

        dropGameState = new GameState();
        newGameState = new NewGameState();
        lastestGameState = new GameState();

        latestMessage = JsonUtility.FromJson<Message>(returnData);
        //Debug.Log("Got THIS: " + latestMessage.cmd.ToString() + " And THIS: " + returnData);
        try
        {
            switch (latestMessage.cmd) {
                case commands.NEW_CLIENT:
                    newGameState = JsonUtility.FromJson<NewGameState>(returnData);
                    //SpawnPlayers();
                    break;
                case commands.UPDATE:
                    lastestGameState = JsonUtility.FromJson<GameState>(returnData);
                    //UpdatePlayers();
                    break;
                case commands.EXIT:
                    //dropGameState = JsonUtility.FromJson<DropGameState>(returnData);
                    //ListOfDroppedPlayers latestDroppedPlayer = JsonUtility.FromJson<ListOfDroppedPlayers>(returnData);
                    //foreach (string player in latestDroppedPlayer.droppedPlayers)
                    //{
                    //    droppedPlayers.Add(player);
                    //}
                    dropGameState = JsonUtility.FromJson<GameState>(returnData);
                    foreach (Player player in dropGameState.players)
                    {
                        deletedPlayers.Add(player);
                    }
                    break;
                case commands.LIST:
                    listGameState = JsonUtility.FromJson<GameState>(returnData);
                    break;
            }
        }
        catch (Exception e) {
            Debug.Log(e.ToString());
        }

// schedule the next receive operation once reading is done:
    socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }   

    void SpawnPlayers() {
        bool isTheSame = false;
        Player newplayer = new Player();

        if (latestMessage.cmd == commands.NEW_CLIENT)
        {
            if (newGameState != null && newGameState.player != null)
            {
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    if (allPlayers[i].id == newGameState.player.id)
                    {
                        isTheSame = true;
                    }
                }
                if (isTheSame == false)
                {
                    if (allPlayers.Contains(newplayer) == false)
                    {
                        newplayer.id = newGameState.player.id;
                        newplayer.color.B = newplayer.color.G = newplayer.color.R = 0;
                        allPlayers.Add(newplayer);
                        cloneTag = (GameObject)Instantiate(cube, new Vector3(0.0f, cubePosition + cubeList.Count, 6.0f), Quaternion.identity);
                        cubeList.Add(cloneTag);
                        cloneTag.name = newplayer.id.ToString();
                        CubeScript script;
                        script = cube.GetComponent<CubeScript>();
                        script.cubeid = newplayer.id;
                    }
                }
            }
        }

        //if (listGameState.players.Count > 0)
        //{
        //    foreach (Player player in listGameState.players)
        //    {
        //        if (player.id.Equals(allPlayers[0].id.ToString()))
        //            continue;
        //        newplayer.id = player.id;
        //        newplayer.color.B = newplayer.color.G = newplayer.color.R = 0;
        //        allPlayers.Add(newplayer);
        //        cloneTag = (GameObject)Instantiate(cube, new Vector3(0.0f, cubePosition + cubeList.Count, 6.0f), Quaternion.identity);
        //        cubeList.Add(cloneTag);
        //        cloneTag.name = newplayer.id.ToString();
        //        CubeScript script;
        //        script = cube.GetComponent<CubeScript>();
        //        script.cubeid = newplayer.id;
        //    }
        //}

    }

    void UpdatePlayers(){
        
        if (latestMessage.cmd == commands.UPDATE)
        { 

            if (lastestGameState != null && lastestGameState.players != null)
            {
                Player newplayer = new Player();
                for (int i = 0; i < lastestGameState.players.Count; i++)
                {
                    for (int j = 0; j < allPlayers.Count; j++)
                    {
                        if (lastestGameState.players[i].id.Equals(allPlayers[j].id))
                        {
                            float colorR = lastestGameState.players[i].color.R;
                            float colorG = lastestGameState.players[i].color.G;
                            float colorB = lastestGameState.players[i].color.B;

                            //GameObject cubex = new GameObject();
                            //cubeList.Find(cubeList.)

                            //string nameX = lastestGameState.players[i].id.ToString();
                            GameObject cubeX = GameObject.Find(lastestGameState.players[i].id.ToString());

                            Renderer myRenderer = cubeX.GetComponent<Renderer>();
                            myRenderer.material.color = new Color(colorR, colorG, colorB);

                        }
                    }
                }


            }
        }
    }

    void DestroyPlayers(){
        if (deletedPlayers.Count > 0)
        {
            foreach (Player player in deletedPlayers)
            {
                //for (int i = 0; i < allPlayers.Count; i++)
                //{
                //    if (player.id.Equals(allPlayers[i].id.ToString()))
                //    {
                //        allPlayers.RemoveAt(i);
                //        break;
                //    }
                //}

                //Debug.Log("CHECKING: " + player.id.ToString());
                //Debug.Log("CHECK AGAIN: " + dropGameState.message.cmd);
                if (dropGameState.message.cmd == commands.NEW_CLIENT)
                {
                    for (int i = 0; i < allPlayers.Count; i++)
                    {
                        if (player.id.Equals(allPlayers[i].id.ToString()))
                        {
                            allPlayers.RemoveAt(i);
                            break;
                        }
                    }
                    GameObject delete = GameObject.Find(player.id.ToString());
                    Destroy(delete);
                    deletedPlayers.Clear();
                }
            }
        }


        //if (deletedPlayers.Count > 0)
        //{
        //    //Debug.Log("CHECK" + deletedPlayers.Count);
        //    for (int i = 0; i < deletedPlayers.Count; i++)
        //    {
        //        for (int j = 0; j < allPlayers.Count; j++)
        //        {
        //            if (deletedPlayers[i].id.Equals(allPlayers[j].id.ToString()))
        //            {
        //                Debug.Log("CHECK" + deletedPlayers[i].id.ToString());
        //                GameObject delete = GameObject.Find(allPlayers[j].id.ToString());
        //                Destroy(delete);
        //            }
        //            allPlayers.RemoveAt(j);
        //        }
        //    }
        //    deletedPlayers.Clear();
        //}


        //if (droppedPlayers.Count > 0)
        //{
        //    foreach (string playerID in droppedPlayers)
        //    {
        //        GameObject delete = GameObject.Find(playerID);
        //    }
        //    droppedPlayers.Clear();
        //}

        //bool isTheSame = false;
        //int isTheSameIndex = -1;
        //Debug.Log("I am GETTING DESTROYED 5");
        //if (dropGameState != null)
        //{
        //    for (int i = 0; i < allPlayers.Count; i++)
        //    {
        //        //for (int j = 0; j < dropGameState.player.Count; j++)
        //        //{
        //        if (allPlayers[i].id.Equals(dropGameState.player.id.ToString()) == true)
        //        {
        //            //Debug.Log("CHEHCKCK: " + dropGameState.player[j].id.ToString());
        //                //isTheSame = true;
        //                //isTheSameIndex = i;
        //                GameObject delete = GameObject.Find(dropGameState.player.id.ToString());
        //                if (isTheSameIndex >= 0)
        //                {
        //                    allPlayers.RemoveAt(isTheSameIndex);
        //                    Destroy(delete);
        //                }
        //        }
        //        //}
        //    }
        //}
        //if (isTheSame == true)
        //{
        //    //Debug.Log("I am GETTING DESTROYED" + isTheSame + dropGameState.player.id.ToString());
        //    CubeScript script1;
        //    script1 = cube.GetComponent<CubeScript>();
        //    for (int i = 0; i < allPlayers.Count; i++)
        //    {
        //        if (allPlayers[i].id.Equals(dropGameState.player.id) == true)
        //        {
        //            GameObject delete = GameObject.Find(dropGameState.player.id.ToString());
        //            if (isTheSameIndex >= 0)
        //            {
        //                allPlayers.RemoveAt(isTheSameIndex);
        //                Destroy(delete);
        //            }
        //        }
        //    }




        //    //if (dropGameState.player.id.Equals(script1.cubeid) == true)
        //    //{
        //    //    GameObject delete = GameObject.Find(dropGameState.player.id.ToString());
        //    //    if (isTheSameIndex >= 0)
        //    //    {
        //    //        allPlayers.RemoveAt(isTheSameIndex);
        //    //        Destroy(delete);
        //    //    }
        //    //    //    allPlayers.RemoveAt(isTheSameIndex);
        //    //    //GameObject delete = GameObject.Find(dropGameState.player.id.ToString());
        //    //    //Destroy(delete);
        //    //}
        //    //GameObject delete = GameObject.Find(dropGameState.player.id.ToString());
        //    //Destroy(delete);
        //    Debug.Log("CHECK: " + allPlayers.Count);
        //}

    }

    void HeartBeat(){
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }

    void Update(){
        SpawnPlayers();
        UpdatePlayers();
        DestroyPlayers();
    }
}
