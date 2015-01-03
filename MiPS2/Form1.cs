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

        int queue, chs, working;
        List<double> chfree, PointX, PointYq, PointYc;
        double next, now;
        double avgavgqueue;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int kkk = (int)Mod1.Value;

            double[] avgqueue = new double[kkk];
            for (int i = 0; i < kkk; i++)
            {
                avgqueue[i] = 0;
            }
            avgavgqueue = 0;

            WD = new WeibullDistribution(1 / (double)Wa1.Value, (double)Wb1.Value);
            ED = new ExponentialDistribution(1 / (double)El1.Value);

            for (int n = 0; n < kkk; n++)
            {
                queue = 0; chs = (int)C1.Value; working = 0;
                chfree = new List<double>(chs);
                next = 0; now = 0;

                PointX = new List<double>();
                PointYq = new List<double>();
                PointYc = new List<double>();

                next = ED.GetRandomValue(RNG);
                now = next;

                for (int i = 0; i < chs; i++)
                {
                    chfree.Add(99999999999999);
                }

                PointX.Add(0);
                if (n == kkk - 1)
                    PointYc.Add(0);
                PointYq.Add(0);

                while ((now = Math.Min(next, chfree[0])) < (double)T1.Value)
                {
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

                    PointX.Add(now);
                    if (n == kkk - 1)
                        PointYc.Add(working);
                    PointYq.Add(queue);
                }

                PointX.Add((double)T1.Value);
                if (n == kkk - 1)
                    PointYc.Add(working);
                PointYq.Add(queue);

                int tms = PointX.Count;

                for (int i = 0; i < tms - 1; i++)
                {
                    avgqueue[n] += PointYq[i] * (PointX[i + 1] - PointX[i]);
                }
                avgqueue[n] /= (double)T1.Value;
                avgavgqueue += avgqueue[n] / kkk;
            }

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

            for (int i = 0; i < PointX.Count; i++)
            {
                chart1.Series[0].Points.AddXY(PointX[i], PointYc[i]);
                chart1.Series[1].Points.AddXY(PointX[i], PointYq[i]);
            }

            double sum = 0;
            for (int i = 0; i < kkk; i++)
            {
                sum += Math.Pow((avgavgqueue - avgqueue[i]), 2);
            }

            StudentDistribution st = new StudentDistribution(kkk - 1);

            double pogr = st.InverseLeftProbability(1 - (double)Stud1.Value / 200.0) * Math.Sqrt(sum / (kkk - 1)) / Math.Sqrt(kkk);

            Avgqueue1.Text = "Средняя длина очереди:\n" + avgavgqueue.ToString() + "±" + pogr.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chart2.Series[0].Points.Clear();
            chart2.Series[1].Points.Clear();

            for (int k = 0; k < 21; k++)
            {
                int Models = (int)Mod2.Value;
                double Wscale = 1 / (double)Wa2.Value;
                double Wshape = (double)Wb2.Value;
                double Emu = 1 / ((double)El2.Value + (double)El2.Value * ((double)Edl2.Value / 100) * (10 - k) / 10);

                double[] avgqueue = new double[Models];
                for (int i = 0; i < Models; i++)
                {
                    avgqueue[i] = 0;
                }
                avgavgqueue = 0;

                WD = new WeibullDistribution(Wscale, Wshape);
                ED = new ExponentialDistribution(Emu);

                for (int n = 0; n < Models; n++)
                {
                    queue = 0; chs = (int)C2.Value; working = 0;
                    chfree = new List<double>(chs);
                    next = 0; now = 0;

                    chart1.Series[1].Points.Clear();

                    next = ED.GetRandomValue(RNG);
                    now = next;

                    for (int i = 0; i < chs; i++)
                    {
                        chfree.Add(99999999999999);
                    }

                    while ((now = Math.Min(next, chfree[0])) < (double)T1.Value)
                    {
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

                        chart1.Series[1].Points.AddXY(now, queue);
                    }

                    int tms = chart1.Series[1].Points.Count;

                    for (int i = 0; i < tms - 1; i++)
                    {
                        avgqueue[n] += chart1.Series[1].Points[i].YValues[0] * (chart1.Series[1].Points[i + 1].XValue - chart1.Series[1].Points[i].XValue);
                    }
                    avgqueue[n] /= (double)T1.Value;
                    avgavgqueue += avgqueue[n];
                }

                double sum = 0;
                for (int i = 0; i < Models; i++)
                {
                    sum += Math.Pow((avgavgqueue - avgqueue[i]), 2);
                }

                StudentDistribution st = new StudentDistribution(Models - 1);

                double pogr = st.InverseLeftProbability(1 - (double)Stud2.Value / 200.0) * Math.Sqrt(sum / (Models - 1)) / Math.Sqrt(Models);

                chart2.Series[0].Points.AddXY(1 / Emu, avgavgqueue + pogr, avgavgqueue - pogr);
                chart2.Series[1].Points.AddXY(1 / Emu, avgavgqueue);

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            chart3.Series[0].Points.Clear();
            chart3.Series[1].Points.Clear();

            for (int k = 0; k < 21; k++)
            {
                int Models = (int)Mod3.Value;
                double Wscale = 1 / ((double)Wa3.Value + (double)Wa3.Value * ((double)Wda3.Value / 100) * (10 - k) / 10);
                double Wshape = (double)Wb3.Value;
                double Emu = 1 / (double)El3.Value;
                chs = (int)C3.Value;

                double[] avgqueue = new double[Models];
                for (int i = 0; i < Models; i++)
                {
                    avgqueue[i] = 0;
                }
                avgavgqueue = 0;

                WD = new WeibullDistribution(Wscale, Wshape);
                ED = new ExponentialDistribution(Emu);

                for (int n = 0; n < Models; n++)
                {
                    queue = 0; working = 0;
                    chfree = new List<double>(chs);
                    next = 0; now = 0;

                    chart1.Series[1].Points.Clear();

                    next = ED.GetRandomValue(RNG);
                    now = next;

                    for (int i = 0; i < chs; i++)
                    {
                        chfree.Add(99999999999999);
                    }

                    while ((now = Math.Min(next, chfree[0])) < (double)T1.Value)
                    {
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

                        chart1.Series[1].Points.AddXY(now, queue);
                    }

                    int tms = chart1.Series[1].Points.Count;

                    for (int i = 0; i < tms - 1; i++)
                    {
                        avgqueue[n] += chart1.Series[1].Points[i].YValues[0] * (chart1.Series[1].Points[i + 1].XValue - chart1.Series[1].Points[i].XValue);
                    }
                    avgqueue[n] /= (double)T1.Value;
                    avgavgqueue += avgqueue[n];
                }

                double sum = 0;
                for (int i = 0; i < Models; i++)
                {
                    sum += Math.Pow((avgavgqueue - avgqueue[i]), 2);
                }

                StudentDistribution st = new StudentDistribution(Models - 1);

                double pogr = st.InverseLeftProbability(1 - (double)Stud3.Value / 200.0) * Math.Sqrt(sum / (Models - 1)) / Math.Sqrt(Models);

                chart3.Series[0].Points.AddXY(1 / Wscale, avgavgqueue + pogr, avgavgqueue - pogr);
                chart3.Series[1].Points.AddXY(1 / Wscale, avgavgqueue);

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            chart4.Series[0].Points.Clear();
            chart4.Series[1].Points.Clear();

            for (int k = 0; k < 5; k++)
            {
                int Models = (int)Mod4.Value;
                double Wscale = 1 / (double)Wa4.Value;
                double Wshape = (double)Wb4.Value;
                double Emu = 1 / (double)El4.Value;
                chs = (int)C4.Value + 2 - k;

                double[] avgqueue = new double[Models];
                for (int i = 0; i < Models; i++)
                {
                    avgqueue[i] = 0;
                }
                avgavgqueue = 0;

                WD = new WeibullDistribution(Wscale, Wshape);
                ED = new ExponentialDistribution(Emu);

                for (int n = 0; n < Models; n++)
                {
                    queue = 0; working = 0;
                    chfree = new List<double>(chs);
                    next = 0; now = 0;

                    chart1.Series[1].Points.Clear();

                    next = ED.GetRandomValue(RNG);
                    now = next;

                    for (int i = 0; i < chs; i++)
                    {
                        chfree.Add(99999999999999);
                    }

                    while ((now = Math.Min(next, chfree[0])) < (double)T1.Value)
                    {
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

                        chart1.Series[1].Points.AddXY(now, queue);
                    }

                    int tms = chart1.Series[1].Points.Count;

                    for (int i = 0; i < tms - 1; i++)
                    {
                        avgqueue[n] += chart1.Series[1].Points[i].YValues[0] * (chart1.Series[1].Points[i + 1].XValue - chart1.Series[1].Points[i].XValue);
                    }
                    avgqueue[n] /= (double)T1.Value;
                    avgavgqueue += avgqueue[n];
                }

                double sum = 0;
                for (int i = 0; i < Models; i++)
                {
                    sum += Math.Pow((avgavgqueue - avgqueue[i]), 2);
                }

                StudentDistribution st = new StudentDistribution(Models - 1);

                double pogr = st.InverseLeftProbability(1 - (double)Stud4.Value / 200.0) * Math.Sqrt(sum / (Models - 1)) / Math.Sqrt(Models);

                chart4.Series[0].Points.AddXY(chs, avgavgqueue + pogr, avgavgqueue - pogr);
                chart4.Series[1].Points.AddXY(chs, avgavgqueue);

            }
        }
        
    }
}
