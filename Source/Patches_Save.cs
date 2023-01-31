using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SurgeryMeds;
[HarmonyPatch]
public static class Patches_Save {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn_PlayerSettings), nameof(Pawn_PlayerSettings.ExposeData))]
    public static void ExposeDataExtra(Pawn ___pawn)
        => SurgeryCareCategory.For(___pawn).ExposeDataExtra();
}
