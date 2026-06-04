using Sxer.Plugin.SaveSystem;
using System;
using static TestSaveLine;

public class TestSaveLine : SaveLine<TestDddData>
{
    

    [Serializable]
    public class TestDddData {
        public string name = "";
        public int index = 0;
    }
}
