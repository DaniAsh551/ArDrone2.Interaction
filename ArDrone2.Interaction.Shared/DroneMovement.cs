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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArDrone2.Interaction
{
    internal class DroneMovement
    {
        // Drone Takeoff
        public static string GetDroneTakeoff(uint sequenceNumber)
        {
            return CreateATREFCommand(sequenceNumber, "290718208");
        }

        // Drone land
        public static string GetDroneLand(uint sequenceNumber)
        {
            return CreateATREFCommand(sequenceNumber, "290717696");
        }

        // Drone will do nothing
        public static string GetHoveringCommand(uint sequenceNumber)
        {
            return CreateATPCMDCommand(sequenceNumber, "0,0,0,0", 0);
        }

        // Strafe drone left or right (x axis) at velocity in range [-1,1], forward or backward (y axis) at velocity in range [-1,1]
        // ascend or descend at velocity in range [-1, 1], roll at velocity in range [-1, 1]
        public static string GetDroneMove(uint sequenceNumber, double velocityX, double velocityY, double velocityAscend, double velocityRoll)
        {
            var valueX = FloatConversion(velocityX);
            var valueY = FloatConversion(velocityY);
            var valueAscend = FloatConversion(velocityAscend);
            var valueRoll = FloatConversion(velocityRoll);

            var command = string.Format("{0},{1},{2},{3}", valueX, valueY, valueAscend, valueRoll);
            return CreateATPCMDCommand(sequenceNumber, command);
        }

        #region Test methods

        // Strafe drone left or right (x axis) at velocity in range [-1,1] and forward or backward (y axis) at velocity in range [-1,1]
        public static string GetDroneStrafe(uint sequenceNumber, double velocityX, double velocityY)
        {
            var valueX = FloatConversion(velocityX);
            var valueY = FloatConversion(velocityY);

            var command = string.Format("{0},{1},0,0", valueX, valueY);
            return CreateATPCMDCommand(sequenceNumber, command);
        }

        // Strafe drone left or right (x axis) at velocity in range [-1,1]
        public static string GetDroneStrafeLeftRight(uint sequenceNumber, double velocity)
        {
            var value = FloatConversion(velocity);
            var command = string.Format("{0},0,0,0", value);

            return CreateATPCMDCommand(sequenceNumber, command);
        }

        // Strafe drone forward or backward (y axis) at velocity in range [-1,1]
        public static string GetDroneStrafeForwardBackward(uint sequenceNumber, double velocity)
        {
            var value = FloatConversion(velocity);
            var command = string.Format("0,{0},0,0", value);

            return CreateATPCMDCommand(sequenceNumber, command);
        }

        // Drone ascend or descend at velocity in range [-1, 1]
        public static string GetDroneAscendDescend(uint sequenceNumber, double velocity)
        {
            var value = FloatConversion(velocity);
            var command = string.Format("0,0,{0},0", value);

            return CreateATPCMDCommand(sequenceNumber, command);
        }

        #endregion

        #region Helpers

        // returns an ATREF command
        private static string CreateATREFCommand(uint sequenceNumber, string command)
        {
            return string.Format("AT*REF={0},{1}{2}", sequenceNumber, command, Environment.NewLine);
        }

        // Return a full ATPCMD command
        private static string CreateATPCMDCommand(uint sequenceNumber, string command, int mode = 1)
        {
            return string.Format("AT*PCMD={0},{1},{2}{3}", sequenceNumber, mode, command, Environment.NewLine);
        }

        // converts a value from range [-1,1] to a signed 32-bit integer that can be used in AT*PCMD commands
        private static int FloatConversion(double velocity)
        {
            var floatBytes = BitConverter.GetBytes((float)velocity);
            return BitConverter.ToInt32(floatBytes, 0);
        }

        #endregion
    }
}