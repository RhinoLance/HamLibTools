using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HamLib.Testing;

public class RigCtldCommunicator : IDisposable
{
	private readonly string _host;
	private readonly int _port;
	private TcpClient _tcpClient;
	private StreamReader _reader;
	private TextWriter _writer;

	private bool _disposing = false;
	private Queue<string> _responseQueue = new Queue<string>();
	private Thread _readerThread;

	public RigCtldCommunicator(string host, int port)
	{
		_host = host;
		_port = port;

		_tcpClient = new TcpClient();
		_tcpClient.Connect(host, port);
		var stream = _tcpClient.GetStream();

		_reader = new StreamReader(stream);
		_writer = new StreamWriter(stream);

		_readerThread = new Thread(ReadResponseStream);
		_readerThread.Start();

	}

	public async Task<string> SendCommand(string command)
	{
		_writer.WriteLine(command);
		await _writer.FlushAsync();

		//var response = await GetNextResponse();
		var response = await GetAllResponses();

		return response;

	}

	private async Task<string> GetNextResponse()
	{
		while (_responseQueue.Count == 0 && !_disposing)
		{
			await Task.Delay(100);
		}

		return _disposing ? "" : _responseQueue.Dequeue();
	}

	private async Task<string> GetAllResponses()
	{
		StringBuilder output = new StringBuilder();

		while (_responseQueue.Count == 0 && !_disposing)
		{
			await Task.Delay(100);
		}

		while (_responseQueue.Count > 0 )
		{
			output.Append(_responseQueue.Dequeue() + " ");
		}

		return output.ToString().Trim();
	}

	private void ReadResponseStream()
	{
		
		while (!_disposing)
		{
			Trace.WriteLine($"Disposing: {_disposing}");
			
			try
			{
				var line = _reader.ReadLine();
				if (line == null) continue;

				Trace.WriteLine(line);
				_responseQueue.Enqueue(line);
			}
			catch(IOException)
			{
				//The stream has probably closed, so acceptible;
				break;
			}
			
			

			

		}
	}

	public void Dispose()
	{
		_disposing = true;
		_writer.Close();
		_writer.Dispose();
		_reader.Close();
		_reader.Dispose();
		_tcpClient.Dispose();
	}

	

}
