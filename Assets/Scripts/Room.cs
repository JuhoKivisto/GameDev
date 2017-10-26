﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
    
        public Vector3 pos;
        public float width, height;
        public List<Room> adjList = new List<Room>();
        public bool visited = false;
        public Room(float x, float z, float width, float height)
        {
            pos = new Vector3(x, 0, z);
            this.width = width;
            this.height = height;
        }

        // check if value is between min and max
        public bool valueInRange(float value, float min, float max)
        {
            return (value >= min) && (value <= max);
        }

        // Rooms are colliding if one edge is inside the other room
        public bool isRoomColliding(Room room)
        {
            // if one room-edge is between the others x-values and
            bool xOverlap = valueInRange(pos.x, room.pos.x, room.pos.x + room.width) || valueInRange(room.pos.x, pos.x, pos.x + width);
            // if one room-edge is between the others y-values
            bool yOverlap = valueInRange(pos.z, room.pos.z, room.pos.z + room.height) || valueInRange(room.pos.z, pos.z, pos.z + height);

            return xOverlap && yOverlap;
        }
    
}