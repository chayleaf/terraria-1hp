# Terraria Mod for 1hp playthroughs

This mod:

- makes 1hp the max possible HP value (don't worry, it will be reverted
  if you load your characters without this mod)
- makes heart crystals not give any bonus HP (but they still unlock
  progression!)
- fixes game UI not being shown when max HP is less than 19 because of a
  division by zero
- fixes a ROD exploit that allowed to use it infinitely if your max HP
  is less than 7 (well, now it just kills you if you use it while the
  debuff is active)

Planned:

- QoL fixes to prevent RNG instadeaths
- Maybe some configuration?
- Maybe don't override the HP of existing characters with over 1HP?

## Installing

Just do it via Steam Workshop. Otherwise:

Make sure you're using the 1.4.4 beta branch of tModLoader (it greatly
simplifies what this mod has to do compared to 1.4.3.6 because its logic
mostly depends on heart crystals consumed, not max health).

On Linux, put the `tmod` file to
`~/.local/share/Terraria/tModLoader-preview/Mods` and enable it in
tModLoader settings.

On Windows, do the same, but put it to
`Documents\My Games\Terraria\tModLoader-preview/Mods` instead.

