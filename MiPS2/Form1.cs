using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Meta.Numerics.Statistics.Distributions;

namespace MiPS2
{
    public partial class Form1 : Form
    {
        private WeibullDistribution WD;
        private ExponentialDistribution ED;
        private Random RNG = new Random();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int queue = 0, chs = (int)C1.Value, working = 0;
            List<double> chfree = new List<double>(chs);
            double next = 0, now = 0;

            double avgqueue = 0;

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

            WD = new WeibullDistribution(1 / (double)Wa1.Value, (double)Wb1.Value);
            ED = new ExponentialDistribution(1 / (double)El1.Value);

            next = ED.GetRandomValue(RNG);
            now = next;

            for (int i = 0; i < chs; i++)
            {
                chfree.Add(99999999999999);
            }

            chart1.Series[0].Points.AddXY(0, 0);
            chart1.Series[1].Points.AddXY(0, 0);


            while ((now=Math.Min(next, chfree[0])) < (double)T1.Value)
            {
               // now = Math.Min(next, chfree[0]);

                if (now == next) // New person
                {
                    queue++;
                    next += ED.GetRandomValue(RNG);
                }
                else // Free channel
                {
                    working--;
                    chfree[0] = 99999999999999;
                }

                chfree.Sort();

                if (working < chs && queue > 0)
                {
                    queue--;
                    chfree[working] = now + WD.GetRandomValue(RNG);
                    working++;
                    chfree.Sort();
                }

                chart1.Series[0].Points.AddXY(now, working);
                chart1.Series[1].Points.AddXY(now, queue);
            }

            chart1.Series[0].Points.AddXY((double)T1.Value, working);
            chart1.Series[1].Points.AddXY((double)T1.Value, queue);

            int tms=chart1.Series[1].Points.Count;

            for (int i = 0; i < tms-1; i++)
            {
                avgqueue += chart1.Series[1].Points[i].YValues[0] * (chart1.Series[1].Points[i + 1].XValue - chart1.Series[1].Points[i].XValue);
            }
            avgqueue /= (double)T1.Value;
            Avgqueue1.Text = "Средняя длина очереди: " + avgqueue.ToString();
        }

    }
}
