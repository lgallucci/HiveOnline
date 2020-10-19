using HiveContracts;
using HiveOnline.GameAssets;

namespace HiveOnline
{

    public abstract class GameEngine
    {
        public abstract bool Update(ref Board board);
    }
}
