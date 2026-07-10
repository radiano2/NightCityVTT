using NightCityVTT.Shared.Models;

namespace NightCityVTT.Shared.Services;

public class DiceEngine
{
    private readonly Random _rng = new();

    private int D10() => _rng.Next(1, 11);
    private int D6()  => _rng.Next(1, 7);

    public RollResult RollSkillCheck(int stat, int skill, int luckSpent)
    {
        var result = new RollResult { Label = "SKILL CHECK" };

        int roll = D10();
        result.Dice.Add(roll);

        if (roll == 1)
        {
            result.IsFumble = true;
            result.Total = 0;
            result.Log.Add($"> [FUMBLE] Natural 1 — critical failure! No modifiers apply.");
            return result;
        }

        int diceTotal = roll;

        // Exploding 10s: re-roll and accumulate
        while (roll == 10)
        {
            result.IsCriticalSuccess = true;
            int extra = D10();
            result.Dice.Add(extra);
            result.Log.Add($"> [CRITICAL!] Natural 10! Extra roll: {extra}.");
            diceTotal += extra;
            roll = extra;
        }

        result.Total = diceTotal + stat + skill + luckSpent;
        result.Log.Add(
            $"> [SKILL CHECK] Dice: {string.Join("+", result.Dice)} + STAT:{stat} + SKILL:{skill} + LUCK:{luckSpent} = {result.Total}");
        return result;
    }

    public RollResult RollDamage(string damageFormula)
    {
        var result = new RollResult { Label = "DAMAGE" };
        damageFormula = damageFormula.Trim().ToUpperInvariant();

        int modifier = 0;
        int plusIdx  = damageFormula.LastIndexOf('+');
        int minusIdx = damageFormula.LastIndexOf('-');

        if (plusIdx > 0 && int.TryParse(damageFormula[(plusIdx + 1)..], out int pos))
        {
            modifier = pos;
            damageFormula = damageFormula[..plusIdx];
        }
        else if (minusIdx > 0 && int.TryParse(damageFormula[(minusIdx + 1)..], out int neg))
        {
            modifier = -neg;
            damageFormula = damageFormula[..minusIdx];
        }

        var parts = damageFormula.Split('D');
        if (parts.Length != 2
            || !int.TryParse(parts[0], out int count)
            || !int.TryParse(parts[1], out int sides))
        {
            result.Log.Add($"> [ERROR] Invalid formula '{damageFormula}'. Use format: 3D6+1");
            return result;
        }

        Func<int> roller = sides == 6 ? D6 : D10;
        for (int i = 0; i < count; i++)
            result.Dice.Add(roller());

        result.Total = result.Dice.Sum() + modifier;
        string modStr = modifier > 0 ? $"+{modifier}" : modifier < 0 ? $"{modifier}" : "";
        result.Log.Add(
            $"> [DAMAGE] {count}D{sides}{modStr}: [{string.Join(", ", result.Dice)}]{modStr} = {result.Total}");
        return result;
    }

    public RollResult RollInitiative(int refStat, int combatSense)
    {
        var result = new RollResult { Label = "INITIATIVE" };
        int roll = D10();
        result.Dice.Add(roll);
        result.Total = roll + refStat + combatSense;
        result.Log.Add($"> [INITIATIVE] Rolled: {roll} + REF:{refStat} + CS:{combatSense} = {result.Total}");
        return result;
    }

    public RollResult PerformSave(int bodyType)
    {
        var result = new RollResult { Label = "SAVE" };
        int roll = D10();
        result.Dice.Add(roll);
        result.Total = roll;
        bool success = roll <= bodyType;
        result.IsCriticalSuccess = success;
        result.IsFumble          = !success;
        result.Log.Add(
            $"> [SAVE] Rolled: {roll} vs BODY:{bodyType} — {(success ? "✓ SUCCESS" : "✗ FAILURE")}!");
        return result;
    }

    public RollResult RollManual(int sides)
    {
        var result = new RollResult { Label = $"D{sides}" };
        int roll = sides == 6 ? D6() : D10();
        result.Dice.Add(roll);
        result.Total = roll;
        result.Log.Add($"> [D{sides}] Rolled: {roll}");
        return result;
    }
}
