using System;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace ClassRunner.common;
public class BarcodeScannerHandler : IDisposable
{
    #region Constructor

    /// <summary>
    /// Constructor that allow to open connection and send <see cref="ScannerAction"/> later
    /// </summary>
    /// <param name="comProperties"><c>COM</c> connection info</param>
    public BarcodeScannerHandler( COMProperties comProperties ) => ActivateSerialPort( comProperties );

    /// <summary>
    /// Constructor that open connection and set the <see cref="ScannerAction"/> action
    /// </summary>
    /// <param name="scannerAction">Scan method</param>
    /// <param name="comProperties"><c>COM</c> connection info</param>
    public BarcodeScannerHandler( Action<string> scannerAction, COMProperties comProperties )
    {
        ScannerAction = scannerAction;
        ActivateSerialPort( comProperties );
    }

    #endregion

    #region Properties

    /// <summary>
    /// Instance of open serial port in the device
    /// </summary>
    private SerialPort SerialPort { get; set; }

    /// <summary>
    /// Reading action fired when user scan barcode
    /// </summary>
    public Action<string> ScannerAction { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Activate the serial port using given COM properties
    /// </summary>
    /// <param name="comProp"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ActivateSerialPort( COMProperties comProp )
    {
        try
        {
            SerialPort = new SerialPort( comProp.PortName,
                                               comProp.BaudRate,
                                               comProp.Parity,
                                               comProp.DataBits,
                                               comProp.StopBits );
            OpenConnection( SingalSerialPort );
        }
        catch ( Exception e )
        {
            throw e;
        }
    }

    /// <summary>
    /// Open the newly created serial port
    /// </summary>
    /// <param name="serialPort">The serial port to be opened</param>
    private void OpenConnection( SerialPort serialPort )
    {
        try
        {
            serialPort.DataReceived += DataReceiver;
            if ( !serialPort.IsOpen )
            {
                serialPort.Open();
            }
        }
        catch ( Exception e )
        {
            throw e;
        }
    }

    /// <summary>
    /// <see cref="IDisposable"/> method
    /// </summary>
    public void Dispose() => Close();

    /// <summary>
    /// Closing the instance manually
    /// </summary>
    public void Close() => Dispatch( CloseHandler );

    /// <summary>
    /// Closing the instance using <c>Closed</c> event
    /// </summary>
    public void Close( object sender, EventArgs e ) => Close();

    /// <summary>
    /// Calling for the application dispatcher
    /// </summary>
    /// <param name="action">The action to be invoked</param>
    private void Dispatch( Action action ) => Application.Current.Dispatcher?.Invoke( action );

    /// <summary>
    /// Recive the data from the COM event
    /// </summary>
    /// <param name="serialPort">The serial port connection</param>
    /// <param name="_event">The <see cref="SerialDataReceivedEventArgs"/> event argument</param>
    private void DataReceiver( object serialPort, SerialDataReceivedEventArgs _event )
        => Dispatch( () => ScannerAction?.Invoke( (serialPort as SerialPort).ReadExisting().Trim() ) );

    /// <summary>
    /// Safe way to close the serial port connection
    /// </summary>
    private void CloseHandler()
    {
        Thread safeClose = new(new ThreadStart(SerialPort.Close));
        safeClose.SetApartmentState( ApartmentState.STA );
        safeClose.Start();
        safeClose.Join();
    }

    #endregion
}
