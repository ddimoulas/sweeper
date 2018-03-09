using System.Windows.Forms;
using System.Drawing;
using System;
namespace Sweeper
{
    class Sweeper : Form
    {
        const int WIDTH = 640;
        const int HEIGHT = 640;

        GameBoard board;

        public Sweeper()
        {
            Text = "Sweeper";
            Size = new Size(WIDTH, HEIGHT);
            DoubleBuffered = true;
            ResizeRedraw = true;

            TableLayoutPanel gameMenu = new TableLayoutPanel();
            gameMenu.Dock = DockStyle.Fill;
            gameMenu.BackColor = Color.Blue;

            /* game menu */
            GameClock clock = new GameClock();
            gameMenu.Controls.Add(clock);

            /* game board */
            board = new GameBoard();
            gameMenu.Controls.Add(board);

            /* menu bar */
            MainMenu mainMenu = new MainMenu();
            MenuItem file = mainMenu.MenuItems.Add("&File");
            MenuItem edit = mainMenu.MenuItems.Add("&Edit");
            file.MenuItems.Add(new MenuItem("&Reset", new EventHandler(board.Restart), Shortcut.CtrlR));
            file.MenuItems.Add(new MenuItem("E&xit", new EventHandler(this.onExit), Shortcut.CtrlX));



            /* mix it all together */
            Menu = mainMenu;
            Controls.Add(gameMenu);
            //Controls.Add(boar
            CenterToScreen();
        }

        void onExit(object sender, EventArgs e)
        {
            Close();
        }

        public static void Main()
        {
            Application.Run(new Sweeper());
        }
    }
}