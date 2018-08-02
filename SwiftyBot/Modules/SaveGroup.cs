using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SwiftyBot.Modules
{
    class SaveGroup
    {

        static ArrayList saveObjs = new ArrayList();

        public SaveGroup(ArrayList save)
        {
            store(save);
        }

        public static void store(ArrayList variable)
        {
            saveObjs = variable;
        }

        public static object get()
        {
            return saveObjs;
        }

    }
}
