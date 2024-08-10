using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Statuses;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Interface.FontIdentifier;
using XIVSlothCombo.Combos.PvE.Content;
using XIVSlothCombo.CustomComboNS;

namespace XIVSlothCombo.Combos.PvE
{
    internal static class GNB
    {
        public const byte JobID = 37;

        public static int MaxCartridges(byte level) => level >= 88 ? 3 : 2;

        public const uint
            KeenEdge = 16137,
            NoMercy = 16138,
            BrutalShell = 16139,
            DemonSlice = 16141,
            SolidBarrel = 16145,
            GnashingFang = 16146,
            SavageClaw = 16147,
            DemonSlaughter = 16149,
            WickedTalon = 16150,
            SonicBreak = 16153,
            Continuation = 16155,
            JugularRip = 16156,
            AbdomenTear = 16157,
            EyeGouge = 16158,
            BowShock = 16159,
            HeartOfLight = 16160,
            BurstStrike = 16162,
            FatedCircle = 16163,
            Aurora = 16151,
            DoubleDown = 25760,
            DangerZone = 16144,
            BlastingZone = 16165,
            Bloodfest = 16164,
            Hypervelocity = 25759,
            LionHeart = 36939,
            NobleBlood = 36938,
            ReignOfBeasts = 36937,
            FatedBrand = 36936,
            LightningShot = 16143;

        public static class Buffs
        {
            public const ushort
                NoMercy = 1831,
                Aurora = 1835,
                ReadyToRip = 1842,
                ReadyToTear = 1843,
                ReadyToGouge = 1844,
                ReadyToRaze = 3839,
                ReadyToBreak = 3886,
                ReadyToReign = 3840,
                ReadyToBlast = 2686;
        }

        public static class Debuffs
        {
            public const ushort
                BowShock = 1838,
                SonicBreak = 1837;
        }

        public static class Config
        {
            public const string
                GNB_VariantCure = "GNB_VariantCure";
        }

        internal class GNB_ST_MainCombo : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_MainCombo;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is KeenEdge)
                {
                    var gauge = GetJobGauge<GNBGauge>();
                    var quarterWeave = GetCooldownRemainingTime(actionID) < 1 && GetCooldownRemainingTime(actionID) > 0.6;
                    float GCD = GetCooldown(KeenEdge).CooldownTotal; // 2.5 supported, 2.45 is iffy

                    // Variant Cure
                    if (IsEnabled(CustomComboPreset.GNB_Variant_Cure) && IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= GetOptionValue(Config.GNB_VariantCure))
                        return Variant.VariantCure;

                    // Ranged Uptime
                    if (IsEnabled(CustomComboPreset.GNB_ST_RangedUptime) && !InMeleeRange() && LevelChecked(LightningShot) && HasBattleTarget())
                        return LightningShot;

                    // No Mercy
                    if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup) && IsEnabled(CustomComboPreset.GNB_ST_NoMercy))
                    {
                        if (ActionReady(NoMercy))
                        {
                            if (CanWeave(actionID))
                            {
                                if ((LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && gauge.Ammo == 0 && lastComboMove is BrutalShell && IsOffCooldown(Bloodfest)) // Lv100 Opener/Reopener (0cart)
                                    || (LevelChecked(ReignOfBeasts) && gauge.Ammo >= 2 && IsOffCooldown(NoMercy)) // Lv100 on CD use (2 or 3 cart, never 1)
                                    || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && gauge.Ammo == 0 && lastComboMove is BrutalShell && IsOffCooldown(Bloodfest)) // Lv90 Opener/Reopener (0cart)
                                    || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)) && gauge.Ammo == 3) // Lv90 2min 3cart force
                                    || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && GetCooldownRemainingTime(Bloodfest) > GCD * 12 && gauge.Ammo >= 2) // Lv90 1min 2 or 3cart
                                    || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && lastComboMove is SolidBarrel && IsOffCooldown(Bloodfest) && gauge.Ammo == 1 && quarterWeave) // <=Lv80 Opener/Reopener (1cart)
                                    || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && (GetCooldownRemainingTime(Bloodfest) > GCD * 12 || (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)) && gauge.Ammo == 2) && quarterWeave)) // <=Lv80 lateweave use
                                    return NoMercy;
                            }
                        }

                        // <Lv30
                        if (!LevelChecked(BurstStrike) && quarterWeave)
                            return NoMercy;
                    }

                    // oGCDs
                    if (CanWeave(actionID))
                    {
                        Status? sustainedDamage = FindTargetEffect(Variant.Debuffs.SustainedDamage);
                        if (IsEnabled(CustomComboPreset.GNB_Variant_SpiritDart) &&
                            IsEnabled(Variant.VariantSpiritDart) &&
                            (sustainedDamage is null || sustainedDamage?.RemainingTime <= 3))
                            return Variant.VariantSpiritDart;

                        if (IsEnabled(CustomComboPreset.GNB_Variant_Ultimatum) && IsEnabled(Variant.VariantUltimatum) && IsOffCooldown(Variant.VariantUltimatum))
                            return Variant.VariantUltimatum;

                        if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                        {
                            // Bloodfest
                            if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && ActionReady(Bloodfest) && gauge.Ammo is 0 && HasEffect(Buffs.NoMercy))
                            {
                                if (IsOnCooldown(NoMercy))
                                    return Bloodfest;
                            }

                            // Zone
                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone) && !JustUsed(NoMercy))
                            {
                                // Lv90
                                if (!LevelChecked(ReignOfBeasts) && !HasEffect(Buffs.NoMercy) && ((IsOnCooldown(GnashingFang) && GetCooldownRemainingTime(NoMercy) > 17) || // >=Lv60
                                    !LevelChecked(GnashingFang))) // <Lv60
                                    return OriginalHook(DangerZone);
                                // Lv100 use
                                if (LevelChecked(ReignOfBeasts) && (WasLastWeaponskill(DoubleDown) || GetCooldownRemainingTime(NoMercy) > 17))
                                    return OriginalHook(DangerZone);
                            }

                            // Hypervelocity
                            if (WasLastWeaponskill(BurstStrike) && LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;

                            // Continuation
                            if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                                (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                                return OriginalHook(Continuation);

                            // 60s weaves
                            if (HasEffect(Buffs.NoMercy) && (GetBuffRemainingTime(Buffs.NoMercy) < 17.5))
                            {
                                // >=Lv90
                                if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock) && LevelChecked(BowShock))
                                    return BowShock;
                                if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                    return OriginalHook(DangerZone);

                                // <Lv90
                                if (!LevelChecked(DoubleDown))
                                {
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock) && LevelChecked(BowShock))
                                        return BowShock;
                                }
                            }
                        }
                    }

                    // GF combo
                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                        (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                        return OriginalHook(Continuation);

                    // Sonic Break special conditions 
                    if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && GetBuffRemainingTime(Buffs.NoMercy) < GCD * 6 && HasEffect(Buffs.ReadyToBreak))
                    {
                        if (LevelChecked(ReignOfBeasts) || !LevelChecked(ReignOfBeasts))
                        {
                            // 1min 2cart
                            if (GetCooldownRemainingTime(Bloodfest) > GCD * 14 && GetCooldownRemainingTime(DoubleDown) > GCD * 14 && gauge.Ammo == 0 &&
                                !HasEffect(Buffs.ReadyToRip) && GetBuffRemainingTime(Buffs.NoMercy) < GCD * 5 &&
                                (WasLastWeaponskill(GnashingFang) || WasLastAbility(SavageClaw)))
                                return SonicBreak;

                            // sks 9th GCD
                            if (GetCooldownRemainingTime(Bloodfest) > GCD * 14 && GetCooldownRemainingTime(DoubleDown) > GCD * 14 && !HasEffect(Buffs.ReadyToReign) && gauge.Ammo == 2 &&
                                !HasEffect(Buffs.ReadyToRip) && GetBuffRemainingTime(Buffs.NoMercy) < GCD)
                                return SonicBreak;
                        }
                    }

                    // Double Down
                    if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup) && (HasEffect(Buffs.NoMercy) || GetBuffRemainingTime(Buffs.NoMercy) >= GCD * 4) && gauge.Ammo >= 2)
                    {
                        // Lv100
                        if (LevelChecked(ReignOfBeasts) && (IsEnabled(CustomComboPreset.GNB_ST_DoubleDown) && GetCooldownRemainingTime(DoubleDown) <= 0.6f))
                        {
                            if ((WasLastWeaponskill(SonicBreak) && gauge.Ammo == 2 && GetCooldownRemainingTime(Bloodfest) < GCD * 4 || IsOffCooldown(Bloodfest)) // 2min
                                || (!HasEffect(Buffs.ReadyToBreak) && WasLastWeaponskill(SonicBreak) && gauge.Ammo == 3) // 1min NM 3 carts
                                || (HasEffect(Buffs.ReadyToBreak) && WasLastWeaponskill(SolidBarrel) && gauge.Ammo == 3 && (GetBuffRemainingTime(Buffs.NoMercy) < 17))) // 1min NM 2 carts
                                return DoubleDown;
                        }

                        // Lv90
                        if (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && IsEnabled(CustomComboPreset.GNB_ST_DoubleDown) && GetCooldownRemainingTime(DoubleDown) <= 0.6f)
                        {
                            if ((gauge.Ammo == 3 && !HasEffect(Buffs.ReadyToBreak) && WasLastWeaponskill(SonicBreak) && (GetCooldownRemainingTime(Bloodfest) < GCD * 4 || IsOffCooldown(Bloodfest))) // 2min NM 3 carts
                                || (!HasEffect(Buffs.ReadyToBreak) && gauge.Ammo == 3 && WasLastWeaponskill(SonicBreak) && GetCooldownRemainingTime(Bloodfest) > GCD * 12) // 1min NM 3 carts
                                || (HasEffect(Buffs.ReadyToBreak) && gauge.Ammo == 3 && WasLastWeaponskill(SolidBarrel) && GetCooldownRemainingTime(Bloodfest) > GCD * 12)) // 1min NM 2 carts
                                return DoubleDown;
                        }

                        // <Lv90
                        if (!LevelChecked(DoubleDown) && !LevelChecked(ReignOfBeasts))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && HasEffect(Buffs.ReadyToBreak) && (GetBuffRemainingTime(Buffs.NoMercy) >= GCD * 4) && !HasEffect(Buffs.ReadyToRip) && IsOnCooldown(GnashingFang))
                                return SonicBreak;
                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone) && !LevelChecked(SonicBreak) && HasEffect(Buffs.NoMercy) || GetCooldownRemainingTime(NoMercy) < 30)  // subLv54
                                return OriginalHook(DangerZone);
                        }
                    }

                    // Sonic Break 
                    if (HasEffect(Buffs.NoMercy) && HasEffect(Buffs.ReadyToBreak))
                    {
                        // Lv100
                        if (LevelChecked(ReignOfBeasts))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                (!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 2 && (GetBuffRemainingTime(Buffs.NoMercy) > GCD * 7) &&
                                WasLastWeaponskill(BurstStrike) && (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest))) // 2min
                                || (!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 3 && (GetBuffRemainingTime(Buffs.NoMercy) < GCD) &&
                                (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest))) // 2min 3 carts
                                || (GetCooldownRemainingTime(Bloodfest) > GCD * 12 && gauge.Ammo == 3 &&
                                GetCooldownRemainingTime(NoMercy) > GCD &&
                                (WasLastWeaponskill(KeenEdge) || WasLastWeaponskill(BrutalShell) || WasLastWeaponskill(SolidBarrel)))) // 1min 3 carts
                                return SonicBreak;
                        }

                        // Lv90
                        if (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                (!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 3 &&
                                GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)) // 2min
                                || (GetCooldownRemainingTime(Bloodfest) > GCD * 12 && gauge.Ammo >= 2 && GetBuffRemainingTime(Buffs.NoMercy) > GCD * 7 &&
                                (WasLastWeaponskill(KeenEdge) || WasLastWeaponskill(BrutalShell) || WasLastWeaponskill(SolidBarrel)))) // 1min 3 carts
                                return SonicBreak;
                        }

                        // <Lv80
                        if (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                (!HasEffect(Buffs.ReadyToBlast) && GetCooldownRemainingTime(NoMercy) < 58 && WasLastWeaponskill(GnashingFang)))
                                return SonicBreak;
                        }
                    }

                    // Gnashing Fang
                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(GnashingFang) && GetCooldownRemainingTime(GnashingFang) <= 0.6f && gauge.Ammo > 0)
                    {
                        if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Starter) && !HasEffect(Buffs.ReadyToBlast) && gauge.AmmoComboStep == 0 
                            && (LevelChecked(ReignOfBeasts) && HasEffect(Buffs.NoMercy) && WasLastWeaponskill(DoubleDown)) // Lv100 odd/even minute use
                            || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && HasEffect(Buffs.NoMercy) && WasLastWeaponskill(DoubleDown)) // Lv90 odd/even minute use
                            || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && GetCooldownRemainingTime(NoMercy) > GCD * 20 && WasLastWeaponskill(DoubleDown)) // Lv90 odd minute scuffed windows
                            || (GetCooldownRemainingTime(NoMercy) > GCD * 4 && IsOffCooldown(Bloodfest)) // Opener/Reopener Conditions
                            || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && GetCooldownRemainingTime(NoMercy) >= GCD * 24) // <Lv90 odd/even minute use
                            || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && LevelChecked(Bloodfest) && gauge.Ammo == 1 && GetCooldownRemainingTime(NoMercy) >= GCD * 24 && IsOffCooldown(Bloodfest)) // <Lv90 Opener/Reopener
                            || (GetCooldownRemainingTime(NoMercy) > GCD * 7 && GetCooldownRemainingTime(NoMercy) < GCD * 14)) // 30s use
                            return GnashingFang;
                    }

                    // Reign combo
                    if (IsEnabled(CustomComboPreset.GNB_ST_Reign) && (LevelChecked(ReignOfBeasts)))
                    {
                        if (GetBuffRemainingTime(Buffs.ReadyToReign) > 0 && IsOnCooldown(GnashingFang) && IsOnCooldown(DoubleDown) && gauge.AmmoComboStep == 0 && GetCooldownRemainingTime(Bloodfest) > GCD * 12)
                        {
                            if (WasLastWeaponskill(WickedTalon) || (WasLastAbility(EyeGouge)))
                                return OriginalHook(ReignOfBeasts);
                        }

                        if (WasLastWeaponskill(ReignOfBeasts) || WasLastWeaponskill(NobleBlood))
                        {
                            return OriginalHook(ReignOfBeasts);
                        }
                    }

                    // Burst Strike
                    if (IsEnabled(CustomComboPreset.GNB_ST_BurstStrike) && LevelChecked(BurstStrike))
                    {
                        if (HasEffect(Buffs.NoMercy))
                        {
                            // Lv100 use
                            if ((LevelChecked(ReignOfBeasts)
                                && gauge.Ammo >= 1 && gauge.AmmoComboStep == 0
                                && GetBuffRemainingTime(Buffs.NoMercy) <= GCD * 3
                                && !HasEffect(Buffs.ReadyToReign))
                                // subLv90 use
                                || (!LevelChecked(ReignOfBeasts)
                                && gauge.Ammo >= 1 && gauge.AmmoComboStep == 0
                                && HasEffect(Buffs.NoMercy) && !HasEffect(Buffs.ReadyToBreak)
                                && IsOnCooldown(DoubleDown) && IsOnCooldown(GnashingFang)))
                                return BurstStrike;
                        }
                    }

                    // Lv100 2cart 2min starter
                    if (LevelChecked(ReignOfBeasts)
                        && GetCooldownRemainingTime(NoMercy) <= GCD
                        && gauge.Ammo is 3
                        && (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)))
                        return BurstStrike;

                    // GF combo
                    if (gauge.AmmoComboStep is 1 or 2) // GF
                        return OriginalHook(GnashingFang);

                    // 123 (overcap included)
                    if (comboTime > 0)
                    {
                        if (lastComboMove == KeenEdge && LevelChecked(BrutalShell))
                            return BrutalShell;
                        if (lastComboMove == BrutalShell && LevelChecked(SolidBarrel))
                        {
                            if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            if (LevelChecked(BurstStrike) && gauge.Ammo == MaxCartridges(level))
                                return BurstStrike;
                            return SolidBarrel;
                        }
                        if (LevelChecked(ReignOfBeasts) && HasEffect(Buffs.NoMercy) && gauge.AmmoComboStep == 0 && LevelChecked(BurstStrike) && (lastComboMove is BrutalShell) && gauge.Ammo == 2)
                            return SolidBarrel;
                        if (!LevelChecked(ReignOfBeasts) && HasEffect(Buffs.NoMercy) && gauge.AmmoComboStep == 0 && LevelChecked(BurstStrike) && (lastComboMove is BrutalShell || WasLastWeaponskill(BurstStrike)) && gauge.Ammo == 2)
                            return SolidBarrel;
                    }

                    return KeenEdge;
                }

                return actionID;
            }
        }

        internal class GNB_GF_Continuation : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_GF_Continuation;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID == GnashingFang)
                {
                    var gauge = GetJobGauge<GNBGauge>();
                    var quarterWeave = GetCooldownRemainingTime(actionID) < 1 && GetCooldownRemainingTime(actionID) > 0.6;
                    float GCD = GetCooldown(KeenEdge).CooldownTotal; // 2.5 supported, 2.45 is iffy

                    // No Mercy
                    if ((LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && gauge.Ammo == 0 && lastComboMove is BrutalShell && IsOffCooldown(Bloodfest)) // Lv100 Opener/Reopener (0cart)
                        || (LevelChecked(ReignOfBeasts) && gauge.Ammo >= 2 && IsOffCooldown(NoMercy)) // Lv100 on CD use (2 or 3 cart, never 1)
                        || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && gauge.Ammo == 0 && lastComboMove is BrutalShell && IsOffCooldown(Bloodfest)) // Lv90 Opener/Reopener (0cart)
                        || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)) && gauge.Ammo == 3) // Lv90 2min 3cart force
                        || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && GetCooldownRemainingTime(Bloodfest) > GCD * 12 && gauge.Ammo >= 2) // Lv90 1min 2 or 3cart
                        || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && lastComboMove is SolidBarrel && IsOffCooldown(Bloodfest) && gauge.Ammo == 1 && quarterWeave) // subLv80 Opener/Reopener (1cart)
                        || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && (GetCooldownRemainingTime(Bloodfest) > GCD * 12 || (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)) && gauge.Ammo == 2) && quarterWeave)) // subLv80 lateweave use
                        return NoMercy;

                    // oGCDs
                    if (CanWeave(actionID))
                    {
                        if (IsEnabled(CustomComboPreset.GNB_GF_Cooldowns))
                        {
                            // Bloodfest
                            if (ActionReady(Bloodfest) && gauge.Ammo is 0 && HasEffect(Buffs.NoMercy))
                            {
                                if (IsOnCooldown(NoMercy))
                                    return Bloodfest;
                            }

                            if (ActionReady(DangerZone) && !JustUsed(NoMercy))
                            {
                                // Lv90
                                if (!LevelChecked(ReignOfBeasts) && !HasEffect(Buffs.NoMercy) && ((IsOnCooldown(GnashingFang) && GetCooldownRemainingTime(NoMercy) > 17) || // >=Lv60
                                    !LevelChecked(GnashingFang))) // <Lv60
                                    return OriginalHook(DangerZone);
                                // Lv100
                                if (LevelChecked(ReignOfBeasts) && (WasLastWeaponskill(DoubleDown) || IsOnCooldown(DoubleDown) || GetCooldownRemainingTime(NoMercy) > 17))
                                    return OriginalHook(DangerZone);
                            }

                            // Hypervelocity
                            if (WasLastWeaponskill(BurstStrike) && LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;

                            // Continuation
                            if (LevelChecked(Continuation) && (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                                return OriginalHook(Continuation);

                            // Zone & BowShock weaves
                            if (HasEffect(Buffs.NoMercy) && (GetBuffRemainingTime(Buffs.NoMercy) < 17.5))
                            {
                                // >=Lv90
                                if (ActionReady(BowShock) && LevelChecked(BowShock))
                                    return BowShock;
                                if (ActionReady(DangerZone))
                                    return OriginalHook(DangerZone);

                                // <Lv90
                                if (!LevelChecked(DoubleDown))
                                {
                                    if (ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                    if (ActionReady(BowShock) && LevelChecked(BowShock))
                                        return BowShock;
                                }
                            }
                        }
                    }

                    // Sonic Break special conditions
                    if (IsEnabled(CustomComboPreset.GNB_GF_SonicBreak) && GetBuffRemainingTime(Buffs.NoMercy) < GCD * 6 && HasEffect(Buffs.ReadyToBreak))
                    {
                        if (LevelChecked(ReignOfBeasts) || !LevelChecked(ReignOfBeasts))
                        {
                            if (GetCooldownRemainingTime(Bloodfest) > GCD * 14 && GetCooldownRemainingTime(DoubleDown) > GCD * 14 && gauge.Ammo == 0 &&
                                !HasEffect(Buffs.ReadyToRip) && GetBuffRemainingTime(Buffs.NoMercy) < GCD * 5 &&
                                (WasLastWeaponskill(GnashingFang) || WasLastAbility(SavageClaw)))
                                return SonicBreak;

                            // sks 9th GCD
                            if (GetCooldownRemainingTime(Bloodfest) > GCD * 14 && GetCooldownRemainingTime(DoubleDown) > GCD * 14 && !HasEffect(Buffs.ReadyToReign) && gauge.Ammo == 2 &&
                                !HasEffect(Buffs.ReadyToRip) && GetBuffRemainingTime(Buffs.NoMercy) < GCD)
                                return SonicBreak;
                        }
                    }

                    // Double Down
                    if (IsEnabled(CustomComboPreset.GNB_GF_DoubleDown) && (HasEffect(Buffs.NoMercy) || GetBuffRemainingTime(Buffs.NoMercy) >= GCD * 4) && gauge.Ammo >= 2)
                    {
                        // Lv100
                        if (LevelChecked(ReignOfBeasts) && GetCooldownRemainingTime(DoubleDown) <= 0.6f)
                        {
                            if ((WasLastWeaponskill(SonicBreak) && gauge.Ammo == 2 && GetCooldownRemainingTime(Bloodfest) < GCD * 4 || IsOffCooldown(Bloodfest)) // 2min NM
                                || (!HasEffect(Buffs.ReadyToBreak) && WasLastWeaponskill(SonicBreak) && gauge.Ammo == 3) // 1min NM 3 carts
                                || (HasEffect(Buffs.ReadyToBreak) && WasLastWeaponskill(SolidBarrel) && gauge.Ammo == 3 && (GetBuffRemainingTime(Buffs.NoMercy) < 17))) // 1min NM 2 carts
                                return DoubleDown;
                        }

                        // Lv90
                        if (!LevelChecked(ReignOfBeasts) && GetCooldownRemainingTime(DoubleDown) <= 0.6f)
                        {
                            if ((gauge.Ammo == 3 && !HasEffect(Buffs.ReadyToBreak) && WasLastWeaponskill(SonicBreak) && (GetCooldownRemainingTime(Bloodfest) < GCD * 4 || IsOffCooldown(Bloodfest))) // 2min NM 3 carts
                                || (!HasEffect(Buffs.ReadyToBreak) && gauge.Ammo == 3 && WasLastWeaponskill(SonicBreak) && GetCooldownRemainingTime(Bloodfest) > GCD * 12) // 1min NM 3 carts
                                || (HasEffect(Buffs.ReadyToBreak) && gauge.Ammo == 3 && WasLastWeaponskill(SolidBarrel) && GetCooldownRemainingTime(Bloodfest) > GCD * 12)) // 1min NM 2 carts
                                return DoubleDown;
                        }

                        // <Lv90
                        if (!LevelChecked(DoubleDown) && !LevelChecked(ReignOfBeasts))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_GF_SonicBreak) && HasEffect(Buffs.ReadyToBreak) && (GetBuffRemainingTime(Buffs.NoMercy) >= GCD * 4) && !HasEffect(Buffs.ReadyToRip) && IsOnCooldown(GnashingFang))
                                return SonicBreak;
                            if (IsEnabled(CustomComboPreset.GNB_GF_Cooldowns) && ActionReady(DangerZone) && !LevelChecked(SonicBreak) && HasEffect(Buffs.NoMercy) || GetCooldownRemainingTime(NoMercy) < 30)  // subLv54
                                return OriginalHook(DangerZone);
                        }
                    }

                    // Sonic Break 
                    if (IsEnabled(CustomComboPreset.GNB_GF_SonicBreak) && HasEffect(Buffs.NoMercy) && HasEffect(Buffs.ReadyToBreak))
                    {
                        // Lv100
                        if (LevelChecked(ReignOfBeasts))
                        {
                            if ((!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 2 && (GetBuffRemainingTime(Buffs.NoMercy) > GCD * 7) &&
                                WasLastWeaponskill(BurstStrike) && (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest))) // 2min
                                || (!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 3 && (GetBuffRemainingTime(Buffs.NoMercy) < GCD) &&
                                (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest))) // 2min 3 carts
                                || (GetCooldownRemainingTime(Bloodfest) > GCD * 12 && gauge.Ammo == 3 && GetCooldownRemainingTime(NoMercy) > GCD &&
                                (WasLastWeaponskill(KeenEdge) || WasLastWeaponskill(BrutalShell) || WasLastWeaponskill(SolidBarrel)))) // 1min 3 carts
                                return SonicBreak;
                        }

                        // Lv90
                        if (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown))
                        {
                            if ((!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 3 &&
                                GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)) // 2min
                                || (GetCooldownRemainingTime(Bloodfest) > GCD * 12 && gauge.Ammo >= 2 && GetBuffRemainingTime(Buffs.NoMercy) > GCD * 7 &&
                                (WasLastWeaponskill(KeenEdge) || WasLastWeaponskill(BrutalShell) || WasLastWeaponskill(SolidBarrel)))) // 1min 3 carts
                                return SonicBreak;
                        }

                        // <Lv90
                        if (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown))
                        {
                            if ((!HasEffect(Buffs.ReadyToBlast) && GetCooldownRemainingTime(NoMercy) < 58 && WasLastWeaponskill(GnashingFang)))
                                return SonicBreak;
                        }
                    }

                    // Reign combo
                    if (IsEnabled(CustomComboPreset.GNB_GF_Reign) && (LevelChecked(ReignOfBeasts)))
                    {
                        if (GetBuffRemainingTime(Buffs.ReadyToReign) > 0 && IsOnCooldown(GnashingFang) && IsOnCooldown(DoubleDown) && gauge.AmmoComboStep == 0 && GetCooldownRemainingTime(Bloodfest) > GCD * 12)
                        {
                            if (WasLastWeaponskill(WickedTalon) || (WasLastAbility(EyeGouge)))
                                return OriginalHook(ReignOfBeasts);
                        }

                        if (WasLastWeaponskill(ReignOfBeasts) || WasLastWeaponskill(NobleBlood))
                        {
                            return OriginalHook(ReignOfBeasts);
                        }
                    }

                    if (IsEnabled(CustomComboPreset.GNB_GF_BurstStrike))
                    {
                        if ((LevelChecked(ReignOfBeasts) // Lv100
                                && gauge.Ammo >= 1 && gauge.AmmoComboStep == 0
                                && !HasEffect(Buffs.ReadyToReign) && GetBuffRemainingTime(Buffs.NoMercy) <= GCD * 3
                                && ((HasEffect(Buffs.NoMercy)) || (!HasEffect(Buffs.NoMercy) && gauge.Ammo == 3 && ComboAction == BrutalShell)))
                                // subLv90 use
                                || (!LevelChecked(ReignOfBeasts) // <=Lv90
                                && gauge.Ammo == MaxCartridges(level) && gauge.AmmoComboStep == 0
                                && HasEffect(Buffs.NoMercy) && !HasEffect(Buffs.ReadyToBreak)
                                && IsOnCooldown(DoubleDown) && IsOnCooldown(GnashingFang)))
                            return BurstStrike;
                        if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                            return Hypervelocity;
                    }
                }

                return actionID;
            }
        }


        internal class GNB_BS : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_BS;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is BurstStrike)
                {
                    var gauge = GetJobGauge<GNBGauge>();
                    float GCD = GetCooldown(KeenEdge).CooldownTotal; // 2.5 supported, 2.45 is iffy

                    if (IsEnabled(CustomComboPreset.GNB_BS_Continuation) && HasEffect(Buffs.ReadyToBlast) && LevelChecked(Hypervelocity))
                        return Hypervelocity;
                    if (IsEnabled(CustomComboPreset.GNB_BS_Bloodfest) && gauge.Ammo is 0 && LevelChecked(Bloodfest) && !HasEffect(Buffs.ReadyToBlast) && GetCooldownRemainingTime(Bloodfest) < 0.6f)
                        return Bloodfest;
                    if (IsEnabled(CustomComboPreset.GNB_BS_DoubleDown) && HasEffect(Buffs.NoMercy) && GetCooldownRemainingTime(DoubleDown) < 2 && gauge.Ammo >= 2 && LevelChecked(DoubleDown))
                        return DoubleDown;
                    if (IsEnabled(CustomComboPreset.GNB_BS_Reign) && (LevelChecked(ReignOfBeasts)))
                    {
                        if (HasEffect(Buffs.ReadyToReign) && GetBuffRemainingTime(Buffs.ReadyToReign) > 0 && IsOnCooldown(GnashingFang) && IsOnCooldown(DoubleDown) && gauge.AmmoComboStep == 0 && GetCooldownRemainingTime(Bloodfest) > GCD * 12)
                        {
                            if (WasLastWeaponskill(WickedTalon) || (WasLastAbility(EyeGouge)))
                                return OriginalHook(ReignOfBeasts);
                        }

                        if (WasLastWeaponskill(ReignOfBeasts) || WasLastWeaponskill(NobleBlood))
                        {
                            return OriginalHook(ReignOfBeasts);
                        }
                    }
                }

                return actionID;
            }
        }

        internal class GNB_AoE_MainCombo : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_AoE_MainCombo;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {

                if (actionID == DemonSlice)
                {
                    var gauge = GetJobGauge<GNBGauge>();
                    float GCD = GetCooldown(KeenEdge).CooldownTotal; // 2.5 supported, 2.45 is iffy
                    if (IsEnabled(CustomComboPreset.GNB_Variant_Cure) && IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= GetOptionValue(Config.GNB_VariantCure))
                        return Variant.VariantCure;

                    if (InCombat())
                    {
                        if (CanWeave(actionID))
                        {
                            Status? sustainedDamage = FindTargetEffect(Variant.Debuffs.SustainedDamage);
                            if (IsEnabled(CustomComboPreset.GNB_Variant_SpiritDart) &&
                                IsEnabled(Variant.VariantSpiritDart) &&
                                (sustainedDamage is null || sustainedDamage?.RemainingTime <= 3))
                                return Variant.VariantSpiritDart;

                            if (IsEnabled(CustomComboPreset.GNB_Variant_Ultimatum) && IsEnabled(Variant.VariantUltimatum) && IsOffCooldown(Variant.VariantUltimatum))
                                return Variant.VariantUltimatum;

                            if (IsEnabled(CustomComboPreset.GNB_AoE_NoMercy) && ActionReady(NoMercy))
                                return NoMercy;
                            if (IsEnabled(CustomComboPreset.GNB_AoE_BowShock) && ActionReady(BowShock) && GetCooldownRemainingTime(NoMercy) > 40)
                                return BowShock;
                            if (IsEnabled(CustomComboPreset.GNB_AOE_DangerZone) && ActionReady(DangerZone) && (HasEffect(Buffs.NoMercy) || GetCooldownRemainingTime(GnashingFang) <= GCD * 7))
                                return OriginalHook(DangerZone);
                            if (IsEnabled(CustomComboPreset.GNB_AoE_Bloodfest) && gauge.Ammo == 0 && ActionReady(Bloodfest) && HasEffect(Buffs.NoMercy))
                                return Bloodfest;
                            if (LevelChecked(FatedBrand) && HasEffect(Buffs.ReadyToRaze) && WasLastWeaponskill(FatedCircle))
                                return FatedBrand;
                        }

                        if (IsEnabled(CustomComboPreset.GNB_AOE_SonicBreak) && HasEffect(Buffs.ReadyToBreak) && !HasEffect(Buffs.ReadyToRaze) && GetBuffRemainingTime(Buffs.NoMercy) <= GCD)
                            return SonicBreak;
                        if (IsEnabled(CustomComboPreset.GNB_AoE_DoubleDown) && gauge.Ammo >= 2 && ActionReady(DoubleDown) && GetCooldownRemainingTime(NoMercy) > 40)
                            return DoubleDown;
                        if (IsEnabled(CustomComboPreset.GNB_AoE_Bloodfest) && LevelChecked(FatedCircle) && gauge.Ammo > 0 && (GetCooldownRemainingTime(Bloodfest) < 6
                            || (HasEffect(Buffs.NoMercy) && IsOnCooldown(DoubleDown))))
                            return FatedCircle;
                    }

                    if (comboTime > 0 && lastComboMove == DemonSlice && LevelChecked(DemonSlaughter))
                    {
                        return (IsEnabled(CustomComboPreset.GNB_AOE_Overcap) && LevelChecked(FatedCircle) && gauge.Ammo == MaxCartridges(level)) ? FatedCircle : DemonSlaughter;
                    }

                    return DemonSlice;
                }

                return actionID;
            }
        }

        internal class GNB_NoMercy_Cooldowns : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_NoMercy_Cooldowns;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID == NoMercy)
                {
                    var gauge = GetJobGauge<GNBGauge>().Ammo;
                    if (IsOnCooldown(NoMercy) && InCombat())
                    {
                        if (IsEnabled(CustomComboPreset.GNB_NoMercy_Cooldowns_DD) && GetCooldownRemainingTime(NoMercy) < 60 && IsOffCooldown(DoubleDown) && gauge >= 2 && LevelChecked(DoubleDown))
                            return DoubleDown;
                        if (IsEnabled(CustomComboPreset.GNB_NoMercy_Cooldowns_SonicBreakBowShock))
                        {
                            if (HasEffect(Buffs.ReadyToBreak))
                                return SonicBreak;
                            if (IsOffCooldown(BowShock))
                                return BowShock;
                        }
                    }
                }

                return actionID;
            }
        }

        internal class GNB_AuroraProtection : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_AuroraProtection;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Aurora)
                {
                    if ((HasFriendlyTarget() && TargetHasEffectAny(Buffs.Aurora)) || (!HasFriendlyTarget() && HasEffectAny(Buffs.Aurora)))
                        return OriginalHook(11);
                }
                return actionID;
            }
        }

        internal class GNB_ST_245 : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_245;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is KeenEdge)
                {
                    var gauge = GetJobGauge<GNBGauge>();
                    var quarterWeave = GetCooldownRemainingTime(actionID) < 1 && GetCooldownRemainingTime(actionID) > 0.6;
                    float GCD = GetCooldown(KeenEdge).CooldownTotal; // GCD is 2.45sks only
                    int minutes = CombatEngageDuration().Minutes;

                    // Variant Cure
                    if (IsEnabled(CustomComboPreset.GNB_Variant_Cure) && IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= GetOptionValue(Config.GNB_VariantCure))
                        return Variant.VariantCure;

                    // Ranged option
                    if (IsEnabled(CustomComboPreset.GNB_ST_RangedUptime) && !InMeleeRange() && LevelChecked(LightningShot) && HasBattleTarget())
                        return LightningShot;

                    //check to see if we'll get a cartridge right as NM comes off cooldown
                    if (LevelChecked(ReignOfBeasts) && !HasEffect(Buffs.NoMercy) && (GetCooldownRemainingTime(NoMercy) == GCD) && gauge.Ammo == MaxCartridges(level) && WasLastWeaponskill(BrutalShell) && CombatEngageDuration().TotalSeconds > 15)
                    {
                        if (IsOffCooldown(Bloodfest))
                        {
                            // Hypervelocity
                            if (WasLastWeaponskill(BurstStrike) && LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            if (WasLastAbility(NoMercy))
                                return SolidBarrel;
                            return BurstStrike;
                        }
                        else
                        {
                            // Hypervelocity
                            if (WasLastWeaponskill(BurstStrike) && LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            return BurstStrike;
                        }
                    }

                    //opener bloodfest
                    if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && ActionReady(Bloodfest) && gauge.Ammo == 0 && CombatEngageDuration().TotalSeconds < 5)
                    {
                        if (IsOffCooldown(NoMercy))
                            return Bloodfest;
                    }

                    // No Mercy
                    if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup) && IsEnabled(CustomComboPreset.GNB_ST_NoMercy))
                    {
                        if (ActionReady(NoMercy))
                        {
                            if (CanWeave(actionID))
                            {
                                if ((CombatEngageDuration().TotalSeconds < 30 && lastComboMove is BrutalShell && quarterWeave) // Opener
                                    || (LevelChecked(ReignOfBeasts) && gauge.Ammo == MaxCartridges(level) && IsOffCooldown(NoMercy) && CombatEngageDuration().TotalSeconds > 50 && quarterWeave) // Lv100 on CD use
                                    || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && minutes % 2 is 1 && gauge.Ammo >= 2 && IsOffCooldown(NoMercy)) // Lv90 1min On CD use
                                    || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && (GetCooldownRemainingTime(Bloodfest) < 30 || IsOffCooldown(Bloodfest)) && gauge.Ammo is 3) // Lv90 2min 3cart force
                                    || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && gauge.Ammo >= 1)) // subLv80 ON CD use
                                    return NoMercy;
                            }
                        }

                        // sub-Lv30
                        if (!LevelChecked(BurstStrike) && quarterWeave)
                            return NoMercy;
                    }

                    // Use of Gnashing Fang
                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(GnashingFang) && GetCooldownRemainingTime(GnashingFang) <= GCD - 0.5)
                    {
                        if ((IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Starter) && !HasEffect(Buffs.ReadyToBlast) && gauge.AmmoComboStep is 0 && HasEffect(Buffs.NoMercy)) // 60s use;
                            || (gauge.Ammo is 1 && HasEffect(Buffs.NoMercy) && GetCooldownRemainingTime(DoubleDown) > GCD * 4) //NMDDGF windows/Scuffed windows
                            || (gauge.Ammo > 0 && GetCooldownRemainingTime(NoMercy) > 17 && GetCooldownRemainingTime(NoMercy) < 35) // 30s use                                                                    
                            || (gauge.Ammo is 1 && GetCooldownRemainingTime(NoMercy) > GCD * 2 && ((IsOffCooldown(Bloodfest) && LevelChecked(Bloodfest)) || !LevelChecked(Bloodfest)))) // Opener Conditions
                            return GnashingFang;
                    }

                    // oGCDs
                    if (CanWeave(actionID))
                    {
                        if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                        {
                            // Continuation
                            if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                                (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                                return OriginalHook(Continuation);

                            // 60s weaves 90+
                            if (HasEffect(Buffs.NoMercy) && (GetBuffRemainingTime(Buffs.NoMercy) >= GCD * 2))
                            {
                                // Post DD
                                if (WasLastAbility(JugularRip))
                                {
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock) && LevelChecked(BowShock))
                                        return BowShock;
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                }

                                // Pre DD
                                if (!LevelChecked(DoubleDown))
                                {
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock) && LevelChecked(BowShock))
                                        return BowShock;
                                }
                            }

                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                            {
                                // Zone outside of NM
                                if (!HasEffect(Buffs.NoMercy) && ((IsOnCooldown(GnashingFang) && GetCooldownRemainingTime(NoMercy) > 17)
                                    || !LevelChecked(GnashingFang))) // Pre GF
                                    return OriginalHook(DangerZone);

                                // Lv100 use
                                if (HasEffect(Buffs.NoMercy) && LevelChecked(DoubleDown) && WasLastWeaponskill(DoubleDown))
                                    return OriginalHook(DangerZone);
                            }

                            //even minute bloodfest
                            if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && ActionReady(Bloodfest) && HasEffect(Buffs.NoMercy) && gauge.Ammo == 0)
                            {
                                if (IsOnCooldown(NoMercy))
                                    return Bloodfest;
                            }

                            // Hypervelocity
                            if (WasLastWeaponskill(BurstStrike) && LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;

                        }
                    }

                    // Double Down
                    if ((HasEffect(Buffs.NoMercy) || GetBuffRemainingTime(Buffs.NoMercy) >= GCD * 1) && IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                    {
                        // Lv100
                        if (LevelChecked(ReignOfBeasts) && WasLastWeaponskill(GnashingFang) && (IsEnabled(CustomComboPreset.GNB_ST_DoubleDown)))
                        {
                            //TODO deal with 1min 2 carts
                            if ((gauge.Ammo is 3 && IsOnCooldown(Bloodfest)) // 2min NM
                                || WasLastWeaponskill(GnashingFang) && gauge.Ammo is 2 // 1min NM 3 carts
                                || (HasEffect(Buffs.ReadyToBreak) && WasLastWeaponskill(SolidBarrel) && gauge.Ammo is 3 && (GetBuffRemainingTime(Buffs.NoMercy) < 17))) // 1min NM 2 carts TODO deal with this
                                return DoubleDown;
                        }

                        //TODO
                        // Lv90
                        if (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && IsEnabled(CustomComboPreset.GNB_ST_DoubleDown))
                        {
                            if ((gauge.Ammo is 3 && !HasEffect(Buffs.ReadyToBreak) && WasLastWeaponskill(SonicBreak) && (GetCooldownRemainingTime(Bloodfest) < GCD * 4 || IsOffCooldown(Bloodfest))) // 2min NM 3 carts
                                || (!HasEffect(Buffs.ReadyToBreak) && gauge.Ammo is 3 && WasLastWeaponskill(SonicBreak) && GetCooldownRemainingTime(Bloodfest) > 30) // 1min NM 3 carts
                                || (HasEffect(Buffs.ReadyToBreak) && gauge.Ammo is 3 && WasLastWeaponskill(SolidBarrel) && GetCooldownRemainingTime(Bloodfest) > 30)) // 1min NM 2 carts
                                return DoubleDown;
                        }

                        //TODO
                        // subLv80
                        if (!LevelChecked(DoubleDown) && !LevelChecked(ReignOfBeasts))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && HasEffect(Buffs.ReadyToBreak) && (GetBuffRemainingTime(Buffs.NoMercy) >= GCD * 4) && !HasEffect(Buffs.ReadyToRip) && IsOnCooldown(GnashingFang))
                                return SonicBreak;
                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone) && !LevelChecked(SonicBreak) && HasEffect(Buffs.NoMercy) || GetCooldownRemainingTime(NoMercy) < 30)  // subLv54
                                return OriginalHook(DangerZone);
                        }
                    }

                    // GF combo usage
                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) && (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                    {
                        return OriginalHook(Continuation);
                    }

                    // Reign combo
                    if (IsEnabled(CustomComboPreset.GNB_ST_Reign) && (LevelChecked(ReignOfBeasts) && (HasEffect(Buffs.NoMercy))))
                    {
                        //2 or 3 GCD, 2 for if you move out during R2R combo, 3 for less flexibility
                        if (HasEffect(Buffs.ReadyToReign) && GetBuffRemainingTime(Buffs.ReadyToReign) >= (2 * GCD))
                        {
                            if (WasLastWeaponskill(WickedTalon) || (WasLastAbility(EyeGouge)))
                                return OriginalHook(ReignOfBeasts);
                        }

                        if (WasLastWeaponskill(ReignOfBeasts) || WasLastWeaponskill(NobleBlood))
                        {
                            return OriginalHook(ReignOfBeasts);
                        }
                    }

                    // Burst Strike lower levels
                    if (!LevelChecked(ReignOfBeasts) && IsEnabled(CustomComboPreset.GNB_ST_BurstStrike))
                    {
                        if (HasEffect(Buffs.NoMercy))
                        {
                            if (gauge.Ammo >= 1 && gauge.AmmoComboStep is 0
                                && GetCooldownRemainingTime(NoMercy) <= GCD * 3
                                && !HasEffect(Buffs.ReadyToBreak))
                                return BurstStrike;
                        }
                    }

                    //Final burst strike usage 100 2 mins
                    if (LevelChecked(ReignOfBeasts) && gauge.Ammo >= 1)
                    {
                        // opener || 2mins
                        if ((WasLastWeaponskill(SolidBarrel) && (GetCooldownRemainingTime(NoMercy) > 31 && CombatEngageDuration().TotalSeconds < 70)) || (HasEffect(Buffs.NoMercy) && WasLastWeaponskill(LionHeart)))
                        {
                            return BurstStrike;
                        }
                    }

                    // Sonic Break 
                    if (HasEffect(Buffs.NoMercy) && HasEffect(Buffs.ReadyToBreak) && IsEnabled(CustomComboPreset.GNB_ST_SonicBreak))
                    {
                        // Lv100
                        if (LevelChecked(ReignOfBeasts))
                        {
                            // opener || 1/2mins
                            if ((GetBuffRemainingTime(Buffs.NoMercy) <= 20 - (GCD * 2) && CombatEngageDuration().TotalSeconds < 60)
                                || GetBuffRemainingTime(Buffs.NoMercy) <= GCD + 0.1)
                                return SonicBreak;
                        }

                        // Lv90 & below TODO
                        if (!LevelChecked(ReignOfBeasts))
                        {
                            // 2min
                            if ((IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                !HasEffect(Buffs.ReadyToBlast) && gauge.Ammo is 3 &&
                                GetCooldownRemainingTime(Bloodfest) < 30 || IsOffCooldown(Bloodfest))
                                // 1min 2 carts
                                || (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                GetCooldownRemainingTime(Bloodfest) > 30 && gauge.Ammo is 0 &&
                                !HasEffect(Buffs.ReadyToRip) && (GetBuffRemainingTime(Buffs.NoMercy) < GCD * 5) &&
                                WasLastWeaponskill(GnashingFang) || WasLastAbility(SavageClaw))
                                // 1min 3 carts
                                || (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                GetCooldownRemainingTime(Bloodfest) > 30 && gauge.Ammo is 3 && (GetBuffRemainingTime(Buffs.NoMercy) > GCD * 7) &&
                                (WasLastWeaponskill(KeenEdge) || WasLastWeaponskill(BrutalShell) || WasLastWeaponskill(SolidBarrel)))
                                // level 80
                                || (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                HasEffect(Buffs.NoMercy) && GetBuffRemainingTime(Buffs.NoMercy) >= GCD + 0.5))
                                return SonicBreak;
                        }
                    }

                    // GF combo
                    if (gauge.AmmoComboStep is 1 or 2) // GF
                        return OriginalHook(GnashingFang);

                    // 123 (overcap included)
                    if (comboTime > 0)
                    {
                        if (lastComboMove is KeenEdge && LevelChecked(BrutalShell))
                            return BrutalShell;
                        if (lastComboMove is BrutalShell && LevelChecked(SolidBarrel))
                        {
                            if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            if (LevelChecked(BurstStrike) && gauge.Ammo == MaxCartridges(level) && CombatEngageDuration().TotalSeconds > 10)
                                return BurstStrike;
                            return SolidBarrel;
                        }
                        if (HasEffect(Buffs.NoMercy) && gauge.AmmoComboStep is 0 && LevelChecked(BurstStrike) && lastComboMove is BrutalShell && gauge.Ammo is 2)
                            return SolidBarrel;
                    }

                    return KeenEdge;
                }

                return actionID;
            }
        }

        internal class GNB_ST_TEA : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_TEA;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is KeenEdge)
                {
                    var gauge = GetJobGauge<GNBGauge>();
                    var quarterWeave = GetCooldownRemainingTime(actionID) < 1 && GetCooldownRemainingTime(actionID) > 0.6;
                    float GCD = GetCooldown(KeenEdge).CooldownTotal; // GCD is 2.45sks only
                    int minutes = CombatEngageDuration().Minutes;

                    // Ranged option
                    if (IsEnabled(CustomComboPreset.GNB_ST_RangedUptime) && !InMeleeRange() && LevelChecked(LightningShot) && HasBattleTarget())
                        return LightningShot;

                    // No Mercy
                    if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup) && IsEnabled(CustomComboPreset.GNB_ST_NoMercy))
                    {
                        if (ActionReady(NoMercy))
                        {
                            if (CanWeave(actionID))
                            {
                                if ((CombatEngageDuration().TotalSeconds < 30 && lastComboMove is SolidBarrel && quarterWeave) // Opener
                                    || (gauge.Ammo >= 0)) // Lv80 ON CD use
                                    return NoMercy;
                            }
                        }
                    }

                    // oGCDs
                    if (CanWeave(actionID))
                    {
                        if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                        {
                            // Continuation
                            if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                                (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                                return OriginalHook(Continuation);

                            //opener bloodfest
                            if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && ActionReady(Bloodfest) && gauge.Ammo == 0 && CombatEngageDuration().TotalSeconds < 40 && HasEffect(Buffs.NoMercy))
                            {
                                if (IsOnCooldown(NoMercy))
                                    return Bloodfest;
                            }

                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                            {
                                // Zone outside of NM
                                if (!HasEffect(Buffs.NoMercy) && ((IsOnCooldown(GnashingFang) && GetCooldownRemainingTime(NoMercy) > 17) || // Post GF
                                    !LevelChecked(GnashingFang))) // Pre GF
                                    return OriginalHook(DangerZone);

                                // Lv100 use
                                if (HasEffect(Buffs.NoMercy) && LevelChecked(DoubleDown) && WasLastWeaponskill(DoubleDown))
                                    return OriginalHook(DangerZone);
                            }

                            // 60s weaves 90+
                            if (HasEffect(Buffs.NoMercy) && (GetBuffRemainingTime(Buffs.NoMercy) >= GCD * 2))
                            {
                                // Pre DD
                                if (!LevelChecked(DoubleDown))
                                {
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock) && LevelChecked(BowShock))
                                        return BowShock;
                                }
                            }

                            //even minute bloodfest
                            if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && ActionReady(Bloodfest) && HasEffect(Buffs.NoMercy) && gauge.Ammo == 0)
                            {
                                if (IsOnCooldown(NoMercy))
                                    return Bloodfest;
                            }
                        }
                    }

                    // Use of Gnashing Fang
                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(GnashingFang) && GetCooldownRemainingTime(GnashingFang) <= GCD + 0.5)
                    {
                        if ((IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Starter) && gauge.AmmoComboStep is 0 && HasEffect(Buffs.NoMercy)) // 60s use;
                            || (gauge.Ammo > 0 && GetCooldownRemainingTime(NoMercy) > 17 && GetCooldownRemainingTime(NoMercy) < 35) // 30s use                                                                    
                            || (gauge.Ammo is 1 && GetCooldownRemainingTime(NoMercy) > GCD * 2 && ((IsOffCooldown(Bloodfest) && LevelChecked(Bloodfest)) || !LevelChecked(Bloodfest)))) // Opener Conditions
                            return GnashingFang;
                    }

                    // GF combo usage
                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) && (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                    {
                        return OriginalHook(Continuation);
                    }

                    // Sonic Break 
                    if (HasEffect(Buffs.NoMercy) && HasEffect(Buffs.ReadyToBreak))
                    {

                        // Lv90 & below TODO
                        if (!LevelChecked(ReignOfBeasts))
                        {
                            // 2min
                            if (// level 80
                                (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                HasEffect(Buffs.NoMercy) && GetBuffRemainingTime(Buffs.NoMercy) >= GCD + 0.5))
                                return SonicBreak;
                        }
                    }

                    // Burst Strike lower levels
                    if (!LevelChecked(ReignOfBeasts) && IsEnabled(CustomComboPreset.GNB_ST_BurstStrike))
                    {
                        if (HasEffect(Buffs.NoMercy))
                        {
                            if (gauge.Ammo >= 1 && gauge.AmmoComboStep is 0
                                && GetCooldownRemainingTime(NoMercy) >= GCD * 3
                                && !HasEffect(Buffs.ReadyToBreak))
                                return BurstStrike;
                        }
                    }

                    // GF combo
                    if (gauge.AmmoComboStep is 1 or 2) // GF
                        return OriginalHook(GnashingFang);

                    // 123 (overcap included)
                    if (comboTime > 0)
                    {
                        if (lastComboMove is KeenEdge && LevelChecked(BrutalShell))
                            return BrutalShell;
                        if (lastComboMove is BrutalShell && LevelChecked(SolidBarrel))
                        {
                            if (LevelChecked(BurstStrike) && gauge.Ammo == MaxCartridges(level))
                                return BurstStrike;
                            return SolidBarrel;
                        }
                        if (HasEffect(Buffs.NoMercy) && gauge.AmmoComboStep is 0 && LevelChecked(BurstStrike) && lastComboMove is BrutalShell && gauge.Ammo is 2)
                            return SolidBarrel;
                    }

                    if (comboTime == 0 && gauge.Ammo == MaxCartridges(level))
                        return actionID;

                    return KeenEdge;
                }

                return actionID;
            }
        }

        internal class GNB_ST_245_Temp : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_245_Temp;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is KeenEdge)
                {
                    var gauge = GetJobGauge<GNBGauge>();
                    var quarterWeave = GetCooldownRemainingTime(actionID) < 1 && GetCooldownRemainingTime(actionID) > 0.6;
                    float GCD = GetCooldown(KeenEdge).CooldownTotal; // 2.5 supported, 2.45 is iffy

                    // Variant Cure
                    if (IsEnabled(CustomComboPreset.GNB_Variant_Cure) && IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= GetOptionValue(Config.GNB_VariantCure))
                        return Variant.VariantCure;

                    // Ranged Uptime
                    if (IsEnabled(CustomComboPreset.GNB_ST_RangedUptime) && !InMeleeRange() && LevelChecked(LightningShot) && HasBattleTarget())
                        return LightningShot;


                    // Bloodfest Opener
                    if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && ActionReady(Bloodfest) && gauge.Ammo is 0 && LevelChecked(ReignOfBeasts))
                    {
                        if (IsOffCooldown(NoMercy))
                            return Bloodfest;
                    }

                    // No Mercy
                    if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup) && IsEnabled(CustomComboPreset.GNB_ST_NoMercy))
                    {
                        if (ActionReady(NoMercy))
                        {
                            if (CanWeave(actionID))
                            {
                                if ((LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && gauge.Ammo == 0 && lastComboMove is BrutalShell && IsOffCooldown(Bloodfest) && GCD == 2.5) // Lv100 Opener/Reopener (0cart)
                                    || (LevelChecked(ReignOfBeasts) && gauge.Ammo >= 2 && IsOffCooldown(NoMercy) && GCD == 2.5) // Lv100 on CD use (2 or 3 cart, never 1)
                                    || (LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && gauge.Ammo == 3 && lastComboMove is BrutalShell && IsOnCooldown(Bloodfest) && quarterWeave) // Level 100 2.45 Opener
                                    || (LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && gauge.Ammo == 3 && lastComboMove is BrutalShell && IsOffCooldown(Bloodfest) && quarterWeave) // Level 100 2.45 Even Minute
                                    || (LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && gauge.Ammo == 3 && IsOnCooldown(Bloodfest) && combatDuration.Seconds > 20) // Level 100 2.45 Odd Minute
                                    || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && gauge.Ammo == 0 && lastComboMove is BrutalShell && IsOffCooldown(Bloodfest)) // Lv90 Opener/Reopener (0cart)
                                    || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)) && gauge.Ammo == 3) // Lv90 2min 3cart force
                                    || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && GetCooldownRemainingTime(Bloodfest) > GCD * 12 && gauge.Ammo >= 2) // Lv90 1min 2 or 3cart
                                    || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && lastComboMove is SolidBarrel && IsOffCooldown(Bloodfest) && gauge.Ammo == 1 && quarterWeave) // <=Lv80 Opener/Reopener (1cart)
                                    || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && (GetCooldownRemainingTime(Bloodfest) > GCD * 12 || (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)) && gauge.Ammo == 2) && quarterWeave)) // <=Lv80 lateweave use
                                    return NoMercy;
                            }
                        }

                        // <Lv30
                        if (!LevelChecked(BurstStrike) && quarterWeave)
                            return NoMercy;
                    }

                    // oGCDs
                    if (CanWeave(actionID))
                    {
                        Status? sustainedDamage = FindTargetEffect(Variant.Debuffs.SustainedDamage);
                        if (IsEnabled(CustomComboPreset.GNB_Variant_SpiritDart) &&
                            IsEnabled(Variant.VariantSpiritDart) &&
                            (sustainedDamage is null || sustainedDamage?.RemainingTime <= 3))
                            return Variant.VariantSpiritDart;

                        if (IsEnabled(CustomComboPreset.GNB_Variant_Ultimatum) && IsEnabled(Variant.VariantUltimatum) && IsOffCooldown(Variant.VariantUltimatum))
                            return Variant.VariantUltimatum;

                        if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                        {
                            // Bloodfest
                            if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && ActionReady(Bloodfest) && gauge.Ammo is 0 && HasEffect(Buffs.NoMercy))
                            {
                                if (IsOnCooldown(NoMercy))
                                    return Bloodfest;
                            }

                            // Zone
                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone) && !JustUsed(NoMercy))
                            {
                                // Lv90
                                if (!LevelChecked(ReignOfBeasts) && !HasEffect(Buffs.NoMercy) && ((IsOnCooldown(GnashingFang) && GetCooldownRemainingTime(NoMercy) > 17) || // >=Lv60
                                    !LevelChecked(GnashingFang))) // <Lv60
                                    return OriginalHook(DangerZone);
                                // Lv100 use
                                if (LevelChecked(ReignOfBeasts) && (WasLastWeaponskill(DoubleDown) || GetCooldownRemainingTime(NoMercy) > 17))
                                    return OriginalHook(DangerZone);
                            }

                            // Hypervelocity
                            if (WasLastWeaponskill(BurstStrike) && LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;

                            // Continuation
                            if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                                (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                                return OriginalHook(Continuation);

                            // 60s weaves
                            if (HasEffect(Buffs.NoMercy))
                            {
                                // >=Lv90
                                if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock) && LevelChecked(BowShock) && WasLastWeaponskill(GnashingFang))
                                    return BowShock;
                                if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                    return OriginalHook(DangerZone);

                                // <Lv90
                                if (!LevelChecked(DoubleDown))
                                {
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone))
                                        return OriginalHook(DangerZone);
                                    if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ActionReady(BowShock) && LevelChecked(BowShock))
                                        return BowShock;
                                }
                            }
                        }
                    }

                    // GF combo
                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                        (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                        return OriginalHook(Continuation);

                    // Sonic Break special conditions 
                    if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && GetBuffRemainingTime(Buffs.NoMercy) < GCD * 6 && HasEffect(Buffs.ReadyToBreak))
                    {
                        if (LevelChecked(ReignOfBeasts) || !LevelChecked(ReignOfBeasts))
                        {
                            // 1min 2cart
                            if (GetCooldownRemainingTime(Bloodfest) > GCD * 14 && GetCooldownRemainingTime(DoubleDown) > GCD * 14 && gauge.Ammo == 0 &&
                                !HasEffect(Buffs.ReadyToRip) && GetBuffRemainingTime(Buffs.NoMercy) < GCD * 5 &&
                                (WasLastWeaponskill(GnashingFang) || WasLastAbility(SavageClaw)))
                                return SonicBreak;

                            // sks 9th GCD
                            if (GetCooldownRemainingTime(Bloodfest) > GCD * 14 && GetCooldownRemainingTime(DoubleDown) > GCD * 14 && !HasEffect(Buffs.ReadyToReign) && gauge.Ammo == 2 &&
                                !HasEffect(Buffs.ReadyToRip) && GetBuffRemainingTime(Buffs.NoMercy) < GCD)
                                return SonicBreak;
                        }
                    }

                    // Gnashing Fang
                    if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(GnashingFang) && GetCooldownRemainingTime(GnashingFang) <= 0.6f && gauge.Ammo > 0)
                    {
                        if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Starter) && !HasEffect(Buffs.ReadyToBlast) && gauge.AmmoComboStep == 0
                            && (LevelChecked(ReignOfBeasts) && HasEffect(Buffs.NoMercy)) // Lv100 odd/even minute use
                            || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && HasEffect(Buffs.NoMercy) && WasLastWeaponskill(DoubleDown)) // Lv90 odd/even minute use
                            || (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && GetCooldownRemainingTime(NoMercy) > GCD * 20 && WasLastWeaponskill(DoubleDown)) // Lv90 odd minute scuffed windows
                            || (GetCooldownRemainingTime(NoMercy) > GCD * 4 && IsOffCooldown(Bloodfest)) // Opener/Reopener Conditions
                            || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && GetCooldownRemainingTime(NoMercy) >= GCD * 24) // <Lv90 odd/even minute use
                            || (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown) && LevelChecked(Bloodfest) && gauge.Ammo == 1 && GetCooldownRemainingTime(NoMercy) >= GCD * 24 && IsOffCooldown(Bloodfest)) // <Lv90 Opener/Reopener
                            || (GetCooldownRemainingTime(NoMercy) > GCD * 7 && GetCooldownRemainingTime(NoMercy) < GCD * 14)) // 30s use
                            return GnashingFang;
                    }

                    // Double Down
                    if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup) && (HasEffect(Buffs.NoMercy) || GetBuffRemainingTime(Buffs.NoMercy) >= GCD * 4) && gauge.Ammo >= 2)
                    {
                        // Lv100
                        if (LevelChecked(ReignOfBeasts) && (IsEnabled(CustomComboPreset.GNB_ST_DoubleDown) && GetCooldownRemainingTime(DoubleDown) <= 0.6f))
                        {
                            if ( (WasLastWeaponskill(GnashingFang) && gauge.Ammo == 2 || IsOffCooldown(Bloodfest)) //2 min
                                || (WasLastWeaponskill(GnashingFang) && gauge.Ammo == 2 && GetCooldownRemainingTime(Bloodfest) < GCD * 4 || IsOffCooldown(Bloodfest)) // 1min
                                || (HasEffect(Buffs.ReadyToBreak) && WasLastWeaponskill(SolidBarrel) && gauge.Ammo == 3 && (GetBuffRemainingTime(Buffs.NoMercy) < 17))) // 1min NM 2 carts
                                return DoubleDown;
                        }

                        // Lv90
                        if (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown) && IsEnabled(CustomComboPreset.GNB_ST_DoubleDown) && GetCooldownRemainingTime(DoubleDown) <= 0.6f)
                        {
                            if ((gauge.Ammo == 3 && !HasEffect(Buffs.ReadyToBreak) && WasLastWeaponskill(SonicBreak) && (GetCooldownRemainingTime(Bloodfest) < GCD * 4 || IsOffCooldown(Bloodfest))) // 2min NM 3 carts
                                || (!HasEffect(Buffs.ReadyToBreak) && gauge.Ammo == 3 && WasLastWeaponskill(SonicBreak) && GetCooldownRemainingTime(Bloodfest) > GCD * 12) // 1min NM 3 carts
                                || (HasEffect(Buffs.ReadyToBreak) && gauge.Ammo == 3 && WasLastWeaponskill(SolidBarrel) && GetCooldownRemainingTime(Bloodfest) > GCD * 12)) // 1min NM 2 carts
                                return DoubleDown;
                        }

                        // <Lv90
                        if (!LevelChecked(DoubleDown) && !LevelChecked(ReignOfBeasts))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && HasEffect(Buffs.ReadyToBreak) && (GetBuffRemainingTime(Buffs.NoMercy) >= GCD * 4) && !HasEffect(Buffs.ReadyToRip) && IsOnCooldown(GnashingFang))
                                return SonicBreak;
                            if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && ActionReady(DangerZone) && !LevelChecked(SonicBreak) && HasEffect(Buffs.NoMercy) || GetCooldownRemainingTime(NoMercy) < 30)  // subLv54
                                return OriginalHook(DangerZone);
                        }
                    }

                    // Sonic Break 
                    if (HasEffect(Buffs.NoMercy) && HasEffect(Buffs.ReadyToBreak))
                    {
                        // Lv100
                        if (LevelChecked(ReignOfBeasts))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                (!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 2 && (GetBuffRemainingTime(Buffs.NoMercy) > GCD * 7) &&
                                WasLastWeaponskill(BurstStrike) && (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)) && GCD == 2.5) // 2min 2.5 GCD
                                || (!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 0 && WasLastWeaponskill(DoubleDown)) // 2min opener
                                || (!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 0 && (GetBuffRemainingTime(Buffs.NoMercy) <= GCD) &&
                                WasLastWeaponskill(BurstStrike)) //2 min reopener 2.45 
                                || (!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 3 && (GetBuffRemainingTime(Buffs.NoMercy) < GCD) &&
                                (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest) && GCD == 2.5)) // 2min 3 carts
                                || (GetCooldownRemainingTime(Bloodfest) > GCD * 12 && gauge.Ammo == 3 &&
                                GetCooldownRemainingTime(NoMercy) > GCD &&
                                (WasLastWeaponskill(KeenEdge) || WasLastWeaponskill(BrutalShell) || WasLastWeaponskill(SolidBarrel)) && GCD == 2.5)) // 1min 3 carts
                                return SonicBreak;
                        }

                        // Lv90
                        if (!LevelChecked(ReignOfBeasts) && LevelChecked(DoubleDown))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                (!HasEffect(Buffs.ReadyToBlast) && gauge.Ammo == 3 &&
                                GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)) // 2min
                                || (GetCooldownRemainingTime(Bloodfest) > GCD * 12 && gauge.Ammo >= 2 && GetBuffRemainingTime(Buffs.NoMercy) > GCD * 7 &&
                                (WasLastWeaponskill(KeenEdge) || WasLastWeaponskill(BrutalShell) || WasLastWeaponskill(SolidBarrel)))) // 1min 3 carts
                                return SonicBreak;
                        }

                        // <Lv80
                        if (!LevelChecked(ReignOfBeasts) && !LevelChecked(DoubleDown))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) &&
                                (!HasEffect(Buffs.ReadyToBlast) && GetCooldownRemainingTime(NoMercy) < 58 && WasLastWeaponskill(GnashingFang)))
                                return SonicBreak;
                        }
                    }

                    // Reign combo
                    if (IsEnabled(CustomComboPreset.GNB_ST_Reign) && (LevelChecked(ReignOfBeasts)))
                    {
                        if (GetBuffRemainingTime(Buffs.ReadyToReign) > 0 && IsOnCooldown(GnashingFang) && IsOnCooldown(DoubleDown) && gauge.AmmoComboStep == 0 && GetCooldownRemainingTime(Bloodfest) > GCD * 12)
                        {
                            if (WasLastWeaponskill(WickedTalon) || (WasLastAbility(EyeGouge)))
                                return OriginalHook(ReignOfBeasts);
                        }

                        if (WasLastWeaponskill(ReignOfBeasts) || WasLastWeaponskill(NobleBlood))
                        {
                            return OriginalHook(ReignOfBeasts);
                        }
                    }

                    // Burst Strike
                    if (IsEnabled(CustomComboPreset.GNB_ST_BurstStrike) && LevelChecked(BurstStrike))
                    {
                        if (HasEffect(Buffs.NoMercy))
                        {
                            // Lv100 use
                            if ((LevelChecked(ReignOfBeasts)
                                && gauge.Ammo >= 1 && gauge.AmmoComboStep == 0
                                && GetBuffRemainingTime(Buffs.NoMercy) <= GCD * 3
                                && !HasEffect(Buffs.ReadyToReign))
                                // subLv90 use
                                || (!LevelChecked(ReignOfBeasts)
                                && gauge.Ammo >= 1 && gauge.AmmoComboStep == 0
                                && HasEffect(Buffs.NoMercy) && !HasEffect(Buffs.ReadyToBreak)
                                && IsOnCooldown(DoubleDown) && IsOnCooldown(GnashingFang)))
                                return BurstStrike;
                        }

                        if (LevelChecked(ReignOfBeasts) && gauge.Ammo == 1 && gauge.AmmoComboStep == 0 && GetCooldownRemainingTime(NoMercy) >= 35)  //Level 100 2 min use)
                            return BurstStrike;
                    }

                    //// Lv100 2cart 2min starter
                    //if (LevelChecked(ReignOfBeasts)
                    //    && GetCooldownRemainingTime(NoMercy) <= GCD
                    //    && gauge.Ammo is 3
                    //    && (GetCooldownRemainingTime(Bloodfest) < GCD * 12 || IsOffCooldown(Bloodfest)))
                    //    return BurstStrike;

                    // GF combo
                    if (gauge.AmmoComboStep is 1 or 2) // GF
                        return OriginalHook(GnashingFang);

                    // 123 (overcap included)
                    if (comboTime > 0)
                    {
                        if (lastComboMove == KeenEdge && LevelChecked(BrutalShell))
                            return BrutalShell;
                        if (lastComboMove == BrutalShell && LevelChecked(SolidBarrel))
                        {
                            if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            if (LevelChecked(BurstStrike) && gauge.Ammo == MaxCartridges(level) && IsOnCooldown(Bloodfest))
                                return BurstStrike;
                            return SolidBarrel;
                        }
                        if (LevelChecked(ReignOfBeasts) && HasEffect(Buffs.NoMercy) && gauge.AmmoComboStep == 0 && LevelChecked(BurstStrike) && (lastComboMove is BrutalShell) && gauge.Ammo == 2)
                            return SolidBarrel;
                        if (!LevelChecked(ReignOfBeasts) && HasEffect(Buffs.NoMercy) && gauge.AmmoComboStep == 0 && LevelChecked(BurstStrike) && (lastComboMove is BrutalShell || WasLastWeaponskill(BurstStrike)) && gauge.Ammo == 2)
                            return SolidBarrel;
                    }

                    return KeenEdge;
                }

                return actionID;
            }

        }
    }
}