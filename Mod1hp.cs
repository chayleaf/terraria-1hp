using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Mod1hp
{
	public class Mod1hpPlayer : ModPlayer
	{
		// the game use statLifeMax and ConsumedLifeCrystals/fruit interchangably when saving/loading
		// to prevent that, we do it ourselves
		public override void SaveData(TagCompound tag) {
			tag.Set("ConsumedLifeCrystals", Player.ConsumedLifeCrystals);
			tag.Set("ConsumedLifeFruit", Player.ConsumedLifeFruit);
			tag.Set("statLifeMax", Player.statLifeMax);
			tag.Set("statLifeMax2", Player.statLifeMax2);
		}
		public override void LoadData(TagCompound tag) {
			try {
				Player.ConsumedLifeCrystals = tag.GetInt("ConsumedLifeCrystals");
				Player.ConsumedLifeFruit = tag.GetInt("ConsumedLifeFruit");
				Player.statLifeMax = tag.GetInt("statLifeMax");
				Player.statLifeMax2 = tag.GetInt("statLifeMax");
			} catch { }
		}
	}
	public class Mod1hp : Mod
	{
		private static void HookPlayerStatsSnapshot(ILContext il) {
			// GameContent.UI.PlayerStatsSnapshot:
			// ...
			// int num = 20f;
			// ...
			// int num2 = player.statLifeMax / 20;
			// ...
			// numLifeHearts = ... / num;
			try {
				var c = new ILCursor(il);
				c.GotoNext(i => i.MatchLdcR4(20f));
				c.Emit(OpCodes.Ldc_R4, 1f);
				c.GotoNext();
				c.Remove();
				c.GotoNext(i => i.MatchLdcI4(20));
				c.Emit(OpCodes.Ldc_I4_1);
				c.GotoNext();
				c.Remove();
			}
			catch (Exception) {
				MonoModHooks.DumpIL(ModContent.GetInstance<Mod1hp>(), il);
				throw;
			}
		}
		private static void HookSpawn(ILContext il) {
			// (min hp given on spawn)
			// statLife = 100;
			try {
				var c = new ILCursor(il);
				c.GotoNext(i => i.MatchLdfld(typeof(Terraria.Player).GetField(nameof(Terraria.Player.statLife))));
				c.GotoNext(i => i.MatchLdcI4(100));
				c.Emit(OpCodes.Ldc_I4_1);
				c.GotoNext();
				c.Remove();
			}
			catch (Exception) {
				MonoModHooks.DumpIL(ModContent.GetInstance<Mod1hp>(), il);
				throw;
			}
		}
		private static void DetourModifyMaxStats(Action<Player> act, Player player) {
			act(player);
			player.statLifeMax = 1;
		}
		private static void DetourResetMaxStats(Action<Player> act, Player player) {
			act(player);
			player.statLifeMax = 1;
		}
		private static void HookUseTeleportRod(ILContext il) {
			// RoD: divide max health by 1, not 7 (so it deals damage when you have <7 max hp)
			try {
				var c = new ILCursor(il);
				c.GotoNext(i => i.MatchLdcI4(7));
				c.Emit(OpCodes.Ldc_I4_1);
				c.GotoNext();
				c.Remove();
			}
			catch (Exception) {
				MonoModHooks.DumpIL(ModContent.GetInstance<Mod1hp>(), il);
				throw;
			}
		}
		private static void HookUseHealthMaxIncreasingItem(ILContext il) {
			// just return
			try {
				var c = new ILCursor(il);
				c.Emit(OpCodes.Ret);
			}
			catch (Exception) {
				MonoModHooks.DumpIL(ModContent.GetInstance<Mod1hp>(), il);
				throw;
			}
		}
		Hook[] detours;
		public override void Load() {
			Terraria.GameContent.UI.ResourceSets.IL_PlayerStatsSnapshot.ctor += HookPlayerStatsSnapshot;
			IL_Player.ItemCheck_UseTeleportRod += HookUseTeleportRod;
			IL_Player.UseHealthMaxIncreasingItem += HookUseHealthMaxIncreasingItem;
			IL_Player.Spawn += HookSpawn;
			/*Type playerLoaderClass = typeof(Terraria.ModLoader.PlayerLoader); // Type.GetType("Terraria.ModLoader.PlayerLoader");
			MethodInfo modifyMaxStats = playerLoaderClass.GetMethod("ModifyMaxStats", BindingFlags.Public | BindingFlags.Static);
			MethodInfo resetMaxStats = playerLoaderClass.GetMethod("ResetMaxStatsToVanilla", BindingFlags.Public | BindingFlags.Static);*/
			MethodInfo modifyMaxStats = typeof(PlayerLoader).GetMethod(nameof(PlayerLoader.ModifyMaxStats), BindingFlags.Public | BindingFlags.Static);
			MethodInfo resetMaxStats = typeof(PlayerLoader).GetMethod(nameof(PlayerLoader.ResetMaxStatsToVanilla), BindingFlags.Public | BindingFlags.Static);
			detours = new Hook[] {
				new Hook(modifyMaxStats, (Action<Action<Player>, Player>)DetourModifyMaxStats),
				new Hook(resetMaxStats, (Action<Action<Player>, Player>)DetourResetMaxStats),
			};
		}
		public override void Unload() {
			if (detours != null) {
				for (int i = 0; i < detours.Length; ++i)
					detours[i].Dispose();
				detours = null;
			}
		}
	}
}
