using System.Collections.Generic;

/// <summary>
/// Результат ОДНОГО совпавшего символа (может объединять несколько паттернов —
/// см. правило стакинга в RewardResolver). Один спин возвращает список из
/// нескольких SpinResult, если сработало несколько паттернов с разными символами.
/// </summary>
public class SpinResult
{
    // Название символа/предмета — для UI и дебага.
    public string label;

    // Если это оружие/предмет — собранный экземпляр с уже накрученными бафами.
    // Если это эффект черепа — остаётся null.
    public ItemInstance itemAwarded;

    // Какие паттерны внесли вклад в этот результат (для UI — подсветить нужные линии на сетке).
    public List<PaylinePattern> contributingPatterns = new List<PaylinePattern>();

    // --- Ветка черепа ---
    public bool isDeathEffect = false;
    public float damageAmount = 0f;
    public bool isTeamWipe = false; // true только для джекпота черепами (вся сетка)
}
