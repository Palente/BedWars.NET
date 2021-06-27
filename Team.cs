using OpenAPI.Player;

namespace BedWars.NET
{
    public class Team
    {
        public TeamColors Color;
        public OpenPlayer Player;
        public TeamStatus Status = TeamStatus.ALIVE;
        public int[] BedPosition;


        public string GetTeamName() => Color.ToString();
    }
}
