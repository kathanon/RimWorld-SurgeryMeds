using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SurgeryMeds;

using DropdownOptions = IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>>;

[HarmonyPatch]
public static class Patches_GUI {
    private const float IconMargin = 2f;

    private static Pawn currentPawn = null;
    private static Pawn lastPayloadPawn = null;
    private static bool addTip = false;
    private static MedicalCareCategory? lastSurgeryCat;
    private static MedicalCareCategory  lastCareCat;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthCardUtility), "DrawOverviewTab")]
    public static void HealthCardOverviewTab_Pre(Pawn pawn) 
        => currentPawn = pawn;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HealthCardUtility), "DrawOverviewTab")]
    public static void HealthCardOverviewTab_Post() 
        => currentPawn = null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MedicalCareUtility), nameof(MedicalCareUtility.MedicalCareSetter))]
    public static void MedicalCareSetter_Pre() {
        addTip = currentPawn != null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MedicalCareUtility), nameof(MedicalCareUtility.MedicalCareSetter))]
    public static void MedicalCareSetter_Post(Rect rect) {
        if (currentPawn == null) return;
        addTip = false;
        var r = rect.LeftPartPixels(rect.width / 5f);
        var state = SurgeryCareCategory.For(currentPawn);

        if (state.Active) {
            state.DoFrame(r, r.width);
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 1) {
            for (MedicalCareCategory cat = MedicalCareCategory.NoCare; cat <= MedicalCareCategory.Best; cat++) {
                if (Mouse.IsOver(r)) {
                    state.Toggle(cat);
                    break;
                } else {
                    r.x += r.width;
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PawnColumnWorker_MedicalCare), nameof(PawnColumnWorker_MedicalCare.Compare))]
    public static void MedicalCare_Column_Compare(Pawn a, Pawn b, ref int __result) {
        if (__result != 0) return;
        __result = ValueFor(a) - ValueFor(b);

        static int ValueFor(Pawn a) 
            => ((int?) SurgeryCareCategory.For(a).value) ?? -1;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MedicalCareUtility), nameof(MedicalCareUtility.MedicalCareSelectButton))]
    public static void MedicalCareSelectButton_Pre(Pawn pawn) {
        lastSurgeryCat = SurgeryCareCategory.For(pawn).value;
        lastCareCat = pawn.CareCategory();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MedicalCareUtility), nameof(MedicalCareUtility.MedicalCareSelectButton))]
    public static void MedicalCareSelectButton_Post(Rect rect, Pawn pawn) {
        var state = SurgeryCareCategory.For(pawn);

        if (state.Changing) {
            var r = new Rect(rect.center, rect.size / 2 - IconMargin * Vector2.one);
            r.y -= r.height;
            state.DoIcon(r);
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 1) {
            if (Mouse.IsOver(rect)) {
                state.DoMenu();
            }
        }

        TooltipHandler.TipRegion(rect, state.ButtonTip, 654198981);

        if (lastSurgeryCat != state.value && lastCareCat == pawn.CareCategory()) {
            SoundDefOf.Click.PlayOneShotOnCamera();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MedicalCareUtility), "MedicalCareSelectButton_GetMedicalCare")]
    public static void MedicalCareSelectButton_GetMedicalCare(Pawn pawn)
        => lastPayloadPawn = pawn;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MedicalCareUtility), "MedicalCareSelectButton_GenerateMenu")]
    public static DropdownOptions MedicalCareSelectButton_GenerateMenu(DropdownOptions __result, Pawn p) 
        => (Event.current.type != EventType.MouseUp) ? AddPiggyback(p, __result) : __result;

    public static DropdownOptions AddPiggyback(Pawn pawn, DropdownOptions options) {
        var state = SurgeryCareCategory.For(pawn);
        var copyFrom = SurgeryCareCategory.For(lastPayloadPawn);
        foreach (var item in options) {
            if (copyFrom != null && (!item.option?.Disabled ?? false)) {
                var old = item.option.action;
                item.option.action = delegate {
                    old();
                    state.value = copyFrom.value;
                };
            }
            yield return item;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TooltipHandler), nameof(TooltipHandler.TipRegion), 
        typeof(Rect), typeof(Func<string>), typeof(int))]
    public static void TipRegion(ref Func<string> textGetter) {
        if (addTip) {
            var local = textGetter;
            var state = SurgeryCareCategory.For(currentPawn);
            textGetter = () => state.UpdateTip(local());
        }
    }
}
