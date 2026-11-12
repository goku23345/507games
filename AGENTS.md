# Project workflow
- Preserve authored scenes and teammates' content.
- Normal builds: BuildStudy.BuildExisting, or the 白模工作台 build menu.
- BuildStudy.Run regenerates the default blockout and default resources. Do not use it for ordinary builds after scene edits; preserve a backup before a task that actually requires regeneration.
- LowerPassage geometry comes from WhiteboxPlatform scene modules; keep their roots when replacing visual children.
- Runtime progress is owned by KnightLevel. Keep key, coins, story archive, checkpoint, and restart state coherent.
- Verify both lower routes, story archive pickup, end interaction return, and combat rectangle boundaries. The former return loop was explicitly removed.
- The automatic elevator after the underground checkpoint is mandatory. Preserve its 4.4 m upper exit and anti-bypass wall; do not connect activation to enemy ramming. Verify rider contact at the bottom, pause/reset, and both routes through the lift before Boss.
- Upper middle timed spikes are a scene-authored WhiteboxSpikes module between the ram door and final gate. Preserve its harmless warning/retraction phases and verify both directions through upper progression.
