using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(DP.DMX))]
public class DMXInspector : Editor {
	
	public override void OnInspectorGUI()
	{
		var script = (DP.DMX) target;
		EditorUtility.SetDirty( target );
		
		//Device
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Device: ");
		EditorGUILayout.TextField(script.serialPorts[script.serialPortIdx].Substring(script.serialPorts[script.serialPortIdx].LastIndexOf('.') + 1));
		EditorGUILayout.EndHorizontal();
		
		//Select Device
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Serial ports: ");
 		int i = EditorGUILayout.Popup(script.serialPortIdx, script.serialPorts.ToArray());
		if (i != script.serialPortIdx)
		{
			script.serialPortIdx = i;
			script.OpenSerialPort();
		}
		EditorGUILayout.EndHorizontal();
	}
}