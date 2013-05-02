﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using OsmSharp.Osm.Data.Core.Processor.List;
using OsmSharp.Osm.Data.XML.Processor;
using OsmSharp.Osm.Simple;
using OsmSharp.Tools;
using OsmSharp.Tools.Math.Geo;
using OsmSharp.Tools.Math.Geo.Projections;
using OsmSharp.UI;
using OsmSharp.UI.Renderer;
using KnownColor = System.Drawing.KnownColor;

namespace OsmSharp.WinForms.UI.Sample
{
    public partial class SampleControlForm : Form
    {
        public SampleControlForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //// initialize a view.
            //var view = View2D.CreateFromCenterAndSize(9000, 9000,
            //                                            534463.21f, 6633094.69f);

            this.sampleControl1.Center = new float[] { 534463.21f, 6633094.69f };
            //this.sampleControl1.Center = new float[] { 0f, 0f };
            this.sampleControl1.ZoomFactor = 1;

            // initialize a test-scene.
            var scene2D = new Scene2D();
            scene2D.BackColor = Color.White.ToArgb();
            scene2D.AddPoint(0, 0, Color.Blue.ToArgb(), 1);

            bool fill = false;
            int color = Color.White.ToArgb();
            int width = 1;

            scene2D.AddPolygon(new float[] { 50, -80, 70 }, new float[] { 20, -10, -70 }, color, width, fill);

            // load test osm file.
            List<SimpleOsmGeo> osmList = new List<SimpleOsmGeo>();
            Stream stream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.WinForms.UI.Sample.test.osm");
            XmlDataProcessorSource xmlDataProcessorSource = new XmlDataProcessorSource(
                stream);
            CollectionDataProcessorTarget collectionDataProcessorTarget = new CollectionDataProcessorTarget(
                osmList);
            collectionDataProcessorTarget.RegisterSource(xmlDataProcessorSource);
            collectionDataProcessorTarget.Pull();

            // build a scene using spherical mercator.
            EllipticalMercator sphericalMercator = new EllipticalMercator();
            Dictionary<long, GeoCoordinate> nodes = new Dictionary<long, GeoCoordinate>();
            foreach (SimpleOsmGeo simpleOsmGeo in osmList)
            {
                if (simpleOsmGeo is SimpleNode)
                {
                    SimpleNode simplenode = (simpleOsmGeo as SimpleNode);
                    double[] point = sphericalMercator.ToPixel(
                        simplenode.Latitude.Value, simplenode.Longitude.Value);
                    nodes.Add(simplenode.Id.Value, new GeoCoordinate(simplenode.Latitude.Value, simplenode.Longitude.Value));
                    scene2D.AddPoint((float)point[0], (float)point[1],
                                     Color.Yellow.ToArgb(),
                                     2);
                }
                else if (simpleOsmGeo is SimpleWay)
                {
                    SimpleWay way = (simpleOsmGeo as SimpleWay);
                    List<float> x = new List<float>();
                    List<float> y = new List<float>();
                    if (way.Nodes != null)
                    {
                        for (int idx = 0; idx < way.Nodes.Count; idx++)
                        {
                            GeoCoordinate nodeCoords;
                            if (nodes.TryGetValue(way.Nodes[idx], out nodeCoords))
                            {
                                x.Add((float) sphericalMercator.LongitudeToX(nodeCoords.Longitude));
                                y.Add((float) sphericalMercator.LatitudeToY(nodeCoords.Latitude));
                            }
                        }
                    }

                    if (x.Count > 0)
                    {
                        scene2D.AddLine(x.ToArray(), y.ToArray(), Color.Blue.ToArgb(), 2);
                    }
                }
            }

            this.sampleControl1.Scene = scene2D;

            this.sampleControl1.Invalidate();
        }
    }
}