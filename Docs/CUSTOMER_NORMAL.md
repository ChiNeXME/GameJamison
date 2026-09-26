# Customer Case — The Two Halves of a Memory

**Tone:** Warm, sincere, and bittersweet

**Customers:** Adult twins Mei and Jian, arriving separately

**Story purpose:** Show that two incomplete memories can preserve something meaningful without perfectly restoring what has been lost.

**Customer order:** Lina's guided tutorial comes first. Mei and Jian follow as the main story and unguided deduction.

## Scene order

1. Opening — the baker finds Artemis beside the dim moonlit window.
2. Mei's arrival — on the first festival after Grandmother's death, Mei brings the damaged tin and describes the half of the recipe she remembers.
3. Jian's arrival — Jian comes separately looking for the tin, admits he lost the written recipe, and supplies the missing memory.
4. Deduction — Artemis reviews the collected clues and the player builds the mooncake.
5. Wrong attempt — one contextual reaction plays, then the player retries immediately.
6. Correct attempt — Grandmother's kitchen memory reveals that both twins helped with one recipe.
7. Resolution — the cake tastes “almost” right; the twins accept that some details are gone and write down what remains together.
8. Artemis's memory — Chang'e's words return; the moon brightens without becoming full and the mold's crack remains.

## Correct recipe

| Recipe category | Correct choice | Evidence |
|---|---|---|
| Filling | Lotus paste | Mei remembers grinding lotus seeds |
| Centre | One salted egg yolk | Jian placed Grandmother's “little moon” in the middle |
| Sweetness | Low | Grandmother used one small spoonful so the lotus stayed clear |
| Finish | Osmanthus | Mei remembers the flowers on Grandmother's sleeve and over the cakes |

## Implementation notes

- The complete chronological script is in `Docs/Dialogue/00_FULL_SCRIPT.md`.
- Every speaking or thinking character has a separate file in `Docs/Dialogue`.
- Artemis never speaks aloud. Her lines appear as thoughts or notebook entries.
- Mei and Jian must not enter together. Mei is already at the counter when Jian arrives later.
- On a wrong recipe, play only the first incorrect category in this order: filling, centre, sweetness, finish.
- On a second error in the same category, use Artemis's direct hint.
- Dialogue remains skippable, and no story dialogue repeats after a failed recipe.
