﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
	//Creates CaveGenerators

	public Transform prefab, caveGenerator, wall, doorParent, plane, roof, Portal, Scrap;
    public PlayerControllerMapTut player;
    public Door door;
    public EnemyBehaviour enemy;

    // for map generator
    private List<Room> graph = new List<Room>();
    private List<Room> tree = new List<Room>();

    public float minWidth = 60, minHeight = 60, maxWidth = 120, maxHeight = 120, space = 15;
    public int minRooms = 6, maxRooms = 10, doorOffset = 0, num_rooms = 7, iterations = 60;

    // Use this for initialization
    void Start () {
        
        makeNewMap();

        graph[0].visited = true; // initialize visited value from the start
        player.currentRoom = graph[0];
        player.spawn(player.currentRoom.width / 2, player.currentRoom.height / 2);

        //slime.setCurrentRoom(graph[0]);
        //Instantiate(slime, slime.getRoom().getRandomRoomPosition(), Quaternion.identity);
        for (int i = 1; i < graph.Count; i++)
        {
            for(int j = 0; j < Random.Range(0, 3); j++)
            {
                EnemyBehaviour slime = Instantiate(enemy, graph[i].getRandomRoomPosition(), Quaternion.identity);
                slime.setCurrentRoom(graph[i]);
            }
        }

        prims();
        // each connection now exists in each rooms adjList(i.e. if you want to know which room is connected to which)
        //  simply check the adjList
        print(graph.Count + " rooms generated");

        //access script before initiation
        // GameObject gameObject = caveGenerator.GetComponent<CellularAutomata>().gameObject;
        //CellularAutomata script = gameObject.GetComponent<CellularAutomata> ();
        //caves
        float roomspace = (space / 5);
        foreach (Room r in graph)
        {
            // caveGenerator.GetComponent<CellularAutomata>().width = Mathf.RoundToInt(r.width);
            // caveGenerator.GetComponent<CellularAutomata>().height = Mathf.RoundToInt(r.height);
            // caveGenerator.GetComponent<CellularAutomata>().randomFillPercent = 30;

            
            // make cave-room at room position
            //Instantiate(caveGenerator, new Vector3(r.pos.x, 0, r.pos.z), Quaternion.identity);

            // create wall to each room
            // lower wall
            wall.GetComponent<RoomWall>().makeSize(r.width + space/2, roomspace);
            Instantiate(wall, new Vector3(r.pos.x + r.width / 2, 0, r.pos.z - roomspace / 2), Quaternion.identity);

            // upper Wall
            wall.GetComponent<RoomWall>().makeSize(r.width + space/2, roomspace);
            Instantiate(wall, new Vector3(r.pos.x + r.width / 2, 0, r.pos.z + r.height + roomspace / 2), Quaternion.identity);

            // left wall
            wall.GetComponent<RoomWall>().makeSize(roomspace, r.height + space/2);
            Instantiate(wall, new Vector3(r.pos.x - roomspace / 2, 0 , r.pos.z + r.height / 2), Quaternion.identity);

            // right wall
            wall.GetComponent<RoomWall>().makeSize(roomspace, r.height + space/2);
            Instantiate(wall, new Vector3(r.pos.x + r.width + roomspace / 2, 0, r.pos.z + r.height / 2), Quaternion.identity);
            
            // create panel showing the room area
            plane.transform.localScale = new Vector3(r.width / 10, 0.1f, r.height / 10);
            Instantiate(plane, new Vector3(r.pos.x + r.width / 2, -4.9f, r.pos.z + r.height / 2), Quaternion.identity);
            roof.transform.localScale = new Vector3(r.width / 10 + 2, 20f, r.height / 10 + 2);
            Instantiate(roof, new Vector3(r.pos.x + r.width / 2, 6f, r.pos.z + r.height / 2), Quaternion.identity);

        }

        Door door1, door2;
        // this is too linear
       /* for(int i = 1; i < tree.Count; i++)
        {
            door1 = Instantiate(door, graph[i].getDoorPosition(graph[i - 1].getRoomCenter(), doorOffset), Quaternion.identity) as Door;
            door1.room = graph[i];
            door2 = Instantiate(door, graph[i - 1].getDoorPosition(graph[i].getRoomCenter(), doorOffset), Quaternion.identity) as Door;
            door2.room = graph[i - 1];

            door1.connectToPair(door2);
            
        }*/

        // this is buggy, as some rooms will be given a connection when distance is too big i.e. big gaps between rooms may occur 
        foreach(Room r in graph)
        {
            foreach(Room n in r.adjList)
            {
                door1 = Instantiate(door, r.getDoorPosition(n.getRoomCenter(), doorOffset), Quaternion.identity) as Door;
                door1.room = r;
                door2 = Instantiate(door, n.getDoorPosition(r.getRoomCenter(), doorOffset), Quaternion.identity) as Door;
                door2.room = n;

                door1.connectToPair(door2);
            }
        }
        PrepareForDfs(graph);

        int itteration = 0; // depth-first index for each room
        DFS(graph, graph[0], itteration);

        int biggestDFI = 0;
        foreach(Room r in graph)
        {
            if (biggestDFI < r.DFI);
        }

        // find room to spawn portal
        Room roomContainingPortal = null;
        foreach (Room r in graph)
        {
            if ((biggestDFI - 2) <= r.DFI)
            {
                roomContainingPortal = r;
            }
        }
        Instantiate(Portal, roomContainingPortal.getRandomRoomPosition(), Quaternion.identity);

        // spawn scrap in rooms

        foreach(Room r in graph)
        {
            for(int i = 0; i < Random.Range(0, 10); i++)
            {
                Instantiate(Scrap, r.getRandomRoomPosition(), Quaternion.identity);
            }
        }

        
    }
// ---- end of start method
    private void Update()
    {
        drawRoomConnectors();
    }

    // Make initial spawn room
    private void makeSpawnRoom()
    {
        Room spawnRoom = new Room(0, 0, minWidth + 20, minHeight + 20);
        graph.Add(spawnRoom);
    }

    // make rooms in succession
    private void makeNewMap()
    {
        makeSpawnRoom();
        num_rooms = Random.Range(minRooms, maxRooms + 1);
        while(graph.Count < num_rooms)
        {
            for (int i = 0; i < graph.Count; i++)
            {
                if (graph.Count > 2 && i == graph.Count - 1) break;
                makeNewRoom(graph[i]);

                if (graph.Count == num_rooms) break;
            }

        }
    }

    // make a new room adjacent to a given room
    private void makeNewRoom(Room parent)
    {
        bool collision = true;
        Room newRoom = new Room(0, 0, 0, 0);
        int direction;
        while (collision)
        {
            float width = Random.Range(minWidth, maxWidth + 1);
            float height = Random.Range(minHeight, maxHeight + 1);
            float x = 0;
            float z = 0;
            direction = Random.Range(1, 5);
            // add 1 at the end of the longer equations or else every room will always collide

            if (direction == 1) // UP
            {
                x = parent.pos.x;
                z = parent.pos.z + parent.height + space;
            }
            else if (direction == 2) // RIGHT
            {
                x = parent.pos.x + parent.width + space;
                z = parent.pos.z;
            }
            else if (direction == 3) // DOWN
            {
                x = parent.pos.x;
                z = parent.pos.z - height - space;
            }
            else if (direction == 4) // LEFT
            {
                x = parent.pos.x - width - space;
                z = parent.pos.z;
            }

            newRoom = new Room(x, z, width, height);
            
            foreach (Room room in graph)
            {
                if (room.isRoomColliding(newRoom))
                {
                    collision = true;
                    break;
                }
                else collision = false;
            }
        }
        //newRoom = new Room(parent.pos.x, parent.pos.z - parent.height - space, Random.Range(minWidth, maxWidth + 1), Random.Range(minHeight, maxHeight + 1));
        graph.Add(newRoom);
    }

    // Connect the rooms with prims algorithm
    private void prims()
    {
        Room roomToConnect = graph[0];
        Room parentRoom = graph[0];
        float temp;
        tree.Add(graph[0]);
        //for(int x = 0; x < 10; x++)
        while (graph.Count > tree.Count)
        {
            float shortest = Mathf.Infinity;

            foreach(Room n in tree)
            {

                for(int i = 0; i < graph.Count; i++)
                {
                    if (!graph[i].visited)
                    {
                        temp = (graph[i].getRoomCenter() - n.getRoomCenter()).magnitude;
                        if (temp < shortest)
                        {
                            shortest = temp;
                            roomToConnect = graph[i];
                            parentRoom = n;
                        }
                    }
                }
            }
            roomToConnect.visited = true;
            parentRoom.adjList.Add(roomToConnect);
            roomToConnect.adjList.Add(parentRoom);
            tree.Add(roomToConnect);
        }
    }

    // prepare for dfs algorithm by preparing the list
    private void PrepareForDfs(List<Room> graph)
    {
        foreach(Room r in graph)
        {
            r.visited = false;
        }
    }

    
    //depth-first traversal to find where to place portal to next level
    private void DFS(List<Room> graph, Room room, int itteration)
    {
        room.visited = true;
        room.DFI = itteration;
        itteration += 1;
        foreach(Room r in room.adjList)
        {
            if (!r.visited) DFS(graph, r, itteration);
        }
    }

    // Show how the minimum spanning tree is built up
        // buggy connections
    void drawRoomConnectors()
    {
        
        foreach( Room r in graph)
        {
            foreach(Room n in r.adjList)
            {
                Debug.DrawLine(r.getRoomCenter(), n.getRoomCenter(), Color.red);
            }
        }

        // linear connections
        /*for (int i = 1; i < graph.Count; i++)
        {
            Debug.DrawLine(graph[i].getRoomCenter(), graph[i - 1].getRoomCenter(), Color.red);
        }*/
    }
}
