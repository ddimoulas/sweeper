using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
namespace Sweeper
{
    enum State
    {
        INGAME,
        START
    }

    class GameBoard : UserControl
    {
        private IContainer components;
        private Button startBtn;
        private Timer timer;
        private MineField field;
        private State state = State.START;
        private int fieldWidth = 10;
        private int fieldHeight = 10;
        private int bombCount = 25;
        private int adjustedOriginX;
        private int adjustedOriginY;

        public GameBoard()
        {
            components = new Container();
            this.Dock = DockStyle.Fill;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.BackColor = Color.Red;

            startBtn = new Button();
            startBtn.Text = "start";
            startBtn.Size = new Size(64, 32);
            startBtn.Anchor = AnchorStyles.Top;
            startBtn.Click += Start;
            startBtn.Parent = this;
            timer = new Timer(this.components);
            timer.Interval = 100;
            timer.Enabled = false;
            timer.Tick += OnTick;

            this.Paint += OnPaint;
            this.MouseDown += OnMouseDown;
            this.MouseUp += OnMouseUp;
        }


        /*
         * callbacks
         */

        public void Start(object sender, EventArgs e)
        {
            timer.Enabled = true;
            startBtn.Enabled = false;
            startBtn.Visible = false;
            field = new MineField(fieldWidth, fieldHeight, bombCount);
            state = State.INGAME;
        }

        public void Restart(object sender, EventArgs e)
        {
            field = new MineField(fieldWidth, fieldHeight, bombCount);
        }

        private void OnTick(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            switch (state)
            {
            case State.INGAME:
                adjustedOriginX = (this.Bounds.Width / 2) - field.Width() / 2;
                adjustedOriginY = (this.Bounds.Height / 2) - field.Height() / 2;

                field.Paint(e.Graphics, adjustedOriginX, adjustedOriginY);
                break;
            }
            e.Graphics.Dispose();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            field.ClickDown(e.X - adjustedOriginX, e.Y - adjustedOriginY, e.Button);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            field.ClickUp(e.Button);
        }
    }
}