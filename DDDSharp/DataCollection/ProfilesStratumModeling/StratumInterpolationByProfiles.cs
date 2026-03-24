using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StratumInterpolation
{    
    #region 基础数据结构
    /// <summary>
    /// 空间点数据结构
    /// </summary>
    public struct ProfilePoint 
    {
        public double X;      // 东向坐标
        public double Y;      // 北向坐标
        public double Z;      // 高程
        public int Value;     // 地层编号，-1表示无地层
        public int ClusterId; // 聚类ID（-1表示未聚类）

        public ProfilePoint(double x, double y, double z, int value, int clusterId = -1)
        {
            X = x;
            Y = y;
            Z = z;
            Value = value;
            ClusterId = clusterId;
        }

        /// <summary>
        /// 计算方向加权距离（基于地层连通方向）
        /// </summary>
        public double DirectionalDistanceTo(ProfilePoint other, Vector3D directionVector,
                                           double horizontalWeight = 2.0,
                                           double verticalWeight = 0.5)
        {
            var pointVector = new Vector3D(other.X - X, other.Y - Y, other.Z - Z);
            double parallelComponent = pointVector.DotProduct(directionVector.Normalize());
            double perpendicularComponent = Math.Sqrt(pointVector.Magnitude() * pointVector.Magnitude() -
                                                     parallelComponent * parallelComponent);
            double zComponent = Math.Abs(Z - other.Z);

            return Math.Sqrt(
                Math.Pow(perpendicularComponent * horizontalWeight, 2) +
                Math.Pow(parallelComponent * verticalWeight, 2) +
                Math.Pow(zComponent * 1.0, 2)
            );
        }

        public bool Equals(ProfilePoint other)
        {
            return X == other.X && Y == other.Y && Z == other.Z && Value == other.Value;
        }

        public override bool Equals(object obj) => obj is ProfilePoint other && Equals(other);
        public override int GetHashCode() 
        { 
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ Value.GetHashCode();
        }

        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2}) -> 地层{Value}(聚类{ClusterId})";
    }

    /// <summary>
    /// 3D向量（用于方向计算）
    /// </summary>
    public class Vector3D
    {
        public double X, Y, Z;

        public Vector3D(double x, double y, double z) { X = x; Y = y; Z = z; }

        /// <summary>
        /// 点积
        /// </summary>
        public double DotProduct(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

        /// <summary>
        /// 模长
        /// </summary>
        public double Magnitude() => Math.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>
        /// 单位向量
        /// </summary>
        public Vector3D Normalize()
        {
            double mag = Magnitude();
            return mag < 1e-10 ? new Vector3D(0, 0, 0) : new Vector3D(X / mag, Y / mag, Z / mag);
        }
    }

    /// <summary>
    /// 地层聚类区域
    /// </summary>
    public class StratumCluster
    {
        public int ClusterId { get; set; }          // 聚类ID
        public int StratumId { get; set; }          // 地层编号
        public int ProfileId { get; set; }          // 所属剖面ID（0=A，1=B）
        public List<ProfilePoint> Points { get; set; }     // 聚类包含的点
        public ProfilePoint Centroid { get; set; }         // 聚类中心
        public Vector3D ConnectDirection { get; set; } // 与另一剖面同源地层的连通方向

        public StratumCluster(int clusterId, int stratumId, int profileId)
        {
            ClusterId = clusterId;
            StratumId = stratumId;
            ProfileId = profileId;
            Points = new List<ProfilePoint>();
            Centroid = new ProfilePoint(0, 0, 0, stratumId);
            ConnectDirection = new Vector3D(0, 1, 0); // 默认Y轴方向
        }

        /// <summary>
        /// 计算聚类中心
        /// </summary>
        public void CalculateCentroid()
        {
            if (Points.Count == 0) return;
            double avgX = Points.Average(p => p.X);
            double avgY = Points.Average(p => p.Y);
            double avgZ = Points.Average(p => p.Z);
            Centroid = new ProfilePoint(avgX, avgY, avgZ, StratumId, ClusterId);
        }
    }
    #endregion

    #region DBSCAN聚类实现（空间密度聚类）
    /// <summary>
    /// DBSCAN聚类算法（用于地层区域划分）
    /// </summary>
    /// <summary>
    /// DBSCAN聚类算法（用于地层区域划分）
    /// </summary>
    public class DBSCANClustering
    {
        private readonly double _epsilon;      // 邻域半径
        private readonly int _minPoints;       // 最小点数
        private int _currentClusterId;

        public DBSCANClustering(double epsilon = 5.0, int minPoints = 5)
        {
            _epsilon = epsilon;
            _minPoints = minPoints;
            _currentClusterId = 0;
        }

        /// <summary>
        /// 对指定地层的点进行聚类
        /// </summary>
        public List<StratumCluster> ClusterStratumPoints(List<ProfilePoint> points, int stratumId, int profileId)
        {
            var clusters = new List<StratumCluster>();
            // 使用字典存储点的访问状态，避免值类型的拷贝问题
            var pointStatus = new Dictionary<ProfilePoint, bool>();
            foreach (var p in points)
            {
                pointStatus[p] = false; // false=未访问，true=已访问
            }

            _currentClusterId = 0;

            foreach (var p in points)
            {
                if (pointStatus[p]) continue; // 跳过已访问的点
                pointStatus[p] = true;

                var neighbors = FindNeighbors(p, points);
                if (neighbors.Count >= _minPoints)
                {
                    var cluster = new StratumCluster(_currentClusterId, stratumId, profileId);
                    // 修复点：使用Queue来处理邻域点，避免foreach遍历中修改列表
                    var seedQueue = new Queue<ProfilePoint>(neighbors);

                    // 扩展聚类的核心逻辑（修复版）
                    ExpandClusterFixed(cluster, seedQueue, pointStatus, points);

                    cluster.CalculateCentroid();
                    clusters.Add(cluster);
                    _currentClusterId++;
                }
            }

            return clusters;
        }

        /// <summary>
        /// 修复后的扩展聚类函数
        /// </summary>
        /// <param name="cluster">当前聚类</param>
        /// <param name="seedQueue">种子点队列</param>
        /// <param name="pointStatus">点的访问状态字典</param>
        /// <param name="allPoints">所有点集合</param>
        private void ExpandClusterFixed(StratumCluster cluster, Queue<ProfilePoint> seedQueue,
                                       Dictionary<ProfilePoint, bool> pointStatus, List<ProfilePoint> allPoints)
        {
            while (seedQueue.Count > 0)
            {
                var currentPoint = seedQueue.Dequeue();

                // 跳过已处理的点
                if (pointStatus[currentPoint])
                {
                    // 如果是核心点但未加入聚类，补充加入
                    if (!cluster.Points.Any(p => p.Equals(currentPoint)))
                    {
                        var pointWithCluster = new ProfilePoint(
                            currentPoint.X, currentPoint.Y, currentPoint.Z,
                            currentPoint.Value, cluster.ClusterId
                        );
                        cluster.Points.Add(pointWithCluster);
                    }
                    continue;
                }

                // 标记为已访问
                pointStatus[currentPoint] = true;

                // 查找当前点的邻域
                var currentNeighbors = FindNeighbors(currentPoint, allPoints);

                // 如果是核心点，将其邻域加入队列
                if (currentNeighbors.Count >= _minPoints)
                {
                    foreach (var neighbor in currentNeighbors)
                    {
                        if (!pointStatus[neighbor] && !seedQueue.Contains(neighbor))
                        {
                            seedQueue.Enqueue(neighbor);
                        }
                    }
                }

                // 将当前点加入聚类
                var newPoint = new ProfilePoint(
                    currentPoint.X, currentPoint.Y, currentPoint.Z,
                    currentPoint.Value, cluster.ClusterId
                );
                cluster.Points.Add(newPoint);
            }
        }

        /// <summary>
        /// 查找邻域内的点
        /// </summary>
        private List<ProfilePoint> FindNeighbors(ProfilePoint p, List<ProfilePoint> allPoints)
        {
            var neighbors = new List<ProfilePoint>();
            foreach (var q in allPoints)
            {
                double dx = p.X - q.X;
                double dy = p.Y - q.Y;
                double dz = p.Z - q.Z;
                double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

                if (distance <= _epsilon)
                {
                    neighbors.Add(q);
                }
            }
            return neighbors;
        }
    }
    #endregion

    #region 核心插值器实现
    /// <summary>
    /// 带地层聚类和连通方向的插值器（最终版）
    /// </summary>
    public class ClusterBasedStratumInterpolator
    {
        // 核心配置
        private double _clusterEpsilon = 5.0;    // 聚类邻域半径
        private int _clusterMinPoints = 5;       // 聚类最小点数
        private readonly int _defaultK = 16;              // 默认近邻点数
        private readonly double _sampleRatio = 0.1;       // 采样比例
        private readonly double _groundTolerance = 0.1;   // 地面容差

        // 聚类结果
        private List<StratumCluster> _profileAClusters;   // 剖面A聚类结果
        private List<StratumCluster> _profileBClusters;   // 剖面B聚类结果
        private Dictionary<int, List<StratumCluster>> _stratumClusterMap; // 地层ID -> 聚类列表

        // KD-Tree（按地层分区构建）
        private Dictionary<int, DirectionalKDTree> _stratumKDTrees;
        private DirectionalKDTree _groundPointsTree;
        public void Clear() 
        {
            _profileAClusters.Clear();
            _profileBClusters.Clear();
            _stratumClusterMap.Clear();
            _stratumKDTrees.Clear();            
        }
        /// <summary>
        /// 初始化插值器
        /// </summary>
        public ClusterBasedStratumInterpolator(ProfilePoint[,] profileA, ProfilePoint[,] profileB,
                                               double clusterepsilon,int clusterminpoints)
        {
            _clusterEpsilon = clusterepsilon;
            _clusterMinPoints = clusterminpoints;
            // 1. 数据清洗：过滤无效点，展平数据
            var profileAValid = FlattenAndClean(profileA, 0);
            var profileBValid = FlattenAndClean(profileB, 1);

            // 2. 按地层聚类：划分地层区域
            _profileAClusters = ClusterProfilePoints(profileAValid, 0);
            _profileBClusters = ClusterProfilePoints(profileBValid, 1);

            // 3. 构建地层-聚类映射
            _stratumClusterMap = BuildStratumClusterMap();

            // 4. 计算地层连通方向
            CalculateStratumConnectDirections();

            // 5. 构建分层KD-Tree
            BuildStratumKDTrees(profileAValid, profileBValid);

            // 6. 构建地面点KD-Tree
            BuildGroundTree(profileAValid, profileBValid);
        }

        #region 数据预处理与聚类
        /// <summary>
        /// 数据清洗：过滤无效点，展平数组
        /// </summary>
        private List<ProfilePoint> FlattenAndClean(ProfilePoint[,] profile, int profileId)
        {
            return profile.Cast<ProfilePoint>()
                          .Where(p => p.Value >= 0)  // 过滤无效地层
                          .Where(p => !IsBoundaryInvalid(p, profile)) // 过滤边界无效点
                          .ToList();
        }

        /// <summary>
        /// 判断是否为边界无效点
        /// </summary>
        private bool IsBoundaryInvalid(ProfilePoint p, ProfilePoint[,] profile)
        {
            // 计算剖面边界范围
            var allPoints = profile.Cast<ProfilePoint>().ToList();
            double minX = allPoints.Min(pt => pt.X);
            double maxX = allPoints.Max(pt => pt.X);
            double minY = allPoints.Min(pt => pt.Y);
            double maxY = allPoints.Max(pt => pt.Y);

            // 边界阈值（剖面范围的5%）
            double xThreshold = (maxX - minX) * 0.05;
            double yThreshold = (maxY - minY) * 0.05;

            // 判断是否在边界区域且无地层
            return (p.X < minX + xThreshold || p.X > maxX - xThreshold ||
                    p.Y < minY + yThreshold || p.Y > maxY - yThreshold) &&
                   p.Value == -1;
        }

        /// <summary>
        /// 对剖面点按地层聚类
        /// </summary>
        private List<StratumCluster> ClusterProfilePoints(List<ProfilePoint> points, int profileId)
        {
            var allClusters = new List<StratumCluster>();
            var dbscan = new DBSCANClustering(_clusterEpsilon, _clusterMinPoints);

            // 按地层编号分组聚类
            foreach (var stratumGroup in points.GroupBy(p => p.Value))
            {
                int stratumId = stratumGroup.Key;
                var stratumPoints = stratumGroup.ToList();

                if (stratumPoints.Count < _clusterMinPoints) continue;

                // 对该地层进行聚类
                var stratumClusters = dbscan.ClusterStratumPoints(stratumPoints, stratumId, profileId);
                allClusters.AddRange(stratumClusters);
            }

            return allClusters;
        }

        /// <summary>
        /// 构建地层-聚类映射
        /// </summary>
        private Dictionary<int, List<StratumCluster>> BuildStratumClusterMap()
        {
            var map = new Dictionary<int, List<StratumCluster>>();
            var allClusters = _profileAClusters.Concat(_profileBClusters).ToList();

            foreach (var cluster in allClusters)
            {
                if (!map.ContainsKey(cluster.StratumId))
                    map[cluster.StratumId] = new List<StratumCluster>();
                map[cluster.StratumId].Add(cluster);
            }

            return map;
        }

        /// <summary>
        /// 计算地层连通方向（匹配两剖面同源地层聚类）
        /// </summary>
        private void CalculateStratumConnectDirections()
        {
            foreach (var kv in _stratumClusterMap)
            {
                int stratumId = kv.Key;
                var clusters = kv.Value;

                // 分离两剖面的聚类
                var profileA = clusters.Where(c => c.ProfileId == 0).ToList();
                var profileB = clusters.Where(c => c.ProfileId == 1).ToList();

                // 匹配同源地层聚类（按距离最近原则）
                foreach (var aCluster in profileA)
                {
                    StratumCluster nearestB = null;
                    double minDistance = double.MaxValue;

                    foreach (var bCluster in profileB)
                    {
                        double distance = aCluster.Centroid.DirectionalDistanceTo(bCluster.Centroid, new Vector3D(0, 1, 0));
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestB = bCluster;
                        }
                    }

                    // 计算连通方向
                    if (nearestB != null)
                    {
                        var direction = new Vector3D(
                            nearestB.Centroid.X - aCluster.Centroid.X,
                            nearestB.Centroid.Y - aCluster.Centroid.Y,
                            nearestB.Centroid.Z - aCluster.Centroid.Z
                        );
                        aCluster.ConnectDirection = direction;
                        nearestB.ConnectDirection = direction;
                    }
                }
            }
        }
        #endregion

        #region KD-Tree构建
        /// <summary>
        /// 按地层构建方向KD-Tree
        /// </summary>
        private void BuildStratumKDTrees(List<ProfilePoint> profileA, List<ProfilePoint> profileB)
        {
            _stratumKDTrees = new Dictionary<int, DirectionalKDTree>();
            var allPoints = profileA.Concat(profileB).ToList();

            // 按地层分组构建KD-Tree
            foreach (var stratumGroup in allPoints.GroupBy(p => p.Value))
            {
                int stratumId = stratumGroup.Key;
                var stratumPoints = SamplePoints(stratumGroup.ToList(), _sampleRatio);

                // 获取该地层的主连通方向
                Vector3D mainDirection = new Vector3D(0, 1, 0); // 默认Y轴
                if (_stratumClusterMap.ContainsKey(stratumId))
                {
                    var firstCluster = _stratumClusterMap[stratumId].FirstOrDefault();
                    if (firstCluster != null)
                        mainDirection = firstCluster.ConnectDirection;
                }

                // 构建该地层的方向KD-Tree
                _stratumKDTrees[stratumId] = new DirectionalKDTree(stratumPoints, mainDirection);
            }
        }

        /// <summary>
        /// 构建地面点KD-Tree
        /// </summary>
        private void BuildGroundTree(List<ProfilePoint> profileA, List<ProfilePoint> profileB)
        {
            var groundPoints = GetGroundPoints(profileA.Concat(profileB).ToList());
            groundPoints = SamplePoints(groundPoints, _sampleRatio);

            if (groundPoints.Count > 0)
            {
                _groundPointsTree = new DirectionalKDTree(groundPoints, new Vector3D(0, 1, 0));
            }
        }

        /// <summary>
        /// 数据采样
        /// </summary>
        private List<ProfilePoint> SamplePoints(List<ProfilePoint> points, double ratio)
        {
            if (points.Count <= 1000 || ratio >= 1.0)
                return points;

            int sampleCount = Math.Max(1000, (int)(points.Count * ratio));
            var sampled = new List<ProfilePoint>();
            int step = points.Count / sampleCount;

            for (int i = 0; i < points.Count; i += step)
                sampled.Add(points[i]);

            while (sampled.Count < sampleCount && sampled.Count < points.Count)
                sampled.Add(points[new Random().Next(points.Count)]);

            return sampled.Distinct().ToList();
        }

        /// <summary>
        /// 提取地面点
        /// </summary>
        private List<ProfilePoint> GetGroundPoints(List<ProfilePoint> allPoints)
        {
            var groundDict = new Dictionary<(double X, double Y), ProfilePoint>();

            foreach (ProfilePoint p in allPoints)
            {
                var key = (Math.Round(p.X, 2), Math.Round(p.Y, 2));
                if (!groundDict.ContainsKey(key) || p.Z > groundDict[key].Z)
                    groundDict[key] = p;
            }

            return groundDict.Values.ToList();
        }
        #endregion

        #region 插值核心逻辑
        /// <summary>
        /// 匹配目标点所属的地层聚类
        /// </summary>
        private int MatchStratumCluster(ProfilePoint target)
        {
            // 1. 先判断是否在地面以上
            if (IsAboveGround(target))
                return -1;

            // 2. 查找最近的地层点，确定候选地层
            int candidateStratum = -1;
            double minDistance = double.MaxValue;

            foreach (var kv in _stratumKDTrees)
            {
                var nearest = kv.Value.FindDirectionalNearest(target, 1);
                if (nearest.Count == 0) continue;

                double distance = target.DirectionalDistanceTo(nearest[0], kv.Value.StratumDirection);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    candidateStratum = kv.Key;
                }
            }

            return candidateStratum;
        }

        /// <summary>
        /// 判断是否在地面以上
        /// </summary>
        private bool IsAboveGround(ProfilePoint p)
        {
            if (_groundPointsTree == null)
                return false;

            var nearestGround = _groundPointsTree.FindDirectionalNearest(p, _defaultK);
            if (nearestGround.Count == 0)
                return false;

            double groundZ = CalculateWeightedElevation(p, nearestGround);
            return p.Z > groundZ + _groundTolerance;
        }

        /// <summary>
        /// 加权计算地面高程
        /// </summary>
        private double CalculateWeightedElevation(ProfilePoint target, List<ProfilePoint> neighbors)
        {
            double totalWeight = 0;
            double weightedZ = 0;

            foreach (var neighbor in neighbors)
            {
                double distance = target.DirectionalDistanceTo(neighbor, new Vector3D(0, 1, 0));
                if (distance < 1e-10)
                    return neighbor.Z;

                double weight = 1.0 / (distance * distance);
                weightedZ += neighbor.Z * weight;
                totalWeight += weight;
            }

            return totalWeight > 0 ? weightedZ / totalWeight : neighbors.Average(p => p.Z);
        }

        /// <summary>
        /// 最终插值方法（聚类+定向）
        /// </summary>
        public int InterpolateStratum(ProfilePoint target)
        {
            // 1. 匹配目标点所属地层
            int stratumId = MatchStratumCluster(target);
            if (stratumId == -1)return -1;

            //字典不含该地层-debug
            if (!_stratumClusterMap.ContainsKey(stratumId)) return -1;

            // 2. 获取该地层的KD-Tree和方向
            if (!_stratumKDTrees.ContainsKey(stratumId))
                return -1;

            var kdTree = _stratumKDTrees[stratumId];
            
            var direction = _stratumClusterMap[stratumId].First().ConnectDirection;

            // 3. 查找方向相关的近邻点
            var nearestPoints = kdTree.FindDirectionalNearest(target, _defaultK, 2.0, 0.5);
            if (nearestPoints.Count == 0)
                return -1;

            // 4. 方向加权投票
            int id = CalculateWeightedVote(target, nearestPoints, direction);
            return id;
        }

        /// <summary>
        /// 方向加权投票计算地层编号
        /// </summary>
        private int CalculateWeightedVote(ProfilePoint target, List<ProfilePoint> neighbors, Vector3D direction)
        {
            var vote = new Dictionary<int, double>();
            double totalWeight = 0;

            foreach (var neighbor in neighbors)
            {
                double distance = target.DirectionalDistanceTo(neighbor, direction);
                double weight = 1.0 / Math.Max(distance, 1e-10);

                if (vote.ContainsKey(neighbor.Value))
                    vote[neighbor.Value] += weight;
                else
                    vote[neighbor.Value] = weight;

                totalWeight += weight;
            }

            // 归一化并返回权重最高的地层
            foreach (var key in vote.Keys.ToList())
                vote[key] /= totalWeight;

            return vote.OrderByDescending(kv => kv.Value).First().Key;
        }

        /// <summary>
        /// 批量插值
        /// </summary>
        public int[] BatchInterpolate(List<ProfilePoint> targets)
        {
            var results = new int[targets.Count];
            System.Threading.Tasks.Parallel.For(0, targets.Count, i =>
            {
                results[i] = InterpolateStratum(targets[i]);
            });
            return results;
        }
        #endregion
    }
    #endregion

    #region 方向KD-Tree实现
    internal class KDNode
    {
        public ProfilePoint Point;
        public KDNode Left, Right;
        public int Axis;

        public KDNode(ProfilePoint point, int axis)
        {
            Point = point;
            Axis = axis;
            Left = Right = null;
        }
    }

    internal class DirectionalKDTree
    {
        private readonly KDNode _root;
        private readonly int _dimensions = 3;
        public Vector3D StratumDirection { get; }        
        public DirectionalKDTree(List<ProfilePoint> points, Vector3D stratumDirection)
        {
            StratumDirection = stratumDirection;
            _root = BuildTree(points, 0);
        }

        private KDNode BuildTree(List<ProfilePoint> points, int depth)
        {
            if (points.Count == 0) return null;

            int axis = depth % _dimensions;
            points.Sort((a, b) => ComparePoints(a, b, axis));

            int median = points.Count / 2;
            var node = new KDNode(points[median], axis);
            node.Left = BuildTree(points.Take(median).ToList(), depth + 1);
            node.Right = BuildTree(points.Skip(median + 1).ToList(), depth + 1);

            return node;
        }

        private int ComparePoints(ProfilePoint a, ProfilePoint b, int axis)
        {
            switch (axis)
            {
                case 0: return a.X.CompareTo(b.X);
                case 1: return a.Y.CompareTo(b.Y);
                case 2: return a.Z.CompareTo(b.Z);
                default: return 0;
            }           
        }

        /// <summary>
        /// 查找方向相关的最近K个点
        /// </summary>
        public List<ProfilePoint> FindDirectionalNearest(ProfilePoint target, int k,
                                                 double horizontalWeight = 2.0,
                                                 double verticalWeight = 0.5)
        {
            var nearest = new SortedList<double, ProfilePoint>();
            SearchNearest(_root, target, k, ref nearest, horizontalWeight, verticalWeight);
            return nearest.Values.Take(k).ToList();
        }

        private void SearchNearest(KDNode node, ProfilePoint target, int k,
                                  ref SortedList<double, ProfilePoint> nearest,
                                  double horizontalWeight, double verticalWeight)
        {
            if (node == null) return;

            // 方向加权距离
            double distance = target.DirectionalDistanceTo(node.Point, StratumDirection,
                                                          horizontalWeight, verticalWeight);

            // 维护最近点列表
            if (!nearest.ContainsKey(distance))
            {
                if (nearest.Count < k)
                    nearest.Add(distance, node.Point);
                else if (distance < nearest.Keys.Last())
                {
                    nearest.RemoveAt(nearest.Count - 1);
                    nearest.Add(distance, node.Point);
                }
            }

            // 搜索方向判断
            int compare = ComparePoints(target, node.Point, node.Axis);
            if (compare < 0)
            {
                SearchNearest(node.Left, target, k, ref nearest, horizontalWeight, verticalWeight);
                if (nearest.Count < k || GetAxisDistance(target, node.Point, node.Axis) < nearest.Keys.Last())
                    SearchNearest(node.Right, target, k, ref nearest, horizontalWeight, verticalWeight);
            }
            else
            {
                SearchNearest(node.Right, target, k, ref nearest, horizontalWeight, verticalWeight);
                if (nearest.Count < k || GetAxisDistance(target, node.Point, node.Axis) < nearest.Keys.Last())
                    SearchNearest(node.Left, target, k, ref nearest, horizontalWeight, verticalWeight);
            }
        }

        private double GetAxisDistance(ProfilePoint a, ProfilePoint b, int axis)
        {
            switch( axis)
            {
                case 0: return Math.Abs(a.X - b.X);
                case 1:return Math.Abs(a.Y - b.Y);
                case 2:return Math.Abs(a.Z - b.Z);
                default:return 0;
            }
        }
    }
    #endregion

    #region 测试程序
    public class TestProgram
    {
        public static void Test()
        {
            // 构造测试数据：两剖面，地层走向不一致
            var profileA = GenerateComplexProfile(0, 1000);  // 剖面A（X=0）
            var profileB = GenerateComplexProfile(10, 1000); // 剖面B（X=10）

            // 初始化插值器
            Console.WriteLine("初始化聚类插值器...");
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var interpolator = new ClusterBasedStratumInterpolator(profileA, profileB,5,5);
            watch.Stop();
            Console.WriteLine($"初始化完成，耗时：{watch.ElapsedMilliseconds}ms");

            // 测试插值
            var testPoints = new List<ProfilePoint>
            {
                new ProfilePoint(5, 250, 20, -1),  // 两剖面中间，地层1/2过渡区
                new ProfilePoint(5, 500, 30, -1),  // 地层2区域
                new ProfilePoint(5, 750, 40, -1),  // 地层3区域
                new ProfilePoint(5, 500, 80, -1)   // 地面以上
            };

            foreach (var p in testPoints)
            {
                int stratum = interpolator.InterpolateStratum(p);
                Console.WriteLine($"点{p} -> 插值地层编号：{stratum}");
            }

            // 批量测试
            var batchPoints = Enumerable.Range(0, 100).Select(i => new ProfilePoint(5, i * 10, 20, -1)).ToList();
            watch.Restart();
            var batchResults = interpolator.BatchInterpolate(batchPoints);
            watch.Stop();
            Console.WriteLine($"\n批量插值100个点耗时：{watch.ElapsedMilliseconds}ms");
            Console.WriteLine("前10个点插值结果：");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"  {batchPoints[i]} -> {batchResults[i]}");
            }
        }

        /// <summary>
        /// 生成复杂剖面数据（地层走向不一致）
        /// </summary>
        private static ProfilePoint[,] GenerateComplexProfile(double x, int count)
        {
            var points = new ProfilePoint[1, count];
            var random = new Random(42);

            for (int i = 0; i < count; i++)
            {
                double y = i * 1.0;
                double z = random.Next(0, 60);
                int stratum = -1;

                // 模拟复杂地层分布
                if (y < 300) stratum = 1;    // 0-300Y：地层1
                else if (y < 600) stratum = 2; // 300-600Y：地层2
                else stratum = 3;             // 600+Y：地层3

                // 加入局部地层突变
                if (random.Next(0, 100) < 3)
                    stratum = random.Next(1, 4);

                points[0, i] = new ProfilePoint(x, y, z, stratum);
            }

            return points;
        }
    }
    #endregion
}