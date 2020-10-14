﻿namespace TSMapEditor.Models
{
    public class TeamType
    {
        public TeamType(string iniName)
        {
            ININame = iniName;
        }

        public string ININame { get; }

        public int Max { get; set; }
        public bool Full { get; set; }
        public string Name { get; set; }
        public int Group { get; set; } = -1;
        public House House { get; set; }
        public Script Script { get; set; }
        public TaskForce TaskForce { get; set; }
        public bool Whiner { get; set; }
        public bool Droppod { get; set; }
        public bool Suicide { get; set; }
        public bool Loadable { get; set; }
        public bool Prebuild { get; set; }
        public int Priority { get; set; }
        public byte Waypoint { get; set; }
        public bool Annoyance { get; set; }
        public bool IonImmune { get; set; }
        public bool Recruiter { get; set; }
        public bool Reinforce { get; set; }
        public int TechLevel { get; set; }
        public bool Aggressive { get; set; }
        public bool Autocreate { get; set; }
        public bool GuardSlower { get; set; }
        public bool OnTransOnly { get; set; }
        public bool AvoidThreats { get; set; }
        public bool LooseRecruit { get; set; }
        public int VeteranLevel { get; set; }
        public bool IsBaseDefense { get; set; }
        public bool OnlyTargetHouseEnemy { get; set; }
        public bool TransportsReturnOnUnload { get; set; }
        public bool AreTeamMembersRecruitable { get; set; }
    }
}