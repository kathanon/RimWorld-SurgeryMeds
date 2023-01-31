using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SurgeryMeds {
    public static class Strings {
        public const string ID = "kathanon.SurgeryMeds";
        public const string Name = "Surgery Meds";

        public const string NamedLabel = "RESTRICTIONLABEL";

        // UI
        public static readonly string ForSurgery = "for surgery: ";
        public static readonly string TipNoLabel = "(right-click to set for surgery)";
        public static readonly string UseVanilla = "Use medical care setting";
        public static readonly string IconTipOn  = "Used for surgery, right-click to unset.";
        public static readonly string IconTipOff = "Right-click to use for surgery.";

        // UI - derived
        public static readonly string ForSurgeryCap = ForSurgery.CapitalizeFirst();

        // UI - parameterized
        public static string TipForLabel(string label) 
            => (ID + ".TipForLabel" ).Translate(label);
        public static string TipForButton(string careLabel, string surgeryLabel) 
            => (ID + ".TipForButton").Translate(careLabel, surgeryLabel);
    }
}
