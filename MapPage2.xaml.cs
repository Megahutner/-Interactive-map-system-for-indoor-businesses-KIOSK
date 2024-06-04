using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UWEKiosk.Entities;
using System.Windows.Media.Media3D;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;


namespace UWEKiosk
{
    /// <summary>
    /// Interaction logic for MapPage2.xaml
    /// </summary>
    public partial class MapPage2 : Page
    {
        private ZoneDetails zoneInfo;
        private ZoneMap mapInfo;
        public MapPage2()
        {
            InitializeComponent();
            GetZoneDetails();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {          
            Uri uri = new Uri("SplashPage.xaml", UriKind.Relative);
            this.NavigationService.Navigate(uri);
        }

        public void UpdateZone()
        {
                    ZoneMap map = new ZoneMap();
                    map.ZoneId = zoneInfo.Id;
                    map.Height = mapGrid.Height;
                    map.Width = mapGrid.Width;
                    map.Map = new GridMap[(int)Math.Round((map.Width / 10)), (int)Math.Round(map.Height / 10)];
                    map.Object = new List<MapObject>();
                    foreach (var item in zoneInfo.ObjectList)
                    {
                        if (item.Type == 2) // normal block
                        {
                    if(item.Category != "Wall")
                    {
                        MapObject blockObject = new MapObject();
                        blockObject.ObjectId = item.ObjectId;
                        blockObject.Width = item.Width;
                        blockObject.Height = item.Height;
                        blockObject.X = item.Lat;
                        blockObject.Y = item.Lng;
                        blockObject.Type = 2;
                        blockObject.Rotate = item.Front;
                        map.Object.Add(blockObject);
                    }                      
                        }
                        else
                        {
                            MapObject terminalObject = new MapObject();
                            terminalObject.ObjectId = item.ObjectId;
                            terminalObject.Width = item.Width;
                            terminalObject.Height = item.Height;
                            terminalObject.X = item.Lat;
                            terminalObject.Y = item.Lng;
                            terminalObject.Type = 1;
                            terminalObject.Rotate = item.Front;
                            map.Object.Add(terminalObject);
                        }
                    }
                       
                    for (var i = 0; i < Math.Round(map.Width / 10); i++) // create
                    {
                        for (var j = 0; j < Math.Round(map.Height / 10); j++)
                        {
                            GridMap grid = new GridMap();
                            grid.I = i;
                            grid.J = j;
                            grid.X = i * 10;
                            grid.Y = j * 10;
                            grid.Occupied = false;
                            grid.CameFrom = new GridMap();
                            grid.Neighbors = new List<GridMap>();
                            grid.F = 100000;
                            grid.G = 100000;
                            grid.H = 100000;
                            map.Map[i, j] = grid;
                        }
                    }

                    foreach (var ob in map.Object)
                    {
                        var bounding = GetBoundingClientRect(ob);
                        var minimumI = 0;
                        var maximumI = 0;
                        var minimumJ = 0;
                        var maximumJ = 0;
                        for (var i = 0; i < map.Map.GetLength(0); i++)
                        {
                            for (var j = 0; j < map.Map.GetLength(1); j++)
                            {
                                var x = map.Map[i, j].X;
                                var y = map.Map[i, j].Y;
                                if ( !((bounding.Top ) >= y + 10  ||
                            bounding.Right  <= x ||
                            bounding.Bottom  <= y ||
                            bounding.Left  >= x + 10) )
                                {

                            if(ob.Type == 2)
                            {
                                map.Map[i, j].Occupied = true;
                            }

                            if (minimumI == 0 && minimumJ == 0 && maximumI == 0 && maximumJ == 0)
                                    {
                                        minimumI = i;
                                        minimumJ = j;
                                        maximumI = i;
                                        maximumJ = j;
                                    }
                                    else
                                    {
                                        if (i < minimumI)
                                        {
                                            minimumI = i;
                                        }
                                        if (j < minimumJ)
                                        {
                                            minimumJ = j;
                                        }
                                        if (i > maximumI)
                                        {
                                            maximumI = i;

                                        }
                                        if (j > maximumJ)
                                        {
                                            maximumJ = j;
                                        }
                                    }
                                }
                            }
                        }
                        ob.MinimumI = minimumI;
                        ob.MaximumI = maximumI;
                        ob.MinimumJ = minimumJ;
                        ob.MaximumJ = maximumJ;
                    }      
            map.Map = FindNeighbors(map.Map);
            mapInfo = map;
            //var data = JsonConvert.SerializeObject(mapInfo, Formatting.Indented, new JsonSerializerSettings
            //{
            //    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            //});
            //File.WriteAllText($"{Directory.GetCurrentDirectory()}/DataInformation/mapInfo.txt", data);

        }

        public void Pathfinding(string ObjectIds)
        {
            erasePathLine();
                GridMap End = new GridMap();
                GridMap Start = new GridMap();
                List<GridMap> resultPath = new List<GridMap>();
                List<GridMap> reset = new List<GridMap>();
                List<GridMap> availableGrid = new List<GridMap>();
                var watch = new Stopwatch();
                var overallWatch = new Stopwatch();
                overallWatch.Start(); // measuring whole function execution time
                //watch.Start(); // measuring deep clone execution time
                //var map = DeepClone(data.Map); // deep copy using serialization
                var map = mapInfo.Map;
                //watch.Stop();              
                var objects = mapInfo.Object.Where(m => (ObjectIds.Contains(m.ObjectId) && m.Type == 2) || ( m.Type == 1)).ToList();
                foreach (var ob in objects)
                {
                    GridMap end = new GridMap();
                    switch (ob.Rotate)
                    {
                        case 0:
                            {
                                var j = (ob.MinimumJ + (ob.MaximumJ - ob.MinimumJ) / 2);
                                for (int i = ob.MaximumI - 1; i <= ob.MaximumI; i++)
                                {
                                    if (i > 0 && j > 0 && i < map.GetLength(0) && j < map.GetLength(1))
                                    {
                                        map[i, j].Occupied = false;
                                        availableGrid.Add(map[i, j]);
                                    }
                                }
                                end = map[ob.MaximumI + 1, (ob.MinimumJ + (ob.MaximumJ - ob.MinimumJ)/2+1)];
                            //    if (map[ob.MaximumI + 1, j].Occupied == true)
                            //    {
                            //    //return new List<GridMap>();
                            //    MessageBox.Show("Empty"); 
                            //    return;
                            //}
                            break;
                            }
                        case 360:
                            {
                                var j = (ob.MinimumJ + (ob.MaximumJ - ob.MinimumJ) / 2);
                                for (int i = ob.MaximumI - 1; i <= ob.MaximumI; i++)
                                {
                                    if (i > 0 && j > 0 && i < map.GetLength(0) && j < map.GetLength(1))
                                    {
                                        map[i, j].Occupied = false;
                                        availableGrid.Add(map[i, j]);
                                    }
                                }
                                end = map[ob.MaximumI =1, (ob.MinimumJ + (ob.MaximumJ - ob.MinimumJ)/2+1)];
                            //    if (map[ob.MaximumI + 1, j].Occupied == true)
                            //    {
                            //    //return new List<GridMap>();
                            //    MessageBox.Show("Empty"); return;
                            //}
                            break;
                            }
                        case 90:
                            {
                                var i = (ob.MinimumI + (ob.MaximumI - ob.MinimumI) / 2);
                                for (int j = ob.MaximumJ - 1; j <= ob.MaximumJ; j++)
                                {
                                    if (i > 0 && j > 0 && i < map.GetLength(0) && j < map.GetLength(1))
                                    {
                                        map[i, j].Occupied = false;
                                        availableGrid.Add(map[i, j]);
                                    }
                                }
                                end = map[(ob.MinimumI + (ob.MaximumI - ob.MinimumI) / 2+1), ob.MaximumJ + 1];
                            //    if (map[i, ob.MaximumJ + 1].Occupied == true)
                            //    {
                            //        //return new List<GridMap>();
                            //    MessageBox.Show("Empty"); return;


                            //}
                            break;
                            }
                        case 180:
                            {
                                var j = (ob.MinimumJ + (ob.MaximumJ - ob.MinimumJ) / 2);
                                for (int i = ob.MinimumI; i <= ob.MinimumI + 1; i++)
                                {
                                    if (i > 0 && j > 0 && i < map.GetLength(0) && j < map.GetLength(1))
                                    {
                                        map[i, j].Occupied = false;
                                        availableGrid.Add(map[i, j]);
                                    }
                                }
                                end = map[ob.MinimumI - 1, (ob.MinimumJ + (ob.MaximumJ - ob.MinimumJ) / 2 + 1)];
                            //    if (map[ob.MinimumI - 1, j].Occupied == true)
                            //    {                                MessageBox.Show("Empty");

                            //    //return new List<GridMap>();
                            //    MessageBox.Show("Empty"); return;


                            //}
                            break;
                            }
                        case 270:
                            {
                                var i = ob.MinimumI + (ob.MaximumI - ob.MinimumI) / 2;
                                for (int j = ob.MinimumJ-1; j <= ob.MinimumJ ; j++)
                                {
                                    if (i > 0 && j > 0 && i < map.GetLength(0) && j < map.GetLength(1))
                                    {
                                        map[i, j].Occupied = false;
                                        availableGrid.Add(map[i, j]);
                                    }
                                }
                                end = map[ob.MinimumI + (ob.MaximumI - ob.MinimumI) / 2, ob.MinimumJ -1];
                            //    if (map[i, ob.MinimumI - 1].Occupied == true) // if path to destination is occupied,
                            //    {
                            //       //return new List<GridMap>();
                            //    MessageBox.Show("Empty");
                            //    return;
                            //}
                            break;
                            }
                    }
                    if (ob.Type == 2)
                    {
                        End = end; // blocks

                    }
                    else
                    {
                        Start = end; // terminal
                    }
                }
                var nextStart = Start;
                List<GridMap> openSet = new List<GridMap>();
                List<GridMap> closedSet = new List<GridMap>();
                List<GridMap> path = new List<GridMap>();
                List<GridMap> optimizedPath = new List<GridMap>();
                // watch.Reset(); // measuring pathfinding algorithm
                watch.Start();
           
                        //closedSet.Clear();
                        openSet.Add(Start);
                        while (openSet.Count() > 0)
                        {
                            var current = openSet.OrderBy(m => m.F).FirstOrDefault();
                            if (current == End)
                            {
                                openSet.Clear();
                                closedSet.Clear();
                                var temp = current;
                                path.Add(temp);
                                while (temp.CameFrom != null)
                                {
                                    path.Add(temp.CameFrom);
                                    temp = temp.CameFrom;
                                }
                                for (var i = 0; i < map.GetLength(0); i++)
                                {
                                    for (var j = 0; j < map.GetLength(1); j++)
                                    {
                                        map[i, j].CameFrom = null;
                                        map[i, j].F = 100000;
                                        map[i, j].G = 100000;
                                        map[i, j].H = 100000;
                                    }
                                }

                                break;
                            }
                            closedSet.Add(current);
                            openSet.Remove(current);
                            var my_neighbors = current.Neighbors;
                            var st = new Stopwatch();
                            st.Restart();
                            int count = 0;
                            for (var i = 0; i < my_neighbors.Count(); i++)
                            {
                                count++;
                                var neighbor = my_neighbors[i];
                                if (!closedSet.Contains(neighbor) && neighbor.Occupied == false)
                                {
                                    var tempG = current.G + 1;
                                    var newPath = false;
                                    if (openSet.Contains(neighbor))
                                    {
                                        if (tempG < neighbor.G)
                                        {
                                            neighbor.G = tempG;
                                            newPath = true;
                                        }
                                    }
                                    else
                                    {
                                        neighbor.G = tempG;
                                        newPath = true;
                                        openSet.Add(neighbor);
                                    }
                                    if (newPath)
                                    {
                                        neighbor.H = Convert.ToInt64(Heuristic(neighbor, End)*0.1);
                                        neighbor.F = neighbor.G + neighbor.H;
                                        //neighbor.G = neighbor.F + neighbor.H;
                                        neighbor.CameFrom = current;
                                    }
                                    closedSet.Add(neighbor);
                                }
                            }
                            st.Stop();
                            //Log.Information($"test {count} - {st.ElapsedMilliseconds}MS");
                        }
            if (optimizedPath.Count() == 0 || path.Count() < optimizedPath.Count())
            {
                optimizedPath = path.ToList();
                //nextStart = end;
            }
            path.Clear();
                    
                    resultPath.AddRange(optimizedPath);
                    //optimizedPath.Clear();

                    //Ends.Remove(nextStart);
                    //Start = nextStart;
                    //continue;
                
                foreach (var grid in availableGrid)
                {
                    grid.Occupied = true;
                }
                watch.Stop();
                //Log.Information($"Algorithm - {watch.ElapsedMilliseconds}");
                overallWatch.Stop();
            //Log.Information($"Pathfinding - {overallWatch.ElapsedMilliseconds}");
            //return resultPath;
            var result = resultPath.Select(m => new
            {
                m.X,
                m.Y,
            });
            foreach (var rec in result)
            {
                Rectangle block = new Rectangle()
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.DarkRed,
                    Uid = rec.X.ToString() + rec.Y.ToString()
                };
                Canvas.SetTop(block, rec.Y);
                Canvas.SetLeft(block, rec.X);
                mapGrid.Children.Add(block);
            }

            Rectangle destination = new Rectangle()
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Yellow,
                Uid = End.X.ToString() + End.Y.ToString()
            };
            Canvas.SetTop(destination, End.Y);
            Canvas.SetLeft(destination, End.X);
            mapGrid.Children.Add(destination);
            //var data = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings
            //{
            //    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            //});
            //File.WriteAllText($"{Directory.GetCurrentDirectory()}/DataInformation/result.txt", data);
        }

        private void erasePathLine()
        {
            List<UIElement> itemstoremove = new List<UIElement>();
            foreach (UIElement ui in mapGrid.Children)
            {
                if (ui is Rectangle)
                {
                    itemstoremove.Add(ui);
                }
            }
            foreach (UIElement ui in itemstoremove)
            {
                mapGrid.Children.Remove(ui);
            }
        }


        private long Heuristic(GridMap a, GridMap b)
        {
            long d = Convert.ToInt64(Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));
            return d;
        }

        private void drawgridline()
        {
            DrawingBrush brush = new DrawingBrush();
            brush.TileMode = TileMode.Tile;
            brush.Viewport = new Rect(0,0,0.02,0.02);
            brush.ViewboxUnits = BrushMappingMode.Absolute;
        
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            
            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = new Rect(0, 0, 10, 10);
            geometryDrawing.Geometry = rectangleGeometry;

            Pen pen = new Pen();
            pen.Brush = new SolidColorBrush(Colors.Gray);
            pen.Thickness = 0.05;

            geometryDrawing.Pen = pen;

            brush.Drawing = geometryDrawing;
            mapGrid.Background = brush;
        }

        private void ObjectClick(object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource is Rectangle)
            {
                Rectangle currentBlock = (Rectangle)e.OriginalSource;
                //MessageBox.Show(currentBlock.Uid);
                //List<string> objectIDs = new List<string>();
                //objectIDs.Add(currentBlock.Uid);
                Pathfinding(currentBlock.Uid);
            }
            else if (e.OriginalSource is TextBlock)
            {
                TextBlock currentBlock = (TextBlock)e.OriginalSource;
                //MessageBox.Show(currentBlock.Uid);
                //List<string> objectIDs = new List<string>();
                //objectIDs.Add(currentBlock.Uid);
                Pathfinding(currentBlock.Uid);
            }
        }

        private void GetZoneDetails()
        {
            if (File.Exists($"{Directory.GetCurrentDirectory()}/DataInformation/json.txt"))
            {
                var text = File.ReadAllText($"{Directory.GetCurrentDirectory()}/DataInformation/json.txt");
                var content = JsonConvert.DeserializeObject<DataResponse>(text);
                zoneInfo = content.data;
                mapGrid.Width = zoneInfo.Width;
                mapGrid.Height = zoneInfo.Height;
                if (File.Exists($"{Directory.GetCurrentDirectory()}/DataInformation/zoneImage.png") && zoneInfo.ImgUrl != "")
                {
                    ImageBrush image = new ImageBrush();
                    image.ImageSource = new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}/DataInformation/zoneImage.png"));
                    //image.Opacity = 0.1;
                    image.Stretch = Stretch.Fill;
                    mapGrid.Background = image;
                }
                zoneObjects.ItemsSource = zoneInfo.ObjectList.Where(m => m.Type == 2 && m.Category!= "Wall");
                ICollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(zoneObjects.ItemsSource);
                PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
                view.GroupDescriptions.Add(groupDescription);
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                foreach (var item in zoneInfo.ObjectList)
                {
                    if (item.Type == 2)
                    {
                        Grid grid = new Grid()
                        {
                            Width = (double)(item.Width),
                            Height = (double)(item.Height),
                        };
                        SolidColorBrush myBrush = Brushes.LightGray;
                        if (item.Color != "" && item.Color != null)
                        {
                            Color color = (Color)ColorConverter.ConvertFromString(item.Color);
                            myBrush = new SolidColorBrush(color);
                        }
                        Rectangle rec = new Rectangle()
                        {
                            Width = (double)(item.Width),
                            Height = (double)(item.Height),
                            Fill = myBrush,
                            Uid = item.ObjectId

                        };
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = item.Name;
                        textBlock.Uid = item.ObjectId;
                        textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                        textBlock.VerticalAlignment = VerticalAlignment.Center;
                       
                        RotateTransform rotateTransform = new RotateTransform();
                        rotateTransform.Angle = item.Front;
                        rec.RenderTransform = rotateTransform;
                        textBlock.RenderTransform = rotateTransform;
                        textBlock.RenderTransformOrigin = new Point(0.5, 0.5);
                        rec.RenderTransformOrigin = new Point(0.5, 0.5);
                        grid.Children.Add(rec);
                        grid.Children.Add(textBlock);
                        mapGrid.Children.Add(grid);
                        Canvas.SetLeft(grid, (double)((item.Lat)));
                        Canvas.SetTop(grid, (double)((item.Lng)));
                    }
                    else
                    {
                        Grid grid = new Grid()
                        {
                            Width = (double)(item.Width),
                            Height = (double)(item.Height),
                        };
                        if (File.Exists($"{Directory.GetCurrentDirectory()}/DataInformation/marker.png"))
                        {
                            Image myImage = new Image();
                            myImage.Source = new BitmapImage(new Uri($"{Directory.GetCurrentDirectory()}/DataInformation/marker.png"));
                            myImage.Stretch = Stretch.Fill;
                            RotateTransform rotateTransform = new RotateTransform();
                            //rotateTransform.Angle = item.Front;
                            //myImage.RenderTransform = rotateTransform;
                            //myImage.RenderTransformOrigin = new Point(0.5, 0.5);
                            grid.Children.Add(myImage);
                            mapGrid.Children.Add(grid);
                            Canvas.SetLeft(grid, (double)((item.Lat)));
                            Canvas.SetTop(grid, (double)((item.Lng)));
                        }
                        else
                        {
                            SolidColorBrush myBrush = Brushes.LightGray;
                            if (item.Color != "" && item.Color != null)
                            {
                                Color color = (Color)ColorConverter.ConvertFromString(item.Color);
                                myBrush = new SolidColorBrush(color);
                            }
                            Rectangle rec = new Rectangle()
                            {
                                Width = (double)(item.Width),
                                Height = (double)(item.Height),
                                Fill = myBrush,
                                Uid = item.ObjectId

                            };
                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = item.Name;
                            textBlock.Uid = item.ObjectId;
                            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                            textBlock.VerticalAlignment = VerticalAlignment.Center;                          
                            RotateTransform rotateTransform = new RotateTransform();
                            rotateTransform.Angle = item.Front;
                            rec.RenderTransform = rotateTransform;
                            textBlock.RenderTransform = rotateTransform;
                            textBlock.RenderTransformOrigin = new Point(0.5, 0.5);
                            rec.RenderTransformOrigin = new Point(0.5, 0.5);
                            grid.Children.Add(rec);
                            grid.Children.Add(textBlock);
                            mapGrid.Children.Add(grid);
                            Canvas.SetLeft(grid, (double)((item.Lat)));
                            Canvas.SetTop(grid, (double)((item.Lng)));
                        }
                    }

                }
                UpdateZone();
            }
            else
            {
                MessageBox.Show("Error Code - " + 400 + " : Message - " + "No data");
                Environment.Exit(0);
            }

        }
        public BoundingClientRect GetBoundingClientRect(MapObject ob)
        {
            BoundingClientRect p = new BoundingClientRect();
            var centerX = ob.X + ob.Width / 2;
            var centerY = ob.Y + ob.Height / 2;
            if (ob.Rotate == 90 || ob.Rotate == 270)
            {
                p.Top = centerY - ob.Width / 2;
                p.Bottom = centerY + ob.Width / 2;
                p.Left = centerX - ob.Height / 2;
                p.Right = centerX + ob.Height / 2;
            }
            else
            {
                p.Top = ob.Y;
                p.Bottom = ob.Y + ob.Height;
                p.Left = ob.X;
                p.Right = ob.X + ob.Width;
            }
            return p;
        }

        private GridMap[,] FindNeighbors(GridMap[,] map)
        {
            for (var i = 0; i < map.GetLength(0); i++) // create
            {
                for (var j = 0; j < map.GetLength(1); j++)
                {
                    if (i < map.GetLength(0) - 1)
                    {
                        if (!map[i, j].Neighbors.Contains(map[i + 1, j]))
                        {
                            map[i, j].Neighbors.Add(map[i + 1, j]);
                        }
                    }
                    if (i > 0)
                    {
                        if (!map[i, j].Neighbors.Contains(map[i - 1, j]))
                        {
                            map[i, j].Neighbors.Add(map[i - 1, j]);
                        }
                    }
                    if (j < map.GetLength(1) - 1)
                    {
                        if (!map[i, j].Neighbors.Contains(map[i, j + 1]))
                        {
                            map[i, j].Neighbors.Add(map[i, j + 1]);
                        }
                    }
                    if (j > 0)
                    {
                        if (!map[i, j].Neighbors.Contains(map[i, j - 1]))
                        {
                            map[i, j].Neighbors.Add(map[i, j - 1]);
                        }
                    }
                }
            }
            return map;
        }

        private void zoneObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (ZoneObject)((ListView)sender).SelectedItem;
            if(selected != null)
            {
                Pathfinding(selected.ObjectId);
            }
        }
    }



    [Serializable]
    public class ZoneMap
    {
        public int ZoneId { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public GridMap[,] Map { get; set; }
        public List<MapObject> Object { get; set; }
    }

    [Serializable]
    public class GridMap
    {
        public int I { get; set; }
        public int J { get; set; }
        public double X { get; set; } // Latitude
        public double Y { get; set; } // Longtitude
        public bool Occupied { get; set; }
        public List<GridMap> Neighbors { get; set; }
        public GridMap CameFrom { get; set; }
        public long F { get; set; }
        public long G { get; set; }
        public long H { get; set; }

    }


    [Serializable]
    public class MapObject
    {
        public string ObjectId { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int MinimumI { get; set; }
        public int MaximumI { get; set; }
        public int MinimumJ { get; set; }
        public int MaximumJ { get; set; }
        public short Type { get; set; } // 1 = terminal / kiosk info, 2 = block 
        public double Rotate { get; set; }
    }

    public class BoundingClientRect
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
    }
}
