using System.IO.Ports;

namespace BarcodeScanner;
public class COMProperties( string portName, Parity parity, int baudRate, int dataBits, StopBits stopBits )
{
    #region Properties

    /// <summary>
    /// The name of the com port as shown in the device manager (COM1 or COM2 etc...)
    /// </summary>
    public string PortName { get; set; } = portName;

    /// <summary>
    /// Method of detecting errors in transmission
    /// </summary>
    public Parity Parity { get; set; } = parity;

    /// <summary>
    /// Common unit of measurement of symbol rate
    /// </summary>
    public int BaudRate { get; set; } = baudRate;

    /// <summary>
    /// The number of data bits in each character can be:
    /// 4 (for Baudot code)
    /// 5 (rarely used)
    /// 6 (for true ASCII)
    /// 7 (for most kinds of data, as this size matches the size of a byte)
    /// 8 (rarely used)
    /// </summary>
    public int DataBits { get; set; } = dataBits;

    /// <summary>
    /// Sent at the end of every character
    /// </summary>
    public StopBits StopBits { get; set; } = stopBits;

    #endregion
}
