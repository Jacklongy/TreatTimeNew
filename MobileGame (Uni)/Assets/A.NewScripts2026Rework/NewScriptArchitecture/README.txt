NewScriptArchitecture
=====================

Game logic, split by where it runs:

LivesInGame/            Only exists in the game scene (one run of play).
LivesInMenu/            Only exists in the main menu scene.
SingletonAcrossScenes/  Shared by every scene (scene changing, XP curve).
Tasks/                  Task definitions, catalog, progress reporting and task UI.

Rule: if a script needs the saved player profile, it reads it from PlayerProfileHolder
(see PlayerData/LocalSaving). It never reads the save file directly.

SCENE LOADING BAR
  Add a UI Image under Fade in the SceneLoader prefab's TransitionCanvas, so it fades with the
  existing loading screen. Assign a sprite, set Image Type to Filled, and choose the fill method
  and origin (for example, Horizontal from Left). Assign the Image to SceneController's Loading
  Bar field on the prefab, and remove or disable the existing loading text if desired.
  SceneLoader persists across scenes, so configure the prefab rather than a scene-only copy.

  The bar is a smoothed visual estimate of Unity's asynchronous scene-load progress, not an exact
  percentage. It reaches 100% only after scene activation. The existing Trans Time is the minimum
  time before activation; the fade stays up if loading takes longer. This covers scene transitions,
  not the separate startup sign-in and cloud-sync steps on the splash screen.

MAIN MENU CLOUDS
  Put CloudScroller on a stationary parent of the repeating cloud groups. Each direct child must
  be an identical group of SpriteRenderers, evenly spaced along world X. For the existing three
  groups, use X positions -7.5, 0 and 7.5 with the same Y and Z. Keep ScrollOffset on the individual
  cloud sprites for page parallax; do not put it on the moving groups or their parent.

  Set Speed in world units per second (positive moves left, negative moves right) and optionally
  assign the orthographic main camera. Groups wrap only when completely off-screen. If the Console
  reports insufficient coverage, add identical groups at the same spacing until the warning clears,
  and test the widest intended aspect ratio and all menu pages.

TASK SETUP
  1. Create task assets via Assets > Create > Tasks > Task Definition. Each task needs a unique,
     permanent ID, description, period, metric, goal and reward. Do not change IDs after release.
     Duplicating an asset copies its ID: create a new asset or give the copy a unique ID.
     Merge Tag is the exact Unity tag on the items being merged, not on the result.
     Single Run applies to merge, feed and score; unfinished progress resets at the next run.
  2. Create a Tasks > Task Catalog asset. Add task asset references to the matching Daily, Weekly
     and Monthly Tasks lists, and at least one valid reward option to each period's Bonuses list.
     Older catalogs can use the Inspector context menu "Move legacy tasks into period lists".
     One bonus option is selected
     randomly and saved for each period; currency uses Amount and Item uses a shop item.
     Configure at least one task for each period. Three or more are needed for that period's
     completion bonus. If fewer than three exist, all available tasks are selected without duplicates,
     but the bonus cannot be claimed. Invalid tasks and duplicate IDs are reported in the console.
  3. Add TaskProgressManager to a root GameObject in MainMenu and assign the task catalog and the
     existing shop catalog. Set Duplicate Item Coins to the desired fallback for already-owned item
     rewards and Save Interval Seconds for the progress checkpoint interval. The manager survives
     scene changes; MainMenuBootstrap initializes it after loading the profile.
  4. On the task card prefab, assign TaskObject's goal, reward and progress texts, filled Image,
     Claim State and Claim Button. Claim State is shown only when its task is ready to claim.
  5. Add a TaskContainer to each daily, weekly and monthly UI section. Set its Period, Task Prefab,
     Task Parent, Bonus Text and Bonus Claim Button. Assign Bonus Image to show the selected reward:
     item icons come from the shop item, while Coin Bonus Sprite and Gem Bonus Sprite are set on
     the container. Use the existing daily, weekly and monthly task prefabs for their different
     background colours. The bonus button appears after all three individual task rewards are
     claimed. All item rewards must also exist in the assigned shop catalog.
  6. Test from the normal splash/menu flow. Completed tasks and unclaimed rewards expire at reset:
     daily at 00:00 UTC, weekly Monday 00:00 UTC, monthly on the first at 00:00 UTC.

  Gameplay reports successful feeds, merges and score immediately. Selection and claims are saved
  locally immediately; progress is checkpointed periodically, on pause and at game over. A crash
  can lose progress since the last checkpoint. Cloud sync uses the existing whole-profile conflict
  policy; it does not merge separate devices' task progress. Reset time uses the device UTC clock.
