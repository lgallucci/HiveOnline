using HiveContracts;

namespace HiveOnline
{

    public abstract class GameEngine
    {
        public abstract bool Update(ref IBoard board);
    }
}
