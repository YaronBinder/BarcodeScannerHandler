using IniFiles;
using DevTools;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace ClassRunner.common;

public class ScannerManager : IDisposable
{
    #region Constructor

    public ScannerManager() => ActivateAllSerialPorts();

    public ScannerManager( Action<string> scannerAction ) : this() => ScannerAction = scannerAction;

    public ScannerManager( Action<string> scannerAction, string portName )
    {
        ScannerAction = scannerAction;
        ActivateSerialPort( portName );
    }

    public ScannerManager( Action<string> scannerAction, COMProperties comProperties )
    {
        ScannerAction = scannerAction;
        ActivateSerialPort( comProperties );
    }

    #endregion

    #region Properties

    /// <summary>
    /// Single instance of open serial port in the device
    /// </summary>
    private SerialPort SingalSerialPort { get; set; }

    /// <summary>
    /// Reading action fired when user scan barcode
    /// </summary>
    public Action<string> ScannerAction { get; set; }

    /// <summary>
    /// List of all seriel ports in the device
    /// </summary>
    private List<SerialPort> ScanCodeSerialPort { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Activate all serial port in the machine
    /// </summary>
    private void ActivateAllSerialPorts()
    {
        try
        {
            ScanCodeSerialPort = [];
            foreach ( string portName in SerialPort.GetPortNames() )
            {
                try
                {
                    SerialPortIniManager spim = new();
                    ScanCodeSerialPort.Add(
                        new SerialPort(
                            portName,
                            Convert.ToInt32( spim.BaudRate ),
                            (Parity)Enum.Parse( typeof( Parity ), spim.Parity ),
                            Convert.ToInt32( spim.DataBits ),
                            (StopBits)Enum.Parse( typeof( StopBits ), spim.StopBits ) ) );
                }
                catch ( Exception e )
                {
                    e.WriteLog();
                }
            }
            ScanCodeSerialPort.ForEach( OpenPort );
        }
        catch ( Exception e )
        {
            e.WriteLog();
            ScanCodeSerialPort = null;
        }
    }

    /// <summary>
    /// Activate the serial port using given or default port name
    /// </summary>
    /// <param name="portName">The port name to be opened</param>
    private void ActivateSerialPort( string portName )
    {
        try
        {
            SerialPortIniManager spim = new();
            SingalSerialPort = new SerialPort(
                portName ?? spim.CurrentPortName,
                Convert.ToInt32( spim.BaudRate ),
                (Parity)Enum.Parse( typeof( Parity ), spim.Parity ),
                Convert.ToInt32( spim.DataBits ),
                (StopBits)Enum.Parse( typeof( StopBits ), spim.StopBits ) );
            OpenPort( SingalSerialPort );
        }
        catch ( Exception e )
        {
            e.WriteLog();
            ScanCodeSerialPort = null;
        }
    }

    /// <summary>
    /// Activate the serial port using given COM properties
    /// </summary>
    /// <param name="comProp"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ActivateSerialPort( COMProperties comProp )
    {
        try
        {
            SingalSerialPort = new SerialPort( comProp.PortName,
                                               comProp.BaudRate,
                                               comProp.Parity,
                                               comProp.DataBits,
                                               comProp.StopBits );
            OpenPort( SingalSerialPort );
        }
        catch ( Exception e )
        {
            e.WriteLog();
            ScanCodeSerialPort = null;
        }
    }

    /// <summary>
    /// Open the newly created serial port
    /// </summary>
    /// <param name="serialPort">The serial port to be opened</param>
    private void OpenPort( SerialPort serialPort )
    {
        try
        {
            serialPort.DataReceived += DataRecive;
            if ( !serialPort.IsOpen )
            {
                serialPort.Open();
            }
        }
        catch ( Exception e )
        {
            e.WriteLog();
        }
    }

    private void DataRecive( object serialPort, SerialDataReceivedEventArgs _event )
        => Dispatch( () => ScannerAction?.Invoke( (serialPort as SerialPort).ReadExisting().Trim() ) );

    public void Close( object sender, EventArgs e ) => Close();

    public void Close() => Dispatch( CloseHandler );

    private void Dispatch( Action action ) => Application.Current.Dispatcher?.Invoke( action );

    private void CloseHandler()
    {
        Thread safeClose = new(new ThreadStart(() =>
        {
            if (ScanCodeSerialPort?.Count is not null and > 0)
            {
                ScanCodeSerialPort.ForEach(port => { if (port?.IsOpen is not null) port.Close(); });
            }
            else if (SingalSerialPort?.IsOpen is not null)
            {
                SingalSerialPort.Close();
            }
        }));
        safeClose.SetApartmentState( ApartmentState.STA );
        safeClose.Start();
        safeClose.Join();
    }

    public void Dispose() => Close();

    #endregion
}