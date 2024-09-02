﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class MyGuid
{
    private static uint id = 1;
    private static HashSet<uint> ids = new HashSet<uint>();

    /// <summary>
    /// 分配Guid
    /// </summary>
    /// <returns></returns>
    public static uint GetGuid()
    {
        if(id==uint.MaxValue)
            id = 1;
        uint res = id++;
        while (ids.Contains(res))
        {
            if (id == uint.MaxValue)
                id = 1;
            id++;
        }
        ids.Add(res);
        return res;
    }
}
