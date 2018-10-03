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
    public static class DroneState
    {
        #region Properties

        // values from [-1, 1]

        private static double strafeX;
        public static double StrafeX
        {
            get
            {
                return strafeX;
            }
            set
            {
                strafeX = Normalize(value);
            }
        }

        private static double strafeY;
        public static double StrafeY
        {
            get { return strafeY; }
            set
            {
                strafeY = Normalize(value);
            }
        }

        private static double ascendY;
        public static double AscendY
        {
            get { return ascendY; }
            set
            {
                ascendY = Normalize(value);
            }
        }

        private static double rollX;
        public static double RollX
        {
            get { return rollX; }
            set
            {
                rollX = Normalize(value);
            }
        }

        // flag for takeoff and landing
        public static bool Flying { get; set; }

        #endregion

        public static void TakeOff()
        {
            Flying = true;
        }

        public static void Land()
        {
            Flying = false;
        }

        // AT*PCMD=230,1,0,0,1065353216,0 (value: 1)
        public static void GoUp(double value = 0.3)
        {
            var d = NormalizePositive(value);
            ascendY = d;
        }

        // AT*PCMD=168,1,0,0,-1082130432,0 (value: 1)
        public static void GoDown(double value = 0.3)
        {
            var d = NormalizePositive(value) * -1;
            ascendY = d;
        }

        public static void StopAscend()
        {
            ascendY = 0;
        }

        public static async Task GoUpForAsync(double value, TimeSpan duration)
        {
            GoUp(value);
            await Task.Delay(duration);
            StopAscend();
        }

        public static async Task GoDownForAsync(double value, TimeSpan duration)
        {
            GoDown(value);
            await Task.Delay(duration);
            StopAscend();
        }

        // AT*PCMD=342,1,0,-1090519040,0,0 (value: 0.5)
        public static void GoForward(double value = 0.3)
        {
            var d = NormalizePositive(value) * -1;
            strafeY = d;
        }

        // AT*PCMD=170,1,0,1056964608,0,0 (value: 0.5)
        public static void GoBackward(double value = 0.3)
        {
            var d = NormalizePositive(value);
            strafeY = d;
        }

        public static void StopForwardBackward()
        {
            strafeY = 0;
        }

        public static async Task GoForwardForAsync(double value, TimeSpan duration)
        {
            GoForward(value);
            await Task.Delay(duration);
            StopForwardBackward();
        }

        public static async Task GoBackwardForAsync(double value, TimeSpan duration)
        {
            GoBackward(value);
            await Task.Delay(duration);
            StopForwardBackward();
        }

        // AT*PCMD=324,1,-1090519040,0,0,0 (value: 0.5)
        public static void GoLeft(double value = 0.3)
        {
            var d = NormalizePositive(value) * -1;
            strafeX = d;
        }

        // AT*PCMD=225,1,1056964608,0,0,0 (value: 0.5)
        public static void GoRight(double value = 0.3)
        {
            var d = NormalizePositive(value);
            strafeX = d;
        }

        public static void StopLeftRight()
        {
            strafeX = 0;
        }

        public static async Task GoLeftForAsync(double value, TimeSpan duration)
        {
            GoLeft(value);
            await Task.Delay(duration);
            StopLeftRight();
        }

        public static async Task GoRightForAsync(double value, TimeSpan duration)
        {
            GoRight(value);
            await Task.Delay(duration);
            StopLeftRight();
        }

        // AT*PCMD=728,1,0,0,0,1056964608 (value: 0.5)
        public static void RotateRight(double value = 0.5)
        {
            var d = NormalizePositive(value);
            rollX = d;
        }

        // AT*PCMD=171,1,0,0,0,-1090519040 (value: 0.5)
        public static void RotateLeft(double value = 0.5)
        {
            var d = NormalizePositive(value) * -1;
            rollX = d;
        }

        public static void StopRotate()
        {
            rollX = 0;
        }

        public static async Task RotateRightForAsync(double value, TimeSpan duration)
        {
            RotateRight(value);
            await Task.Delay(duration);
            StopRotate();
        }

        public static async Task RotateLeftForAsync(double value, TimeSpan duration)
        {
            RotateLeft(value);
            await Task.Delay(duration);
            StopRotate();
        }

        public static void Stop()
        {
            strafeX = 0;
            strafeY = 0;
            ascendY = 0;
            rollX = 0;
        }

        #region Helpers

        private static bool isFlying;

        private static double Normalize(double d)
        {
            if (d < -1)
                return -1;

            if (d > 1)
                return 1;

            return d;
        }

        private static double NormalizePositive(double d)
        {
            if (d < 0)
                return 0;

            if (d > 1)
                return d;

            return d;
        }

        internal static string GetNextCommand(uint sequenceNumber)
        {
            // Determine if the drone needs to take off or land
            if (Flying && !isFlying)
            {
                isFlying = true;
                return DroneMovement.GetDroneTakeoff(sequenceNumber);
            }
            else if (!Flying && isFlying)
            {
                isFlying = false;
                return DroneMovement.GetDroneLand(sequenceNumber);
            }

            // If the drone is flying, sends movement commands to it.
            if (isFlying && (StrafeX != 0 || StrafeY != 0 || AscendY != 0 || RollX != 0))
                return DroneMovement.GetDroneMove(sequenceNumber, StrafeX, StrafeY, AscendY, RollX);

            return DroneMovement.GetHoveringCommand(sequenceNumber);
        }

        #endregion
    }
}
