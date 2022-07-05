using Xunit;
using HamLib.Testing;
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
		[InlineData("v", "VFOA")]
		[InlineData("f", "146500000")]
		[InlineData("V VFOB", "")]
		[InlineData("G 444000000", "set_freq: 444000000")]		
		[InlineData("G 44410000", "set_freq: 444100000")]		
		[InlineData("G 444200000", "set_freq: 442000000")]
		[InlineData("G 444300000", "set_freq: 443000000")]		
		[InlineData("G 444400000", "set_freq: 444000000")]		
		[InlineData("G 444500000", "set_freq: 445000000")]

		public async void SendCommandTest(string command, string expected)
		{

			//Arrange
			var com = new RigCtldCommunicator("localhost", 3000);

			//Act
			string response = await com.SendCommand(command);
			
			//Assert
			Assert.Equal(expected, response);
		}
	}
}