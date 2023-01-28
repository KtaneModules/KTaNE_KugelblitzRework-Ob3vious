using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class KugelblitzCalculation
{
    public static string Calculate(KugelblitzLobby kugelblitzes)
    {
        KugelblitzBaseData baseData = kugelblitzes.GetQuirk<BaseStageManager>().GetFinalData();
        byte initialDirection = kugelblitzes.GetInitialDirection();

        kugelblitzes.GetLogger().WriteFormat("Final values: {0}, with initial direction of {1}.", baseData, initialDirection);

        byte[] offsets = new byte[7]; //red
        bool[] inverts = new bool[7]; //orange
        bool[] inserts = null; //yellow
        byte[] lengths = new byte[] { 1, 2, 3, 4, 5, 6, 7 }; //green
        byte[] turns = new byte[] { 3, 3, 3, 3, 3, 3 }; //blue
        bool[] flips = new bool[6]; //indigo
        KugelblitzGrid grid = new KugelblitzGrid(new WrapTransform(false, true, false), new WrapTransform(true, false, false)); //violet


        try
        {
            KugelblitzOffsetData offsetData = kugelblitzes.GetQuirk<OffsetStageManager>().GetFinalData();
            offsets = Enumerable.Range(0, 7).Select(x => (byte)((offsetData.GetFromIndex(x) + 6) % 7 + 1)).ToArray();

            kugelblitzes.GetLogger().WriteFormat("Final values for the red quirk: {0}.", offsetData);
        }
        catch { }

        try
        {
            KugelblitzInvertData invertData = kugelblitzes.GetQuirk<InvertStageManager>().GetFinalData();
            inverts = Enumerable.Range(0, 7).Select(x => invertData.GetFromIndex(x)).ToArray();

            kugelblitzes.GetLogger().WriteFormat("Final values for the orange quirk: {0}.", invertData);
        }
        catch { }

        try
        {
            KugelblitzInsertData insertData = kugelblitzes.GetQuirk<InsertStageManager>().GetFinalData();
            inserts = Enumerable.Range(0, 7).Select(x => insertData.GetFromIndex(x)).ToArray();

            kugelblitzes.GetLogger().WriteFormat("Final values for the yellow quirk: {0}.", insertData);
        }
        catch { }

        try
        {
            KugelblitzLengthData lengthData = kugelblitzes.GetQuirk<LengthStageManager>().GetFinalData();
            lengths = Enumerable.Range(0, 7).Select(x => (byte)((lengthData.GetFromIndex(x) + 6) % 7 + 1)).ToArray();

            kugelblitzes.GetLogger().WriteFormat("Final values for the green quirk: {0}.", lengthData);
        }
        catch { }

        try
        {
            KugelblitzTurnData turnData = kugelblitzes.GetQuirk<TurnStageManager>().GetFinalData();
            turns = Enumerable.Range(0, 6).Select(x => (byte)((turnData.GetFromIndex(x) + 2) % 3 + 1)).ToArray();

            kugelblitzes.GetLogger().WriteFormat("Final values for the blue quirk: {0}.", turnData);
        }
        catch { }

        try
        {
            KugelblitzFlipData flipData = kugelblitzes.GetQuirk<FlipStageManager>().GetFinalData();
            flips = Enumerable.Range(0, 6).Select(x => flipData.GetFromIndex(x)).ToArray();

            kugelblitzes.GetLogger().WriteFormat("Final values for the indigo quirk: {0}.", flipData);
        }
        catch { }

        try
        {
            KugelblitzWrapData wrapData = kugelblitzes.GetQuirk<WrapStageManager>().GetFinalData();
            grid = new KugelblitzGrid(wrapData.GetHWrap(), wrapData.GetVWrap());

            kugelblitzes.GetLogger().WriteFormat("Final values for the violet quirk: {0}.", wrapData);
        }
        catch { }





        Pivot pivot = new Pivot(
            baseData.GetX(), baseData.GetY(), baseData.GetR(),
            initialDirection,
            grid);

        int[] values = new int[7];

        for (int i = 0; i < values.Length; i++)
        {
            List<string> positions = new List<string>();
            for (int j = 0; j < lengths[i]; j++)
            {
                values[i] += pivot.Read();
                positions.Add(pivot.Coordinate());
                pivot.Step();
            }

            kugelblitzes.GetLogger().WriteFormat("Read from the following coordinates in order: {0}.", positions.Join(", "));

            if (i == values.Length - 1)
                break;

            if (flips[i])
                pivot.FlipTurningDirection();
            pivot.Turn(turns[i]);
        }

        values = Enumerable.Range(0, values.Length)
            .Select(x => (values[x] + offsets[x]) % 7).ToArray();

        string[] binaryGroups = Enumerable.Range(0, values.Length).Select(x => ((inserts == null) ? "" : (inserts[x] ? "1" : "0")) + ValueReader.DigitToBits(values[x])).ToArray();

        string binary = "1" + Enumerable.Range(0, binaryGroups.Length).Select(x => inverts[x] ? ValueReader.InvertBits(binaryGroups[x]) : binaryGroups[x]).Join("");

        string input = ValueReader.BitsToInput(binary);

        kugelblitzes.GetLogger().WriteFormat("Resulting digits after potential modifications are {0}.", values.Join(""));

        kugelblitzes.GetLogger().WriteFormat("Resulting binary after potential modifications are {0}.", binary);

        kugelblitzes.GetLogger().WriteFormat("Expecting a final input of {0}.", input);

        return input;
    }




    public class KugelblitzGrid
    {
        private readonly byte[,] _grid = {
            {5, 1, 3, 6, 4, 0, 2},
            {1, 2, 6, 4, 0, 5, 3},
            {4, 0, 5, 1, 3, 2, 6},
            {3, 5, 2, 0, 1, 6, 4},
            {0, 3, 4, 2, 6, 1, 5},
            {6, 4, 0, 5, 2, 3, 1},
            {2, 6, 1, 3, 5, 4, 0}
        };
        public readonly byte GridSize = 7;

        private bool _xFlipped;
        private bool _yFlipped;
        private bool _xySwapped;

        private WrapTransform _hWrap;
        private WrapTransform _vWrap;

        public KugelblitzGrid(WrapTransform hWrap, WrapTransform vWrap)
        {
            _hWrap = hWrap;
            _vWrap = vWrap;
        }

        public void WrapHorizontal()
        {
            Transform(_hWrap);
        }

        public void WrapVertical()
        {
            Transform(_vWrap);
        }

        private void Transform(WrapTransform wrapTransform)
        {
            Transform(wrapTransform.GetFlipX(), wrapTransform.GetFlipY(), wrapTransform.GetSwapXY());
        }

        private void Transform(bool flipX, bool flipY, bool swapXY)
        {
            if (!_xySwapped)
            {
                _xFlipped ^= flipX;
                _yFlipped ^= flipY;
            }
            else
            {
                _xFlipped ^= flipY;
                _yFlipped ^= flipX;
            }
            _xySwapped ^= swapXY;
        }

        public byte Get(byte x, byte y)
        {
            if (_xFlipped)
                x = (byte)(GridSize - 1 - x);
            if (_yFlipped)
                y = (byte)(GridSize - 1 - y);
            if (_xySwapped)
            {
                byte s = x;
                x = y;
                y = s;
            }
            return _grid[y, x];
        }

        public string TransformedCoordinate(byte x, byte y)
        {
            if (_xFlipped)
                x = (byte)(GridSize - 1 - x);
            if (_yFlipped)
                y = (byte)(GridSize - 1 - y);
            if (_xySwapped)
            {
                byte s = x;
                x = y;
                y = s;
            }
            return "(" + x + "," + y + ")";
        }
    }

    public class Pivot
    {
        private byte _x;
        private byte _y;
        private bool _r;
        private byte _direction;
        private KugelblitzGrid _grid;

        public Pivot(byte x, byte y, bool r, byte direction, KugelblitzGrid grid)
        {
            _x = x;
            _y = y;
            _r = r;
            _direction = direction;
            _grid = grid;
        }

        public void FlipTurningDirection()
        {
            _r = !_r;
        }

        public void Turn(byte steps)
        {
            steps %= 8;
            if (_r)
                steps = (byte)((8 - steps) % 8);
            _direction = (byte)((_direction + steps) % 8);
        }

        public void Step()
        {
            Move(Direction.directions[_direction]);
        }

        private void Move(Direction direction)
        {
            int x = _x + direction.GetX();
            int y = _y + direction.GetY();

            if (x >= 7)
            {
                x -= 7;
                _grid.WrapHorizontal();
            }
            else if (x < 0)
            {
                x += 7;
                _grid.WrapHorizontal();
            }

            if (y >= 7)
            {
                y -= 7;
                _grid.WrapVertical();
            }
            else if (y < 0)
            {
                y += 7;
                _grid.WrapVertical();
            }

            _x = (byte)x;
            _y = (byte)y;
        }

        public int Read()
        {
            return _grid.Get(_x, _y);
        }

        public string Coordinate()
        {
            return _grid.TransformedCoordinate(_x, _y);
        }

        public override string ToString()
        {
            return "(" + _x + "," + _y + "|" + _direction + (_r ? '-' : '+') + ")";
        }
    }

    public class Direction
    {
        public static Direction[] directions = new Direction[]
        {
            new Direction(0, -1),
            new Direction(1, -1),
            new Direction(1, 0),
            new Direction(1, 1),
            new Direction(0, 1),
            new Direction(-1, 1),
            new Direction(-1, 0),
            new Direction(-1, -1)
        };

        private int _xComp;
        private int _yComp;

        private Direction(int x, int y)
        {
            _xComp = x;
            _yComp = y;
        }

        public int GetX()
        {
            return _xComp;
        }

        public int GetY()
        {
            return _yComp;
        }
    }



    public class ValueReader
    {
        public static string DigitToBits(int digit)
        {
            return "" + ((digit >> 2) & 1) + ((digit >> 1) & 1) + ((digit >> 0) & 1);
        }

        public static string BitsToInput(string bits)
        {
            bool holding = false;
            string output = "";
            int stacking = 0; //positive = i stack, negative = p stack; 

            foreach (char c in bits)
            {
                if (stacking == 3)
                {
                    output += '.';
                    stacking = -1;
                    continue;
                }

                if (stacking == (holding ? -2 : -1))
                {
                    output += holding ? ']' : '[';
                    holding = !holding;
                    stacking = 1;
                    continue;
                }

                if (c == '0')
                {
                    output += '.';
                    stacking = Math.Min(0, stacking) - 1;
                    continue;
                }

                if (c == '1')
                {
                    output += holding ? ']' : '[';
                    holding = !holding;
                    stacking = Math.Max(0, stacking) + 1;
                    continue;
                }

                Debug.Log("Something weird happened. Please report to Obvious#5283");
            }

            if (holding)
                output += ']';

            while (output.Last() == '.')
                output = output.Take(output.Length - 1).Join("");

            return output;
        }

        public static string InvertBits(string bits)
        {
            string output = "";
            foreach (char bit in bits)
                output += ('1' - bit);
            return output;
        }
    }
}
