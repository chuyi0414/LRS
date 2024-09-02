using System;
using System.Collections.Generic;
using System.Text;


public static class PlayerManager
{
    public static Dictionary<uint,Player> players = new Dictionary<uint,Player>();

    public static Player GetPlayer(uint guid)
    {
        if(players.ContainsKey(guid))
        {
            return players[guid];
        }
        else
        {
            return null;
        }
        
    }

    public static void AddPlayer(uint guid,Player player)
    {
        players.Add(guid, player);
    }

    public static void RemovePlayer(uint guid)
    {
        players.Remove(guid);
    }
}