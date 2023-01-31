using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SurgeryMeds;
public class SurgeryCareCategory {
    private static readonly Color frameColor = new(0f, 0.2f, 0.6f);
    private static readonly Texture2D[] icons = {
        Textures.NoCare,
        Textures.NoMeds,
        ThingDefOf.MedicineHerbal    .uiIcon,
        ThingDefOf.MedicineIndustrial.uiIcon,
        ThingDefOf.MedicineUltratech .uiIcon,
    };
    private static readonly MedicalCareCategory?[] menuOrder = {
        null,
        MedicalCareCategory.Best,
        MedicalCareCategory.NormalOrWorse,
        MedicalCareCategory.HerbalOrWorse,
    };
    private static readonly Dictionary<string, MedicalCareCategory> fromLabel =
        Enum.GetValues(typeof(MedicalCareCategory))
            .OfType<MedicalCareCategory>()
            .ToDictionary(x => x.GetLabel().CapitalizeFirst());
    private static readonly Dictionary<MedicalCareCategory, string> toLabel =
        fromLabel.ToDictionary(x => x.Value, x => x.Key);

    private static readonly ConditionalWeakTable<Pawn, SurgeryCareCategory> db = new();

    public readonly Pawn pawn;
    public MedicalCareCategory? value;

    public static SurgeryCareCategory For(Pawn p)
        => (p != null) ? db.GetValue(p, x => new(x)) : null;

    public bool Active
        => value != null;

    public bool Changing
        => value != null && value != BaseVal;

    private SurgeryCareCategory(Pawn p)
        => pawn = p;

    private string Label
        => LabelFor(value);

    private Texture2D Icon
        => IconFor(value);

    private MedicalCareCategory BaseVal
        => pawn.CareCategory();

    private static string LabelFor(MedicalCareCategory? cat)
        => (cat == null) ? Strings.UseVanilla : toLabel[cat.Value];

    public static Texture2D IconFor(MedicalCareCategory? cat)
        => (cat == null) ? BaseContent.ClearTex : icons[(int) cat.Value];

    public MedicalCareCategory CategoryFor(RecipeDef recipe)
        => (Active && recipe.IsInvasiveSurgery()) ? value.Value : BaseVal;

    public IEnumerable<FloatMenuOption> Menu()
        => menuOrder.Select(MenuOption);

    private FloatMenuOption MenuOption(MedicalCareCategory? x)
        => new(LabelFor(x), () => value = x, IconFor(x), Color.white);

    public void DoMenu()
        => Find.WindowStack.Add(new FloatMenu(Menu().ToList(), Strings.ForSurgeryCap));

    public void DoIcon(Rect rect)
        => GUI.DrawTexture(rect, Icon);

    public void DoFrame(Rect rect, float offset) {
        if (value == null || value == BaseVal) return;
        rect.x += offset * (int) value;
        GUI.color = frameColor;
        Widgets.DrawBox(rect, 3);
        GUI.color = Color.white;
    }

    public string ButtonTip() 
        => Strings.TipForButton(toLabel[BaseVal], Changing ? Strings.TipForLabel(Label) : Strings.TipNoLabel);

    public string UpdateTip(string tip) {
        string extra = (Active && fromLabel[tip] == value) ? Strings.IconTipOn : Strings.IconTipOff;
        return $"{tip}\n{extra}";
    }

    public void Toggle(MedicalCareCategory clicked)
        => value = (clicked == value) ? null : clicked;

    public void ExposeDataExtra() {
        Scribe_Values.Look(ref value, Strings.ID, null);
    }
}

public static class SurgeryCareCategoryExtension {
    public static bool IsInvasiveSurgery(this RecipeDef def) 
        => def.IsSurgery && def.ingredients.Any(x => x.filter.AnyAllowedDef.IsMedicine);

    public static MedicalCareCategory CareCategory(this Pawn pawn)
        => pawn.playerSettings?.medCare ?? MedicalCareCategory.Best;
}
