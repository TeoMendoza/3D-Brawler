# 1. Game Initialization & Ownership

### GameManager responsibilities
- Subscribes to the player table and listens for new row entries (e.g., when a player joins).
- Spawns a character prefab for each player using that row’s initial position and configuration data.
- After spawning, relinquishes responsibility; it does not continue managing or tracking character instances.

### Character instance responsibilities
- Each spawned character instance subscribes only to the tables relevant to itself (its own player row and any directly associated rows it needs to render/state-sync).
- On a given client (e.g., Player 1’s machine), there will be:
  - **One local instance** (Player 1’s own character): subscribes to its own data and processes input; permitted to send reducers for its own row.
  - **N–1 remote instances** (other players): each subscribes only to its own data and updates visuals on Player 1’s client; input/command code paths are disabled.
- Instance scripts are responsible for updating their own transforms/animations from state changes; the GameManager is not a runtime authority post-spawn.

### Instance lifecycle
- Spawn → self-subscribe → render & react to DB updates.
- Local instance additionally handles input → state requests → reducers (for its own row only).
- Remote instances never send reducers; they are read-only reflections of their corresponding DB state.

---

# 2. Character Instance Behavior (Local vs. Remote)

### Overview
Each client renders one local instance (the player-controlled character) and N–1 remote instances (other players). All instances subscribe only to their own authoritative rows in the database. The distinction is purely about what each instance is permitted to do (read vs. write) and which code paths are enabled (input, reducers, effect processing).

### Subscriptions
- **Local instance:** Subscribes to its own player row (and directly associated rows it needs to function/render).
- **Remote instances:** Each subscribes to its own row; they do not subscribe to the local player’s rows or any global authority.

### Read/Write Authority
**Local instance (player-controlled):**
- Reads DB updates for its own state (same as remotes).
- Writes via reducers to its own row only, including:
  - State transition requests (e.g., enter `ProjectileThrowing`).
  - Position/velocity updates (movement requests).
  - Ability/attack usage requests.
  - Config lock/unlock updates tied to animation lifecycle (see Section 7).
  - Effect-queue appends targeted at other players only through validated game events (e.g., projectile hit reducer), not by directly mutating another player’s row.

**Remote instances (read-only mirrors):**
- Read-only. Never send reducers.
- Update visuals/animation purely from observed DB state changes.

### Processing Loop (per instance)
1. **Apply DB updates →** Update transform, visible state, and animation (state-driven).  
2. **Local instance only:**
   - Input mapping (e.g., Ability1, Jump, Attack).
   - Capability check (consult CapabilityConfig locks; deny if any lock list is non-empty for the requested action).
   - Reducer dispatch (state request, movement/ability usage).
   - Effect queue processing for self (consume actions appended to the local player’s effect queue, applying via receiver interfaces).
3. **Remote instances:** Skip input, reducer dispatch, and effect processing; they only reflect state.

### Capability & Permissions Model
- **CapabilityConfig:** A dictionary of permission keys → set/list of locking states currently occupying that permission (e.g., `CanMove`, `UpperLocked`).
- **Permit rule:** An action is permitted only if the corresponding lock list is empty.
- **Locking model:** States add/remove themselves to/from the relevant lists (deduped; one state cannot occupy the same list twice).
- **Overlap-safe:** Multiple states may lock the same permission concurrently; permission unlocks only when the list becomes empty (see Section 7 for animation-locked details).

### Animation Control
- Animations are state-driven, not directly controlled by input.
- All instances (local and remote) render the same animation for the same state.
- The owner (local instance) is responsible for animation-completion callbacks that trigger reducers to remove config locks (and for manual cleanup on interruption).

### Effects & Command Pattern
- **Appending effects:** Only through validated game events (e.g., projectile hit), a reducer appends actions to the target’s effect queue.
- **Processing effects:** The target’s own client consumes its queue and applies effects via interfaces (e.g., `IReceiveDamage`, `IReceiveSnare`), enabling:
  - Local resistance/immunity/config adjustments.
  - Character/state-specific behavior (e.g., ult forms ignoring knockback).
- **Duration effects:** Managed by a duration-effects table and periodic reducer (see Section 8). Reapplication is triggered on any state change by resetting the per-effect applied flag(s), ensuring the current state’s base stats remain authoritative.

### Authority Invariants
- A client never writes to another player’s core state (position, health, configs) directly.
- Cross-player impact occurs only via validated reducers (e.g., projectile hit → append to target queue), with final application performed by the target.

---

# 3. Projectile System

### Lifecycle
- When a player fires a projectile, their client inserts a new row into the projectiles table via a reducer.
- Each projectile row contains:
  - `id`, `owner_id`, `position`, `velocity`, `type`, and any additional metadata.
- Projectiles travel linearly at constant velocity (no bullet drop). They persist until they hit a target, expire, or are otherwise removed.

### Subscriptions & Tracking
- All clients subscribe to the projectiles table.
  - **Insert →** spawn a local visual prefab.
  - **Update →** move the prefab to the new server-authoritative position.
  - **Delete →** remove the prefab.
- Clients track all active projectiles in the match, regardless of ownership.
- Projectile prefabs are purely visual; no client simulates movement locally.

### Server-Side Updates
- SpacetimeDB runs a reducer on a fixed interval to update all projectile rows:
  - Each update advances the projectile’s position by its stored velocity.
  - This ensures consistent, server-authoritative movement.
- Clients simply render the new positions — they do not perform prediction or simulation.

### Collision & Hit Reporting
**Owner responsibility:**
- The projectile’s owner client performs collision checks locally using Unity physics (e.g., `OverlapSphere`).
- If a hit is detected, the owner submits a reducer containing `{bullet_id, target_id, hit_context}`.

**Reasoning:**
- Full 3D collision simulation in the DB would be too costly.
- Offloading collision to the owner avoids centralized computation, while still allowing server-side validation.

### Server-Side Validation
- SpacetimeDB validates reported hits using the **Sphere of Truth**:
  - A generous bounding sphere is derived from the target’s default pose (encapsulating the longest model dimension).
  - Validation checks whether the projectile’s reported position overlaps the sphere.
  - This design assumption ensures fairness despite network delay/jitter, while avoiding complex model-shape collision on the server.
- If valid:
  - The projectile is removed.
  - Effects are appended to the target’s effect queue (see Section 4).

### Effect Application
- Reducers append effect actions such as:
  - **Simple:** `ReceiveDamage`, `ReceiveSlow`, `Blind`.
  - **Custom:** `AirPalmEffect`, `SnareBoltEffect`.
- The target’s client processes its own effect queue, applying actions via receiver interfaces (e.g., `IReceiveDamage`, `IReceiveSnare`).
- This approach ensures modularity, character-specific logic, and resistance/immunity handling.

---

# 4. Animation-Locked States

### Rationale
Certain actions must complete critical animation beats before the player regains control (e.g., release frame of a throw, recovery of a heavy attack). Locking input via state-owned permissions ensures:
- **Gameplay integrity:** Prevents canceling out of keyframes that define risk/reward timing.
- **Net consistency:** Keeps local and remote renders aligned because animation is state-driven—not input-driven.
- **Exploit resistance:** Blocks rapid state thrashing (e.g., stutter-step cancels) that would desync movement/aim/ability use.
- **Clear feel:** Players learn reliable timings (startup → active → recovery), improving readability and fairness.

### Terms
- **CapabilityConfig:** Dictionary of `Permission → Set<LockingState>`.  
  Examples: `CanMove`, `CanRotate`, `UpperLocked`, `LowerLocked`, `CanUseAbility`, `CanAttack`, `CanQueueNext`.
- **Locking state:** A state that temporarily occupies one or more permission lists (e.g., `ProjectileThrowing` adds itself to `UpperLocked` and `CanUseAbility`).

### Core Mechanics
- **Permit rule:** An action is permitted only if the corresponding permission’s lock set is empty.
- **Owner-only mutation:** Only the local owner adds/removes its state from lock sets (via reducers).
- **De-duped occupancy:** The same state cannot occupy the same permission more than once; extra inserts are ignored.
- **Overlap-safe:** Multiple states may occupy the same permission; a permission unlocks only when its set becomes empty.

### Lifecycle (per locking state)
1. **Enter**  
Owner sends a single reducer adding this state to each targeted permission set. Animations/VFX are chosen by state; all clients render them identically.
2. **Active**  
Capability checks reference lock sets; disallowed actions are denied (no reducers sent).
3. **Exit (natural)**  
On local animation completion, owner invokes `OnAnimationFinished` and sends a single reducer removing this state from all sets it occupied.
4. **Exit (interrupted)**  
If an effect interrupts the state (e.g., stun/knockback), the owner processes the effect queue locally and triggers manual cleanup to remove this state from all occupied sets.

### Interactions with Duration/State Changes
- On any state change, duration effects apply per design (resetting applied flags).
- Animation-lock cleanup is scoped: a state only removes its own locks; other states’ locks remain.

---

# 5. Effect Queue & Duration Effects

### Effect Queue System
- Effects like `TakeDamage`, `Burn`, or `Blind` are appended to the target’s effect queue via validated reducers (e.g., after a server-validated hit).
- Only the target’s client processes its own queue.
- The target listens for updates to its effect queue (detecting changes via previous vs. new data) and applies actions through receiver interfaces (e.g., `IReceiveDamage`, `IReceiveAirPalm`).

### Duration Effects (separate table)
- Duration-based effects are stored in a **duration effects table**.
- The effect’s interface is responsible for sending the reducer that adds a row to this table, packaging the data needed to apply the effect (e.g., slow amount, burn DPS, duration).

### Periodic Processing (reducer)
A periodic reducer iterates all rows in the duration effects table and performs two steps:

1. **Update remaining time**  
   - Decrement the duration.  
   - If duration ≤ 0: revert if needed and remove the row.

2. **Apply the effect**  
   - **Single-application effects (persist, then revert on expiry):**  
     - Use a boolean `applied`.  
     - If `applied == false`, apply the effect according to its package, then set `applied = true`.  
     - On any state change, a reducer marks relevant rows for the target back to `applied = false` so the effect re-applies against the new base state.  
   - **Tick effects (apply repeatedly):**  
     - Use `applied = null` (or a distinct mode) to indicate “apply every tick” (or at a configured interval field).  
     - Effect is re-applied each tick/interval until duration expires, then removed.

### Ownership & Authority
- Each duration effect row includes the target player ID so processing cleanly associates rows with the correct player.
- Cross-player additions to a target’s queue/table are only introduced via validated reducers (e.g., projectile hits); targets never accept direct foreign writes to core state.

