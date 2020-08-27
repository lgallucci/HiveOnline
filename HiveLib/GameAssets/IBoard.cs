using HiveContracts;
using System.Collections.Generic;

namespace HiveOnline.GameAssets
{
    public interface IBoard
    {
        List<ITile> Tiles { get; set; }
        Layout Layout { get; set; }
    }
}