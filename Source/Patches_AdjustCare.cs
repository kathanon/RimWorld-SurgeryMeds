using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SurgeryMeds;
[HarmonyPatch]
public static class Patches_AdjustCare {
    private static RecipeDef recipe = null;
    private static string namedArgReplacement = null;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorkGiver_DoBill), nameof(WorkGiver_DoBill.GetMedicalCareCategory))]
    public static void GetMedicalCareCategory(Thing billGiver, ref MedicalCareCategory __result) {
        if (recipe != null && billGiver is Pawn pawn) {
            var state = SurgeryCareCategory.For(pawn);
            if (state != null) {
                __result = state.CategoryFor(recipe);
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NamedArgumentUtility), nameof(NamedArgumentUtility.Named))]
    public static void NamedArgument(ref object arg, string label) {
        if (namedArgReplacement != null && label == Strings.NamedLabel) {
            arg = namedArgReplacement;
            namedArgReplacement = null;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthCardUtility), "CanDoRecipeWithMedicineRestriction")]
    public static void CanDoRecipeWithMedicineRestriction_Pre(RecipeDef recipe) 
        => Patches_AdjustCare.recipe = recipe;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HealthCardUtility), "CanDoRecipeWithMedicineRestriction")]
    public static void CanDoRecipeWithMedicineRestriction_Post(IBillGiver giver, bool __result) {
        if (!__result && giver is Pawn pawn) {
            var value = SurgeryCareCategory.For(pawn).CategoryFor(recipe);
            if (pawn.CareCategory() != value) {
                namedArgReplacement = $"{Strings.ForSurgery} {value.GetLabel()}";
            }
        }
        recipe = null;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
    public static void TryFindBestBillIngredients_Pre(Bill bill) 
        => recipe = bill.recipe;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorkGiver_DoBill), "StartOrResumeBillJob")]
    public static void StartOrResumeBillJob_Post() 
        => recipe = null;
}
