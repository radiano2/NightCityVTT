using System;
using System.Collections.Generic;

namespace NightCityVTT.Shared.Services;

/// <summary>
/// Hit Locations mapped to 1d10 rolls.
/// </summary>
public enum HitLocation { Head = 1, Torso, RightArm, LeftArm, RightLeg, LeftLeg }

/// <summary>
/// Wound states mapped to Total Damage Taken.
/// </summary>
public enum WoundState { Uninjured, Light, Serious, Critical, Mortal0, Mortal1, Mortal2, Mortal3, Mortal4, Mortal5, Mortal6, Dead }

/// <summary>
/// Represents a character's state in combat for FNFF calculations.
/// </summary>
public class CharacterCombatState
{
    public int REF { get; set; } = 5;
    public int BODY { get; set; } = 5;
    public int COOL { get; set; } = 5;
    
    public Dictionary<string, int> Skills { get; set; } = new();
    
    // SP per hit location
    public Dictionary<HitLocation, int> ArmorSP { get; set; } = new();
    
    public int TotalDamageTaken { get; set; }
    
    /// <summary>
    /// Body Type Modifier (BTM). Reduces penetrating damage (minimum 1).
    /// </summary>
    public int BTM => BODY switch
    {
        <= 2 => 0,
        <= 4 => -1,
        <= 7 => -2,
        <= 9 => -3,
        10 => -4,
        _ => -5
    };
    
    /// <summary>
    /// Tracks wounds across the Cyberpunk 2020 track (4 points per state).
    /// </summary>
    public WoundState CurrentWoundState => TotalDamageTaken switch
    {
        0 => WoundState.Uninjured,
        <= 4 => WoundState.Light,
        <= 8 => WoundState.Serious,
        <= 12 => WoundState.Critical,
        <= 16 => WoundState.Mortal0,
        <= 20 => WoundState.Mortal1,
        <= 24 => WoundState.Mortal2,
        <= 28 => WoundState.Mortal3,
        <= 32 => WoundState.Mortal4,
        <= 36 => WoundState.Mortal5,
        <= 40 => WoundState.Mortal6,
        _ => WoundState.Dead
    };

    /// <summary>
    /// Penalty to Stun/Shock saves based on damage.
    /// </summary>
    public int StunSaveModifier => TotalDamageTaken switch
    {
        <= 4 => 0,
        <= 8 => -1,
        <= 12 => -2,
        <= 16 => -3,
        <= 20 => -4,
        <= 24 => -5,
        <= 28 => -6,
        <= 32 => -7,
        <= 36 => -8,
        <= 40 => -9,
        _ => -10
    };
}

/// <summary>
/// Weapon statistics for FNFF calculations.
/// </summary>
public class WeaponStats
{
    public int WA { get; set; }
    public string DamageDice { get; set; } = "1D6"; 
    public int ROF { get; set; }
    public int BaseRange { get; set; }
    public string WeaponType { get; set; } = "Pistol";
}

public class HitResult
{
    public bool IsHit { get; set; }
    public bool IsCritical { get; set; }
    public bool IsFumble { get; set; }
    public int TotalRoll { get; set; }
    public int TargetNumber { get; set; }
}

public class DamageResult
{
    public HitLocation Location { get; set; }
    public int RawDamage { get; set; }
    public int PenetratingDamage { get; set; }
    public int FinalDamageApplied { get; set; }
    public WoundState NewWoundState { get; set; }
    public bool RequiresStunSave { get; set; }
    public bool RequiresDeathSave { get; set; }
}

/// <summary>
/// Friday Night Firefight (FNFF) Engine encapsulating core mathematical logic.
/// </summary>
public class FnffEngine
{
    private readonly Random _rng = new();

    public int Roll1D10() => _rng.Next(1, 11);

    /// <summary>
    /// Rolls 1d10 with Exploding Tens and Fumble on 1.
    /// </summary>
    public int RollExploding1D10(out bool isFumble, out bool isCrit)
    {
        isFumble = false;
        isCrit = false;
        
        int roll = Roll1D10();
        if (roll == 1)
        {
            isFumble = true;
            return roll;
        }
        
        int total = roll;
        while (roll == 10)
        {
            isCrit = true;
            roll = Roll1D10();
            total += roll;
        }
        return total;
    }

    /// <summary>
    /// Calculates To-Hit logic: 1d10 + REF + Skill + WA + Modifiers vs Range Target.
    /// </summary>
    public HitResult RollToHit(CharacterCombatState attacker, WeaponStats weapon, string skillName, int targetDistanceMeters, int modifiers = 0)
    {
        int targetNumber = DetermineTargetNumber(weapon.BaseRange, targetDistanceMeters);
        int skillLevel = attacker.Skills.GetValueOrDefault(skillName, 0);
        
        int rollTotal = RollExploding1D10(out bool isFumble, out bool isCrit);
        
        // On fumble, usually a separate fumble table is rolled, but for To-Hit calculation it automatically misses or takes penalties.
        int finalTotal = isFumble ? rollTotal : (rollTotal + attacker.REF + skillLevel + weapon.WA + modifiers);
        
        return new HitResult
        {
            IsHit = !isFumble && (finalTotal >= targetNumber),
            IsCritical = isCrit,
            IsFumble = isFumble,
            TotalRoll = finalTotal,
            TargetNumber = targetNumber
        };
    }

    /// <summary>
    /// FNFF Range Table (Point Blank=10, Close=15, Medium=20, Long=25, Extreme=30).
    /// </summary>
    private int DetermineTargetNumber(int baseRange, int targetDistance)
    {
        if (targetDistance <= 1) return 10; 
        if (targetDistance <= baseRange / 4) return 15; 
        if (targetDistance <= baseRange / 2) return 20; 
        if (targetDistance <= baseRange) return 25; 
        return 30; // Extreme
    }

    /// <summary>
    /// Rolls 1d10 and resolves the hit location.
    /// </summary>
    public HitLocation RollHitLocation()
    {
        int roll = Roll1D10();
        return roll switch
        {
            1 => HitLocation.Head,
            2 or 3 or 4 => HitLocation.Torso,
            5 => HitLocation.RightArm,
            6 => HitLocation.LeftArm,
            7 or 8 => HitLocation.RightLeg,
            _ => HitLocation.LeftLeg
        };
    }

    /// <summary>
    /// Resolves damage, applying Cover, Armor Layering, SP, Headshot multipliers, and BTM.
    /// </summary>
    public DamageResult CalculateDamage(CharacterCombatState target, string damageDice, HitLocation location, int coverSP = 0, bool armorLayeringActive = false)
    {
        int rawDamage = ParseAndRollDamage(damageDice);
        int targetSP = target.ArmorSP.GetValueOrDefault(location, 0);
        
        // Simplified Armor Layering: Proportional Bonus (adds +5 SP as a flat proxy for layered soft/hard armor in this engine context)
        if (armorLayeringActive && targetSP > 0)
        {
            targetSP += 5; 
        }
        
        int totalSP = targetSP + coverSP;
        int penetratingDamage = rawDamage - totalSP;
        int finalDamage = 0;
        
        if (penetratingDamage > 0)
        {
            // Headshots double the damage that penetrates armor
            if (location == HitLocation.Head)
            {
                penetratingDamage *= 2;
            }

            // Apply BTM (reduces damage, minimum of 1 damage goes through)
            int btmApplied = penetratingDamage + target.BTM;
            finalDamage = Math.Max(1, btmApplied);
            
            target.TotalDamageTaken += finalDamage;
        }

        return new DamageResult
        {
            Location = location,
            RawDamage = rawDamage,
            PenetratingDamage = penetratingDamage,
            FinalDamageApplied = finalDamage,
            NewWoundState = target.CurrentWoundState,
            RequiresStunSave = finalDamage > 0,
            RequiresDeathSave = target.CurrentWoundState >= WoundState.Mortal0
        };
    }

    /// <summary>
    /// Rolls 1d10 against BODY minus wound modifiers.
    /// </summary>
    public bool MakeStunSave(CharacterCombatState target)
    {
        int roll = Roll1D10();
        int saveTarget = target.BODY + target.StunSaveModifier;
        return roll <= saveTarget;
    }

    /// <summary>
    /// Rolls 1d10 against BODY minus Mortal wound modifiers.
    /// </summary>
    public bool MakeDeathSave(CharacterCombatState target)
    {
        int roll = Roll1D10();
        
        int deathModifier = target.CurrentWoundState switch
        {
            WoundState.Mortal0 => 0,
            WoundState.Mortal1 => -1,
            WoundState.Mortal2 => -2,
            WoundState.Mortal3 => -3,
            WoundState.Mortal4 => -4,
            WoundState.Mortal5 => -5,
            WoundState.Mortal6 => -6,
            WoundState.Dead => -99,
            _ => 0
        };

        int saveTarget = target.BODY + deathModifier;
        return roll <= saveTarget;
    }

    /// <summary>
    /// Parses common RPG dice notation (e.g., "2D6", "1D6+1") and rolls it.
    /// </summary>
    private int ParseAndRollDamage(string diceExpr)
    {
        int total = 0;
        string expr = diceExpr.ToUpperInvariant().Trim();
        int modifier = 0;
        
        if (expr.Contains('+'))
        {
            var parts = expr.Split('+');
            expr = parts[0];
            int.TryParse(parts[1], out modifier);
        }
        else if (expr.Contains('-'))
        {
            var parts = expr.Split('-');
            expr = parts[0];
            if (parts.Length > 1 && int.TryParse(parts[1], out int mod))
            {
                modifier = -mod;
            }
        }

        if (expr.Contains('D'))
        {
            var parts = expr.Split('D');
            if (int.TryParse(parts[0], out int count) && int.TryParse(parts[1], out int sides))
            {
                for (int i = 0; i < count; i++)
                {
                    total += _rng.Next(1, sides + 1);
                }
            }
        }
        else
        {
            int.TryParse(expr, out total);
        }

        return Math.Max(0, total + modifier);
    }
}
