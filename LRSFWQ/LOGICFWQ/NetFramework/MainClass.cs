using System;
using System.Collections.Generic;
using System.Text;


public class MainClass
{
    public static void Main()
    {
        NetManager.Connect("127.0.0.1", 9000);

        while (true) { }
    }
}