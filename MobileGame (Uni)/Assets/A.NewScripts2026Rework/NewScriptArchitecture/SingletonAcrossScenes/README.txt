SingletonAcrossScenes
=====================

Things every scene uses.

SceneController
  Stays alive between scenes (DontDestroyOnLoad). LoadScene(name) plays the transition animation,
  waits transTime seconds, then loads the scene. Everything changes scene through this.

XPCurve/
  XPCurveConfig is a ScriptableObject holding the XP-per-level curve (the XPCurveConfig.asset file).
  Both XPManager (game) and MainMenuXPDisplay (menu) read it, so levels are calculated the same everywhere.
  Edit the curve in the asset's Inspector.
