using System;

namespace Server
{
    public class Item
    {
        public int Id { get; set; }
        public string Command { get; set; }
        public int Result { get; set; }
        public DateTime DateOfTime { get; set; }

        public override string ToString()
        {
            return $"Command={Command}, Result={Result}, DateOfTime={DateOfTime}";
        }
    }
}
