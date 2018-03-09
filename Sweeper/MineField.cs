using System;
using System.Drawing;
using System.Windows.Forms;
namespace Sweeper
{
    public class MineField
    {
        private const int tileSpace = 2;
        private const int tileSize = 24;

        /* total bomb count */
        private int bombs;
        /* cells across and down */
        private int width, height;
        /* use this to keep track of cells the player clicks but moves their mouse off of */
        private int pressed = -1;

        /* only run initialization once */
        private static bool imagesLoaded;
        /* bitmaps */
        private static Bitmap tileUp;
        private static Bitmap[] tiles = new Bitmap[9];
        private static Bitmap bombTile;
        private static Bitmap flagTile;
        private static Font font = new Font(FontFamily.GenericMonospace, 1);

        /* use this for bounds checking / recursive stuffs */
        private static Transformation[] transformations = new Transformation[8];

        /* actual cell representation */
        private Cell[] field;


        public int Width() => ((tileSpace + tileSize) * width) - tileSpace;
        public int Height() => ((tileSpace + tileSize) * height) - tileSpace;

        public MineField(int width, int height, int bombCount)
        {
            this.width = width;
            this.height = height;
            this.bombs = bombCount;

            if (!imagesLoaded) loadImages();
            initializeMines();
        }

        private static void loadImages()
        {
            try
            {
                tileUp = new Bitmap("../../Assets/tileUp.png");

                tiles[0] = new Bitmap("../../Assets/tileDown.png");
                tiles[1] = new Bitmap("../../Assets/tile1.png");
                tiles[2] = new Bitmap("../../Assets/tile2.png");
                tiles[3] = new Bitmap("../../Assets/tile3.png");
                tiles[4] = new Bitmap("../../Assets/tile4.png");
                tiles[5] = new Bitmap("../../Assets/tile5.png");
                tiles[6] = new Bitmap("../../Assets/tile6.png");
                tiles[7] = new Bitmap("../../Assets/tile7.png");
                tiles[8] = new Bitmap("../../Assets/tile8.png");

                bombTile = new Bitmap("../../Assets/mine.png");
                flagTile = new Bitmap("../../Assets/flag.png");
            } catch (Exception e)
            {
                Console.WriteLine("Failed to load resources:\n" + e.Message);
                Environment.Exit(-1);
            }
            imagesLoaded = true;
        }

        private void initializeMines()
        {
            /* initialize field */
            field = new Cell[width * height];
            for (int i = 0; i < width * height; i++) field[i] = new Cell();

            /* randomly fill with bombs */
            Random prng = new Random();
            while (bombs-- > 0)
            {
                int x = prng.Next(width);
                int y = prng.Next(height);
                if (!field[y * width + x].IsBomb())
                    field[y * width + x].SetBomb();
                else bombs++;
            }

            /* set up transformations */
            transformations[0] = new Transformation(1, Direction.East);
            transformations[1] = new Transformation(-1, Direction.West);
            transformations[2] = new Transformation(width, Direction.South);
            transformations[3] = new Transformation(-width, Direction.North);

            transformations[4] = new Transformation(width + 1, Direction.East | Direction.South);
            transformations[5] = new Transformation(width - 1, Direction.West | Direction.South);
            transformations[6] = new Transformation((-width) + 1, Direction.North | Direction.East);
            transformations[7] = new Transformation((-width) - 1, Direction.North | Direction.West);

            /* now calculate neighbors 
             * use transformations to protect
             * against checking out of bounds */
            int totalSize = width * height;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int neighbors = 0;
                    int index = i * width + j;
                    foreach (Transformation transformation in transformations)
                    {
                        int newIndex = transformation.ApplyTo(index);
                        /* continue here if transformation is out of bounds */
                        if (newIndex >= totalSize || newIndex < 0) continue;
                        /* continue here if we're on the left edge of the board
                         * and the direction of the transformation is left */
                        if (j == 0 && (transformation.Dir & Direction.West) == Direction.West) continue;
                        /* continue here if we're on the right edge of the board
                         * and the direction of the transformation is right */
                        if (j == width - 1 && (transformation.Dir & Direction.East) == Direction.East) continue;

                        /* do the actual check */
                        if (field[newIndex].IsBomb()) neighbors++;
                    }
                    field[index].SetNeighbors(neighbors);
                }
            }

        }

        public void ClickDown(int x, int y, MouseButtons b)
        {
            bool clicked = false;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = i * width + j;
                    if (field[index].Underneath(j, i, x, y))
                    {
                        pressed = index;
                        clicked = true;
                        field[index].ClickDown();
                        break;
                    }
                }
                if (clicked) break;
            }
        }

        public void ClickUp(MouseButtons b)
        {
            if (pressed < 0) return;
            if (b == MouseButtons.Left) RecursiveClick(pressed);
            else if (b == MouseButtons.Right)
            {
                field[pressed].Flag();
                UpdateFlagCounts();
            }
            pressed = -1;
        }

        void RecursiveClick(int index)
        {
            if (field[index].flag) return;
            field[index].ClickUp();

            int totalSize = width * height;
            if (field[index].adjFlags == field[index].adjBombs && !field[index].hidden)
            { /* if flags == bombs, click all surrounding tiles */
                foreach (Transformation t in transformations)
                {
                    int newIndex = t.ApplyTo(index);
                    if (newIndex >= totalSize || newIndex < 0) continue;
                    if (index % width == 0 && (t.Dir & Direction.West) == Direction.West) continue;
                    if (index % width == width - 1 && (t.Dir & Direction.East) == Direction.East) continue;
                    if (field[newIndex].hidden) RecursiveClick(newIndex);
                }
                return;
            }
            if (field[index].adjBombs != 0) return;
            foreach (Transformation t in transformations)
            {
                int newIndex = t.ApplyTo(index);
                if (newIndex >= totalSize || newIndex < 0) continue;
                if (index % width == 0 && (t.Dir & Direction.West) == Direction.West) continue;
                if (index % width == width - 1 && (t.Dir & Direction.East) == Direction.East) continue;
                if (field[newIndex].hidden) RecursiveClick(newIndex);
            }
        }

        void UpdateFlagCounts()
        {
            int totalSize = width * height;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int neighbors = 0;
                    int index = i * width + j;
                    foreach (Transformation transformation in transformations)
                    {
                        int newIndex = transformation.ApplyTo(index);
                        /* continue here if transformation is out of bounds */
                        if (newIndex >= totalSize || newIndex < 0) continue;
                        /* continue here if we're on the left edge of the board
                         * and the direction of the transformation is left */
                        if (j == 0 && (transformation.Dir & Direction.West) == Direction.West) continue;
                        /* continue here if we're on the right edge of the board
                         * and the direction of the transformation is right */
                        if (j == width - 1 && (transformation.Dir & Direction.East) == Direction.East) continue;

                        /* do the actual check */
                        if (field[newIndex].flag) neighbors++;
                    }
                    field[index].setFlaggedNeighbors(neighbors);
                }
            }
        }

        public void Paint(Graphics g, int originX, int originY)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = i * width + j;
                    field[index].Paint(g, j, i, originX, originY);
                }
            }
        }

        /*
         * Cell Declaration
         */

        class Cell
        {
            private bool isBomb;
            private bool pressed;

            public int adjBombs;
            public int adjFlags;
            public bool flag;
            public bool hidden = true;

            public bool IsBomb() => isBomb;
            public void SetBomb() => isBomb = true;
            public void SetNeighbors(int x) => adjBombs = x;
            public void setFlaggedNeighbors(int x) => adjFlags = x;

            public void Flag()
            {
                if (!hidden) return;
                flag = !flag;
                pressed = false;
            }

            public bool Underneath(int tileX, int tileY, int mouseX, int mouseY)
            {
                /* do some math save some time */
                int leftSide = tileX * (tileSize + tileSpace);
                int topSide = tileY * (tileSize + tileSpace);

                /* check if the x co ord is between the left and ride sides */
                /* check if the y co ord is between the top and bottom sides */
                if (mouseX < leftSide) return false;
                if (mouseX > leftSide + tileSize) return false;
                if (mouseY < topSide) return false;
                if (mouseY > topSide + tileSize) return false;

                /* if it is, click it */
                return true;
            }

            public void ClickUp()
            {
                pressed = false;
                hidden = false;
            }

            public void ClickDown()
            {
                pressed = true;
            }

            public void Paint(Graphics g, int x, int y, int originX, int originY)
            {
                Bitmap target;
                int drawX = x * (tileSize + tileSpace);
                int drawY = y * (tileSize + tileSpace);

                /* default to up or down
                 * if not hidden, and bomb draw bomb
                 * if not bomb, draw neighbors */
                target = pressed ? tiles[0] : tileUp;
                target = flag ? flagTile : target;
                target = hidden ? target : isBomb ? bombTile : tiles[adjBombs];

                g.DrawImage(target, originX + drawX, originY + drawY);
            }
        }

        [Flags]
        enum Direction
        {
            East = 0x01,
            West = 0x02,
            South = 0x04,
            North = 0x08
        }

        class Transformation
        {
            int value;
            public Direction Dir;

            public Transformation(int v, Direction d)
            {
                value = v;
                Dir = d;
            }

            public int ApplyTo(int x) => x + value;
        }
    }
}