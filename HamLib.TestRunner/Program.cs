using System.Text.Json;
using HamLib.Testing;
using HamLib.TestRunner;

string filePath = Environment.GetCommandLineArgs()[1];
string server = Environment.GetCommandLineArgs()[2];
int port = Int32.Parse(Environment.GetCommandLineArgs()[3]);

var logger = new TestLogger();
int passCount = 0;
int failCount = 0;
DateTimeOffset start;
DateTimeOffset end;

try
{
	var tests = GetTests(filePath);

	if( tests.Count() == 0)
	{
		Console.WriteLine("No tests found");
		Environment.Exit(0);
	}

	start = DateTimeOffset.UtcNow;
	await ExecuteTests(server, port, tests);
	end = DateTimeOffset.UtcNow;

	writeResult();


}
catch(Exception ex)
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

async Task ExecuteTests( string host, int port, IEnumerable<TestItem> testList )
{
	int testNum = 0;
	

	using (var com = new RigCtldCommunicator(host, port))
	{
		string result = "";
		
		foreach (var test in testList)
		{
			testNum++;

			if( !string.IsNullOrWhiteSpace(test.Comment))
			{
				logger.WriteColoured(test.Comment, ConsoleColor.Gray, ConsoleColor.DarkCyan);
			}

			if (test.RequiresUserConfirmation)
			{
				logger.Write(" (Y/n) ");
				var answer = Console.ReadKey();
				var pass = answer.KeyChar != 'n';
				updateCount(pass);

				logger.AddResultToLine(pass);
			}

			if (!string.IsNullOrWhiteSpace(test.Command))
			{
				var extraTab = test.Command.Length < 8 ? "\t" : "";
				logger.Write($" Sending '{test.Command}' \t{extraTab}==>\t ");
				
				result = await com.SendCommand(test.GetCommandForSending());

				logger.Write($"'{result}'");

			}

			if (!string.IsNullOrWhiteSpace(test.Expect))
			{
				var pass = result == test.Expect;
				updateCount(pass);
				if (!pass)
				{
					logger.Write($" \t Expected '{test.Expect}'");
				}

				logger.AddResultToLine(pass);
			}

			logger.WriteLine();

			
		}
	};

}

void updateCount(bool pass)
{
	if( pass)
	{
		passCount++;
	}
	else
	{
		failCount++;
	}
}

void writeResult()
{

	var completionTime = end.Subtract(start).TotalSeconds;
	logger.WriteLine();
	logger.Write($"{passCount + failCount} tests completed in {(int)end.Subtract(start).TotalSeconds} seconds. ");
	logger.WriteLine();
	logger.WriteColoured( $"{passCount} passed ", ConsoleColor.Green);
	logger.WriteColoured($"{failCount} failed ", failCount > 0 ? ConsoleColor.Red : ConsoleColor.Gray);
	logger.WriteLine();
}
