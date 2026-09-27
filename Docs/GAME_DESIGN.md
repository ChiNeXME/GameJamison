# The Last Mooncake — Game Design Document

**Status:** Pre-production / jam scope  
**Engine:** Unity 6, 2D URP  
**Document owner:** Design lead  
**Last updated:** 25 September 2026  
**Source of truth:** If a feature conflicts with this document, discuss it before building it.

---

## 1. The game in one sentence

**The Last Mooncake** is a short, cozy narrative deduction game in which Artemis, a forgotten moon goddess trapped as a cat, listens to customers' stories and helps an elderly baker make the mooncakes hidden in their memories.

## 2. Player promise

> I am a clever little cat who listens for the details people overlook, turns their memories into food, and quietly helps them reconnect.

The player should finish the game feeling warm, slightly wistful, and newly conscious of how traditions keep people connected.

## 3. Jam theme: Roots

The game interprets **Roots** in three connected ways:

1. **Family roots:** a recipe links the present family to an absent or deceased relative.
2. **Cultural roots:** mooncakes regain meaning when people remember why they share them.
3. **Artemis's roots:** solving the family's story restores part of her forgotten identity and purpose.

The theme should be visible in play, not only mentioned in dialogue. Clues from the past are necessary ingredients in the final solution.

## 4. Design pillars

Every proposed feature must support at least one pillar. Features that support none should be cut.

### 4.1 A cat notices what people miss

Artemis stays beside the baker and listens carefully. Her reactions, thoughts, and magical senses draw attention to emotionally important details without directly giving away the answer.

### 4.2 Food carries memory

Ingredients are narrative clues. The correct mooncake is understood by learning what each ingredient meant to the family, not by following an exposed checklist.

### 4.3 Quietly reconnect people

Artemis cannot solve the conflict with dialogue. She changes the situation so that people remember, speak, and reconcile on their own.

### 4.4 Small scope, polished feeling

One complete emotional story is better than several unfinished stories. Reuse interactions and concentrate effort on animation, sound, feedback, and the final payoff.

---

## 5. Recommended jam scope

### Target experience

- **Format:** single-player, linear, in-place narrative deduction game
- **Target length:** 15–20 minutes
- **Primary input:** keyboard and mouse; controller support only if inexpensive
- **Location:** one bakery counter and mooncake preparation screen
- **Tutorial story:** Lina prepares to reconnect with her estranged father
- **Main story:** one strained twin relationship
- **Main recipe:** grandmother's mooncake
- **Ending:** bake and share an imperfect reconstruction; preserve what remains and recover one memory of Chang'e

This is a vertical slice with one short guided customer followed by the complete sibling story. Additional customers belong to a post-jam version.

### MVP — must exist

- A customer can tell their story through concise, skippable dialogue.
- The story contains clear hints identifying the correct recipe choices.
- Artemis can react to important lines and record short memory notes.
- The player selects and assembles a mooncake recipe.
- A correct solution produces the ending.
- A wrong solution gives readable feedback and allows an immediate retry.
- The moon visibly dims during the story and brightens at the resolution.
- The game has a title, basic instructions, credits, and a restart option.

### Stretch goals — only after MVP is playable

1. Additional Artemis reactions and animations.
2. A simple oven timing interaction.
3. Optional dialogue choices or requests to repeat parts of the story.
4. Controller support.
5. A second customer vignette.

### Explicitly out of scope

- Open-world village exploration
- Player movement, platforming, or a traversable level
- Farming, harvesting, economy, or shop management
- Branching narrative or multiple endings
- Combat, fail states, lives, or player punishment
- Fully simulated cooking
- More than one major family conflict in the jam build
- A separate minigame for every production step
- Artemis regaining a playable divine form

---

## 6. Core player loop

```text
Customer arrives and tells a personal story
                      ↓
 Listen for recipe hints hidden in the memory
                      ↓
 Artemis records a few short memory notes
                      ↓
 Choose the ingredients and preparation method
                      ↓
          Press and bake the mooncake
                 ↙             ↘
        Wrong recipe          Correct recipe
      Reaction + hint       Memory is revealed
        + quick retry              ↓
                         Customer responds
                                  ↓
                    Moonlight and Artemis progress
```

### Full loop

1. **A customer arrives.** They describe a person, event, or half-remembered mooncake. Their story is the puzzle, so it should be short enough to follow but rich enough to feel human.
2. **Listen for meaningful details.** Specific lines imply the filling, centre, sweetness, and finish. For example, “She always said the paste should taste like lotus, not sugar” points to lotus paste with low sweetness.
3. **Review Artemis's memory notes.** Artemis automatically records a few short phrases after the conversation. These preserve the customer's wording without converting every hint into an explicit answer.
4. **Prepare the mooncake.** At the same counter, the player chooses the recipe options and presses the mold. Ingredient availability is not a separate collection mechanic.
5. **Receive immediate feedback.** A wrong recipe causes a small character reaction and highlights one useful memory note before an instant retry. After a second mistake, Artemis interprets the relevant hint more directly.
6. **Reveal the memory.** A correct mooncake makes the mold glow and plays a short visual memory showing why those details mattered.
7. **Complete the emotional beat.** The customer recognizes the mooncake and responds to the memory. The moon grows brighter and Artemis remembers a small part of her own past.

**Design rule:** the story gives the hints, the recipe is the player's answer, and the revealed memory is the reward. Do not add exploration or ingredient collection unless the central loop is already complete and polished.

The jam build contains one complete version of this loop: the two siblings and their grandmother's mooncake. A larger post-jam game may repeat the loop for several customers, with each completed mooncake advancing Artemis's larger story.

## 7. Intended player experience by minute

| Time | Beat | Player activity | Intended feeling |
|---|---|---|---|
| 0:00–1:00 | The fading moon | Short introduction at the bakery counter | Intrigue |
| 1:00–4:00 | The customers' story | Listen to two siblings remember different parts of their grandmother's recipe | Curiosity |
| 4:00–5:00 | Memory notes | Review the important phrases Artemis noticed | Deduction |
| 5:00–8:00 | Mooncake preparation | Choose the filling, centre, sweetness, and finish | Synthesis |
| 8:00–10:00 | The revealed memory | The siblings recognize that each remembered one part; Artemis remembers Chang'e | Warmth and closure |

---

## 8. The playable story

### Setup

The Mid-Autumn moon is unusually dim. Artemis sleeps behind an elderly baker's shop, apparently an ordinary stray. When the baker presses dough into an old damaged mold, moonlight flickers through it and awakens a fragment of Artemis's memory.

After tutorial customer Lina leaves, adult twins Mei and Jian arrive separately on the first Mid-Autumn Festival after their grandmother's death. Mei brings the damaged tin and recalls the lotus filling, low sweetness, and osmanthus scent. Jian arrives later looking for the tin and admits he lost the written recipe while clearing their grandmother's home. He remembers placing a salted yolk—"the moon"—in its centre. Their accounts form the best reconstruction still possible, though some details are gone.

### Story clues

Because Artemis cannot speak, she listens from the counter as each twin tells the baker what they remember during separate arrivals. Their dialogue communicates:

| Clue | Location | Information communicated |
|---|---|---|
| Remembered saying | One sibling recalls that grandmother wanted the lotus to taste stronger than the sugar | Use lotus paste and less sugar |
| Childhood memory | The other sibling remembers being allowed to place “the moon” in the middle | Put one yolk in the centre |
| Scent memory / keepsake | They remember the floral scent on grandmother's sleeves and cake box | Finish the cake with osmanthus |

Each sibling was correct about a different part. Their conflict came from treating a partial memory as the complete truth.

### Resolution

Artemis brings or reveals the clues to the baker. The player assembles:

- Lotus paste
- One centred salted egg yolk
- Low sweetness
- Osmanthus finish

When the mooncake is pressed, the damaged mold glows but does not become whole. The twins share the cake and agree that it tastes “almost” like their grandmother's kitchen. They accept that the past cannot be perfectly recovered and write down what they remember together. A memory shows Chang'e teaching Artemis that remembrance means carrying what remains while accepting what is missing.

The moon brightens without becoming completely full. The baker places a tiny cat-shaped mooncake on the windowsill. Artemis takes one bite as the mold continues to glow with its crack still visible. Fade to title and credits.

### Narrative rule

The game should not claim that one pastry magically repairs a serious relationship. The mooncake creates the opportunity to remember and talk; the siblings choose to reconnect.

---

## 9. Mechanics

### 9.1 In-place presentation

There is no player movement or traversable level. The game stays at one illustrated bakery counter with three main views:

- Customer dialogue
- Artemis's memory notes
- Mooncake preparation

Transitions should be quick and preserve the feeling that everything happens in the same intimate space.

### 9.2 Interaction

The player advances dialogue, opens the memory notes, selects recipe options, and confirms the mooncake. Artemis remains expressive through idle, listening, thinking, doubtful, and pleased reactions.

### 9.3 Story-hint system

Each important detail provides:

1. A distinctive line of customer dialogue
2. A small visual or audio emphasis when appropriate
3. A short memory note recorded by Artemis

The notebook should record interpreted information, not lengthy transcripts. Example: **“Grandmother reduced the sugar so the lotus could be tasted.”**

All required hints appear naturally during the conversation. The preparation view unlocks when the story ends, and the player may reread the notes at any time.

### 9.4 Mooncake assembly puzzle

The player makes four choices through a hybrid work surface. Physical ingredients are dragged into the mooncake, while recipe decisions that are not objects use explicit controls:

1. Filling: drag lotus paste or red bean paste into the filling slot
2. Centre: drag in one salted yolk or deliberately select **Leave empty**
3. Sweetness: select **Less sugar** or **Regular sugar** on a two-position control
4. Finish: drag osmanthus or sesame onto the cake

`No yolk` and the sweetness levels must not appear as draggable ingredients. They describe an omission or an amount, so presenting them as physical objects would be confusing. The empty-centre control is explicit so the game can distinguish a deliberate choice from an unfinished recipe.

The chosen ingredients and settings appear visually in a cross-section or on the work surface. Replacing a physical ingredient returns the previous one to its tray. The player then presses the mold.

**Correct answer:** lotus + yolk + low sweetness + osmanthus.

**Incorrect answer:** the mold releases weak light; Artemis reacts; the notebook subtly emphasizes the conflicting clue. Ingredients reset immediately. There is no inventory loss and no lengthy rebake animation.

### 9.5 Moonlight as progress feedback

Moonlight communicates narrative progress:

- Opening: cool, faint moon; bakery mostly warm artificial light
- Each clue: a small pulse through the mold and slightly stronger moonbeam
- Correct recipe: mold pattern glows around its still-visible crack
- Ending: brighter but incomplete moonlight reaches the bakery

This is presentation feedback, not a countdown. The player cannot run out of time.

---

## 10. Screen layout

Use one persistent bakery-counter composition. The customer occupies one side, the baker and Artemis the other, and the moon remains visible through a window. Dialogue and memory notes overlay this scene. When preparation begins, the counter becomes the work surface while the characters remain present in the background.

The player should never need to navigate between rooms or search for the correct screen. Changes in character pose, lighting, counter props, and music provide visual progression without requiring a level.

---

## 11. Art direction

### Visual target

A handcrafted autumn picture-book look: warm bakery interiors against cool blue-violet moonlight. Shapes should be readable at game-jam scale and animation may be limited but expressive.

### Palette roles

- Warm amber and toasted brown: bakery, food, living relationships
- Muted rust and red: autumn and festival decoration
- Desaturated blue-violet: fading moon and forgotten history
- Pale gold: memory, restored tradition, successful clues

### Character priorities

1. Artemis silhouette and readable cat animation
2. Baker portrait or simple in-world character
3. Lina portrait
4. Mei portrait or economical character rig
5. Jian portrait or economical character rig
6. Chang'e memory silhouette

### Customer character descriptions

- **Lina Chen:** The first customer and guided tutorial. A quietly confident Chinese florist in her mid-twenties who has returned home after five years away. She has burgundy hair, green eyes, an osmanthus hair clip, a rust blouse, and a deep-green apron dress. She carries a square golden-brown mooncake box tied with a yellow ribbon. Her story teaches all four recipe controls in order before she takes the cake to her estranged father's door.
- **Mei:** One of a pair of adult Chinese twins. She is composed and practical, with shoulder-length dark hair, a muted teal blouse, rust cardigan, and the family's damaged mooncake tin held carefully in both hands. She arrives alone, protective of the memory she believes Jian has neglected.
- **Jian:** Mei's twin brother. He shares her eyes and face shape without looking identical, and has tousled dark hair, an ochre overshirt, and a blue-grey shirt. He arrives separately, earnest and slightly defensive, looking for the family tin and unaware that Mei is already inside.

If character animation capacity is limited, use portrait dialogue and reserve in-world animation for Artemis and the final sharing gesture.

### Essential asset list

- Artemis: idle, listen, think, doubtful reaction, pleased reaction, eat
- Bakery modular tiles and props
- Moon and moonbeam states
- Five physical ingredient choices: lotus, red bean, salted yolk, osmanthus, and sesame
- Centre and sweetness controls, including clear selected states
- Damaged and illuminated mold
- Three clue close-ups
- Baker, Lina, Mei, and Jian portraits
- Mooncake assembly UI
- Ending illustration or staged scene

---

## 12. Audio direction

- Gentle, intimate score using a small palette; avoid a constant overly sentimental swell.
- Warm bakery ambience: oven, wooden room, paper packaging, distant festival sounds.
- Cat actions need satisfying, light feedback.
- Each clue adds one musical layer or bell tone.
- Correct mold press gets the strongest sonic payoff.
- Let the reconciliation briefly become quiet before the moonlight theme resolves.

Audio priorities if time is short: interaction sounds, clue sting, mold success, ending music transition.

---

## 13. UX and accessibility

- Show controls contextually, then let prompts fade.
- Keep dialogue concise and skippable; never make the player replay it after a failed recipe.
- Use icons plus text; do not communicate ingredients by colour alone.
- Provide separate music and sound-volume controls if a settings screen already exists.
- Avoid a small decorative font for body copy.
- Make interactable highlights optional if implementation is cheap.
- Include pause, restart from checkpoint, and return to title.

## 14. Failure and hint philosophy

This is a game about interpretation, not punishment.

- Traversal has no death state.
- Wrong recipes are accepted as attempts and immediately returned for revision.
- After one wrong attempt, Artemis looks toward a relevant notebook entry.
- After two wrong attempts, the notebook explicitly marks which choice conflicts with evidence.
- The solution should become obvious after the player understands the clues; randomness must not be involved.

---

## 15. Production plan

### First playable

The first milestone is a greybox in which the player can:

1. Read one complete customer conversation
2. Review three or four memory notes
3. Make four recipe choices
4. Trigger success or retry feedback
5. View the revealed memory and reach the ending screen

Do not wait for final art, writing, or animation before testing this loop.

### Suggested workstreams

| Discipline | First priority | Second priority |
|---|---|---|
| Design | Validate story hints and recipe deduction | Tune hints and pacing |
| Programming | Dialogue-to-recipe game-state spine | Recipe UI, save/checkpoint, polish |
| Art | Artemis and environment style test | Required assets, then decoration |
| Narrative | Final clue text and dialogue | Memory scene and optional flavour text |
| Audio | Core interaction and success SFX | Ambience and adaptive music layers |

If people cover multiple roles, these remain the order of operations rather than separate job assignments.

### Implementation spine

Use a small explicit sequence of states:

```text
Intro → CustomerStory → ReviewNotes → RecipeAvailable
      → RecipeAttempt → Resolution → Ending
```

Required clues and recipe options should be data-driven where convenient, but avoid constructing a generalized quest framework for a single jam scenario.

### Definition of done

The jam build is done when a new player can start without explanation, finish the story, understand why the recipe was correct, and reach credits without a blocker.

---

## 16. Playtest questions

Observe before explaining. Ask after the run:

1. What were you trying to achieve?
2. What did each clue tell you?
3. Why did you choose those ingredients?
4. Did playing as a cat matter, or could the character have been anything?
5. What did “Roots” mean in this game?
6. Where were you bored, lost, or waiting?
7. What moment do you remember most?

Critical success criteria:

- At least 4 of 5 testers complete without developer intervention.
- Most testers can explain the recipe using the clues.
- Most testers understand the siblings each held part of the memory.
- Total completion time stays near 10–15 minutes.

---

## 17. Risks and responses

| Risk | Warning sign | Response |
|---|---|---|
| Too much story, too little play | The player only advances text for several minutes | Keep dialogue concise, emphasize hints, and let the player open notes during natural pauses |
| Deduction feels automatic | Notes state the exact recipe choices | Preserve the customer's wording and let the player interpret it |
| Puzzle becomes guessing | Testers cannot connect clues to choices | Rewrite clues and visually pair them with recipe categories |
| Cooking grows into several minigames | Each step needs new controls and UI | Keep assembly as one evidence puzzle; use baking as presentation |
| Art workload explodes | Multiple detailed humans require full rigs | Use portraits or still illustrations for humans |
| Ending lacks impact | Moon brightens without a human change | Prioritize the twins accepting the imperfect result, speaking honestly, and writing the surviving recipe together |

---

## 18. Post-jam expansion, not current scope

If the vertical slice works, the complete game can add customers whose conflicts explore different kinds of roots:

- Former friends and an undelivered apology
- A deceased relative's partially remembered recipe
- A gift shared with family overseas
- Mold fragments that reveal Artemis's history with Chang'e
- A finale in which the village's individual mooncakes form one lunar pattern

The final thematic choice remains: Artemis chooses life with the baker because stories need caretakers on Earth, not another distant goddess.

---

## 19. Open decisions

Resolve these through prototypes or a short team discussion. Do not let them silently become assumptions.

- Exact jam deadline and available person-hours
- Keyboard-only versus keyboard/controller at submission
- Dialogue system: in-world bubbles or portrait box
- Whether mooncake assembly is drag-and-drop or button/card selection
- Whether the ending is animated in-engine or delivered as illustrations

## 20. Decision log

Record meaningful changes here so the team knows what was deliberately changed.

| Date | Decision | Reason |
|---|---|---|
| 25 Sep 2026 | Scope the emotional core to one sibling story | Protect polish and ensure a full emotional arc |
| 25 Sep 2026 | Use Roots as family, cultural, and personal memory | Integrates the jam theme directly into story and puzzle |
| 25 Sep 2026 | Make cooking an evidence-selection puzzle | Keeps narrative and mechanics unified without excessive minigames |
| 25 Sep 2026 | Use physical dragging only for tangible ingredients; use controls for omissions and amounts | Avoids representing “no yolk” and sweetness levels as draggable objects |
| 26 Sep 2026 | Cut the unused extra customer and have twins Mei and Jian arrive separately | Focus the jam build on one emotional story while giving each twin an independent entrance and voice |
| 27 Sep 2026 | Add Lina as the first, fully guided customer | Teach all four recipe controls before the unguided twins puzzle |
| 27 Sep 2026 | Keep the twins' resolution bittersweet and incomplete | Let remembrance preserve what remains without pretending the past can be restored perfectly |
