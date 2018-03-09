using System;
using System.Windows.Forms;
namespace Sweeper
{
    class GameClock : Label
    {
        Timer timer;
        int minutes, seconds;
        public GameClock()
        {
            Text = "test";
            Anchor = AnchorStyles.Top;
            BorderStyle = BorderStyle.Fixed3D;

            timer = new Timer();
            timer.Interval = 1000;
            timer.Enabled = false;
            timer.Tick += Update;
        }

        void Update(object sender, EventArgs e)
        {
            Text = string.Format("{0:000}:{1:00}", minutes, seconds++);
            if (seconds >= 60)
            {
                minutes++;
                seconds -= 60;
            }
        }

        public void Start()
        {
            timer.Enabled = true;
        }

        public void Stop()
        {
            timer.Enabled = false;
        }

        public void Reset()
        {
            minutes = 0;
            seconds = 0;
        }
    }
}
