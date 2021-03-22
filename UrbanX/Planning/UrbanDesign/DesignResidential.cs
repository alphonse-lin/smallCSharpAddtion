using System;
using System.Collections.Generic;
using System.Linq;

using Rhino.Geometry;
using Rhino.Geometry.Intersect;

using UrbanX.Algorithms.Geometry;
using UrbanX.Algorithms.Mathematics;
using UrbanX.Algorithms.Utility;



namespace UrbanX.Planning.UrbanDesign
{
    public class DesignResidential: IDisposable
    {
        private readonly ResidentialStyles _style;

        private readonly double _tolerance;

        private readonly string[] _inputTypes;

        private readonly int _averageFloor;

        // Representing the raito of sunDistance that the first row should be setback
        private const double _k = 1.0 / 3;

        /// <summary>
        /// siteArea * FAR 
        /// </summary>
        private readonly double _targetTotalArea;

        private readonly int _cityIndex;

        private readonly double _radiant;

        private readonly Curve _site;

        private readonly double _siteArea;

        private string _buildingTypeName;

        /// <summary>
        /// Only used for row radiance and row major styles.
        /// </summary>
        private int _lastRow;

        // Those fields storing the result from solver.

        private int[] _maxCountPerLine;

        private double _depth;

        private double _width;

        private double _areaError;

        public BuildingGeometry[] BuildingGeometries { get; private set; }

        

        public int BuildingCount { get; private set; }

        /// <summary>
        /// Each residential site only has one setback curve.
        /// </summary>
        public Curve SetBack { get; }


        /// <summary>
        /// Storing the building floors: no mixed-use {0,n} ; mixed-use{bottom-n , upper-n} , total_floor = bottom-n + upper-n;
        /// </summary>
        public int[] BuildingFloors { get; set; }

        public Curve[] CentreLines { get; private set; }


        public DesignResidential(int cityIndex, Curve site, double radiant, string[] buildingTypes, int averageFloor, double targetTotalArea, ResidentialStyles style, double tolerance)
        {
            _style = style;

            _tolerance = tolerance;
            _site = site.DuplicateCurve();
            _siteArea = AreaMassProperties.Compute(_site).Area;

            var setbackDistance = BuildingDataset.GetSetbackRtype(averageFloor) * 2;

            DesignToolbox.SafeOffsetCurve(site, setbackDistance, tolerance, out Curve tempSetback);

            var pl = DesignToolbox.ConvertToPolyline(tempSetback, tolerance);
            pl.ReduceSegments(1);
            SetBack = pl.ToPolylineCurve();

            _inputTypes = buildingTypes;
            _averageFloor = averageFloor;
            _targetTotalArea = targetTotalArea;
            _cityIndex = cityIndex;


            ValidatingRadiant(ref radiant);
            _radiant = radiant;


            // Initialize fields and properties.
            _maxCountPerLine = null;
            _depth = 0;
            _width = 0;
            _areaError = 0;
            //_lastRow = 1;
            _lastRow = 0;

            BuildingGeometries = null;
            _buildingTypeName = null;
            BuildingCount = 0;
            BuildingFloors = new int[2];
            CentreLines = null;

            ResidentialSolver();
        }



        #region Public methods
        /// <summary>
        /// Method for fitting buildings in a residential site.
        /// </summary>
        private void ResidentialSolver()
        {
            List<int> floors = new List<int>();
            List<int> counts = new List<int>();
            List<string> allTypes = new List<string>();
            List<double> totalAreaErrors = new List<double>();
            List<double> areaErrorsAbs = new List<double>();

            List<Curve[]> centreLinesList = new List<Curve[]>();
            List<int[]> maxCountList = new List<int[]>();

            List<double> depths = new List<double>();
            List<double> widths = new List<double>();

            foreach (var type in _inputTypes)
            {
                // 0. Get building parameters.
                var bp = BuildingDataset.GetBuildingParameters(type);
   

                // 33 is the upper limit of floors.
                var initialFloor = _averageFloor > 45? 45:_averageFloor ;

                // 1. Calculate the total building area.
                while (initialFloor <= 45)
                {
                    Curve[] centreLines = null;
                    int[] totalCount = null;
                    bool flag = false;
                    double depth = 0;
                    double width = 0;

                    switch (_style)
                    {
                        case ResidentialStyles.RowRadiance:
                            flag = ComputeRowRadiance(bp, initialFloor, out centreLines, out totalCount, out depth, out width);

                            break;

                        case ResidentialStyles.DotRowMajor:
                            flag = ComputeDotRowMajor(bp, initialFloor, out centreLines, out totalCount, out depth, out width);

                            break;

                        case ResidentialStyles.DotColumnMajor:
                            flag = ComputeDotColumnMajor(bp, initialFloor, out centreLines, out totalCount, out depth, out width);

                            break;
                    }

                    if (flag == false)
                        break;                  

                    // 1.3 fitting to target.
                    var targetCount = (int) Math.Round(_targetTotalArea / bp.Area / initialFloor);
                    if (targetCount == 0)
                    {
                        targetCount++;
                        initialFloor = (int) Math.Round(_targetTotalArea / bp.Area /1.0);
                        if (initialFloor == 0)
                            initialFloor++;
                    }

                    var currentTotalCount = totalCount.Sum();

                    if(targetCount< currentTotalCount || targetCount ==1)
                    {
                        centreLinesList.Add(centreLines);
                        maxCountList.Add(totalCount);
                        depths.Add(depth);
                        widths.Add(width);

                        var areaError = _targetTotalArea - bp.Area * initialFloor *targetCount;

                        floors.Add(initialFloor);
                        totalAreaErrors.Add(areaError);
                        areaErrorsAbs.Add(Math.Abs(areaError));
                        counts.Add(targetCount);
                        allTypes.Add(type);
                        break;
                    }
                    else if (initialFloor == 45 && areaErrorsAbs.Count == 0)
                    {
                        centreLinesList.Add(centreLines);
                        maxCountList.Add(totalCount);
                        depths.Add(depth);
                        widths.Add(width);

                        var areaError = _targetTotalArea - bp.Area * initialFloor * currentTotalCount;

                        floors.Add(initialFloor);
                        totalAreaErrors.Add(areaError);
                        areaErrorsAbs.Add(Math.Abs(areaError));
                        counts.Add(currentTotalCount);
                        allTypes.Add(type);
                        break;
                    }
                    else
                    {
                        // targetCount > current count.                 
                        if (totalCount.Length > 1)
                        {
                            var totalAreaError = _targetTotalArea - bp.Area * initialFloor * totalCount.Sum();
                            var lastRowCount = totalCount[1];

                            int lastRowFloor = (int)Math.Round(totalAreaError / bp.Area / lastRowCount) + initialFloor;

                            if(lastRowFloor*1.0/initialFloor < 1.5)
                            {
                                centreLinesList.Add(centreLines);
                                maxCountList.Add(totalCount);
                                depths.Add(depth);
                                widths.Add(width);

                                // Using totalCount as current count.
                                var areaError = _targetTotalArea - bp.Area * (initialFloor * (currentTotalCount - lastRowCount) + lastRowFloor * lastRowCount);

                                // Add targetFloor in list.
                                floors.Add(initialFloor);

                                totalAreaErrors.Add(totalAreaError);
                                areaErrorsAbs.Add(Math.Abs(areaError));
                                counts.Add(currentTotalCount);
                                allTypes.Add(type);

                                break;
                            }
                            else
                            {
                                initialFloor++;
                                continue;
                            }
                        }
                        else
                        {
                            // totalCount.Length = 1
                            int targetFloor = (int)Math.Round(_targetTotalArea / bp.Area / currentTotalCount);

                            centreLinesList.Add(centreLines);
                            maxCountList.Add(totalCount);
                            depths.Add(depth);
                            widths.Add(width);

                            // Using totalCount as current count.
                            var areaError = _targetTotalArea - bp.Area * targetFloor * currentTotalCount;

                            // Add targetFloor in list.
                            floors.Add(targetFloor);
                            totalAreaErrors.Add(areaError);
                            areaErrorsAbs.Add(Math.Abs(areaError));
                            counts.Add(currentTotalCount);
                            allTypes.Add(type);

                            break;
                        }
                    }
                }
            }

            // 2. Finding the smallest area error. Record error for northest row.
            var min = areaErrorsAbs.IndexOf(areaErrorsAbs.Min());
            _areaError = totalAreaErrors[min];
            BuildingFloors[1] = floors[min];
            BuildingCount = counts[min];
            _buildingTypeName = allTypes[min];
            CentreLines = centreLinesList[min];

            _maxCountPerLine = maxCountList[min];
            _depth = depths[min];
            _width = widths[min];
        }


        public void GeneratingBuildings()
        {
            var centreLineCount = DistributingCount(_maxCountPerLine, BuildingCount);

            List<int> cleanLineCount = new List<int>();
            List<Curve> cleanCentreLine = new List<Curve>();

            if (CentreLines.Length == 1)
            {
                _lastRow = 0;
            }
            //var lastRow = CentreLines[_lastRow];

            for (int i = 0; i < centreLineCount.Length; i++)
            {
                if (centreLineCount[i] != 0)
                {
                    cleanLineCount.Add(centreLineCount[i]);
                    cleanCentreLine.Add(CentreLines[i]);
                }
            }
            CentreLines = cleanCentreLine.ToArray();
            //_lastRow = CentreLines.ToList().IndexOf(lastRow);
            _lastRow = CentreLines.Length - 1;

            switch (_style)
            {
                case ResidentialStyles.RowRadiance:
                    BuildingGeometries = GetBuildingGeometries0(cleanLineCount.ToArray());
                    break;

                case ResidentialStyles.DotRowMajor:
                    BuildingGeometries = GetBuildingGeometries1(cleanLineCount.ToArray());
                    break;

                case ResidentialStyles.DotColumnMajor:
                    BuildingGeometries = GetBuildingGeometries2(cleanLineCount.ToArray());
                    break;
            }
        }


        /// <summary>
        /// Method for calculating the second line's length of the boundingbox. 
        /// </summary>
        /// <param name="site">CCurrent curve.</param>
        /// <param name="radiant">Radiant for getting boundingbox. Radiant need to be validated.</param>
        /// <returns>Edge length.</returns>
        public static double GetEdgeLength(Curve site, double radiant)
        {
            ValidatingRadiant(ref radiant);
            var edges = SiteBoundingRect.GetEdges(site, radiant);
            return edges[1].Length;
        }

        #endregion



        #region Three main methods for computing.
        private bool ComputeRowRadiance(BuildingParameters bp, int tempFloor, out Curve[] centreLines, out int[] totalCount, out double depth, out double width)
        {
            var edges = SiteBoundingRect.GetEdges(SetBack, _radiant);

            // 1.0 Getting parameters.
            var height = bp.FloorHeight * tempFloor;
            var sunDistance = BuildingDataset.GetSunlightDistance(height, _cityIndex) * Math.Cos(_radiant);
            var setback = BuildingDataset.GetSetbackRtype(tempFloor) * 2; 

            var loDepth = bp.Depth[0];
            var hiDepth = bp.Depth[1];
            depth = (loDepth + hiDepth) * 0.5;


            // Math.Cos(_radiant)*sunDistance:d ; depth: D ; count : n ; edges[1].Length : L
            // (2k +n-1) *d + n*D <= L
            // d = (L - n*D) / (2k+n-1);
            // n = (L-(2k-1)d)/(D+d);
            // D = 

            var lineCount = (int)Math.Round(  (edges[1].Length -(2*_k-1)* Math.Cos(_radiant) * sunDistance)
                /(Math.Cos(_radiant) * sunDistance+depth));

            // FOr debug
            var lineCount1 = (int)Math.Round((edges[1].Length + sunDistance) / (sunDistance + depth));  //Checked

            // Handle the excpation where edge.length is smaller than depth.
            if (lineCount < 0)
            {
                centreLines = null;
                totalCount = null;
                width = 0;
                return false;
            }

            else
            {
                // For debug
                var depth1 = Math.Round((edges[1].Length + sunDistance) / lineCount - sunDistance, 2); //Check
                depth = Math.Round((edges[1].Length - (2 * _k - 1) * Math.Cos(_radiant) * sunDistance) / lineCount - Math.Cos(_radiant) * sunDistance,3);

                if (depth > hiDepth)
                {
                    depth = hiDepth;
                    // Recalculate line count.
                    lineCount = (int)Math.Round((edges[1].Length - (2 * _k - 1) * Math.Cos(_radiant) * sunDistance) 
                        / (Math.Cos(_radiant) * sunDistance + depth));
                }

                if (depth < loDepth)
                {
                    depth = loDepth;
                    // Recalculate line count.
                    lineCount = (int)Math.Round((edges[1].Length - (2 * _k - 1) * Math.Cos(_radiant) * sunDistance)
                        / (Math.Cos(_radiant) * sunDistance + depth));
                }


                width = Math.Round(bp.Area / depth, 2);


                // Lines' order is 0,n,1,n-1,2, n-2 .......
                //centreLines = ParallelSplit(SetBack, edges, lineCount, _tolerance);
                centreLines = ParallelSplit(SetBack, edges, lineCount,depth,Math.Cos(_radiant)*sunDistance, _tolerance);


                // 1.2 Getting the total count of how many buildings each line can hold.
                totalCount = CalculateTotalCount0(ref centreLines, width, setback);

                return true;
            }
        }

        /// <summary>
        /// Method for calculating the total count in rowRadiant style.
        /// </summary>
        /// <param name="centreLines"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private int[] CalculateTotalCount0(ref Curve[] centreLines, double width, double setback)
        {
            List<int> result = new List<int>();
            List<Curve> cResult = new List<Curve>();
            for (int i = 0; i < centreLines.Length; i++)
            {
                var lineLengh = centreLines[i].GetLength();
                var tempCount = Math.Floor(lineLengh / width);
                

                if (tempCount > 3)
                {
                    var slutsCount = Math.Floor(tempCount / 3.0);
                    if (tempCount % 3 == 0)
                        slutsCount--;

                    lineLengh -= slutsCount * setback;
                }

                // Handle exception 1: c may be zero.
                var c = (int)Math.Floor(lineLengh / width);
                if (c != 0)
                {
                    result.Add(c);
                    cResult.Add(centreLines[i]);
                }
            }

            // Handle exception 2: result may have no value, which will cause error. Select the middle curve and extend itself to hold builidng.
            if (result.Sum() == 0)
            {
                var selectCurve = centreLines[centreLines.Length / 2];
    
       
                var vector = new Vector3d(selectCurve.PointAtEnd - selectCurve.PointAtStart);
                var spt = selectCurve.PointAtStart;
                var ept = spt + vector*width;
                cResult.Add(new PolylineCurve(new Point3d[] { spt, ept }));
                result.Add(1);
            }


            centreLines = cResult.ToArray();
            return result.ToArray();
        }

    
        #region Obsolete
        //private BuildingGeometry[] GetBuildingGeometries0(int[] centreLineCount)
        //{
        //    List<BuildingGeometry> result = new List<BuildingGeometry>();

        //    // Calculating line by line.
        //    for (int i = 0; i < CentreLines.Length; i++)
        //    {
        //        // Some lines may be too short for fitting a building.
        //        if (centreLineCount[i] == 0)
        //            continue;

        //        // Calculate the count of sluts.
        //        var slutsCount = 0;

        //        slutsCount += centreLineCount[i] / 3;
        //        if (centreLineCount[i] % 3 == 0)
        //            slutsCount--;

        //        // Calculate the standard dash line.
        //        int[] dash = new int[slutsCount + 1];
        //        for (int d = 0; d < dash.Length; d++)
        //        {
        //            if (d == dash.Length - 1)
        //            {
        //                dash[d] = centreLineCount[i] - 3 * d;
        //                break;
        //            }

        //            dash[d] = 3;
        //        }


        //        // Randomly altering the standard dash line.
        //        RandomAlter(ref dash, dash.Length);


        //        // Split line by dash.
        //        var parameters = GetSplitParameters(dash, CentreLines[i].Domain);
        //        Curve[] dashLines;


        //        if (parameters.Length == 0)
        //        {
        //            dashLines = new Curve[] { CentreLines[i] };
        //        }
        //        else
        //        {
        //            if (parameters.Length == 1)
        //            {
        //                dashLines = CentreLines[i].Split(parameters[0]);
        //            }
        //            else
        //            {
        //                dashLines = CentreLines[i].Split(parameters);
        //            }
        //        }


        //        // Get outline for each dash line.
        //        var outlines = GetOutlineForDash0(dashLines, dash, _width, _depth);


        //        var floorsCopy = new int[BuildingFloors.Length];
        //        BuildingFloors.CopyTo(floorsCopy, 0);

        //        // Handle the area error by adding more floors into the buildings located at the northest row. 
        //        if (CentreLines.Length == 1 || i == _lastRow)
        //        {
        //            var extraFloors = (int)Math.Ceiling(_areaError / (_width * _depth * outlines.Length));
        //            floorsCopy[1] += extraFloors;
        //        }

        //        // Generate building geometries.
        //        foreach (var outline in outlines)
        //        {
        //            var bType = new BuildingType(_buildingTypeName, floorsCopy, _siteArea);
        //            var building = new BuildingGeometry(bType, _tolerance);
        //            building.GeneratingResidentialAloneStyle(outline);
        //            result.Add(building);
        //        }
        //    }

        //    return result.ToArray();
        //}
        #endregion

        private BuildingGeometry[] GetBuildingGeometries0(int[] centreLineCount)
        {
            List<BuildingGeometry> result = new List<BuildingGeometry>();
            List<Curve[]> totalOutlines = new List<Curve[]>();
            IntervalTree intervalTree = new IntervalTree();
            List<IntervalNode> treeNodes = new List<IntervalNode>();

            var totalWidth = _width * Math.Cos(_radiant) + _depth * Math.Abs(Math.Sin(_radiant));

            // Calculating line by line.
            for (int i = 0; i < CentreLines.Length; i++)
            {
                // Some lines may be too short for fitting a building.
                if (centreLineCount[i] == 0)
                    continue;

                // Calculate the count of sluts.
                var slutsCount = 0;

                slutsCount += centreLineCount[i] / 3;
                if (centreLineCount[i] % 3 == 0)
                    slutsCount--;

                // Calculate the standard dash line.
                int[] dash = new int[slutsCount + 1];
                for (int d = 0; d < dash.Length; d++)
                {
                    if (d == dash.Length - 1)
                    {
                        dash[d] = centreLineCount[i] - 3 * d;
                        break;
                    }

                    dash[d] = 3;
                }


                // Randomly altering the standard dash line.
                RandomAlter(ref dash, dash.Length);

                // Split line by dash.
                var parameters = GetSplitParameters(dash, CentreLines[i].Domain);
                Curve[] dashLines;


                if (parameters.Length == 0)
                {
                    dashLines = new Curve[] { CentreLines[i] };
                }
                else
                {
                    if (parameters.Length == 1)
                    {
                        dashLines = CentreLines[i].Split(parameters[0]);
                    }
                    else
                    {
                        dashLines = CentreLines[i].Split(parameters);
                    }
                }

                // Get outline for each dash line. This method returns only for one line, while totalOutlines is a collection for all the lines.
                var outlines = GetOutlineForDash0(dashLines, dash, _width, _depth);

                for (int n = 0; n < outlines.Length; n++)
                {
                    // For each group.
                    List<double> leftX = new List<double>();
                    List<double> rightX = new List<double>();

                    totalOutlines.Add(outlines[n]);

                    for (int d = 0; d < outlines[n].Length; d++)
                    {
                        // For each outline.
                        var outline = outlines[n][d];

                        var pt = AreaMassProperties.Compute(outline).Centroid;
                        leftX.Add(pt.X - 0.5 * totalWidth);
                        rightX.Add(pt.X + 0.5 * totalWidth);
                    }

                    IntervalNode node = new IntervalNode(new UInterval(leftX.Min(), rightX.Max()), totalOutlines.Count - 1);
                    intervalTree.InsertNode(node);
                    treeNodes.Add(node);
                }            
            }

            // areaError>0 ; areaError<0 ; areError = 0
            // Create a new list for last row. Buildings locating in the last row should be higher.
            List<Curve> lastOrFirstRowOutlines = new List<Curve>();

            for (int n = 0; n < totalOutlines.Count; n++)
            {
                // For each group.
                var currentNode = treeNodes[n];
                var overlapedNodes = intervalTree.SearchOverlaps(currentNode, intervalTree.Root);

                List<double> outlineCentreY = new List<double>();
                foreach (var item in overlapedNodes)
                {
                    // Overlaped nodes includes the target node itself.
                    if (item._id == n)
                        continue;

                    // Warning: item._id represents the overlaped group not outline.
                    outlineCentreY.Add(GetCentroidY(totalOutlines[item._id][0])+ GetCentroidY(totalOutlines[item._id][totalOutlines[item._id].Length-1]));
                }

                if (outlineCentreY.Count == 0)
                {
                    // Find the last row outline.
                    lastOrFirstRowOutlines.AddRange(totalOutlines[n]);
                    continue;
                }

                // Sorting this list by using default comparer: 1,2,3,4,5,6
                outlineCentreY.Sort();

                int reverse = 1;
                if (_areaError <= 0)
                {
                    reverse *= -1;
                    outlineCentreY.Reverse();
                }
                    

                if ( (GetCentroidY(totalOutlines[n][0]) + GetCentroidY(totalOutlines[n][totalOutlines[n].Length - 1])) * reverse > outlineCentreY[outlineCentreY.Count-1]*reverse)
                {
                    // Find the last row outline.
                    lastOrFirstRowOutlines.AddRange(totalOutlines[n]);
                    continue;
                }
                else
                {
                    // Rest rows outlines. It's redy for extruding.
                    foreach (var outline in totalOutlines[n])
                    {
                        var bType = new BuildingType(_buildingTypeName, BuildingFloors, _siteArea);
                        var building = new BuildingGeometry(bType, _tolerance);
                        building.GeneratingResidentialAloneStyle(outline);
                        result.Add(building);
                    }
                }
            }


            // Find the special row items.
            var floorsCopy = new int[BuildingFloors.Length];
            BuildingFloors.CopyTo(floorsCopy, 0);

            var extraFloors = (int)Math.Round(_areaError / (_width * _depth * lastOrFirstRowOutlines.Count));
            floorsCopy[1] += extraFloors;

            foreach (var lastOuline in lastOrFirstRowOutlines)
            {

                var bType = new BuildingType(_buildingTypeName, floorsCopy, _siteArea);
                var building = new BuildingGeometry(bType, _tolerance);
                building.GeneratingResidentialAloneStyle(lastOuline);
                result.Add(building);
            }

            return result.ToArray();
        }

        private bool ComputeDotRowMajor(BuildingParameters bp, int tempFloor, out Curve[] centreLines, out int[] totalCount, out double depth, out double width)
        {
            var edges = SiteBoundingRect.GetEdges(_site, _radiant);

            // 1.0 Getting parameters.
            var height = bp.FloorHeight * tempFloor;
            var sunDistance = BuildingDataset.GetSunlightDistance(height, _cityIndex);
            double setback = BuildingDataset.GetSetbackRtype(tempFloor) * 2.0;

            var loDepth = bp.Depth[0];
            var hiDepth = bp.Depth[1];
            depth = (loDepth + hiDepth) * 0.5;

            // From bottom to up: (depth+sunDist) / (depth+sunDist) / (depth+sunDist) / (depth+sunDist) / ....../(depth) 
            var lineCount = (int)Math.Round((edges[1].Length + sunDistance * Math.Cos(_radiant)) / (sunDistance * Math.Cos(_radiant) + depth / Math.Cos(_radiant)));  //Check

            // Handle the excpation where edge.length is smaller than depth.
            if (lineCount < 0)
            {
                centreLines = null;
                totalCount = null;
                width = 0;
                return false;
            }

            else
            {
                depth = Math.Round(((edges[1].Length + sunDistance * Math.Cos(_radiant)) / lineCount - sunDistance * Math.Cos(_radiant)) * Math.Cos(_radiant), 2); //Check

                if (depth > hiDepth)
                {
                    depth = hiDepth;
                    // Recalculate line count.
                    lineCount = (int)Math.Floor((edges[1].Length + sunDistance * Math.Cos(_radiant)) / (sunDistance * Math.Cos(_radiant) + depth / Math.Cos(_radiant)));
                }

                if (depth < loDepth)
                {
                    depth = loDepth;
                    // Recalculate line count.
                    lineCount = (int)Math.Floor((edges[1].Length + sunDistance * Math.Cos(_radiant)) / (sunDistance * Math.Cos(_radiant) + depth / Math.Cos(_radiant)));
                }

                width = Math.Round(bp.Area / depth, 2);

                // Lines' order is 0,n,1,n-1,2, n-2 .......
                centreLines = ParallelSplit(_site, edges, lineCount, depth / Math.Cos(_radiant), sunDistance * Math.Cos(_radiant), _tolerance);

                // 1.2 Getting the total count of how many buildings each line can hold.
                totalCount = CalculateTotalCount1(centreLines, width, setback);

                return true;
            }
        }


        private int[] CalculateTotalCount1(Curve[] centreLines, double width, double setback)
        {
            int[] result = new int[centreLines.Length];
            for (int i = 0; i < centreLines.Length; i++)
            {
                var lineLengh = centreLines[i].GetLength();

                result[i] = (int)Math.Floor((lineLengh + setback / Math.Cos(_radiant)) / (width / Math.Cos(_radiant) + setback / Math.Cos(_radiant)));
            }

            return result;
        }


        private BuildingGeometry[] GetBuildingGeometries1(int[] centreLineCount)
        {
            List<BuildingGeometry> result = new List<BuildingGeometry>();
            List<Curve> totalOutlines = new List<Curve>();
            IntervalTree intervalTree = new IntervalTree();
            List<IntervalNode> treeNodes = new List<IntervalNode>();

            // Calculating line by line.
            for (int i = 0; i < CentreLines.Length; i++)
            {
                // Some lines may be too short for fitting a building.
                if (centreLineCount[i] == 0)
                    continue;

                // Calculate the standard dash line.
                double splitLength;

                if (centreLineCount[i] == 1)
                {
                    splitLength = CentreLines[i].GetLength();
                }
                else
                {
                    splitLength = (CentreLines[i].GetLength() - _width / Math.Cos(_radiant)) / (centreLineCount[i] - 1);
                }


                double[] splitLengths = new double[centreLineCount[i]];
                for (int s = 0; s < splitLengths.Length; s++)
                {
                    if (s == splitLengths.Length - 1)
                    {
                        splitLengths[s] = _width / Math.Cos(_radiant);
                        break;
                    }
                    splitLengths[s] = splitLength;
                }


                // Split line by dash.
                var parameters = GetSplitParameters(splitLengths, CentreLines[i].Domain);
                Curve[] dashLines;


                if (parameters.Length == 0)
                {
                    dashLines = new Curve[] { CentreLines[i] };
                }
                else
                {
                    if (parameters.Length == 1)
                    {
                        dashLines = CentreLines[i].Split(parameters[0]);
                    }
                    else
                    {
                        dashLines = CentreLines[i].Split(parameters);
                    }
                }


                // Get outline for each dash line.
                var outlines = GetOutlineForDash1(dashLines, _width, _depth, 0);


                // For this style, the oritation of building is zero radiance.
                var totalWidth = _width * Math.Cos(Math.Abs(0)) + _depth * Math.Sin(Math.Abs(0));

                foreach (var outline in outlines)
                {
                    totalOutlines.Add(outline);

                    var pt = AreaMassProperties.Compute(outline).Centroid;
                    IntervalNode node = new IntervalNode(new UInterval(pt.X - 0.5 * totalWidth, pt.X + 0.5 * totalWidth), totalOutlines.Count - 1);
                    intervalTree.InsertNode(node);
                    treeNodes.Add(node);
                }
            }


            // areaError>0 ; areaError<0 ; areError = 0
            if (_areaError > 0)
            {
                // Create a new list for last row.
                List<Curve> lastRowOutlines = new List<Curve>();


                for (int n = 0; n < totalOutlines.Count; n++)
                {
                    var currentNode = treeNodes[n];
                    var overlapedNodes = intervalTree.SearchOverlaps(currentNode, intervalTree.Root);

                    // Have overlaped nodes includes the target node itself.
                    List<double> outlineCentreY = new List<double>();
                    foreach (var item in overlapedNodes)
                    {
                        if (item._id == n)
                            continue;
                        outlineCentreY.Add(GetCentroidY(totalOutlines[item._id]));
                    }

                    if (outlineCentreY.Count == 0)
                    {
                        // Find the last row outline.
                        lastRowOutlines.Add(totalOutlines[n]);
                        continue;
                    }


                    if (GetCentroidY(totalOutlines[n]) > outlineCentreY.Max())
                    {
                        // Find the last row outline.
                        lastRowOutlines.Add(totalOutlines[n]);
                        continue;
                    }
                    else
                    {
                        // Non-last row outlines.
                        //var breps = GetGeometries(_buildingTypeName, totalOutlines[n], BuildingFloors, out string[] functions, out Curve[] layers);
                        //result.Add(new BuildingGeometry(breps, functions, layers));
                        var bType = new BuildingType(_buildingTypeName, BuildingFloors, _siteArea);
                        var building = new BuildingGeometry(bType, _tolerance);
                        building.GeneratingResidentialAloneStyle(totalOutlines[n]);
                        result.Add(building);
                    }
                }

                // Find the last row items.
                var floorsCopy = new int[BuildingFloors.Length];
                BuildingFloors.CopyTo(floorsCopy, 0);

                var extraFloors = (int)Math.Round(_areaError / (_width * _depth * lastRowOutlines.Count));
                floorsCopy[1] += extraFloors;

                foreach (var lastOuline in lastRowOutlines)
                {
                    //var breps = GetGeometries(_buildingTypeName, lastOuline, floorsCopy, out string[] functions, out Curve[] layers);
                    //result.Add(new BuildingGeometry(breps, functions, layers));

                    var bType = new BuildingType(_buildingTypeName, floorsCopy, _siteArea);
                    var building = new BuildingGeometry(bType, _tolerance);
                    building.GeneratingResidentialAloneStyle(lastOuline);
                    result.Add(building);
                }
            }
            else
            {
                for (int n = 0; n < totalOutlines.Count; n++)
                {
                    //var breps = GetGeometries(_buildingTypeName, totalOutlines[n], BuildingFloors, out string[] functions, out Curve[] layers);
                    //result.Add(new BuildingGeometry(breps, functions, layers));

                    var bType = new BuildingType(_buildingTypeName, BuildingFloors, _siteArea);
                    var building = new BuildingGeometry(bType, _tolerance);
                    building.GeneratingResidentialAloneStyle(totalOutlines[n]);
                    result.Add(building);
                }
            }

            return result.ToArray();
        }


        private bool ComputeDotColumnMajor(BuildingParameters bp, int tempFloor, out Curve[] centreLines, out int[] totalCount, out double depth, out double width)
        {
            var edges = SiteBoundingRect.GetEdges(_site, Math.PI / 2);

            // 1.0 Getting parameters.
            var height = bp.FloorHeight * tempFloor;
            var sunDistance = BuildingDataset.GetSunlightDistance(height, _cityIndex);
            double setback = BuildingDataset.GetSetbackRtype(tempFloor) * 2.0;

            var loDepth = bp.Depth[0];
            var hiDepth = bp.Depth[1];
            depth = (loDepth + hiDepth) * 0.5;

            var wd = bp.Area / depth * Math.Cos(_radiant) + depth * Math.Abs(Math.Sin(_radiant));


            var lineCount = (int)Math.Round(edges[1].Length / (wd + setback));

            // Handle the excpation where edge.length is smaller than depth.
            if (lineCount < 0)
            {
                centreLines = null;
                totalCount = null;
                width = 0;
                return false;
            }
            else
            {
                var tempWd = edges[1].Length / lineCount - setback;

                // sinA x^2 - tempWd x + Area*cosA = 0. width = x1 ; depth = x2.
                // SinA may equals to zero.
                var flag = SolveQuadratic.Compute(Math.Abs(Math.Sin(_radiant)), -tempWd, bp.Area * Math.Cos(_radiant), out double[] roots);

                if (flag)
                {
                    // Choose the positive root.
                    depth = Math.Round(roots.Min(), 2);
                }

                if (depth > hiDepth)
                {
                    depth = hiDepth;
                    // Recalculate line count.
                    lineCount = (int)Math.Floor(edges[1].Length / (setback + bp.Area / depth * Math.Cos(_radiant) + depth * Math.Sin(Math.Abs(_radiant))));
                }

                if (depth < loDepth)
                {
                    depth = loDepth;
                    // Recalculate line count.
                    lineCount = (int)Math.Floor(edges[1].Length / (setback + bp.Area / depth * Math.Cos(_radiant) + depth * Math.Sin(Math.Abs(_radiant))));
                }

                width = Math.Round(bp.Area / depth, 2);

                // Lines' order is 0,n,1,n-1,2, n-2 .......
                centreLines = ParallelSplit(_site, edges, lineCount, _tolerance);

                // 1.2 Getting the total count of how many buildings each line can hold.
                totalCount = CalculateTotalCount2(centreLines, depth, width, sunDistance);

                return true;
            }
        }


        private int[] CalculateTotalCount2(Curve[] centreLines, double depth, double width, double sunDistance)
        {
            int[] result = new int[centreLines.Length];
            for (int i = 0; i < centreLines.Length; i++)
            {
                var lineLengh = centreLines[i].GetLength();
                result[i] = (int)Math.Floor((lineLengh - width * Math.Sin(Math.Abs(_radiant)) - depth * Math.Cos(_radiant)) / (depth / Math.Cos(_radiant) + sunDistance)) + 1;

            }
            return result;
        }


        private BuildingGeometry[] GetBuildingGeometries2(int[] centreLineCount)
        {
            List<BuildingGeometry> result = new List<BuildingGeometry>();
            double height = BuildingDataset.GetBuildingParameters(_buildingTypeName).FloorHeight * BuildingFloors.Sum();
            //double setback = BuildingDataset.GetSetbackRtype(BuildingFloors.Sum()) * 2.0;
            var sunDistance = BuildingDataset.GetSunlightDistance(height, _cityIndex);
            var ratio = sunDistance / height;
            var totalExtraFloors = (int)Math.Round(_areaError / (_width * _depth));

            // For all outlines.
            OutlineEqualityComparer comparer = new OutlineEqualityComparer();
            Dictionary<Curve, int> outlineExtraFloors = new Dictionary<Curve, int>(comparer);
            // For outlines in the last row.

            HashSet<Curve> lastRowOutlines = new HashSet<Curve>(comparer);
            int lastRowExtraFloorsSum = 0;

            // Calculating line by line.
            for (int i = 0; i < CentreLines.Length; i++)
            {
                // Some lines may be too short for fitting a building.
                if (centreLineCount[i] == 0)
                    continue;


                // Calculate the standard dash line.
                var rectDepth = _width * Math.Abs(Math.Sin(_radiant)) + _depth * Math.Cos(_radiant);
                double splitLength;

                if (centreLineCount[i] == 1)
                {
                    splitLength = CentreLines[i].GetLength();
                }
                else
                {
                    splitLength = (CentreLines[i].GetLength() - rectDepth) / (centreLineCount[i] - 1);
                }

                double[] splitLengths = new double[centreLineCount[i]];
                Curve[] dashLines;

                // Move the basePt from the startpoint of segment to the center of outline.
                double basePtTrans = Math.Sqrt(_width * _width + _depth * _depth) * 0.5 * Math.Sin(Math.Asin(_depth / Math.Sqrt(_width * _width + _depth * _depth)) + Math.Abs(_radiant));

                for (int s = 0; s < splitLengths.Length; s++)
                {
                    if (s == splitLengths.Length - 1)
                    {
                        splitLengths[s] = rectDepth;
                        break;
                    }

                    splitLengths[s] = splitLength;
                }


                // Split line by dash.
                var parameters = GetSplitParameters(splitLengths, CentreLines[i].Domain);

                if (parameters.Length == 0)
                {
                    dashLines = new Curve[] { CentreLines[i] };
                }
                else
                {
                    if (parameters.Length == 1)
                    {
                        dashLines = CentreLines[i].Split(parameters[0]);
                    }
                    else
                    {
                        dashLines = CentreLines[i].Split(parameters);
                    }
                }

                // Get outline for each dash line.
                var outlines = GetOutlineForDash2(dashLines, _width, _depth, _radiant, basePtTrans);


                int extraFloors = 0;
                if (_areaError > 0 && splitLength - _depth * Math.Cos(_radiant) > sunDistance)
                {
                    var extraHeight = (splitLength - _depth * Math.Cos(_radiant) - sunDistance) / ratio;
                    extraFloors = (int)Math.Floor(extraHeight / BuildingDataset.GetBuildingParameters(_buildingTypeName).FloorHeight);
                }


                foreach (var outline in outlines)
                {
                    outlineExtraFloors.Add(outline, extraFloors);
                }

                lastRowOutlines.Add(outlines.Last());
                lastRowExtraFloorsSum += extraFloors;

            }


            var extraSum = outlineExtraFloors.Values.Sum();
            // There are two scienarios. TotalExtraFloors is larger or smaller than extraSum.
            if (totalExtraFloors <= extraSum)
            {
                // Generate building geometries.
                foreach (var item in outlineExtraFloors)
                {
                    // Create floors copy.
                    var floorsCopy = new int[BuildingFloors.Length];
                    BuildingFloors.CopyTo(floorsCopy, 0);

                    if (extraSum != 0)
                    {
                        floorsCopy[1] += (int)Math.Floor(item.Value * 1.0 / outlineExtraFloors.Values.Sum() * totalExtraFloors);
                    }

                    //var breps = GetGeometries(_buildingTypeName, item.Key, floorsCopy, out string[] functions, out Curve[] layers);
                    //result.Add(new BuildingGeometry(breps, functions, layers));

                    var bType = new BuildingType(_buildingTypeName, floorsCopy, _siteArea);
                    var building = new BuildingGeometry(bType, _tolerance);
                    building.GeneratingResidentialAloneStyle(item.Key);
                    result.Add(building);
                }
            }
            else
            {
                var floorsMargin = totalExtraFloors - extraSum;
                var averageFloor = (int)Math.Round((CentreLines.Length * BuildingFloors.Sum() + floorsMargin + lastRowExtraFloorsSum) * 1.0 / CentreLines.Length);

                Dictionary<Curve, int[]> lastRowFloors = new Dictionary<Curve, int[]>(comparer);
                int lastRowTotalMarginFloors = 0;

                // Generate building geometries.
                foreach (var item in outlineExtraFloors)
                {
                    // Create floors copy.
                    var floorsCopy = new int[BuildingFloors.Length];
                    BuildingFloors.CopyTo(floorsCopy, 0);

                    if (extraSum != 0)
                    {
                        floorsCopy[1] += item.Value;
                        if (lastRowOutlines.Contains(item.Key) && floorsCopy.Sum() < averageFloor)
                        {
                            lastRowFloors.Add(item.Key, floorsCopy);
                            lastRowTotalMarginFloors += averageFloor - floorsCopy.Sum();
                            continue;
                        }
                    }

                    //var breps = GetGeometries(_buildingTypeName, item.Key, floorsCopy, out string[] functions, out Curve[] layers);
                    //result.Add(new BuildingGeometry(breps, functions, layers));

                    var bType = new BuildingType(_buildingTypeName, floorsCopy, _siteArea);
                    var building = new BuildingGeometry(bType, _tolerance);
                    building.GeneratingResidentialAloneStyle(item.Key);
                    result.Add(building);
                }

                foreach (var item in lastRowFloors)
                {
                    item.Value[1] += (int)Math.Floor(((averageFloor - item.Value.Sum() * 1.0) / lastRowTotalMarginFloors) * floorsMargin);
                    //var breps = GetGeometries(_buildingTypeName, item.Key, item.Value, out string[] functions, out Curve[] layers);
                    //result.Add(new BuildingGeometry(breps, functions, layers));

                    var bType = new BuildingType(_buildingTypeName, item.Value, _siteArea);
                    var building = new BuildingGeometry(bType, _tolerance);
                    building.GeneratingResidentialAloneStyle(item.Key);
                    result.Add(building);
                }
            }

            return result.ToArray();
        }
        #endregion


        #region Help methods for Generating Builidngs

        private int[] DistributingCount(int[] totalCount, int typeCount)
        {
            if (totalCount.Sum() < typeCount)
                return totalCount;

            int[] result = new int[totalCount.Length];
            for (int i = 0; i < totalCount.Length; i++)
            {
                var temp = Math.Round(totalCount[i] * 1.0 / totalCount.Sum() * typeCount, 0);
                if (temp > totalCount[i])
                    result[i] = totalCount[i];
                else
                    result[i] = (int)temp;
            }

            // Handle the error.
            int error = typeCount - result.Sum();
            if (error > 0)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i] < totalCount[i])
                    {
                        result[i]++;
                        error--;
                    }

                    if (error == 0)
                        break;
                }
                return result;
            }
            else if (error < 0)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i] > 0)
                    {
                        result[i]--;
                        error++;
                    }

                    if (error == 0)
                        break;
                }
                return result;
            }
            else
            {
                return result;
            }
        }


        /// <summary>
        /// Helper method for splitting line. If double[].length ==0, means there is no need for splitting.
        /// </summary>
        /// <param name="dash"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        private double[] GetSplitParameters(int[] dash, Interval domain)
        {
            double[] parameters = new double[dash.Length - 1];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i == 0)
                {
                    parameters[i] = dash[i] * 1.0 / dash.Sum() * domain.Length + domain.Min;
                    continue;
                }

                parameters[i] = dash[i] * 1.0 / dash.Sum() * domain.Length + parameters[i - 1];
            }

            return parameters;
        }

        private double[] GetSplitParameters(double[] splitLength, Interval domain)
        {
            double[] parameters = new double[splitLength.Length - 1];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i == 0)
                {
                    parameters[i] = splitLength[i] / splitLength.Sum() * domain.Length + domain.Min;
                    continue;
                }

                parameters[i] = splitLength[i] / splitLength.Sum() * domain.Length + parameters[i - 1];
            }

            return parameters;
        }


        private void RandomAlter(ref int[] array, int iteration)
        {
            array.Shuffle();

            Random random = new Random();
            for (int i = 0; i < iteration; i++)
            {
                int n = random.Next(array.Length);
                int m = random.Next(array.Length);

                if (array[n] >= 4 || array[m] <= 2)
                    continue;

                array[n]++;
                array[m]--;
            }

            List<int> temp = new List<int>();
            int count =0;

            for (int i = 0; i < array.Length; i++)
            {
                if(array[i] != 1)
                {
                    temp.Add(array[i]);
                }
                else
                {
                    count++;
                }
            }

            while (count>0)
            {
                if (temp.Count == 0)
                {
                    temp.Add(count);
                    break;
                }

                var min = temp.IndexOf(temp.Min());
                temp[min]++;
                count--;
            }

            array = temp.ToArray();
        }

        private double GetCentroidY(Curve x)
        {
            var ptx = AreaMassProperties.Compute(x).Centroid;

            // Only need to compare the value of y coordinate of centroid.
            return ptx.Y;
        }


        private Curve[][] GetOutlineForDash0(Curve[] dashLines, int[] dash, double width, double depth)
        {
            List<Curve[]> result = new List<Curve[]>();

            for (int i = 0; i < dashLines.Length; i++)
            {
                // v1 is the direction from left to right. v2 is the direction from bottom to up.
                Vector3d v1 = new Vector3d(dashLines[0].PointAtEnd - dashLines[0].PointAtStart);
                v1.Unitize();
                Vector3d v2 = new Vector3d(v1);
                v2.Rotate(Math.PI / 2.0, Plane.WorldXY.ZAxis);


                if (i % 2 == 0)
                {
                    // From left to right.
                    int index = i / 2;

                    Curve[] groupCurves = new Curve[dash[index]];
                    for (int d = 0; d < dash[index]; d++)
                    {
                        var margin = (dashLines[index].GetLength() - width * dash[index]) / 2.0;
                        if (i == 0) margin = 0;

                        var basePt = dashLines[index].PointAtStart + v1 * (width * d + margin);
   

                        Polyline outline = GetOutlineFromPts(basePt, width, depth, v1, v2);
                        //result.Add(outline.ToPolylineCurve());
                        groupCurves[d] = outline.ToPolylineCurve();
                    }
                    result.Add(groupCurves);
                }
                else
                {
                    // From right to left.
                    int index = dashLines.Length - (int)Math.Ceiling(i / 2.0);

                    Curve[] groupCurves = new Curve[dash[index]];
                    for (int d = 0; d < dash[index]; d++)
                    {
                        var margin = (dashLines[index].GetLength() - width * dash[index]) / 2.0;
                        if (i == 1) margin = 0;

                        var basePt = dashLines[index].PointAtEnd - v1 * (width * (d+1) + margin)  ;

                        Polyline outline = GetOutlineFromPts(basePt, width, depth, v1, v2);
                        //result.Add(outline.ToPolylineCurve());
                        groupCurves[d] = outline.ToPolylineCurve();
                    }
                    result.Add(groupCurves);
                }
            }
            return result.ToArray();
        }


        /// <summary>
        /// Method for creating the outline from point lists. 
        /// </summary>
        /// <param name="basePt"></param>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="vLeftToRight"></param>
        /// <param name="vBottmToUp"></param>
        /// <returns></returns>
        private Polyline GetOutlineFromPts(Point3d basePt, double width, double depth, Vector3d vLeftToRight, Vector3d vBottmToUp)
        {
            var pt0 = basePt - vBottmToUp * depth * 0.4;
            var pt1 = pt0 + vLeftToRight * width * 0.12;
            var pt2 = pt1 - vBottmToUp * depth * 0.1;
            var pt3 = pt2 + vLeftToRight * width * 0.76;
            var pt4 = pt3 + vBottmToUp * depth * 0.1;
            var pt5 = pt4 + vLeftToRight * width * 0.12;

            var pt6 = pt5 + vBottmToUp * depth * 0.9;
            var pt7 = pt6 - vLeftToRight * width ;


            Polyline outline = new Polyline(new Point3d[] { pt0, pt1, pt2, pt3, pt4,pt5,pt6,pt7,pt0});
            return outline;
        }



        /// <summary>
        /// Each dash line only has one outline. Base point the middle point of each dash.
        /// </summary>
        /// <param name="dashLines"></param>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="radiant"></param>
        /// <returns></returns>
        private Curve[] GetOutlineForDash1(Curve[] dashLines, double width, double depth, double radiant)
        {
            List<Curve> result = new List<Curve>();

            for (int i = 0; i < dashLines.Length; i++)
            {
                // v1 is the direction from left to right. v2 is the direction from bottom to up.
                Vector3d v1 = Vector3d.XAxis;

                v1.Rotate(radiant, Vector3d.ZAxis);

                Vector3d v2 = new Vector3d(v1);
                v2.Rotate(Math.PI / 2.0, Vector3d.ZAxis);

                var basePt = dashLines[i].PointAtStart;
                basePt += Vector3d.YAxis * 0.5 * Math.Tan(_radiant) * width;

                var pt0 = basePt - v2 * depth * 0.5;
                var pt1 = pt0 + v1 * width;
                var pt2 = pt1 + v2 * depth;
                var pt3 = pt2 - v1 * width;

                Polyline outline = new Polyline(new Point3d[] { pt0, pt1, pt2, pt3, pt0 });
                result.Add(outline.ToPolylineCurve());
            }
            return result.ToArray();
        }


        private Curve[] GetOutlineForDash2(Curve[] dashLines, double width, double depth, double radiant, double transform)
        {
            List<Curve> result = new List<Curve>();

            for (int i = 0; i < dashLines.Length; i++)
            {
                // v1 is the direction from left to right. v2 is the direction from bottom to up.
                Vector3d v1 = Vector3d.XAxis;

                v1.Rotate(radiant, Vector3d.ZAxis);

                Vector3d v2 = new Vector3d(v1);
                v2.Rotate(Math.PI / 2.0, Vector3d.ZAxis);

                var basePt = dashLines[i].PointAtStart;
                basePt += Vector3d.YAxis * transform;

                var pt0 = basePt - v2 * depth * 0.5 - v1 * width * 0.5;
                var pt1 = pt0 + v1 * width;
                var pt2 = pt1 + v2 * depth;
                var pt3 = pt2 - v1 * width;

                Polyline outline = new Polyline(new Point3d[] { pt0, pt1, pt2, pt3, pt0 });
                result.Add(outline.ToPolylineCurve());
            }
            return result.ToArray();
        }

        #endregion


        #region Help methods for ResidentialSolver

        /// <summary>
        /// Method to make sure radiant is within -30~ 30.
        /// </summary>
        /// <param name="radiant"></param>
        private static void ValidatingRadiant(ref double radiant)
        {
            if (Math.Abs(radiant) > Math.PI / 2)
            {
                var remin = radiant % (Math.PI / 2);

                radiant = remin;
            }

            if (radiant > Math.PI / 4)
            {
                radiant -= Math.PI / 2;
            }

            if (radiant < -Math.PI / 4)
            {
                radiant += Math.PI / 2;
            }

            if (radiant > Math.PI / 6 || radiant < -Math.PI / 6)
            {
                radiant = radiant / Math.Abs(radiant) * Math.PI / 6;
            }
        }



        /// <summary>
        /// Lines' order is 0,n,1,n-1,2, n-2 .......
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="count"></param>
        /// <param name="depth"></param>
        /// <param name="sunDistance"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private static Curve[] ParallelSplit(Curve curve, Line[] edges, int count, double depth, double sunDistance, double tolerance)
        {
            

            // Handle exception.
            count = count < 1 ? 1 : count;

            var brep = Brep.CreatePlanarBreps(curve,tolerance)[0];

            // Cut the edges[1] and edges[3]
            List<Curve> tempList = new List<Curve>(count);
            Curve baseline = edges[0].ToNurbsCurve().DuplicateCurve();
            var vector = edges[1].Direction;
            vector.Unitize();

            // sunDistance:d ; depth: D ; count : n ; edges[1].Length : L
            // (2k +n-1) *d + n*D <= L
            // d = (L - n*D) / (2k+n-1);
           
            var len = edges[1].Length;
            var d = (len - count * depth) / (2*_k + count -1);


            if (count == 1)
            {
                baseline.Translate(vector * 0.5 * len);
            }
            else
            {
                baseline.Translate(vector *( 0.5 * depth + _k * d));
            }

            // Lines' order is bottom-up.
            for (int i = 0; i < count; i++)
            {
                var temp = baseline.DuplicateCurve();

                temp.Translate(vector * (depth + d) * i);
                Intersection.CurveBrep(temp, brep, tolerance, out Curve[] overlape, out _);

                // move overlape curve up and down.
                // For L-shape or U-shape site, overlape for one test line may yield more than one segment.
                if (overlape.Length != 0)
                {
                    for (int o = 0; o < overlape.Length; o++)
                    {
                        var tempTest = overlape[o].DuplicateCurve();
                        tempTest.Translate(vector * 0.5 * depth);
                        var flag1= Intersection.CurveBrep(tempTest, brep, tolerance, out Curve[] upperCurves, out _);
                        tempTest.Translate(vector * -1 * depth);
                        var flag2 = Intersection.CurveBrep(tempTest, brep, tolerance, out Curve[] downCurves, out _);

                        if (!flag1 || !flag2) continue;

                        foreach (var uCurve in upperCurves)
                        {
                            temp.ClosestPoint(uCurve.PointAtStart, out double ut0);
                            Point3d uStart = temp.PointAt(ut0);

                            temp.ClosestPoint(uCurve.PointAtEnd, out double ut1);
                            Point3d uEnd = temp.PointAt(ut1);

                            foreach (var dCurve in downCurves)
                            {
                                temp.ClosestPoint(dCurve.PointAtStart, out double dt0);
                                Point3d dStart = temp.PointAt(dt0);

                                temp.ClosestPoint(dCurve.PointAtEnd, out double dt1);
                                Point3d dEnd = temp.PointAt(dt1);

                                // Compare upper and down
                                var pStart = uStart.X > dStart.X ? uStart : dStart;
                                var pEnd = uEnd.X < dEnd.X ? uEnd : dEnd;

                                // Compare p and temp
                                pStart = pStart.X > temp.PointAtStart.X ? pStart : temp.PointAtStart;
                                pEnd = pEnd.X < temp.PointAtEnd.X ? pEnd : temp.PointAtEnd;

                                tempList.Add(new PolylineCurve(new Point3d[] { pStart,pEnd}));
                            }
                        }
                    }
                }
                    
            }

            return tempList.ToArray();
        }


        /// <summary>
        /// Lines' order is 0,n,1,n-1,2, n-2 .......
        /// </summary>
        /// <param name="brep"></param>
        /// <param name="edges"></param>
        /// <param name="count"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private static Curve[] ParallelSplit(Curve curve, Line[] edges, int count, double tolerance)
        {
            var brep = Brep.CreatePlanarBreps(curve, tolerance)[0];

            // Cut the edges[1] and edges[3]
            List<Curve> tempList = new List<Curve>(count);
            Curve baseline = edges[0].ToNurbsCurve().DuplicateCurve();

            var vector = edges[1].Direction;
            vector.Unitize();

            var distance = edges[1].Length / count;

            baseline.Translate(vector * 0.5 * distance);

            // Lines' order is bottom-up.
            for (int i = 0; i < count; i++)
            {
                var temp = baseline.DuplicateCurve();

                temp.Translate(vector * distance * i);
                Intersection.CurveBrep(temp, brep, tolerance, out Curve[] overlape, out Point3d[] pts);

                // For L-shape or U-shape site, overlape for one test line may yield more than one segment.
                if (overlape.Length != 0)
                    tempList.AddRange(overlape);
            }

            // Change the order of centrelines: eg. 6 lines, 0,5,1,4,2,3 . 
            var result = new Curve[tempList.Count];
            for (int i = 0; i < tempList.Count; i++)
            {
                if (i % 2 == 0)
                {
                    result[i] = tempList[i / 2];
                }
                else
                {
                    var index = tempList.Count - (int)Math.Ceiling(i / 2.0);
                    result[i] = tempList[index];
                }
            }

            return result;
        }

        public void Dispose()
        {
            _site.Dispose();
            BuildingGeometries = null;
            SetBack.Dispose();
            BuildingFloors = null;
            CentreLines = null;
        }


        /// <summary>
        /// Comparer used for finding the same outline in the column major style.
        /// </summary>
        private class OutlineEqualityComparer : EqualityComparer<Curve>
        {
            public override bool Equals(Curve x, Curve y)
            {
                return AreaMassProperties.Compute(x).Centroid == AreaMassProperties.Compute(y).Centroid;
            }

            public override int GetHashCode(Curve obj)
            {
                var pt = AreaMassProperties.Compute(obj).Centroid;
                return pt.X.GetHashCode() ^ pt.Y.GetHashCode();
            }
        }




        #endregion
    }
}
