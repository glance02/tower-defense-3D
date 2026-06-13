using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileSlot)), CanEditMultipleObjects]

public class TileSlotEditor : Editor
{
    // TileSlot 的自定义 Inspector：提供一键切换地块、旋转和升降的编辑器按钮。
    private GUIStyle centeredStyle;

    public override void OnInspectorGUI()
    {
        // 支持多选 TileSlot，一次性批量修改多个地块。
        serializedObject.Update();
        base.OnInspectorGUI();

        centeredStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 16
        };

        TileSetHolder tileHolder = FindFirstObjectByType<TileSetHolder>();

        float oneButtonWidth = EditorGUIUtility.currentViewWidth - 25;
        float twoButtonWidth = (EditorGUIUtility.currentViewWidth - 25) / 2;
        float threeButtonWidth = (EditorGUIUtility.currentViewWidth - 25) / 3;

        GUILayout.Label("Tile Options", centeredStyle);
        GUILayout.BeginHorizontal();
        MakeButtonSwitchTile("Field", tileHolder.tileField, threeButtonWidth);
        MakeButtonSwitchTile("Road", tileHolder.tileRoad, threeButtonWidth);
        MakeButtonSwitchTile("Sideway", tileHolder.tileSideway, threeButtonWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        MakeButtonSwitchTile("Hill 1", tileHolder.tileHill_1, threeButtonWidth);
        MakeButtonSwitchTile("Hill 2", tileHolder.tileHill_2, threeButtonWidth);
        MakeButtonSwitchTile("Hill 3", tileHolder.tileHill_3, threeButtonWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        MakeButtonSwitchTile("Bridge Field", tileHolder.tileBridgeField, threeButtonWidth);
        MakeButtonSwitchTile("Bridge Road", tileHolder.tileBridgeRoad, threeButtonWidth);
        MakeButtonSwitchTile("Bridge Sideway", tileHolder.tileBridgeSideway, threeButtonWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        MakeButtonSwitchTile("Big Inner Corner", tileHolder.tileCornerInnerBig, twoButtonWidth);
        MakeButtonSwitchTile("Big Outer Corner", tileHolder.tileCornerOuterBig, twoButtonWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        MakeButtonSwitchTile("Small Inner Corner", tileHolder.tileCornerInnerSmall, twoButtonWidth);
        MakeButtonSwitchTile("Small Outer Corner", tileHolder.tileCornerOuterSmall, twoButtonWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        MakeButtonSwitchTile("Level Tile", tileHolder.tileLevel, oneButtonWidth);
        GUILayout.EndHorizontal();


        GUILayout.Label("Rotate Options", centeredStyle);
        GUILayout.BeginHorizontal();
        MakeButtonRotateTile("Rotate Left", -1, twoButtonWidth);
        MakeButtonRotateTile("Rotate Right", 1, twoButtonWidth);
        GUILayout.EndHorizontal();


        GUILayout.Label("Position Options", centeredStyle);
        GUILayout.BeginHorizontal();
        MakeButtonPositionTile("Up", 1, twoButtonWidth);
        MakeButtonPositionTile("Down", -1, twoButtonWidth);
        GUILayout.EndHorizontal();
    }

    // Make a simple button that return true when clicked
    // Find TileSetHolder component and apply a new tile
    // Switch each selected tile to the new tile
    private void MakeButtonSwitchTile(string tileText, GameObject tileObject, float buttonWidth)
    {
        // 点击按钮后把当前选中的所有 TileSlot 都替换成对应地块。
        if (GUILayout.Button(tileText, GUILayout.Width(buttonWidth)))
        {
            foreach (var target in targets)
            {
                ((TileSlot)target).SwitchTile(tileObject);
            }
        }
    }

    private void MakeButtonRotateTile(string tileText, int direction, float buttonWidth)
    {
        // 批量旋转选中的地块。
        if (GUILayout.Button(tileText, GUILayout.Width(buttonWidth)))
        {
            foreach (var target in targets)
            {
                ((TileSlot)target).AdjustYRotation(direction);
            }
        }
    }

    private void MakeButtonPositionTile(string tileText, int verticalDirection, float buttonWidth)
    {
        // 批量调整选中地块高度。
        if (GUILayout.Button(tileText, GUILayout.Width(buttonWidth)))
        {
            foreach (var target in targets)
            {
                ((TileSlot)target).AdjustYPosition(verticalDirection);
            }
        }
    }
}
