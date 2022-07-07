using Xunit;
using HamLib.Testing;
using HamLib.TestRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;



namespace HamLib.Testing.Tests
{
	public class RigCtldCommunicatorTests
	{
		private readonly ITestOutputHelper _output;

		public RigCtldCommunicatorTests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Theory()]
		[InlineData("V VFOA", "RPRT 0")]
		
		public async void SendCommandTest(string command, string expected)
		{

			//Arrange
			var com = new RigCtldCommunicator("localhost", 3000);

			//Act
			string response = await com.SendCommand(command);
			
			//Assert
			Assert.Equal(expected, response);
		}

		[Theory()]
		[InlineData(0x91, "719")]
		//[InlineData(0x93, "123")]
		public async void SenasfdHexCommandTest(int code, string expected)
		{

			//Arrange
			var command = Convert.ToChar(code);
			var com = new RigCtldCommunicator("localhost", 3000);

			//Act
			string response = await com.SendCommand(command.ToString());

			//Assert
			Assert.Equal(expected, response);
		}


		[Theory()]
		[InlineData("0x90 719", "RPRT 0")]
		[InlineData("0x91", "719")]
		[InlineData("0x8b", "0")]
		public async void SendHexCommandTest(string command, string expected)
		{

			//Arrange
			var testItem = new TestItem(command, expected, "", false);
			var com = new RigCtldCommunicator("localhost", 3000);

			//Act
			string response = await com.SendCommand(testItem.GetCommandForSending());

			//Assert
			Assert.Equal(expected, response);
		}
	}
}