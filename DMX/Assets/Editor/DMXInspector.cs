using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(DMX))]
public class DMXInspector : Editor {
	
	public override void OnInspectorGUI()
	{
		var script = (DMX) target;
		EditorUtility.SetDirty( target );
		
		//Device ID
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Device s/n: ");
		EditorGUILayout.LabelField(script.deviceSerialNumber);
		EditorGUILayout.EndHorizontal();
		
		//Device
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Device name: ");
		
		script.deviceName =  EditorGUILayout.TextField(script.deviceName.Substring(script.deviceName.LastIndexOf('.') + 1));
		EditorGUILayout.EndHorizontal();
		
		//Select Device
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Serial ports: ");
 		int i = EditorGUILayout.Popup(script.serial_port_idx, script.serial_ports.ToArray());
		if (i != script.serial_port_idx)
		{
			script.serial_port_idx = i;
			script.OpenConnection();
		}
		EditorGUILayout.EndHorizontal();
		
		//Number of channels
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Number Channels: ");
		i = EditorGUILayout.IntField(script.nChannels);
		{
			i = Mathf.Clamp(i, 24, 1024);
			if (i != script.nChannels)
			{
				script.nChannels = i;
				script.setChannels();
			}
		}
		EditorGUILayout.EndHorizontal();

	}
}