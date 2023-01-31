using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SurgeryMeds {
    [StaticConstructorOnStartup]
    public static class Textures {
        private const string Prefix = Strings.ID + "/";
        private const string Medical = "UI/Icons/Medical/";

        // public static readonly Texture2D Name = ContentFinder<Texture2D>.Get(Prefix + "Name");

        // Vanilla textures
        public static readonly Texture2D NoCare = ContentFinder<Texture2D>.Get(Medical + "NoCare");
        public static readonly Texture2D NoMeds = ContentFinder<Texture2D>.Get(Medical + "NoMeds");
    }
}
