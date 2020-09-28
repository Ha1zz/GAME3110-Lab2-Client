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

    public UdpClient udp;
    // Start is called before the first frame update
    void Start()
    {

        cubeList = new List<GameObject>();

        cloneTag = new GameObject();

        udp = new UdpClient();

        //udp.Connect("3.91.228.18", 12345);

        udp.Connect("localhost", 12345);

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

    public class DropGameState
    {
        public Message message;
        public NewPlayer player;
    }

    //public Player[] allPlayers;
    public List<Player> allPlayers;
    GameObject cloneTag;

    public NewGameState newGameState;
    public DropGameState dropGameState;
    public SuperDropGameState superDropGameState;
    private float ex_message = 0;

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

        dropGameState = new DropGameState();
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
                    dropGameState = JsonUtility.FromJson<DropGameState>(returnData);
                    break;
                    //superDropGameState = JsonUtility.FromJson<SuperDropGameState>(returnData);
                    //Debug.Log("Error: " + dropGameState.player.id + "And Error: " + dropGameState.message.cmd.ToString());
                    //Debug.Log("Error 3: " + superDropGameState.player.id + "And Error 3: " + superDropGameState.message.cmd.ToString());
                    //Debug.Log("ERROR MESSAGE: " + latestMessage.cmd.ToString());
                    //DestroyPlayers();
                    //break;
                //default:
                //    Debug.Log("ERROR");
                //    break;
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
                        //Player newplayer = new Player();
                        //Debug.Log("XXX" + newGameState.player.id);
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
                    //newplayer.id = newGameState.player.id;
                    //newplayer.color.B = newplayer.color.G = newplayer.color.R = 0;
                    //allPlayers.Add(newplayer);
                    //cubeList.Add((GameObject)Instantiate(cube, new Vector3(0.0f, cubePosition + cubeList.Count, 6.0f), Quaternion.identity));
                    //CubeScript script;
                    //script = cube.GetComponent<CubeScript>();
                    //script.cubeid = newplayer.id;
                    //Debug.Log("CHECK ME: " + allPlayers.Count);
                }
            }
        }
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

                            //cubeX.GetComponent<Renderer>().material.color = new Color(colorR, colorG, colorB);
                            //CubeScript script;
                            //script = cube.GetComponent<CubeScript>();
                            //script.cubeid = lastestGameState.players[i].id;
                        }
                    }
                }


            }
        }
    }

    void DestroyPlayers(){
        //if (latestMessage.cmd == commands.EXIT)
        if (latestMessage.cmd == commands.EXIT)
        {
            bool isTheSame = false;
            int isTheSameIndex = -1;
            Debug.Log("I am GETTING DESTROYED 5");
            if (dropGameState != null)
            {
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    if (allPlayers[i].id.Equals(dropGameState.player.id) == true)
                    {
                        isTheSame = true;
                        isTheSameIndex = i;
                    }
                }
            }
            if (isTheSame == true)
            {
                Debug.Log("I am GETTING DESTROYED" + isTheSame + dropGameState.player.id.ToString());
                CubeScript script1;
                script1 = cube.GetComponent<CubeScript>();
                //Debug.Log("I am GETTING DESTROYED THERE" + script1.cubeid);
                if (dropGameState.player.id.Equals(script1.cubeid) == true)
                {
                    //script1.SelfDestruct();
                    //(GameObject.FindWithTag("cube"));
                    if (isTheSameIndex >= 0)
                        allPlayers.RemoveAt(isTheSameIndex);
                }
                GameObject delete = GameObject.Find(dropGameState.player.id.ToString());
                Destroy(delete);
            }
        }
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
