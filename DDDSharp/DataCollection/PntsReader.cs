using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{
    /// <summary>
    /// PNTS点云读取器
    /// </summary>
    public class PntsPointCloudReader
    {
        /// <summary>
        /// 读取PNTS文件并返回点云坐标数组
        /// </summary>
        /// <param name="filePath">PNTS文件路径</param>
        /// <returns>三维点坐标数组（Vector32类型）</returns>
        public List<Vector32> ReadPntsFile(string filePath)
        {
            // 校验文件是否存在
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("PNTS文件不存在", filePath);
            }

            List<Vector32> pointList = new List<Vector32>();

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                // 1. 解析PNTS文件头（共28字节）
                // 魔数（4字节）：必须为"pnts"
                byte[] magicBytes = br.ReadBytes(4);
                string magic = System.Text.Encoding.ASCII.GetString(magicBytes);
                if (magic != "pnts")
                {
                    throw new FormatException("不是有效的PNTS文件（魔数校验失败）");
                }

                // 版本号（4字节）：通常为1
                uint version = br.ReadUInt32();
                if (version != 1)
                {
                    throw new NotSupportedException($"不支持的PNTS版本：{version}（仅支持版本1）");
                }
                uint byteLength = br.ReadUInt32();  //瓦片文件大小
                // Feature Table JSON字节长度（4字节）
                uint featureTableJsonByteLength = br.ReadUInt32();
                // Feature Table字节长度（4字节）
                uint featureTableBinaryByteLength = br.ReadUInt32();
                // Feature Table JSON字节长度（4字节）
                uint batchTableJsonByteLength = br.ReadUInt32();
                // Feature Table字节长度（4字节）
                uint batchTableBinaryByteLength = br.ReadUInt32();


                // 2. 解析Feature Table JSON（点云元数据）                
                byte[] featureTableJsonBytes = br.ReadBytes((int)featureTableJsonByteLength);
                string featureTableJson = System.Text.Encoding.UTF8.GetString(featureTableJsonBytes, 0, (int)featureTableJsonByteLength);

                // 解析JSON获取点数量和坐标类型（简化版：仅处理POSITION为FLOAT32的情况）
                // 实际生产环境建议使用Newtonsoft.Json解析JSON，此处简化为字符串提取核心参数
                int pointCount = ExtractPointCountFromJson(featureTableJson);
                if (pointCount <= 0)
                {
                    throw new InvalidDataException("PNTS文件中未找到有效点数量");
                }

                // 3. 解析Feature Table二进制数据（核心坐标数据）
                // 跳过Batch Table（如果存在，此处仅关注坐标，暂不处理）
                // 计算Feature Table二进制数据起始位置
                long featureTableBinaryStart = 28 + featureTableJsonByteLength;
                fs.Seek(featureTableBinaryStart, SeekOrigin.Begin);

                // 读取POSITION字段（三维坐标，FLOAT32类型，每个点12字节）
                int bytesPerPoint = 12; // 3个FLOAT32（X/Y/Z各4字节）
                byte[] positionBytes = br.ReadBytes(pointCount * bytesPerPoint);

                // 4. 解析字节数据为三维坐标
                for (int i = 0; i < pointCount; i++)
                {
                    // 计算当前点的起始索引
                    int startIndex = i * bytesPerPoint;
                    // 读取X/Y/Z（小端序）
                    float x = BitConverter.ToSingle(positionBytes, startIndex);
                    float y = BitConverter.ToSingle(positionBytes, startIndex + 4);
                    float z = BitConverter.ToSingle(positionBytes, startIndex + 8);
                    pointList.Add(new Vector32(x, y, z));
                }
            }

            return pointList;
        }

        /// <summary>
        /// 从Feature Table JSON中提取点数量（简化版）
        /// 生产环境建议使用Newtonsoft.Json解析
        /// </summary>
        /// <param name="json">Feature Table JSON字符串</param>
        /// <returns>点数量</returns>
        private static int ExtractPointCountFromJson(string json)
        {
            // 简化提取：匹配"POINTS_LENGTH":数字 格式
            string key = "\"POINTS_LENGTH\":";
            int keyIndex = json.IndexOf(key);
            if (keyIndex == -1)
            {
                return 0;
            }

            int start = keyIndex + key.Length;
            int end = json.IndexOf(',', start);
            if (end == -1)
            {
                end = json.IndexOf('}', start);
            }

            string countStr = json.Substring(start, end - start).Trim();
            if (int.TryParse(countStr, out int count))
            {
                return count;
            }

            return 0;
        }

        /// <summary>
        /// 将长度补齐到8字节对齐（PNTS格式要求）
        /// </summary>
        /// <param name="length">原始长度</param>
        /// <returns>对齐后的长度</returns>
        private static long AlignTo8Bytes(ulong length)
        {
            long mod = (long)length % 8;
            return mod == 0 ? (long)length : (long)length + (8 - mod);
        }       
        
    }
}
