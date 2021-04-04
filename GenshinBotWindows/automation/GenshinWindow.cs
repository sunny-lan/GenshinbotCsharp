﻿using genshinbot.automation;
using System;

namespace genshinbot
{
    public class GenshinWindow : WindowAutomator
    {
        private GenshinWindow(string TITLE, string CLASS) : base(TITLE, CLASS)
        {
            throw new NotSupportedException();
        }

        public static IWindowAutomator FindExisting()
        {
            return new WindowAutomator("Genshin Impact", "UnityWndClass");
        }

    }
}