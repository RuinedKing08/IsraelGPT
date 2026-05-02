using UnityEditor;

namespace SergiosAdventure.Game.Editor
{
    [InitializeOnLoad]
    public static class FinalGameAutoRebuild
    {
        private const string SessionKey = "SergiosAdventure.FinalGameAutoRebuild";

        static FinalGameAutoRebuild()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            EditorApplication.delayCall += TryRebuild;
        }

        private static void TryRebuild()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryRebuild;
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += TryRebuild;
                return;
            }

            FinalGameBuilder.RebuildFinalGame();
        }
    }
}
