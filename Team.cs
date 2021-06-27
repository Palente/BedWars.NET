using MiNET.Utils.Vectors;
using OpenAPI.Player;

namespace BedWars.NET
{
    public class Team
    {
        public TeamColors Color;
        public OpenPlayer Player;
        public TeamStatus Status = TeamStatus.ALIVE;
        public BlockCoordinates[] BedPosition;
        public PlayerLocation SpawnPosition;
        public string GetTeamName() => Color.ToString();


        public override string ToString()
        {
            return Color.ToString();
        }
    }
}