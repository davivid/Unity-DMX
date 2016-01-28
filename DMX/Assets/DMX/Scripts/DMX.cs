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
	//DMX USB Pro API
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
	
	public int nChannels = 24;
	public byte[] levels;
	public string[] labels;
	private byte[] rxPacket;
	private int rxPacket_idx;
	private bool rxStarted, rxEnded;
	
	public bool sendDMX = false;
	
	public byte[] txPacket;
	
	void Start()
	{	
		
		rxPacket = new byte[600];
		setChannels();
		
		GetPortNames();

		if (deviceName != "") OpenConnection();
	}
	
	public void setChannels()
	{
		levels = new byte[nChannels];
		labels = new string[nChannels];
	}
	
	public void setLevel(int channel, int val)
	{
		int c = channel-1;
		
		if (c >= 0 && c < nChannels)
		{
			levels[c] = (byte)Mathf.Clamp(val,0,255);
		}
		
		sendDMX = true;
	}

	public void setLabel(int channel, string label)
	{
		int c = channel-1;
		
		if (c >= 0 && c < nChannels)
		{
			labels[c] = label;
		}
	}	
	
	void SendPacket(byte label, byte[] data)
	{
		if (serialPort == null || !serialPort.IsOpen) return;

		int dataSize = data.Length;
		if (dataSize > 0) dataSize += DMX_PRO_START_CODE_SIZE;
		
		txPacket = new byte[DMX_PRO_HEADER_SIZE + dataSize + DMX_PRO_END_MSG_SIZE];

		txPacket[0] = DMX_PRO_START_MSG;
		txPacket[1] = label;
		txPacket[2] = (byte)(dataSize & 255);
		txPacket[3] = (byte)((dataSize >> 8) & 255);
		
		if (dataSize > 0)
		{
			txPacket[4] = DMX_PRO_START_CODE;
			
			for(int i = 0; i < dataSize-1; i++)
			{
				txPacket[DMX_PRO_HEADER_SIZE+1 + i] = data[i];  
			}
		}
		
		txPacket[DMX_PRO_HEADER_SIZE + dataSize] = DMX_PRO_END_MSG;
		
		serialPort.Write(txPacket, 0, txPacket.Length);
	}
	
	void ProcessSerialNumber(byte[] data)
	{	
		deviceSerialNumber = data[3].ToString("X") + data[2].ToString("X") + data[1].ToString("X") + data[0].ToString("X");
	}
	
	void ProcessRxPacket()
	{
		int label = rxPacket[1];
		int dataSizeLSB = rxPacket[2];
		int dataSizeMSB = rxPacket[3];
		
		byte[] data = new byte[dataSizeLSB];
		Buffer.BlockCopy(rxPacket,DMX_PRO_HEADER_SIZE,data,0,dataSizeLSB);
		
		switch (label)
		{
			case DMX_PRO_LABEL_SERIAL_NUMBER:
				ProcessSerialNumber(data);
			break;
		}
	}
	
	void Update()
	{
		if (serialPort == null || !serialPort.IsOpen) return;
		
		if (sendDMX) 
		{
			sendDMX = false;
			SendPacket(DMX_PRO_LABEL_DMX, levels);
		}
		
		while (serialPort.BytesToRead > 0)
		{
			byte inByte = (byte)serialPort.ReadByte();
			
			if (inByte == DMX_PRO_START_MSG)
			{
				rxPacket_idx = 0;
				rxStarted = true;
				rxEnded = false;
				
				rxPacket[rxPacket_idx++] = inByte;
			}
			
			else if (inByte == DMX_PRO_END_MSG)
			{
				rxEnded = true;
				if (rxStarted && rxEnded) ProcessRxPacket();
			}
			
			else if (rxStarted) 
			{
				rxPacket[rxPacket_idx++] = inByte;
			}
			
			//TODO: if (Timeout) return;
		}
	}
	
	void GetPortNames()
	{
		int p = (int)System.Environment.OSVersion.Platform;
		serial_ports = new List<string>();
		serial_ports.Add("");
		
		if(p == 4 || p == 128 || p == 6)
		{
			string[] ttys = Directory.GetFiles("/dev/", "tty.*");
			foreach(string dev in ttys)
			{
				serial_ports.Add(dev.Replace("/", "\\")); //Replace forward slash to play nicely with gui.		
			}
		}
	}
	
	public void OpenConnection()
	{
		if (serialPort != null) { serialPort.Close(); serialPort.Dispose(); }

		serialPort = new SerialPort(serial_ports[serial_port_idx].Replace("\\", "/"), 57600, Parity.None, 8, StopBits.One);
		
		try 
		{
			serialPort.Open();
			serialPort.ReadTimeout = 50;
			deviceName = serial_ports[serial_port_idx].Replace("\\", "/");
			SendPacket(DMX_PRO_LABEL_SERIAL_NUMBER, new byte[0]);
		}
		catch (System.Exception e) 
		{
			deviceSerialNumber = "";
			deviceName = "";
			Debug.LogException(e);
		}
	}
	
	void OnApplicationQuit()
	{
		for (int i =0; i < nChannels; i++)
		{
			levels[i] = 0;
		}

		SendPacket(DMX_PRO_LABEL_DMX, levels);

		if (serialPort != null) serialPort.Close();
	}
}