/* Copyright 2014 Marco Minerva,  marco.minerva@gmail.com

   Licensed under the Microsoft Public License (MS-PL) (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://opensource.org/licenses/MS-PL

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS_APP || WINDOWS_PHONE_APP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net.Sockets;
#endif

namespace ArDrone2.Interaction
{
    public static class DroneControl
    {
#if WINDOWS_APP || WINDOWS_PHONE_APP
        private static DatagramSocket udpSocket;
        private static DataWriter udpWriter;
#else
        private static UdpClient udpSocket;
#endif

        private static volatile uint sequenceNumber = 1;
        private static volatile bool stop = false;

        public static bool StubMode { get; set; }

        private const string HOST_NAME = "192.168.1.1";
        private const string REMOTE_PORT = "5556";

        public static async Task ConnectAsync(string hostName = HOST_NAME, string port = REMOTE_PORT)
        {
            if (!StubMode)
            {
#if WINDOWS_APP || WINDOWS_PHONE_APP
                // Set up the UDP connection.
                var droneIP = new HostName(hostName);

                udpSocket = new DatagramSocket();
                await udpSocket.BindServiceNameAsync(port);
                await udpSocket.ConnectAsync(droneIP, port);
                udpWriter = new DataWriter(udpSocket.OutputStream);

                udpWriter.WriteByte(1);
                await udpWriter.StoreAsync();
#else
                udpSocket = new UdpClient();
                udpSocket.Connect(hostName, int.Parse(port));

                byte[] datagram = new byte[1] { 1 };
                await udpSocket.SendAsync(datagram, datagram.Length);
#endif
            }

            var loop = Task.Run(() => DroneLoop());
        }

        // Send a command to the drone
        private static async Task SendCommandAsync(string command)
        {
            var message = string.Format("Command: {0}", command);

#if WINDOWS_APP || WINDOWS_PHONE_APP
            Debug.WriteLine(message);
#else
            Trace.WriteLine(message);
#endif

            if (!StubMode)
            {
#if WINDOWS_APP || WINDOWS_PHONE_APP
                udpWriter.WriteString(command);
                await udpWriter.StoreAsync();
#else
                byte[] datagram = Encoding.ASCII.GetBytes(command);
                await udpSocket.SendAsync(datagram, datagram.Length);
#endif
            }
        }

        // destroy the connection
        public static void Close()
        {
            if (!StubMode)
            {
#if WINDOWS_APP || WINDOWS_PHONE_APP
                udpSocket.Dispose();

                udpSocket = null;
                udpWriter = null;
#else
                udpSocket.Close();
                udpSocket = null;
#endif
            }

            Stop();
        }

        private static async Task DroneLoop()
        {
            while (true)
            {
                if (stop)
                {
                    stop = false;
                    sequenceNumber = 1;
                    break;
                }

                var commandToSend = DroneState.GetNextCommand(sequenceNumber);
                await SendCommandAsync(commandToSend);

                sequenceNumber++;
                await Task.Delay(30);
            }
        }

        // Suspends the control loop and resets the sequence number counter
        public static void Stop()
        {
            stop = true;
        }
    }
}
