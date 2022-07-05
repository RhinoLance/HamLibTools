using System.Text.Json;
using HamLib.Testing;
using HamLib.TestRunner;

string filePath = Environment.GetCommandLineArgs()[1];
string server = Environment.GetCommandLineArgs()[2];
int port = Int32.Parse(Environment.GetCommandLineArgs()[3]);

try
{
	var tests = GetTests(filePath);

	if( tests.Count() == 0)
	{
		Console.WriteLine("No tests found");
		Environment.Exit(0);
	}

	await ExecuteTests(server, port, tests);
}
catch(IOException ex)
{
	Console.WriteLine(ex.Message);
	Environment.Exit(ex.HResult);
}

static IEnumerable<TestItem> GetTests(string filePath)
{
	string tests;

	try
	{
		tests = File.ReadAllText(filePath);
		
	}
	catch (IOException ex)
	{
		
		var exception = new IOException("Input file could not be opened.\n" + ex.Message );
		exception.HResult = ex.HResult;

		throw exception;

	}

	var testList = new List<TestItem>();

	try
	{
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
		testList = JsonSerializer.Deserialize<List<TestItem>>(tests, options);

		if (testList == null)
		{
			throw new FormatException();
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine($"The source file was not valid JSON.\n{ex.Message}");
		throw new FormatException();
	}

	return testList;
}

static async Task ExecuteTests( string host, int port, IEnumerable<TestItem> testList )
{
	int testNum = 0;
	using (var com = new RigCtldCommunicator(host, port))
	{
		string output = "";
		foreach (var test in testList)
		{
			testNum++;

			if (string.IsNullOrWhiteSpace(test.Command) || string.IsNullOrWhiteSpace(test.Command))
			{
				Console.WriteLine($"Test {testNum} invalid with Command: '{test.Command}', Expect: '{test.Expect}'");
				continue;
			}

			Console.Write($"> {test.Command}");
			var extraTab = test.Command.Length < 8 ? "\t" : "";
			output = $" Sending '{test.Command}' \t{extraTab}==>\t '{test.Expect}' ";

			var result = await com.SendCommand(test.Command);
			if (result == test.Expect)
			{
				ConsoleWriteResult(true, output);
			}
			else
			{
				output += $" \t Returned '{result}' instead";
				ConsoleWriteResult(false, output);
			}
		}
	};

}

static void ConsoleWriteResult(bool pass, string output)
{
	var (left, top) = Console.GetCursorPosition();
	Console.SetCursorPosition(0,top);
	Console.ForegroundColor = pass ? ConsoleColor.Green : ConsoleColor.Red;
	Console.Write(pass ? "PASS" : "FAIL");
	Console.ResetColor();
	Console.WriteLine(output);
}

