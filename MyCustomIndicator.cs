

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Security.Cryptography.X509Certificates;
using System.Security.RightsManagement;
using SwissEphNet;

#endregion

/*
    YiQuan 1/5/2023-3/5/2023
    Skype: live:.cid.79e2e1b1d6fcdcb2
    Outlook: light7fi@outlook.com
*/

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class MyCustomIndicator : Indicator
    {

        // Define the data series for the indicator

        #region Properties  

        [Display(Name = "TimeZone")]
        public TIMEZONE myTimeZone
        {
            get; set;
        }

        [Display(Name = "MSD")]
        [Range(1, 9)]
        public int msd
        {
            get;set;
        }
        [Display(Name = "MSD Sub")]
        [Range(1, 9)]
        public int msdsub
        {
            get; set;
        }
        [Range(1, 4)]
        [Display(Name = "Significant price level")]
        public int Level
        { get; set; }

        [Display(GroupName = "My Strings")]
        public string MyCustomString
        { get; set; }

        #endregion
        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Indicator here.";
                Name = "MyCustomIndicator";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                msd = 1;
                msdsub= 1;
                Level = 1;
                AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "Line");

            }
            else if (State == State.Configure)
            {
              
            }

        }


        public enum TIMEZONE
        {
            Chicago,
            Frankfurt,
            Hong_Kong,
            Kiev,
            London,
            Moscow,
            Mumbai,
            New_York,
            Paris,
            Sao_Paolo,
            Seoul,
            Shanghai,
            Sydney,
            Tokyo
        }

        public struct POSITION
        {
            public int cx;
            public int cy;


        }

        public int getValue(POSITION pos)
        {
            int layer = Math.Max(Math.Abs(pos.cx), Math.Abs(pos.cy));
            int minoflayer = (2 * layer + 1) * (2 * layer + 1) - (2 * layer) * 4;
            int m = 0;
            int q = 0;

            if (pos.cx == layer) { m = 2; q = -pos.cy + layer; }
            if (pos.cx == -layer) { m = 0; q = pos.cy + layer; }
            if (pos.cy == layer) { m = 1; q = pos.cx + layer; }
            if (pos.cy == -layer) { m = 3; q = -pos.cx + layer; }

            int v = 0;
            v = (2 * layer) * m + q + minoflayer;
            return v;
        }

        public POSITION[] getCrossPosByDirection(int angle, int value, int direction)
        {
            POSITION[] crossPos = new POSITION[2];

            int curLayer = getLayer(value);
            POSITION curPos = getPos(value);
            int sunLayer = getLayer(angle);
            POSITION sunPos = getPos(angle);

            switch (direction)
            {
                case 0:
                    {
                        int dl = Math.Abs(curLayer - sunLayer);
                        if (dl != 0)
                        {

                            crossPos[0].cx = -curLayer;
                            crossPos[0].cy = sunPos.cy;
                            crossPos[1].cx = curLayer;
                            crossPos[1].cy = sunPos.cy;
                            /***/
                            int d1 = 0, d2 = 0;
                            d1 = getValue(crossPos[0]) - value;
                            d2 = getValue(crossPos[1]) - value;
                            if (d1 * d2 > 0)
                            {
                                int d3 = Math.Max(Math.Abs(d1), Math.Abs(d2));
                                if (Math.Abs(d1) == d3)
                                {
                                    int lx = Math.Abs(crossPos[0].cx);
                                    crossPos[0].cx -= crossPos[0].cx / lx;
                                }
                                else
                                {
                                    int lx = Math.Abs(crossPos[1].cx);
                                    crossPos[1].cx -= crossPos[1].cx / lx;
                                }
                            }
                        }
                        else
                        {
                            crossPos[0].cx = sunPos.cx;
                            crossPos[0].cy = sunPos.cy;
                            crossPos[1].cx = sunPos.cx;
                            crossPos[1].cy = sunPos.cy;
                        }
                    }
                    break;
                case 1:
                    {
                        int dl = Math.Abs(curLayer - sunLayer);
                        if (dl != 0)
                        {
                            if (sunPos.cy == sunLayer)
                            {
                                crossPos[0].cx = sunPos.cx - dl;
                                crossPos[0].cy = curLayer;
                                crossPos[1].cx = curLayer;
                                crossPos[1].cy = sunPos.cy + sunPos.cx - curLayer;
                            }
                            if (sunPos.cx == sunLayer)
                            {
                                crossPos[0].cx = sunPos.cx + sunPos.cy - curLayer;
                                crossPos[0].cy = curLayer;
                                crossPos[1].cx = curLayer;
                                crossPos[1].cy = sunPos.cy - dl;
                            }
                            if (sunPos.cy == -sunLayer)
                            {
                                crossPos[0].cx = -curLayer;
                                crossPos[0].cy = sunPos.cy + sunPos.cx + curLayer;
                                crossPos[1].cx = sunPos.cx + dl;
                                crossPos[1].cy = -curLayer;
                            }
                            if (sunPos.cx == -sunLayer)
                            {
                                crossPos[0].cx = -curLayer;
                                crossPos[0].cy = sunPos.cy + dl;
                                crossPos[1].cx = sunPos.cx + sunPos.cy + curLayer;
                                crossPos[1].cy = -curLayer;
                            }

                            /***/
                            int d1 = 0, d2 = 0;
                            d1 = (getValue(crossPos[0]) - value);
                            d2 = (getValue(crossPos[1]) - value);
                            if (d1 * d2 > 0)
                            {
                                int d3 = Math.Max(Math.Abs(d1), Math.Abs(d2));
                                if (Math.Abs(d1) == d3)
                                {
                                    int lx = Math.Abs(crossPos[0].cx);
                                    if (lx == curLayer)
                                    {
                                        crossPos[0].cx += crossPos[0].cx / lx;
                                    }
                                }
                                else
                                {
                                    int ly = Math.Abs(crossPos[1].cy);
                                    if (ly == curLayer)
                                    {
                                        crossPos[1].cy -= crossPos[1].cy / ly;
                                    }
                                }
                            }
                        }
                        else
                        {
                            crossPos[0].cx = sunPos.cx;
                            crossPos[0].cy = sunPos.cy;
                            crossPos[1].cx = sunPos.cx;
                            crossPos[1].cy = sunPos.cy;
                        }
                    }
                    break;
                case 2:
                    {
                        int dl = Math.Abs(curLayer - sunLayer);
                        if (dl != 0)
                        {
                            crossPos[0].cx = sunPos.cx;
                            crossPos[0].cy = curLayer;
                            crossPos[1].cx = sunPos.cx;
                            crossPos[1].cy = -curLayer;
                            /***/
                            int d1 = 0, d2 = 0;
                            d1 = getValue(crossPos[0]) - value;
                            d2 = getValue(crossPos[1]) - value;
                            if (d1 * d2 > 0)
                            {
                                int d3 = Math.Max(Math.Abs(d1), Math.Abs(d2));
                                if (Math.Abs(d1) == d3)
                                {
                                    int ly = Math.Abs(crossPos[0].cy);
                                    crossPos[0].cy -= crossPos[0].cy / ly;
                                }
                                else
                                {
                                    int ly = Math.Abs(crossPos[1].cy);
                                    crossPos[1].cy -= crossPos[1].cy / ly;
                                }
                            }
                        }
                        else
                        {
                            crossPos[0].cx = sunPos.cx;
                            crossPos[0].cy = sunPos.cy;
                            crossPos[1].cx = sunPos.cx;
                            crossPos[1].cy = sunPos.cy;
                        }
                    }
                    break;
                case 3:
                    {
                        int dl = Math.Abs(curLayer - sunLayer);
                        if (dl != 0)
                        {
                            if (sunPos.cy == -sunLayer)
                            {
                                crossPos[0].cx = sunPos.cx - dl;
                                crossPos[0].cy = -curLayer;
                                crossPos[1].cx = curLayer;
                                crossPos[1].cy = sunPos.cy - sunPos.cx + curLayer;
                            }
                            if (sunPos.cx == -sunLayer)
                            {
                                crossPos[0].cx = -curLayer;
                                crossPos[0].cy = sunPos.cy - dl;
                                crossPos[1].cx = sunPos.cx + curLayer - sunPos.cy;
                                crossPos[1].cy = curLayer;
                            }
                            if (sunPos.cy == sunLayer)
                            {
                                crossPos[0].cx = -curLayer;
                                crossPos[0].cy = sunPos.cy - sunPos.cx - curLayer;
                                crossPos[1].cx = sunPos.cx + dl;
                                crossPos[1].cy = curLayer;
                            }
                            if (sunPos.cx == sunLayer)
                            {
                                crossPos[0].cx = curLayer;
                                crossPos[0].cy = sunPos.cy + dl;
                                crossPos[1].cx = sunPos.cx - sunPos.cy - curLayer;
                                crossPos[1].cy = -curLayer;
                            }

                            /***/
                            int d1 = 0, d2 = 0;
                            d1 = (getValue(crossPos[0]) - value);
                            d2 = (getValue(crossPos[1]) - value);
                            if (d1 * d2 > 0)
                            {
                                int d3 = Math.Max(Math.Abs(d1), Math.Abs(d2));
                                if (Math.Abs(d1) == d3)
                                {
                                    int ly = Math.Abs(crossPos[0].cy);
                                    if (ly == curLayer)
                                    {
                                        crossPos[0].cy -= crossPos[0].cy / ly;
                                    }
                                }
                                else
                                {
                                    int lx = Math.Abs(crossPos[1].cx);
                                    if (lx == curLayer)
                                    {
                                        crossPos[1].cx -= crossPos[1].cx / lx;
                                    }
                                }
                            }
                        }
                        else
                        {
                            crossPos[0].cx = sunPos.cx;
                            crossPos[0].cy = sunPos.cy;
                            crossPos[1].cx = sunPos.cx;
                            crossPos[1].cy = sunPos.cy;
                        }

                    }
                    break;
                default:
                    break;
            }

            return crossPos;
        }
        public int getLayer(int angle)
        {
            int i = 0;
            for (i = 1; ; i++)
            {
                int c = (2 * i + 1);
                int maxoflayer = c * c;
                int minoflayer = maxoflayer - (c - 1) * 4 - 1;
                if (angle >= minoflayer && angle <= maxoflayer)
                {
                    return i;
                }
            }
            return 0;
        }
        public POSITION getPos(int angle)
        {
            POSITION pos = new POSITION();
            int layer = getLayer(angle);
            int offest = angle - (2 * layer + 1) * (2 * layer + 1) + (2 * layer) * 4;
            int m = offest / (2 * layer);
            int q = (offest) % (2 * layer);
            switch (m)
            {
                case 0:
                    pos.cx = -layer;
                    pos.cy = q - layer;
                    break;
                case 1:
                    pos.cx = -layer + q;
                    pos.cy = layer;
                    break;
                case 2:
                    pos.cx = layer;
                    pos.cy = layer - q;
                    break;
                default:
                    pos.cx = layer - q;
                    pos.cy = -layer;
                    break;
            }
            return pos;
        }
        public double getSunAngle(TIMEZONE location)
        {
            double longitude = 0, latitude = 0;
            int utcOffset = 0;
            switch (location)
            {
                case TIMEZONE.Chicago:
                    longitude = (41 + 51) / 180 * Math.PI;
                    latitude = (87 + 39) / 180 * Math.PI;
                    utcOffset = -6;
                    break;
                case TIMEZONE.Frankfurt:
                    longitude = (50 + 7) / 180 * Math.PI;
                    latitude = (8 + 4) / 180 * Math.PI;
                    utcOffset = +1;
                    break;
                case TIMEZONE.Mumbai:
                    longitude = (18 + 58) / 180 * Math.PI;
                    latitude = (72 + 50) / 180 * Math.PI;
                    utcOffset = 8;
                    break;
                case TIMEZONE.London:
                    longitude = (51 + 30) / 180 * Math.PI;
                    latitude = (0 + 10) / 180 * Math.PI;
                    utcOffset = 1;
                    break;
                case TIMEZONE.Hong_Kong:
                    longitude = (22 + 17) / 180 * Math.PI;
                    latitude = (114 + 9) / 180 * Math.PI;
                    utcOffset = 8;
                    break;
                case TIMEZONE.Sydney:
                    longitude = (33 + 52) / 180 * Math.PI;
                    latitude = (151 + 13) / 180 * Math.PI;
                    utcOffset = 10;
                    break;
                case TIMEZONE.Shanghai:
                    longitude = (31 + 14) / 180 * Math.PI;
                    latitude = (121 + 28) / 180 * Math.PI;
                    utcOffset = 8;
                    break;
                case TIMEZONE.Kiev:
                    longitude = (50 + 26) / 180 * Math.PI;
                    latitude = (30 + 31) / 180 * Math.PI;
                    utcOffset = 2;
                    break;
                case TIMEZONE.Moscow:
                    longitude = (55 + 45) / 180 * Math.PI;
                    latitude = (37 + 35) / 180 * Math.PI;
                    utcOffset = 3;
                    break;
                case TIMEZONE.New_York:
                    longitude = (40 + 43 / 60.0) / 180 * Math.PI;
                    latitude = (71 + 1 / 60.0) / 180 * Math.PI;
                    utcOffset = -4;
                    break;
                case TIMEZONE.Paris:
                    longitude = (50 + 7 / 60.0) / 180 * Math.PI;
                    latitude = (8 + 4 / 60.0) / 180 * Math.PI;
                    utcOffset = 2;
                    break;
                case TIMEZONE.Sao_Paolo:
                    longitude = (23 + 33) / 180 * Math.PI;
                    latitude = (46 + 38) / 180 * Math.PI;
                    utcOffset = -3;
                    break;
                case TIMEZONE.Seoul:
                    longitude = (37 + 33) / 180 * Math.PI;
                    latitude = (126 + 58) / 180 * Math.PI;
                    utcOffset = 9;
                    break;
            }

            DateTime currentTime = DateTime.UtcNow;

            double jut = currentTime.Hour + (currentTime.Minute / 60.0) + (currentTime.Second / 3600.0);
            var swe = new SwissEph();
            double tjd = swe.swe_julday(currentTime.Year, currentTime.Month, currentTime.Day, jut, SwissEph.SE_GREG_CAL);

            double[] xx = new double[6]; String serr = null;
            swe.swe_calc(tjd, SwissEph.SE_SUN, SwissEph.SEFLG_MOSEPH, xx, ref serr);


            // Convert the position to longitude and latitude
            double sunLongitude = xx[0];
            double sunLatitude = xx[1];
            /*
            // Calculate the angle between the sun and the location
            double angle = Math.Atan2(Math.Sin(sunLongitude - longitude),
                                        Math.Cos(latitude) * Math.Tan(sunLatitude) - Math.Sin(latitude) * Math.Cos(sunLongitude - longitude));

            // Convert the angle to degrees
            double degrees = Math.Abs(angle) * 180.0 / Math.PI;
            */
            return Math.Round(sunLongitude);

        }
        protected override void OnBarUpdate()
        {
            //Add your custom indicator logic here.
            int z = 0;
            double v = Close[0];

            while (v < Math.Pow(10, msd + 2))
            {
                v *= 10;
                z--;
            }
            while (v > Math.Pow(10, msd + 3)-1)
            {
                v /= 10;
                z++;
            }
           
            int m = (int)v / 1000;
            int v_ = (int)v % 1000 + msdsub * 1000;
          
            double[] priceupLevels = new double[4];
            double[] pricelowLevels = new double[4];
            for (int i = 0; i < 4; i++)
            {
                POSITION[] levelPos = getCrossPosByDirection(44, v_, i);

                int hv = Math.Max(getValue(levelPos[0]), getValue(levelPos[1]));
                int lv_ = Math.Min(getValue(levelPos[0]), getValue(levelPos[1]));
             
                int lv = lv_, zz=0;
                if (lv < 1000)
                {
                    lv *= 10;
                    zz -= 1;
                }
                priceupLevels[i] = ((m * 1000 + hv % 1000) * Math.Pow(10, z));
                pricelowLevels[i] = ((m * 1000 + lv % 1000 * Math.Pow(10,zz)) * Math.Pow(10, z));
            }
            Array.Sort(priceupLevels);
            Array.Sort(pricelowLevels);
            for (int i = 0; i < Level; i++)
            {
                Print("--------------" + i + "--------------");
               
                Print((int)getSunAngle(myTimeZone));
                Print(priceupLevels[i]);
                Print(pricelowLevels[3-i]);

                Draw.HorizontalLine(this, "Current", Close[0], Brushes.Red, DashStyleHelper.Dot, 2, true);
                
                Draw.HorizontalLine(this, "Up" + i, priceupLevels[i], Plots[0].Brush, Plots[0].DashStyleHelper, (int)Plots[0].Width, true);
                Draw.HorizontalLine(this, "Down" + i, pricelowLevels[3-i], Plots[0].Brush, Plots[0].DashStyleHelper, (int)Plots[0].Width, true);

                Print("-----------------------------");
            }
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MyCustomIndicator[] cacheMyCustomIndicator;
		public MyCustomIndicator MyCustomIndicator()
		{
			return MyCustomIndicator(Input);
		}

		public MyCustomIndicator MyCustomIndicator(ISeries<double> input)
		{
			if (cacheMyCustomIndicator != null)
				for (int idx = 0; idx < cacheMyCustomIndicator.Length; idx++)
					if (cacheMyCustomIndicator[idx] != null &&  cacheMyCustomIndicator[idx].EqualsInput(input))
						return cacheMyCustomIndicator[idx];
			return CacheIndicator<MyCustomIndicator>(new MyCustomIndicator(), input, ref cacheMyCustomIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MyCustomIndicator MyCustomIndicator()
		{
			return indicator.MyCustomIndicator(Input);
		}

		public Indicators.MyCustomIndicator MyCustomIndicator(ISeries<double> input )
		{
			return indicator.MyCustomIndicator(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MyCustomIndicator MyCustomIndicator()
		{
			return indicator.MyCustomIndicator(Input);
		}

		public Indicators.MyCustomIndicator MyCustomIndicator(ISeries<double> input )
		{
			return indicator.MyCustomIndicator(input);
		}
	}
}

#endregion
