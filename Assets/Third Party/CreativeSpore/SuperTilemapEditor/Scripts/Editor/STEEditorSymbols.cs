using UnityEditor;

namespace CreativeSpore.SuperTilemapEditor
{
    [InitializeOnLoad]
    public class STEEditorSymbols : Editor
    {
        /// <summary>
        /// Symbol that will be added to the editor
        /// </summary>
        private static string _EditorSymbol = "SUPER_TILEMAP_EDITOR";//
 
        /// <summary>
        /// Add a new symbol as soon as Unity gets done compiling.
        /// </summary>
        static STEEditorSymbols()
        {
            string lSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!lSymbols.Contains(_EditorSymbol))
            {
                lSymbols += (";" + _EditorSymbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, lSymbols);
            }
        }
    }
}