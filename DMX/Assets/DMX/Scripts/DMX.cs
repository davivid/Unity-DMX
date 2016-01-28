using UnityEngine;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class DMX : MonoBehaviour 
{
	static DMX instance = null;

	//DMX USB PRO API
	private const byte DMX_PRO_HEADER_SIZE = 4;
	private const byte DMX_PRO_START_MSG = 0x7E;
	private const byte DMX_PRO_LABEL_DMX = 6;
	private const byte DMX_PRO_LABEL_SERIAL_NUMBER = 10;
	private const byte DMX_PRO_START_CODE = 0;
	private const byte DMX_PRO_START_CODE_SIZE = 1;
	private const byte DMX_PRO_END_MSG = 0xE7;
	private const byte DMX_PRO_END_MSG_SIZE = 1;
	//

	private static SerialPort serialPort;
	
	public List<string> serial_ports;
	public int serial_port_idx;
	public string deviceSerialNumber;
	public string deviceName;
	
	private int nChannels;
	private byte[] levels;
	private string[] labels;
	private byte[] txPacket, rxPacket;
	private int rxPacket_idx;
	private bool rxStarted, rxEnded;
	
	private bool sendDMX = false;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
