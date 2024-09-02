using System;
using System.Collections.Generic;
using System.Text;


public static class RoomManager
{
    private static int maxId = 0;

    private static Room room = new Room();
    
    private static readonly object roomLock = new object(); // 用于线程安全

    public static Room GetRoom()
    {
        room.lockStepManager.room = room;
        return room;
    }
}