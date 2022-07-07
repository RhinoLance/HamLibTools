using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamLib.TestRunner
{
	internal class TestLogger
	{
		string _lineBuffer = "";

		public void Write(string text)
		{
			_lineBuffer += text;
			Console.Write(text);
			
		}

		public void WriteLine() { 
			WriteLine("");
		}
		public void WriteLine(string text)
		{
			Console.WriteLine(text);
			
			_lineBuffer = "";
		}

		public void AddResultToLine(bool pass)
		{
			ClearLine();
			var colour = pass ? ConsoleColor.Green : ConsoleColor.Red;
			WriteColoured(pass ? "PASS" : "FAIL", colour);
			Console.Write(" " + _lineBuffer);
		}

		public void WriteColoured(string text, ConsoleColor foreground)
		{
			WriteColoured(text, foreground, null);
		}
		public void WriteColoured(string text, ConsoleColor foreground, ConsoleColor? background)
		{
			Console.ForegroundColor = foreground;
			if( background != null)
			{
				Console.BackgroundColor = (ConsoleColor)background;
			}
			Console.Write(text);
			Console.ResetColor();
		}

		private void ClearLine()
		{
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new String(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, Console.CursorTop);
		}

		


	}

	internal enum LinePosition
	{
		Start,
		End
	}
}
