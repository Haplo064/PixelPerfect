

using System;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace PixelPerfect
{
  public unsafe class UIStateHelper
  {
    public static bool IsWeaponUnsheathed()
    {
      return Convert.ToBoolean(UIState.Instance()->WeaponState.IsUnsheathed);
    }
  }
}
