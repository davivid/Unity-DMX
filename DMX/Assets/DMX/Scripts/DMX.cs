using UnityEngine;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System;

namespace DP
{
	/// <summary>
	/// DMX Interface that targets the Enttec DMX USB Pro Mk2 and Clones.
	/// </summary>
	/// <remarks>
	/// Author: David Penney 2016.
	/// 
	/// Notes:
	///   1. Must set Player Settings | Optimisations | API Compatibility Level to ".NET 2.0" which enables Serial I/O
	///
	/// </remarks>

[ExecuteInEditMode]
public class DMX : MonoBehaviour 
{

	private const int N_DMX_CHANNELS = 512;
	public int nChannels {get {return N_DMX_CHANNELS;}}

	#region --- DMX PRO API
	/// <summary>
	/// DMX PRO API
	/// https://www.enttec.com/docs/dmx_usb_pro_api_spec.pdf
	///	The Mk2 API is available upon request to Enttec
	/// </summary>
	private const byte DMX_PRO_HEADER_SIZE = 4;
	private const byte DMX_PRO_START_MSG = 0x7E;
	private const byte DMX_PRO_LABEL_DMX = 6;
	private const byte DMX_PRO_LABEL_SERIAL_NUMBER = 10;
	private const byte DMX_PRO_START_CODE = 0;
	private const byte DMX_PRO_START_CODE_SIZE = 1;
	private const byte DMX_PRO_END_MSG = 0xE7;
	private const byte DMX_PRO_END_MSG_SIZE = 1;
	//
	private const int DMX_PRO_DATA_INDEX_OFFSET = 5;
	private const int DMX_PRO_MESSAGE_OVERHEAD = 6;
	#endregion

	/// <summary>
	/// Thread that handles the serial I/O.
	/// </summary>
	private Thread dmxThread;

	/// <summary>
	/// Flag to indicate Levels have been updated and DMX needs to be sent
	/// </summary>
	private bool updateDMX;

	/// <summary>
	/// The instance of the serial port used to communicate with the DMX controller.
	/// </summary>
	private static SerialPort serialPort;

	/// <summary>
	/// List containing strings of all available serial ports
	/// </summary>
	public List<string> serialPorts;

	/// <summary>
	/// Index of the selected serial port
	/// </summary>
	public int serialPortIdx;

	/// <summary>
	/// Buffer containg level of all DMX channels.
	/// </summary>
	private byte[] DMXLevels = new byte[N_DMX_CHANNELS];

	/// <summary>
	/// DMX message buffer length.
	/// </summary>
	private const int TX_BUFFER_LENGTH = DMX_PRO_MESSAGE_OVERHEAD + N_DMX_CHANNELS;

	/// <summary>
	/// DMX message buffer.
	/// </summary>
	private byte[] TxBuffer = new byte[DMX_PRO_MESSAGE_OVERHEAD + N_DMX_CHANNELS];

	void Start () 
	{	

		//List avialable serial ports and connected to last selected.
		GetPortNames();
		if (serialPortIdx > 0) OpenSerialPort();

		//Init the TX Buffer
		initTXBuffer();

		//Start the serial io thread
		dmxThread = new Thread(ThreadedIO);
		dmxThread.Start();

		//Flag to send default DMX values
		updateDMX = true;
	}

	/// <summary>
	/// Gets of sets the DMX level at specified channel
	/// </summary>
	public int this[int index]
	{
		get
		{
			if (index < 1 || index > N_DMX_CHANNELS)
			{
				throw new UnityException("Channel out fo range: " +index);
			}
			else
			{
				return (int)DMXLevels[index-1];
			}
		}
		set
		{
			if (index < 1 || index > N_DMX_CHANNELS)
			{
				throw new UnityException("Channel out fo range: " +index);
			}
			else
			{
				if (value < 0 || value > 255)
				{
					throw new UnityException("Level out fo range");
				}
				DMXLevels[index-1] = (byte)Mathf.Clamp(value,0,255);
				updateDMX = true;
			}
		}
	}

	/// <summary>
	/// The Threaded function that processes the serial i/o.
	/// </summary>
	private void ThreadedIO()
	{
	Debug.Log("Thread Start");
		while(true)
		{
			if (updateDMX)
			{
				updateDMX = false;
				Buffer.BlockCopy(DMXLevels,0,TxBuffer,DMX_PRO_DATA_INDEX_OFFSET,N_DMX_CHANNELS);
				if (serialPort != null && serialPort.IsOpen) {serialPort.Write(TxBuffer, 0, TX_BUFFER_LENGTH); };
			}

			//TODO: Recieve Serial
			//if (serialPort.BytesToRead > 0)
		}
	}

	/// <summary>
	/// List the avilable serial ports
	/// </summary>
	private void GetPortNames()
	{
		int p = (int)System.Environment.OSVersion.Platform;
		serialPorts = new List<string>();
		foreach (string name in SerialPort.GetPortNames()) {
			serialPorts.Add(name);
		}
	}

	/// <summary>
	/// Open the selected serial port
	/// </summary>
	public void OpenSerialPort()
	{
		if (serialPort != null)
		{
			serialPort.Close();
			serialPort.Dispose();
		}

		serialPort = new SerialPort(serialPorts[serialPortIdx].Replace("\\", "/"), 57600, Parity.None, 8, StopBits.One);

		try 
		{
			serialPort.Open();
			serialPort.ReadTimeout = 50;
			updateDMX = true;
		}
		catch (System.Exception e) 
		{
			Debug.LogException(e);
			serialPortIdx = 0;
		}
	}

	/// <summary>
	/// Init the TxBufffer with Header and End bytes
	/// </summary>
	private void initTXBuffer()
	{
		for(int i=0; i<TX_BUFFER_LENGTH; i++) TxBuffer[i] = (byte)255;

		TxBuffer[000] = DMX_PRO_START_MSG;
		TxBuffer[001] = DMX_PRO_LABEL_DMX;
		TxBuffer[002] = (byte)(N_DMX_CHANNELS+1 & 255);; 
		TxBuffer[003] = (byte)((N_DMX_CHANNELS+1 >> 8) & 255);
		TxBuffer[004] = DMX_PRO_START_CODE;
		TxBuffer[517] = DMX_PRO_END_MSG;
	}

	/// <summary>
	/// Shutdown DMX and cleanup.
	/// </summary>
	void OnApplicationQuit()
	{
		//Reset DMX levels to 0;
		for(int i=0; i< N_DMX_CHANNELS; i++) DMXLevels[i] = (byte)0x00;
		updateDMX = true;

		//Clean up
		dmxThread.Abort();
		if (serialPort != null)
		{
			serialPort.Close();
			serialPort.Dispose();
		}
	}
}
}

