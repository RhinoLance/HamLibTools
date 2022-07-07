

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HamLib.TestRunner;

public class TestItem
{

	public string Command { get; set; }
	public string Expect { get; set; }
	public string Comment { get; set; }
	public bool RequiresUserConfirmation { get; set; }

	public TestItem() {
		Command = "";
		Expect = "";
		Comment = "";
		RequiresUserConfirmation = false;
	}

	public TestItem(string command, string expect, string comment, bool requiresUserConfirmation)
	{
		Command = command;
		Expect = expect;
		Comment = comment;
		RequiresUserConfirmation = requiresUserConfirmation;
	}

	public string GetCommandForSending()
	{
		if (Command == null) return "";

		var rg = new Regex(@"(0x[0-9a-h]{2})(.*)");
		var match = rg.Match(Command);

		if (!match.Success)
		{
			return Command;
		}

		string retval = "";

		//convert the hex part to a string.
		var hexVal = Convert.ToInt32(match.Groups[1].Value, 16);
		retval += Convert.ToChar(hexVal).ToString();

		//add the rest
		if (match.Length >= 3)
		{
			retval += match.Groups[2].Value;
		}

		return retval;

	}

}
