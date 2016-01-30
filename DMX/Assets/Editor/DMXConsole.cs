using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class DMXConsole: EditorWindow {
	
	GameObject dmxObject;
	DP.DMX dmx;
	
	public Vector2 scroll = new Vector2();

	[MenuItem ("Window/DMX/Console")]
	static void Init () 
	{
		// Get existing open window or if none, make a new one:
		DMXConsole window = (DMXConsole)EditorWindow.GetWindow (typeof (DMXConsole));
		window.titleContent.text = "DMX Console";
		window.Show();
	}
	
	void OnInspectorUpdate()
	{
		Repaint();
	}

	void OnGUI () 
	{
		dmxObject = GameObject.Find("DMXObject");
		dmx = dmxObject.GetComponent<DP.DMX>();

		//Levels
		scroll = EditorGUILayout.BeginScrollView(scroll);
		GUIStyle labelWidth = new GUIStyle();
		labelWidth.fixedWidth = 300;
		for (int f = 1; f < dmx.nChannels+1; f++)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("[Channel " + f + "] ", GUILayout.Width(100));
			int i = EditorGUILayout.IntSlider(dmx[f], 0, 255, GUILayout.Width(300));
			if (i != dmx[f])
			{
				dmx[f] = (byte)i;
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
	}
}