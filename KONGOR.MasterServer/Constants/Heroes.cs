namespace KONGOR.MasterServer.Constants;

public static class Heroes
{
    public static List<string> AllHeroIdentifiers()
    {
        List<string> identifiers = [];

        MethodBase? methodBase = MethodBase.GetCurrentMethod();

        if (methodBase == null) return identifiers;

        Type? declaringType = methodBase.DeclaringType;

        if (declaringType == null) return identifiers;

        foreach (Type type in declaringType.GetNestedTypes())
        {
            FieldInfo? fieldInfo = type.GetField("Identifier");

            if (fieldInfo == null) continue;

            string? identifier = (string?) fieldInfo.GetValue(null);

            if (identifier == null) continue;

            identifiers.Add(identifier);
        }

        return identifiers;
    }

    public static class Accursed
    {
        public const string Name = "Accursed";
        public const string Identifier = "Hero_Accursed";

        public static class Abilities
        {
            public const string First = "Ability_Accursed1";
            public const string Second = "Ability_Accursed2";
            public const string Third = "Ability_Accursed3";
            public const string Fourth = "Ability_Accursed4";
        }
    }

    public static class Adrenaline
    {
        public const string Name = "Adrenaline";
        public const string Identifier = "Hero_Adrenaline";

        public static class Abilities
        {
            public const string First = "Ability_Adrenaline1";
            public const string Second = "Ability_Adrenaline2";
            public const string Third = "Ability_Adrenaline3";
            public const string Fourth = "Ability_Adrenaline4";
        }
    }

    public static class Aluna
    {
        public const string Name = "Aluna";
        public const string Identifier = "Hero_Aluna";

        public static class Abilities
        {
            public const string First = "Ability_Aluna1";
            public const string Second = "Ability_Aluna2";
            public const string Third = "Ability_Aluna3";
            public const string Fourth = "Ability_Aluna4";
        }
    }

    public static class Andromeda
    {
        public const string Name = "Andromeda";
        public const string Identifier = "Hero_Andromeda";

        public static class Abilities
        {
            public const string First = "Ability_Andromeda1";
            public const string Second = "Ability_Andromeda2";
            public const string Third = "Ability_Andromeda3";
            public const string Fourth = "Ability_Andromeda4";
        }
    }

    public static class Apex
    {
        public const string Name = "Apex";
        public const string Identifier = "Hero_Apex";

        public static class Abilities
        {
            public const string First = "Ability_Apex1";
            public const string Second = "Ability_Apex2";
            public const string Third = "Ability_Apex3";
            public const string Fourth = "Ability_Apex4";
        }
    }

    public static class Arachna
    {
        public const string Name = "Arachna";
        public const string Identifier = "Hero_Arachna";

        public static class Abilities
        {
            public const string First = "Ability_Arachna1";
            public const string Second = "Ability_Arachna2";
            public const string Third = "Ability_Arachna3";
            public const string Fourth = "Ability_Arachna4";
        }
    }

    public static class Armadon
    {
        public const string Name = "Armadon";
        public const string Identifier = "Hero_Armadon";

        public static class Abilities
        {
            public const string First = "Ability_Armadon1";
            public const string Second = "Ability_Armadon2";
            public const string Third = "Ability_Armadon3";
            public const string Fourth = "Ability_Armadon4";
        }
    }

    public static class Artesia
    {
        public const string Name = "Artesia";
        public const string Identifier = "Hero_Artesia";

        public static class Abilities
        {
            public const string First = "Ability_Artesia1";
            public const string Second = "Ability_Artesia2";
            public const string Third = "Ability_Artesia3";
            public const string Fourth = "Ability_Artesia4";
        }
    }

    public static class Artillery
    {
        public const string Name = "Artillery";
        public const string Identifier = "Hero_Artillery";

        public static class Abilities
        {
            public const string First = "Ability_Artillery1";
            public const string Second = "Ability_Artillery2";
            public const string Third = "Ability_Artillery3";
            public const string Fourth = "Ability_Artillery4";
        }
    }

    public static class WretchedHag
    {
        public const string Name = "Wretched Hag";
        public const string Identifier = "Hero_BabaYaga";

        public static class Abilities
        {
            public const string First = "Ability_BabaYaga1";
            public const string Second = "Ability_BabaYaga2";
            public const string Third = "Ability_BabaYaga3";
            public const string Fourth = "Ability_BabaYaga4";
        }
    }

    public static class Behemoth
    {
        public const string Name = "Behemoth";
        public const string Identifier = "Hero_Behemoth";

        public static class Abilities
        {
            public const string First = "Ability_Behemoth1";
            public const string Second = "Ability_Behemoth2";
            public const string Third = "Ability_Behemoth3";
            public const string Fourth = "Ability_Behemoth4";
        }
    }

    public static class Balphagore
    {
        public const string Name = "Balphagore";
        public const string Identifier = "Hero_Bephelgor";

        public static class Abilities
        {
            public const string First = "Ability_Bephelgor1";
            public const string Second = "Ability_Bephelgor2";
            public const string Third = "Ability_Bephelgor3";
            public const string Fourth = "Ability_Bephelgor4";
        }
    }

    public static class Berzerker
    {
        public const string Name = "Berzerker";
        public const string Identifier = "Hero_Berzerker";

        public static class Abilities
        {
            public const string First = "Ability_Berzerker1";
            public const string Second = "Ability_Berzerker2";
            public const string Third = "Ability_Berzerker3";
            public const string Fourth = "Ability_Berzerker4";
        }
    }

    public static class Blitz
    {
        public const string Name = "Blitz";
        public const string Identifier = "Hero_Blitz";

        public static class Abilities
        {
            public const string First = "Ability_Blitz1";
            public const string Second = "Ability_Blitz2";
            public const string Third = "Ability_Blitz3";
            public const string Fourth = "Ability_Blitz4";
        }
    }

    public static class Bombardier
    {
        public const string Name = "Bombardier";
        public const string Identifier = "Hero_Bombardier";

        public static class Abilities
        {
            public const string First = "Ability_Bombardier1";
            public const string Second = "Ability_Bombardier2";
            public const string Third = "Ability_Bombardier3";
            public const string Fourth = "Ability_Bombardier4";
        }
    }

    public static class Bubbles
    {
        public const string Name = "Bubbles";
        public const string Identifier = "Hero_Bubbles";

        public static class Abilities
        {
            public const string First = "Ability_Bubbles1";
            public const string Second = "Ability_Bubbles2";
            public const string Third = "Ability_Bubbles3";
            public const string Fourth = "Ability_Bubbles4";
        }
    }

    public static class Bushwack
    {
        public const string Name = "Bushwack";
        public const string Identifier = "Hero_Bushwack";

        public static class Abilities
        {
            public const string First = "Ability_Bushwack1";
            public const string Second = "Ability_Bushwack2";
            public const string Third = "Ability_Bushwack3";
            public const string Fourth = "Ability_Bushwack4";
        }
    }

    public static class Calamity
    {
        public const string Name = "Calamity";
        public const string Identifier = "Hero_Calamity";

        public static class Abilities
        {
            public const string First = "Ability_Calamity1";
            public const string Second = "Ability_Calamity2";
            public const string Third = "Ability_Calamity3";
            public const string Fourth = "Ability_Calamity4";
        }
    }

    public static class Qi
    {
        public const string Name = "Qi";
        public const string Identifier = "Hero_Chi";

        public static class Abilities
        {
            public const string First = "Ability_Chi1";
            public const string Second = "Ability_Chi2";
            public const string Third = "Ability_Chi3";
            public const string Fourth = "Ability_Chi4";
        }
    }

    public static class TheChipper
    {
        public const string Name = "The Chipper";
        public const string Identifier = "Hero_Chipper";

        public static class Abilities
        {
            public const string First = "Ability_Chipper1";
            public const string Second = "Ability_Chipper2";
            public const string Third = "Ability_Chipper3";
            public const string Fourth = "Ability_Chipper4";
        }
    }

    public static class Chronos
    {
        public const string Name = "Chronos";
        public const string Identifier = "Hero_Chronos";

        public static class Abilities
        {
            public const string First = "Ability_Chronos1";
            public const string Second = "Ability_Chronos2";
            public const string Third = "Ability_Chronos3";
            public const string Fourth = "Ability_Chronos4";
        }
    }

    public static class Circe
    {
        public const string Name = "Circe";
        public const string Identifier = "Hero_Circe";

        public static class Abilities
        {
            public const string First = "Ability_Circe1";
            public const string Second = "Ability_Circe2";
            public const string Third = "Ability_Circe3";
            public const string Fourth = "Ability_Circe4";
        }
    }

    public static class CorruptedDisciple
    {
        public const string Name = "Corrupted Disciple";
        public const string Identifier = "Hero_CorruptedDisciple";

        public static class Abilities
        {
            public const string First = "Ability_CorruptedDisciple1";
            public const string Second = "Ability_CorruptedDisciple2";
            public const string Third = "Ability_CorruptedDisciple3";
            public const string Fourth = "Ability_CorruptedDisciple4";
        }
    }

    public static class Cthulhuphant
    {
        public const string Name = "Cthulhuphant";
        public const string Identifier = "Hero_Cthulhuphant";

        public static class Abilities
        {
            public const string First = "Ability_Cthulhuphant1";
            public const string Second = "Ability_Cthulhuphant2";
            public const string Third = "Ability_Cthulhuphant3";
            public const string Fourth = "Ability_Cthulhuphant4";
        }
    }

    public static class Dampeer
    {
        public const string Name = "Dampeer";
        public const string Identifier = "Hero_Dampeer";

        public static class Abilities
        {
            public const string First = "Ability_Dampeer1";
            public const string Second = "Ability_Dampeer2";
            public const string Third = "Ability_Dampeer3";
            public const string Fourth = "Ability_Dampeer4";
        }
    }

    public static class Deadlift
    {
        public const string Name = "Deadlift";
        public const string Identifier = "Hero_Deadlift";

        public static class Abilities
        {
            public const string First = "Ability_Deadlift1";
            public const string Second = "Ability_Deadlift2";
            public const string Third = "Ability_Deadlift3";
            public const string Fourth = "Ability_Deadlift4";
        }
    }

    public static class Deadwood
    {
        public const string Name = "Deadwood";
        public const string Identifier = "Hero_Deadwood";

        public static class Abilities
        {
            public const string First = "Ability_Deadwood1";
            public const string Second = "Ability_Deadwood2";
            public const string Third = "Ability_Deadwood3";
            public const string Fourth = "Ability_Deadwood4";
        }
    }

    public static class Defiler
    {
        public const string Name = "Defiler";
        public const string Identifier = "Hero_Defiler";

        public static class Abilities
        {
            public const string First = "Ability_Defiler1";
            public const string Second = "Ability_Defiler2";
            public const string Third = "Ability_Defiler3";
            public const string Fourth = "Ability_Defiler4";
        }
    }

    public static class Devourer
    {
        public const string Name = "Devourer";
        public const string Identifier = "Hero_Devourer";

        public static class Abilities
        {
            public const string First = "Ability_Devourer1";
            public const string Second = "Ability_Devourer2";
            public const string Third = "Ability_Devourer3";
            public const string Fourth = "Ability_Devourer4";
        }
    }

    public static class PlagueRider
    {
        public const string Name = "Plague Rider";
        public const string Identifier = "Hero_DiseasedRider";

        public static class Abilities
        {
            public const string First = "Ability_DiseasedRider1";
            public const string Second = "Ability_DiseasedRider2";
            public const string Third = "Ability_DiseasedRider3";
            public const string Fourth = "Ability_DiseasedRider4";
        }
    }

    public static class DoctorRepulsor
    {
        public const string Name = "Doctor Repulsor";
        public const string Identifier = "Hero_DoctorRepulsor";

        public static class Abilities
        {
            public const string First = "Ability_DoctorRepulsor1";
            public const string Second = "Ability_DoctorRepulsor2";
            public const string Third = "Ability_DoctorRepulsor3";
            public const string Fourth = "Ability_DoctorRepulsor4";
        }
    }

    public static class LordSalforis
    {
        public const string Name = "Lord Salforis";
        public const string Identifier = "Hero_Dreadknight";

        public static class Abilities
        {
            public const string First = "Ability_Dreadknight1";
            public const string Second = "Ability_Dreadknight2";
            public const string Third = "Ability_Dreadknight3";
            public const string Fourth = "Ability_Dreadknight4";
        }
    }

    public static class DrunkenMaster
    {
        public const string Name = "Drunken Master";
        public const string Identifier = "Hero_DrunkenMaster";

        public static class Abilities
        {
            public const string First = "Ability_DrunkenMaster1";
            public const string Second = "Ability_DrunkenMaster2";
            public const string Third = "Ability_DrunkenMaster3";
            public const string Fourth = "Ability_DrunkenMaster4";
        }
    }

    public static class Blacksmith
    {
        public const string Name = "Blacksmith";
        public const string Identifier = "Hero_DwarfMagi";

        public static class Abilities
        {
            public const string First = "Ability_DwarfMagi1";
            public const string Second = "Ability_DwarfMagi2";
            public const string Third = "Ability_DwarfMagi3";
            public const string Fourth = "Ability_DwarfMagi4";
        }
    }

    public static class Slither
    {
        public const string Name = "Slither";
        public const string Identifier = "Hero_Ebulus";

        public static class Abilities
        {
            public const string First = "Ability_Ebulus1";
            public const string Second = "Ability_Ebulus2";
            public const string Third = "Ability_Ebulus3";
            public const string Fourth = "Ability_Ebulus4";
        }
    }

    public static class Electrician
    {
        public const string Name = "Electrician";
        public const string Identifier = "Hero_Electrician";

        public static class Abilities
        {
            public const string First = "Ability_Electrician1";
            public const string Second = "Ability_Electrician2";
            public const string Third = "Ability_Electrician3";
            public const string Fourth = "Ability_Electrician4";
        }
    }

    public static class Ellonia
    {
        public const string Name = "Ellonia";
        public const string Identifier = "Hero_Ellonia";

        public static class Abilities
        {
            public const string First = "Ability_Ellonia1";
            public const string Second = "Ability_Ellonia2";
            public const string Third = "Ability_Ellonia3";
            public const string Fourth = "Ability_Ellonia4";
        }
    }

    public static class EmeraldWarden
    {
        public const string Name = "Emerald Warden";
        public const string Identifier = "Hero_EmeraldWarden";

        public static class Abilities
        {
            public const string First = "Ability_EmeraldWarden1";
            public const string Second = "Ability_EmeraldWarden2";
            public const string Third = "Ability_EmeraldWarden3";
            public const string Fourth = "Ability_EmeraldWarden4";
        }
    }

    public static class Empath
    {
        public const string Name = "Empath";
        public const string Identifier = "Hero_Empath";

        public static class Abilities
        {
            public const string First = "Ability_Empath1";
            public const string Second = "Ability_Empath2";
            public const string Third = "Ability_Empath3";
            public const string Fourth = "Ability_Empath4";
        }
    }

    public static class Engineer
    {
        public const string Name = "Engineer";
        public const string Identifier = "Hero_Engineer";

        public static class Abilities
        {
            public const string First = "Ability_Engineer1";
            public const string Second = "Ability_Engineer2";
            public const string Third = "Ability_Engineer3";
            public const string Fourth = "Ability_Engineer4";
        }
    }

    public static class Fayde
    {
        public const string Name = "Fayde";
        public const string Identifier = "Hero_Fade";

        public static class Abilities
        {
            public const string First = "Ability_Fade1";
            public const string Second = "Ability_Fade2";
            public const string Third = "Ability_Fade3";
            public const string Fourth = "Ability_Fade4";
        }
    }

    public static class Nymphora
    {
        public const string Name = "Nymphora";
        public const string Identifier = "Hero_Fairy";

        public static class Abilities
        {
            public const string First = "Ability_Fairy1";
            public const string Second = "Ability_Fairy2";
            public const string Third = "Ability_Fairy3";
            public const string Fourth = "Ability_Fairy4";
        }
    }

    public static class Draconis
    {
        public const string Name = "Draconis";
        public const string Identifier = "Hero_FlameDragon";

        public static class Abilities
        {
            public const string First = "Ability_FlameDragon1";
            public const string Second = "Ability_FlameDragon2";
            public const string Third = "Ability_FlameDragon3";
            public const string Fourth = "Ability_FlameDragon4";
        }
    }

    public static class FlintBeastwood
    {
        public const string Name = "Flint Beastwood";
        public const string Identifier = "Hero_FlintBeastwood";

        public static class Abilities
        {
            public const string First = "Ability_FlintBeastwood1";
            public const string Second = "Ability_FlintBeastwood2";
            public const string Third = "Ability_FlintBeastwood3";
            public const string Fourth = "Ability_FlintBeastwood4";
        }
    }

    public static class Flux
    {
        public const string Name = "Flux";
        public const string Identifier = "Hero_Flux";

        public static class Abilities
        {
            public const string First = "Ability_Flux1";
            public const string Second = "Ability_Flux2";
            public const string Third = "Ability_Flux3";
            public const string Fourth = "Ability_Flux4";
        }
    }

    public static class ForsakenArcher
    {
        public const string Name = "Forsaken Archer";
        public const string Identifier = "Hero_ForsakenArcher";

        public static class Abilities
        {
            public const string First = "Ability_ForsakenArcher1";
            public const string Second = "Ability_ForsakenArcher2";
            public const string Third = "Ability_ForsakenArcher3";
            public const string Fourth = "Ability_ForsakenArcher4";
        }
    }

    public static class Glacius
    {
        public const string Name = "Glacius";
        public const string Identifier = "Hero_Frosty";

        public static class Abilities
        {
            public const string First = "Ability_Frosty1";
            public const string Second = "Ability_Frosty2";
            public const string Third = "Ability_Frosty3";
            public const string Fourth = "Ability_Frosty4";
        }
    }

    public static class Gauntlet
    {
        public const string Name = "Gauntlet";
        public const string Identifier = "Hero_Gauntlet";

        public static class Abilities
        {
            public const string First = "Ability_Gauntlet1";
            public const string Second = "Ability_Gauntlet2";
            public const string Third = "Ability_Gauntlet3";
            public const string Fourth = "Ability_Gauntlet4";
        }
    }

    public static class Gemini
    {
        public const string Name = "Gemini";
        public const string Identifier = "Hero_Gemini";

        public static class Abilities
        {
            public const string First = "Ability_Gemini1";
            public const string Second = "Ability_Gemini2";
            public const string Third = "Ability_Gemini3";
            public const string Fourth = "Ability_Gemini4";
        }
    }

    public static class Geomancer
    {
        public const string Name = "Geomancer";
        public const string Identifier = "Hero_Geomancer";

        public static class Abilities
        {
            public const string First = "Ability_Geomancer1";
            public const string Second = "Ability_Geomancer2";
            public const string Third = "Ability_Geomancer3";
            public const string Fourth = "Ability_Geomancer4";
        }
    }

    public static class TheGladiator
    {
        public const string Name = "The Gladiator";
        public const string Identifier = "Hero_Gladiator";

        public static class Abilities
        {
            public const string First = "Ability_Gladiator1";
            public const string Second = "Ability_Gladiator2";
            public const string Third = "Ability_Gladiator3";
            public const string Fourth = "Ability_Gladiator4";
        }
    }

    public static class Goldenveil
    {
        public const string Name = "Goldenveil";
        public const string Identifier = "Hero_Goldenveil";

        public static class Abilities
        {
            public const string First = "Ability_Goldenveil1";
            public const string Second = "Ability_Goldenveil2";
            public const string Third = "Ability_Goldenveil3";
            public const string Fourth = "Ability_Goldenveil4";
        }
    }

    public static class Grinex
    {
        public const string Name = "Grinex";
        public const string Identifier = "Hero_Grinex";

        public static class Abilities
        {
            public const string First = "Ability_Grinex1";
            public const string Second = "Ability_Grinex2";
            public const string Third = "Ability_Grinex3";
            public const string Fourth = "Ability_Grinex4";
        }
    }

    public static class Gunblade
    {
        public const string Name = "Gunblade";
        public const string Identifier = "Hero_Gunblade";

        public static class Abilities
        {
            public const string First = "Ability_Gunblade1";
            public const string Second = "Ability_Gunblade2";
            public const string Third = "Ability_Gunblade3";
            public const string Fourth = "Ability_Gunblade4";
        }
    }

    public static class Hammerstorm
    {
        public const string Name = "Hammerstorm";
        public const string Identifier = "Hero_Hammerstorm";

        public static class Abilities
        {
            public const string First = "Ability_Hammerstorm1";
            public const string Second = "Ability_Hammerstorm2";
            public const string Third = "Ability_Hammerstorm3";
            public const string Fourth = "Ability_Hammerstorm4";
        }
    }

    public static class NightHound
    {
        public const string Name = "Night Hound";
        public const string Identifier = "Hero_Hantumon";

        public static class Abilities
        {
            public const string First = "Ability_Hantumon1";
            public const string Second = "Ability_Hantumon2";
            public const string Third = "Ability_Hantumon3";
            public const string Fourth = "Ability_Hantumon4";
        }
    }

    public static class SoulReaper
    {
        public const string Name = "Soul Reaper";
        public const string Identifier = "Hero_HellDemon";

        public static class Abilities
        {
            public const string First = "Ability_HellDemon1";
            public const string Second = "Ability_HellDemon2";
            public const string Third = "Ability_HellDemon3";
            public const string Fourth = "Ability_HellDemon4";
        }
    }

    public static class Hellbringer
    {
        public const string Name = "Hellbringer";
        public const string Identifier = "Hero_Hellbringer";

        public static class Abilities
        {
            public const string First = "Ability_Hellbringer1";
            public const string Second = "Ability_Hellbringer2";
            public const string Third = "Ability_Hellbringer3";
            public const string Fourth = "Ability_Hellbringer4";
        }
    }

    public static class Swiftblade
    {
        public const string Name = "Swiftblade";
        public const string Identifier = "Hero_Hiro";

        public static class Abilities
        {
            public const string First = "Ability_Hiro1";
            public const string Second = "Ability_Hiro2";
            public const string Third = "Ability_Hiro3";
            public const string Fourth = "Ability_Hiro4";
        }
    }

    public static class BloodHunter
    {
        public const string Name = "Blood Hunter";
        public const string Identifier = "Hero_Hunter";

        public static class Abilities
        {
            public const string First = "Ability_Hunter1";
            public const string Second = "Ability_Hunter2";
            public const string Third = "Ability_Hunter3";
            public const string Fourth = "Ability_Hunter4";
        }
    }

    public static class Myrmidon
    {
        public const string Name = "Myrmidon";
        public const string Identifier = "Hero_Hydromancer";

        public static class Abilities
        {
            public const string First = "Ability_Hydromancer1";
            public const string Second = "Ability_Hydromancer2";
            public const string Third = "Ability_Hydromancer3";
            public const string Fourth = "Ability_Hydromancer4";
        }
    }

    public static class Ichor
    {
        public const string Name = "Ichor";
        public const string Identifier = "Hero_Ichor";

        public static class Abilities
        {
            public const string First = "Ability_Ichor1";
            public const string Second = "Ability_Ichor2";
            public const string Third = "Ability_Ichor3";
            public const string Fourth = "Ability_Ichor4";
        }
    }

    public static class Magebane
    {
        public const string Name = "Magebane";
        public const string Identifier = "Hero_Javaras";

        public static class Abilities
        {
            public const string First = "Ability_Javaras1";
            public const string Second = "Ability_Javaras2";
            public const string Third = "Ability_Javaras3";
            public const string Fourth = "Ability_Javaras4";
        }
    }

    public static class Jeraziah
    {
        public const string Name = "Jeraziah";
        public const string Identifier = "Hero_Jereziah";

        public static class Abilities
        {
            public const string First = "Ability_Jereziah1";
            public const string Second = "Ability_Jereziah2";
            public const string Third = "Ability_Jereziah3";
            public const string Fourth = "Ability_Jereziah4";
        }
    }

    public static class Kane
    {
        public const string Name = "Kane";
        public const string Identifier = "Hero_Kane";

        public static class Abilities
        {
            public const string First = "Ability_Kane1";
            public const string Second = "Ability_Kane2";
            public const string Third = "Ability_Kane3";
            public const string Fourth = "Ability_Kane4";
        }
    }

    public static class Kinesis
    {
        public const string Name = "Kinesis";
        public const string Identifier = "Hero_Kenisis";

        public static class Abilities
        {
            public const string First = "Ability_Kenisis1";
            public const string Second = "Ability_Kenisis2";
            public const string Third = "Ability_Kenisis3";
            public const string Fourth = "Ability_Kenisis4";
        }
    }

    public static class KingKlout
    {
        public const string Name = "King Klout";
        public const string Identifier = "Hero_KingKlout";

        public static class Abilities
        {
            public const string First = "Ability_KingKlout1";
            public const string Second = "Ability_KingKlout2";
            public const string Third = "Ability_KingKlout3";
            public const string Fourth = "Ability_KingKlout4";
        }
    }

    public static class Klanx
    {
        public const string Name = "Klanx";
        public const string Identifier = "Hero_Klanx";

        public static class Abilities
        {
            public const string First = "Ability_Klanx1";
            public const string Second = "Ability_Klanx2";
            public const string Third = "Ability_Klanx3";
            public const string Fourth = "Ability_Klanx4";
        }
    }

    public static class Kraken
    {
        public const string Name = "Kraken";
        public const string Identifier = "Hero_Kraken";

        public static class Abilities
        {
            public const string First = "Ability_Kraken1";
            public const string Second = "Ability_Kraken2";
            public const string Third = "Ability_Kraken3";
            public const string Fourth = "Ability_Kraken4";
        }
    }

    public static class MoonQueen
    {
        public const string Name = "Moon Queen";
        public const string Identifier = "Hero_Krixi";

        public static class Abilities
        {
            public const string First = "Ability_Krixi1";
            public const string Second = "Ability_Krixi2";
            public const string Third = "Ability_Krixi3";
            public const string Fourth = "Ability_Krixi4";
        }
    }

    public static class Thunderbringer
    {
        public const string Name = "Thunderbringer";
        public const string Identifier = "Hero_Kunas";

        public static class Abilities
        {
            public const string First = "Ability_Kunas1";
            public const string Second = "Ability_Kunas2";
            public const string Third = "Ability_Kunas3";
            public const string Fourth = "Ability_Kunas4";
        }
    }

    public static class Legionnaire
    {
        public const string Name = "Legionnaire";
        public const string Identifier = "Hero_Legionnaire";

        public static class Abilities
        {
            public const string First = "Ability_Legionnaire1";
            public const string Second = "Ability_Legionnaire2";
            public const string Third = "Ability_Legionnaire3";
            public const string Fourth = "Ability_Legionnaire4";
        }
    }

    public static class Lodestone
    {
        public const string Name = "Lodestone";
        public const string Identifier = "Hero_Lodestone";

        public static class Abilities
        {
            public const string First = "Ability_Lodestone1";
            public const string Second = "Ability_Lodestone2";
            public const string Third = "Ability_Lodestone3";
            public const string Fourth = "Ability_Lodestone4";
        }
    }

    public static class Magmus
    {
        public const string Name = "Magmus";
        public const string Identifier = "Hero_Magmar";

        public static class Abilities
        {
            public const string First = "Ability_Magmar1";
            public const string Second = "Ability_Magmar2";
            public const string Third = "Ability_Magmar3";
            public const string Fourth = "Ability_Magmar4";
        }
    }

    public static class Maliken
    {
        public const string Name = "Maliken";
        public const string Identifier = "Hero_Maliken";

        public static class Abilities
        {
            public const string First = "Ability_Maliken1";
            public const string Second = "Ability_Maliken2";
            public const string Third = "Ability_Maliken3";
            public const string Fourth = "Ability_Maliken4";
        }
    }

    public static class Martyr
    {
        public const string Name = "Martyr";
        public const string Identifier = "Hero_Martyr";

        public static class Abilities
        {
            public const string First = "Ability_Martyr1";
            public const string Second = "Ability_Martyr2";
            public const string Third = "Ability_Martyr3";
            public const string Fourth = "Ability_Martyr4";
        }
    }

    public static class MasterOfArms
    {
        public const string Name = "Master Of Arms";
        public const string Identifier = "Hero_MasterOfArms";

        public static class Abilities
        {
            public const string First = "Ability_MasterOfArms1";
            public const string Second = "Ability_MasterOfArms2";
            public const string Third = "Ability_MasterOfArms3";
            public const string Fourth = "Ability_MasterOfArms4";
        }
    }

    public static class Midas
    {
        public const string Name = "Midas";
        public const string Identifier = "Hero_Midas";

        public static class Abilities
        {
            public const string First = "Ability_Midas1";
            public const string Second = "Ability_Midas2";
            public const string Third = "Ability_Midas3";
            public const string Fourth = "Ability_Midas4";
        }
    }

    public static class Xemplar
    {
        public const string Name = "Xemplar";
        public const string Identifier = "Hero_Mimix";

        public static class Abilities
        {
            public const string First = "Ability_Mimix1";
            public const string Second = "Ability_Mimix2";
            public const string Third = "Ability_Mimix3";
            public const string Fourth = "Ability_Mimix4";
        }
    }

    public static class Moira
    {
        public const string Name = "Moira";
        public const string Identifier = "Hero_Moira";

        public static class Abilities
        {
            public const string First = "Ability_Moira1";
            public const string Second = "Ability_Moira2";
            public const string Third = "Ability_Moira3";
            public const string Fourth = "Ability_Moira4";
        }
    }

    public static class Monarch
    {
        public const string Name = "Monarch";
        public const string Identifier = "Hero_Monarch";

        public static class Abilities
        {
            public const string First = "Ability_Monarch1";
            public const string Second = "Ability_Monarch2";
            public const string Third = "Ability_Monarch3";
            public const string Fourth = "Ability_Monarch4";
        }
    }

    public static class MonkeyKing
    {
        public const string Name = "Monkey King";
        public const string Identifier = "Hero_MonkeyKing";

        public static class Abilities
        {
            public const string First = "Ability_MonkeyKing1";
            public const string Second = "Ability_MonkeyKing2";
            public const string Third = "Ability_MonkeyKing3";
            public const string Fourth = "Ability_MonkeyKing4";
        }
    }

    public static class Moraxus
    {
        public const string Name = "Moraxus";
        public const string Identifier = "Hero_Moraxus";

        public static class Abilities
        {
            public const string First = "Ability_Moraxus1";
            public const string Second = "Ability_Moraxus2";
            public const string Third = "Ability_Moraxus3";
            public const string Fourth = "Ability_Moraxus4";
        }
    }

    public static class Pharaoh
    {
        public const string Name = "Pharaoh";
        public const string Identifier = "Hero_Mumra";

        public static class Abilities
        {
            public const string First = "Ability_Mumra1";
            public const string Second = "Ability_Mumra2";
            public const string Third = "Ability_Mumra3";
            public const string Fourth = "Ability_Mumra4";
        }
    }

    public static class Nitro
    {
        public const string Name = "Nitro";
        public const string Identifier = "Hero_Nitro";

        public static class Abilities
        {
            public const string First = "Ability_Nitro1";
            public const string Second = "Ability_Nitro2";
            public const string Third = "Ability_Nitro3";
            public const string Fourth = "Ability_Nitro4";
        }
    }

    public static class Nomad
    {
        public const string Name = "Nomad";
        public const string Identifier = "Hero_Nomad";

        public static class Abilities
        {
            public const string First = "Ability_Nomad1";
            public const string Second = "Ability_Nomad2";
            public const string Third = "Ability_Nomad3";
            public const string Fourth = "Ability_Nomad4";
        }
    }

    public static class Oogie
    {
        public const string Name = "Oogie";
        public const string Identifier = "Hero_Oogie";

        public static class Abilities
        {
            public const string First = "Ability_Oogie1";
            public const string Second = "Ability_Oogie2";
            public const string Third = "Ability_Oogie3";
            public const string Fourth = "Ability_Oogie4";
        }
    }

    public static class Ophelia
    {
        public const string Name = "Ophelia";
        public const string Identifier = "Hero_Ophelia";

        public static class Abilities
        {
            public const string First = "Ability_Ophelia1";
            public const string Second = "Ability_Ophelia2";
            public const string Third = "Ability_Ophelia3";
            public const string Fourth = "Ability_Ophelia4";
        }
    }

    public static class Pandamonium
    {
        public const string Name = "Pandamonium";
        public const string Identifier = "Hero_Panda";

        public static class Abilities
        {
            public const string First = "Ability_Panda1";
            public const string Second = "Ability_Panda2";
            public const string Third = "Ability_Panda3";
            public const string Fourth = "Ability_Panda4";
        }
    }

    public static class Parallax
    {
        public const string Name = "Parallax";
        public const string Identifier = "Hero_Parallax";

        public static class Abilities
        {
            public const string First = "Ability_Parallax1";
            public const string Second = "Ability_Parallax2";
            public const string Third = "Ability_Parallax3";
            public const string Fourth = "Ability_Parallax4";
        }
    }

    public static class Parasite
    {
        public const string Name = "Parasite";
        public const string Identifier = "Hero_Parasite";

        public static class Abilities
        {
            public const string First = "Ability_Parasite1";
            public const string Second = "Ability_Parasite2";
            public const string Third = "Ability_Parasite3";
            public const string Fourth = "Ability_Parasite4";
        }
    }

    public static class Pearl
    {
        public const string Name = "Pearl";
        public const string Identifier = "Hero_Pearl";

        public static class Abilities
        {
            public const string First = "Ability_Pearl1";
            public const string Second = "Ability_Pearl2";
            public const string Third = "Ability_Pearl3";
            public const string Fourth = "Ability_Pearl4";
        }
    }

    public static class Pestilence
    {
        public const string Name = "Pestilence";
        public const string Identifier = "Hero_Pestilence";

        public static class Abilities
        {
            public const string First = "Ability_Pestilence1";
            public const string Second = "Ability_Pestilence2";
            public const string Third = "Ability_Pestilence3";
            public const string Fourth = "Ability_Pestilence4";
        }
    }

    public static class Bramble
    {
        public const string Name = "Bramble";
        public const string Identifier = "Hero_Plant";

        public static class Abilities
        {
            public const string First = "Ability_Plant1";
            public const string Second = "Ability_Plant2";
            public const string Third = "Ability_Plant3";
            public const string Fourth = "Ability_Plant4";
        }
    }

    public static class PollywogPriest
    {
        public const string Name = "Pollywog Priest";
        public const string Identifier = "Hero_PollywogPriest";

        public static class Abilities
        {
            public const string First = "Ability_PollywogPriest1";
            public const string Second = "Ability_PollywogPriest2";
            public const string Third = "Ability_PollywogPriest3";
            public const string Fourth = "Ability_PollywogPriest4";
        }
    }

    public static class Predator
    {
        public const string Name = "Predator";
        public const string Identifier = "Hero_Predator";

        public static class Abilities
        {
            public const string First = "Ability_Predator1";
            public const string Second = "Ability_Predator2";
            public const string Third = "Ability_Predator3";
            public const string Fourth = "Ability_Predator4";
        }
    }

    public static class Prisoner945
    {
        public const string Name = "Prisoner 945";
        public const string Identifier = "Hero_Prisoner";

        public static class Abilities
        {
            public const string First = "Ability_Prisoner1";
            public const string Second = "Ability_Prisoner2";
            public const string Third = "Ability_Prisoner3";
            public const string Fourth = "Ability_Prisoner4";
        }
    }

    public static class Prophet
    {
        public const string Name = "Prophet";
        public const string Identifier = "Hero_Prophet"; // NOTE: For the custom map "prophets", the identifier `wl_Warlock` is used instead.

        public static class Abilities
        {
            public const string First = "Ability_Prophet1";
            public const string Second = "Ability_Prophet2";
            public const string Third = "Ability_Prophet3";
            public const string Fourth = "Ability_Prophet4";
        }
    }

    public static class PuppetMaster
    {
        public const string Name = "Puppet Master";
        public const string Identifier = "Hero_PuppetMaster";

        public static class Abilities
        {
            public const string First = "Ability_PuppetMaster1";
            public const string Second = "Ability_PuppetMaster2";
            public const string Third = "Ability_PuppetMaster3";
            public const string Fourth = "Ability_PuppetMaster4";
        }
    }

    public static class Pyromancer
    {
        public const string Name = "Pyromancer";
        public const string Identifier = "Hero_Pyromancer";

        public static class Abilities
        {
            public const string First = "Ability_Pyromancer1";
            public const string Second = "Ability_Pyromancer2";
            public const string Third = "Ability_Pyromancer3";
            public const string Fourth = "Ability_Pyromancer4";
        }
    }

    public static class AmunRa
    {
        public const string Name = "Amun-Ra";
        public const string Identifier = "Hero_Ra";

        public static class Abilities
        {
            public const string First = "Ability_Ra1";
            public const string Second = "Ability_Ra2";
            public const string Third = "Ability_Ra3";
            public const string Fourth = "Ability_Ra4";
        }
    }

    public static class Rally
    {
        public const string Name = "Rally";
        public const string Identifier = "Hero_Rally";

        public static class Abilities
        {
            public const string First = "Ability_Rally1";
            public const string Second = "Ability_Rally2";
            public const string Third = "Ability_Rally3";
            public const string Fourth = "Ability_Rally4";
        }
    }

    public static class Rampage
    {
        public const string Name = "Rampage";
        public const string Identifier = "Hero_Rampage";

        public static class Abilities
        {
            public const string First = "Ability_Rampage1";
            public const string Second = "Ability_Rampage2";
            public const string Third = "Ability_Rampage3";
            public const string Fourth = "Ability_Rampage4";
        }
    }

    public static class Ravenor
    {
        public const string Name = "Ravenor";
        public const string Identifier = "Hero_Ravenor";

        public static class Abilities
        {
            public const string First = "Ability_Ravenor1";
            public const string Second = "Ability_Ravenor2";
            public const string Third = "Ability_Ravenor3";
            public const string Fourth = "Ability_Ravenor4";
        }
    }

    public static class Revenant
    {
        public const string Name = "Revenant";
        public const string Identifier = "Hero_Revenant";

        public static class Abilities
        {
            public const string First = "Ability_Revenant1";
            public const string Second = "Ability_Revenant2";
            public const string Third = "Ability_Revenant3";
            public const string Fourth = "Ability_Revenant4";
        }
    }

    public static class Rhapsody
    {
        public const string Name = "Rhapsody";
        public const string Identifier = "Hero_Rhapsody";

        public static class Abilities
        {
            public const string First = "Ability_Rhapsody1";
            public const string Second = "Ability_Rhapsody2";
            public const string Third = "Ability_Rhapsody3";
            public const string Fourth = "Ability_Rhapsody4";
        }
    }

    public static class Riftwalker
    {
        public const string Name = "Riftwalker";
        public const string Identifier = "Hero_Riftmage";

        public static class Abilities
        {
            public const string First = "Ability_Riftmage1";
            public const string Second = "Ability_Riftmage2";
            public const string Third = "Ability_Riftmage3";
            public const string Fourth = "Ability_Riftmage4";
        }
    }

    public static class Riptide
    {
        public const string Name = "Riptide";
        public const string Identifier = "Hero_Riptide";

        public static class Abilities
        {
            public const string First = "Ability_Riptide1";
            public const string Second = "Ability_Riptide2";
            public const string Third = "Ability_Riptide3";
            public const string Fourth = "Ability_Riptide4";
        }
    }

    public static class Pebbles
    {
        public const string Name = "Pebbles";
        public const string Identifier = "Hero_Rocky";

        public static class Abilities
        {
            public const string First = "Ability_Rocky1";
            public const string Second = "Ability_Rocky2";
            public const string Third = "Ability_Rocky3";
            public const string Fourth = "Ability_Rocky4";
        }
    }

    public static class Salomon
    {
        public const string Name = "Salomon";
        public const string Identifier = "Hero_Salomon";

        public static class Abilities
        {
            public const string First = "Ability_Salomon1";
            public const string Second = "Ability_Salomon2";
            public const string Third = "Ability_Salomon3";
            public const string Fourth = "Ability_Salomon4";
        }
    }

    public static class SandWraith
    {
        public const string Name = "Sand Wraith";
        public const string Identifier = "Hero_SandWraith";

        public static class Abilities
        {
            public const string First = "Ability_SandWraith1";
            public const string Second = "Ability_SandWraith2";
            public const string Third = "Ability_SandWraith3";
            public const string Fourth = "Ability_SandWraith4";
        }
    }

    public static class Sapphire
    {
        public const string Name = "Sapphire";
        public const string Identifier = "Hero_Sapphire";

        public static class Abilities
        {
            public const string First = "Ability_Sapphire1";
            public const string Second = "Ability_Sapphire2";
            public const string Third = "Ability_Sapphire3";
            public const string Fourth = "Ability_Sapphire4";
        }
    }

    public static class TheMadman
    {
        public const string Name = "The Madman";
        public const string Identifier = "Hero_Scar";

        public static class Abilities
        {
            public const string First = "Ability_Scar1";
            public const string Second = "Ability_Scar2";
            public const string Third = "Ability_Scar3";
            public const string Fourth = "Ability_Scar4";
        }
    }

    public static class Scout
    {
        public const string Name = "Scout";
        public const string Identifier = "Hero_Scout";

        public static class Abilities
        {
            public const string First = "Ability_Scout1";
            public const string Second = "Ability_Scout2";
            public const string Third = "Ability_Scout3";
            public const string Fourth = "Ability_Scout4";
        }
    }

    public static class Shadowblade
    {
        public const string Name = "Shadowblade";
        public const string Identifier = "Hero_ShadowBlade";

        public static class Abilities
        {
            public const string First = "Ability_ShadowBlade1";
            public const string Second = "Ability_ShadowBlade2";
            public const string Third = "Ability_ShadowBlade3";
            public const string Fourth = "Ability_ShadowBlade4";
        }
    }

    public static class DementedShaman
    {
        public const string Name = "Demented Shaman";
        public const string Identifier = "Hero_Shaman";

        public static class Abilities
        {
            public const string First = "Ability_Shaman1";
            public const string Second = "Ability_Shaman2";
            public const string Third = "Ability_Shaman3";
            public const string Fourth = "Ability_Shaman4";
        }
    }

    public static class Shellshock
    {
        public const string Name = "Shellshock";
        public const string Identifier = "Hero_Shellshock";

        public static class Abilities
        {
            public const string First = "Ability_Shellshock1";
            public const string Second = "Ability_Shellshock2";
            public const string Third = "Ability_Shellshock3";
            public const string Fourth = "Ability_Shellshock4";
        }
    }

    public static class Silhouette
    {
        public const string Name = "Silhouette";
        public const string Identifier = "Hero_Silhouette";

        public static class Abilities
        {
            public const string First = "Ability_Silhouette1";
            public const string Second = "Ability_Silhouette2";
            public const string Third = "Ability_Silhouette3";
            public const string Fourth = "Ability_Silhouette4";
        }
    }

    public static class SirBenzington
    {
        public const string Name = "Sir Benzington";
        public const string Identifier = "Hero_SirBenzington";

        public static class Abilities
        {
            public const string First = "Ability_SirBenzington1";
            public const string Second = "Ability_SirBenzington2";
            public const string Third = "Ability_SirBenzington3";
            public const string Fourth = "Ability_SirBenzington4";
        }
    }

    public static class Skrap
    {
        public const string Name = "Skrap";
        public const string Identifier = "Hero_Skrap";

        public static class Abilities
        {
            public const string First = "Ability_Skrap1";
            public const string Second = "Ability_Skrap2";
            public const string Third = "Ability_Skrap3";
            public const string Fourth = "Ability_Skrap4";
        }
    }

    public static class Solstice
    {
        public const string Name = "Solstice";
        public const string Identifier = "Hero_Solstice";

        public static class Abilities
        {
            public const string First = "Ability_Solstice1";
            public const string Second = "Ability_Solstice2";
            public const string Third = "Ability_Solstice3";
            public const string Fourth = "Ability_Solstice4";
        }
    }

    public static class Soulstealer
    {
        public const string Name = "Soulstealer";
        public const string Identifier = "Hero_Soulstealer";

        public static class Abilities
        {
            public const string First = "Ability_Soulstealer1";
            public const string Second = "Ability_Soulstealer2";
            public const string Third = "Ability_Soulstealer3";
            public const string Fourth = "Ability_Soulstealer4";
        }
    }

    public static class Succubus
    {
        public const string Name = "Succubus";
        public const string Identifier = "Hero_Succubis";

        public static class Abilities
        {
            public const string First = "Ability_Succubis1";
            public const string Second = "Ability_Succubis2";
            public const string Third = "Ability_Succubis3";
            public const string Fourth = "Ability_Succubis4";
        }
    }

    public static class Gravekeeper
    {
        public const string Name = "Gravekeeper";
        public const string Identifier = "Hero_Taint";

        public static class Abilities
        {
            public const string First = "Ability_Taint1";
            public const string Second = "Ability_Taint2";
            public const string Third = "Ability_Taint3";
            public const string Fourth = "Ability_Taint4";
        }
    }

    public static class Tarot
    {
        public const string Name = "Tarot";
        public const string Identifier = "Hero_Tarot";

        public static class Abilities
        {
            public const string First = "Ability_Tarot1";
            public const string Second = "Ability_Tarot2";
            public const string Third = "Ability_Tarot3";
            public const string Fourth = "Ability_Tarot4";
        }
    }

    public static class Tempest
    {
        public const string Name = "Tempest";
        public const string Identifier = "Hero_Tempest";

        public static class Abilities
        {
            public const string First = "Ability_Tempest1";
            public const string Second = "Ability_Tempest2";
            public const string Third = "Ability_Tempest3";
            public const string Fourth = "Ability_Tempest4";
        }
    }

    public static class KeeperOfTheForest
    {
        public const string Name = "Keeper Of The Forest";
        public const string Identifier = "Hero_Treant";

        public static class Abilities
        {
            public const string First = "Ability_Treant1";
            public const string Second = "Ability_Treant2";
            public const string Third = "Ability_Treant3";
            public const string Fourth = "Ability_Treant4";
        }
    }

    public static class Tremble
    {
        public const string Name = "Tremble";
        public const string Identifier = "Hero_Tremble";

        public static class Abilities
        {
            public const string First = "Ability_Tremble1";
            public const string Second = "Ability_Tremble2";
            public const string Third = "Ability_Tremble3";
            public const string Fourth = "Ability_Tremble4";
        }
    }

    public static class Tundra
    {
        public const string Name = "Tundra";
        public const string Identifier = "Hero_Tundra";

        public static class Abilities
        {
            public const string First = "Ability_Tundra1";
            public const string Second = "Ability_Tundra2";
            public const string Third = "Ability_Tundra3";
            public const string Fourth = "Ability_Tundra4";
        }
    }

    public static class Valkyrie
    {
        public const string Name = "Valkyrie";
        public const string Identifier = "Hero_Valkyrie";

        public static class Abilities
        {
            public const string First = "Ability_Valkyrie1";
            public const string Second = "Ability_Valkyrie2";
            public const string Third = "Ability_Valkyrie3";
            public const string Fourth = "Ability_Valkyrie4";
        }
    }

    public static class TheDarkLady
    {
        public const string Name = "The Dark Lady";
        public const string Identifier = "Hero_Vanya";

        public static class Abilities
        {
            public const string First = "Ability_Vanya1";
            public const string Second = "Ability_Vanya2";
            public const string Third = "Ability_Vanya3";
            public const string Fourth = "Ability_Vanya4";
        }
    }

    public static class Vindicator
    {
        public const string Name = "Vindicator";
        public const string Identifier = "Hero_Vindicator";

        public static class Abilities
        {
            public const string First = "Ability_Vindicator1";
            public const string Second = "Ability_Vindicator2";
            public const string Third = "Ability_Vindicator3";
            public const string Fourth = "Ability_Vindicator4";
        }
    }

    public static class VoodooJester
    {
        public const string Name = "Voodoo Jester";
        public const string Identifier = "Hero_Voodoo";

        public static class Abilities
        {
            public const string First = "Ability_Voodoo1";
            public const string Second = "Ability_Voodoo2";
            public const string Third = "Ability_Voodoo3";
            public const string Fourth = "Ability_Voodoo4";
        }
    }

    public static class Warchief
    {
        public const string Name = "Warchief";
        public const string Identifier = "Hero_Warchief";

        public static class Abilities
        {
            public const string First = "Ability_Warchief1";
            public const string Second = "Ability_Warchief2";
            public const string Third = "Ability_Warchief3";
            public const string Fourth = "Ability_Warchief4";
        }
    }

    public static class WitchSlayer
    {
        public const string Name = "Witch Slayer";
        public const string Identifier = "Hero_WitchSlayer";

        public static class Abilities
        {
            public const string First = "Ability_WitchSlayer1";
            public const string Second = "Ability_WitchSlayer2";
            public const string Third = "Ability_WitchSlayer3";
            public const string Fourth = "Ability_WitchSlayer4";
        }
    }

    public static class WarBeast
    {
        public const string Name = "War Beast";
        public const string Identifier = "Hero_WolfMan";

        public static class Abilities
        {
            public const string First = "Ability_WolfMan1";
            public const string Second = "Ability_WolfMan2";
            public const string Third = "Ability_WolfMan3";
            public const string Fourth = "Ability_WolfMan4";
        }
    }

    public static class Torturer
    {
        public const string Name = "Torturer";
        public const string Identifier = "Hero_Xalynx";

        public static class Abilities
        {
            public const string First = "Ability_Xalynx1";
            public const string Second = "Ability_Xalynx2";
            public const string Third = "Ability_Xalynx3";
            public const string Fourth = "Ability_Xalynx4";
        }
    }

    public static class Wildsoul
    {
        public const string Name = "Wildsoul";
        public const string Identifier = "Hero_Yogi";

        public static class Abilities
        {
            public const string First = "Ability_Yogi1";
            public const string Second = "Ability_Yogi2";
            public const string Third = "Ability_Yogi3";
            public const string Fourth = "Ability_Yogi4";
        }
    }

    public static class Zephyr
    {
        public const string Name = "Zephyr";
        public const string Identifier = "Hero_Zephyr";

        public static class Abilities
        {
            public const string First = "Ability_Zephyr1";
            public const string Second = "Ability_Zephyr2";
            public const string Third = "Ability_Zephyr3";
            public const string Fourth = "Ability_Zephyr4";
        }
    }
}
