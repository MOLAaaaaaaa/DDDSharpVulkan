using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DDDSharp
{
    internal static class AppLocalization
    {
        public const string EnglishCultureName = "en-US";
        public const string ChineseCultureName = "zh-CN";

        public static event EventHandler LanguageChanged;

        private static readonly Dictionary<string, string> ChineseTextMap = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["&About"] = "关于",
            ["&Analyze"] = "分析",
            ["&Create"] = "创建",
            ["&Data"] = "数据",
            ["&Default"] = "默认",
            ["&Delete"] = "删除",
            ["&Edit"] = "编辑",
            ["&File"] = "文件",
            ["&GeoObjects"] = "地质对象",
            ["&Help"] = "帮助",
            ["&Load"] = "载入",
            ["&Lights&&Material"] = "灯光与材质",
            ["&Objects"] = "对象",
            ["&OK"] = "确定",
            ["&Remove"] = "移除",
            ["&Remove All"] = "全部移除",
            ["&Reset View"] = "重置视图",
            ["&Save"] = "保存",
            ["&Save As"] = "另存为",
            ["&SHP Files"] = "SHP 文件",
            ["&Simplify"] = "简化",
            ["&Smooth"] = "平滑",
            ["&View"] = "视图",
            ["2D Outline"] = "二维轮廓",
            ["3D Engine"] = "三维引擎",
            ["3D Grid"] = "三维网格",
            ["3D Grid file"] = "三维网格文件",
            ["3D Grid file(*.3DGrid)|*.3DGrid"] = "三维网格文件(*.3DGrid)|*.3DGrid",
            ["3D Grids"] = "三维网格",
            ["3D Model"] = "三维模型",
            ["3D Surfer - data visualization"] = "3D Surfer - 数据可视化",
            ["3D Surfer Plus"] = "3D Surfer Plus",
            ["3D Surfer Plus--data visualization"] = "3D Surfer Plus--数据可视化",
            ["3D Surfer Plus--untitled project"] = "3D Surfer Plus--未命名项目",
            ["About 3D Surfer"] = "关于 3D Surfer",
            ["About 3D Surfer --Unregistered version"] = "关于 3D Surfer --未注册版本",
            ["Add"] = "添加",
            ["All Rights Reserved."] = "保留所有权利。",
            ["Apply"] = "应用",
            ["Axis options"] = "坐标轴选项",
            ["Background"] = "背景",
            ["Cancel"] = "取消",
            ["Close"] = "关闭",
            ["Color"] = "颜色",
            ["Coordinate"] = "坐标",
            ["Coordinate options"] = "坐标选项",
            ["Coordinate rotating"] = "坐标旋转",
            ["Delete Selected Objects"] = "删除选中对象",
            ["Display"] = "显示",
            ["Edit"] = "编辑",
            ["Earth mapped system"] = "地理坐标系统",
            ["Error occurred!!!"] = "发生错误！！！",
            ["Errors occurred!"] = "发生错误！",
            ["This is a unregistered version."] = "这是未注册版本。",
            ["Unreconginized register information."] = "无法识别的注册信息。",
            ["Remote verifying failed,please check the network."] = "远程验证失败，请检查网络。",
            ["Vulkan not supported,auto switch to OpenGL."] = "不支持 Vulkan，已自动切换到 OpenGL。",
            ["Recording saved to"] = "录制已保存到",
            ["Recording canceled!"] = "录制已取消！",
            ["start recording..."] = "开始录制...",
            ["Start recording"] = "开始录制",
            ["End recording"] = "结束录制",
            ["Please select an object."] = "请选择一个对象。",
            ["No models presented."] = "当前没有模型。",
            ["Loading data failed"] = "载入数据失败",
            ["Loading data failed."] = "载入数据失败。",
            ["Loading data failed !!!"] = "载入数据失败！！！",
            ["Load data failed."] = "载入数据失败。",
            ["Loading data faild."] = "载入数据失败。",
            ["Loading texture failed."] = "载入纹理失败。",
            ["Export data successfully."] = "导出数据成功。",
            ["Failed to export file."] = "导出文件失败。",
            ["Failed to export data to file."] = "导出数据到文件失败。",
            ["Failed to export data to file: "] = "导出数据到文件失败：",
            ["Failed to save data to file."] = "保存数据到文件失败。",
            ["Data exported to file: "] = "数据已导出到文件：",
            ["Data saved to file: "] = "数据已保存到文件：",
            ["Saving Changes?"] = "保存更改？",
            ["Save Changes？"] = "保存更改？",
            ["Changes have not been saved, save it?"] = "更改尚未保存，是否保存？",
            ["Changes have not been saved, save it first?"] = "更改尚未保存，是否先保存？",
            ["This product had been authorised to "] = "本产品已授权给 ",
            ["Unregistered version "] = "未注册版本",
            ["light set not correct."] = "灯光参数不正确。",
            ["File"] = "文件",
            ["Fill"] = "填充",
            ["Graphics"] = "图形设备",
            ["Help"] = "帮助",
            ["Information"] = "信息",
            ["Language"] = "语言",
            ["Load"] = "载入",
            ["Load scripts"] = "载入脚本",
            ["Loading data failed."] = "载入数据失败。",
            ["Loading texture failed."] = "载入纹理失败。",
            ["Modeling"] = "建模",
            ["Mouse Control"] = "鼠标控制",
            ["New Project"] = "新建项目",
            ["No"] = "否",
            ["OK"] = "确定",
            ["Open"] = "打开",
            ["Option"] = "设置",
            ["Objects"] = "对象",
            ["Parameters"] = "参数",
            ["position"] = "位置",
            ["Position"] = "位置",
            ["Projection"] = "投影",
            ["Quit"] = "退出",
            ["Remove"] = "移除",
            ["Reset View"] = "重置视图",
            ["Save"] = "保存",
            ["Save As"] = "另存为",
            ["Save Changes?"] = "保存更改？",
            ["Screen Recording"] = "屏幕录制",
            ["Setting"] = "设置",
            ["Shape"] = "形状",
            ["Slicer"] = "切片",
            ["Stratums Editor"] = "地层编辑器",
            ["Stratum Color Scheme"] = "地层颜色方案",
            ["Test"] = "测试",
            ["Tools"] = "工具",
            ["View"] = "视图",
            ["Warning!!!"] = "警告！！！",
            ["X Axis"] = "X 轴",
            ["Y Axis"] = "Y 轴",
            ["Z Axis"] = "Z 轴",
            ["Yes"] = "是",
            ["Loading "] = "载入",
            ["Load "] = "载入",
            ["Save "] = "保存",
            ["Delete selected objects: "] = "删除所选对象：",
            ["Delete selected objects?"] = "是否删除已选中对象？"
        };

        private static readonly Dictionary<string, string> EnglishTextMap = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["成矿空间分析"] = "Mineralization Spatial Analysis",
            ["打开文件错误！"] = "Failed to open file!",
            ["导入数据错误！"] = "Error occurred while importing data!",
            ["读取数据错误！"] = "Error occurred while reading data!",
            ["保存数据错误！"] = "Error occurred while saving data!",
            ["参数无效！"] = "Invalid parameters!",
            ["采样完成，采样点数："] = "Sampling completed, sample count: ",
            ["采样完成，采样点数"] = "Sampling completed, sample count: ",
            ["删除对象："] = "Delete object: ",
            ["是否删除该切片对象？"] = "Delete this slicer object?",
            ["无有效采样点数据！"] = "No valid sample data.",
            ["数据输出成功！"] = "Data exported successfully!",
            ["是否删除已选中对象？"] = "Delete selected objects?",
            ["数据已经修改，是否保存已修改数据？"] = "Data has been modified, save changes?",
            ["是否保存数据？"] = "Save data?",
            ["图片导出成功！"] = "Image exported successfully!",
            ["图片导出失败！"] = "Image export failed!",
            ["第0页/共100页"] = "Page 0 / 100",
            ["Latitude"] = "Latitude",
            ["Longitude"] = "Longitude",
            ["Earth Coordinate"] = "Earth Coordinate",
            ["Image Location"] = "Image Location",
            ["Model Boundaries"] = "Model Boundaries",
            ["Auto Calculating"] = "Auto Calculate",
            ["Create Polygon"] = "Create Polygon",
            ["Radiu"] = "Radius",
            ["Length"] = "Length",
            ["Grid Info"] = "Grid Info",
            ["Grid Information"] = "Grid Information",
            ["Column Select"] = "Column Select",
            ["Column Selection"] = "Column Selection",
            ["Earth Coordinates"] = "Earth Coordinates",
            ["Encoding"] = "Encoding",
            ["Value:"] = "Value:",
            ["Null value"] = "Null value",
            ["File Path"] = "File Path",
            ["Information"] = "Information",
            ["Interval"] = "Interval",
            ["Rotate X"] = "Rotate X",
            ["Zoom Speed"] = "Zoom Speed",
            ["Normal"] = "Normal",
            ["Slow"] = "Slow",
            ["Fast"] = "Fast",
            ["Distance Filter"] = "Distance Filter",
            ["Redundant Filter"] = "Redundant Filter",
            ["Delays ( ms )"] = "Delay (ms)",
            ["Ascent"] = "Ascent",
            ["Descent"] = "Descent",
            ["Player Options"] = "Player Options",
            ["Target Range"] = "Target Range",
            ["Axis Select"] = "Axis Select",
            ["Browse"] = "Browse",
            ["Gridded"] = "Gridded",
            ["Txt"] = "Text",
            ["Overlap"] = "Overlap",
            ["Functions"] = "Functions",
            ["Volume Statics"] = "Volume Statistics",
            ["Multiple Properties ISO Surfaces"] = "Multiple Properties ISO Surfaces",
            ["Extract"] = "Extract",
            ["Properties"] = "Properties",
            ["Closed Values"] = "Closed Values",
            ["Create from color levels"] = "Create from color levels",
            ["New"] = "New",
            ["Delete"] = "Delete",
            ["Combine"] = "Combine",
            ["Device details"] = "Device details",
            ["Driver Version"] = "Driver Version",
            ["Installed Driver"] = "Installed Driver",
            ["Memory Size"] = "Memory Size",
            ["Device Name"] = "Device Name",
            ["Graphics Device"] = "Graphics Device",
            ["Graphics Engine"] = "Graphics Engine",
            ["OpenGL details"] = "OpenGL details",
            ["Sources"] = "Sources",
            ["Targets List"] = "Targets List",
            ["Current"] = "Current",
            ["KeepWith"] = "Keep With",
            ["Unselect All"] = "Unselect All",
            ["Select All"] = "Select All",
            ["Create Slicer"] = "Create Slicer",
            ["Slicers"] = "Slicers",
            ["Slicer Plan"] = "Slicer Plan",
            ["Axis Slicer"] = "Axis Slicer",
            ["Draw Slicer"] = "Draw Slicer",
            ["Load from"] = "Load from",
            ["Add new"] = "Add new",
            ["Insert"] = "Insert",
            ["Before"] = "Before",
            ["After"] = "After",
            ["New slicer name"] = "New slicer name",
            ["Smooth"] = "Smooth",
            ["closed"] = "Closed",
            ["Save"] = "Save",
            ["Export"] = "Export",
            ["Load"] = "Load",
            ["Import"] = "Import",
            ["Close"] = "Close",
            ["Start"] = "Start",
            ["Update"] = "Update",
            ["Remove"] = "Remove",
            ["Cancel"] = "Cancel",
            ["OK"] = "OK"
        };

        private static readonly Dictionary<string, string> ChineseUiFallbackMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CANCEL"] = "取消",
            ["Re&name"] = "重命名",
            ["Filter"] = "过滤",
            ["Zoom Out"] = "缩小",
            ["Zoom In"] = "放大",
            ["Pan Screen"] = "平移屏幕",
            ["Select tool"] = "选择工具",
            ["Refresh"] = "刷新",
            ["Create"] = "创建",
            ["Options"] = "选项",
            ["Location"] = "位置",
            ["&Editor"] = "编辑器",
            ["Move Object"] = "移动对象",
            ["Polygon"] = "多边形",
            ["Replace"] = "替换",
            ["Fill Tool"] = "填充工具",
            ["Num"] = "数量",
            ["Gridding Geometry"] = "网格几何",
            ["Save &As"] = "另存为",
            ["Spacing"] = "间距",
            ["up"] = "上",
            ["Down"] = "下",
            ["Stop"] = "停止",
            [" Reset view to fit screen"] = "重置视图以适配屏幕",
            ["Reset view to fit screen"] = "重置视图以适配屏幕",
            ["Resample"] = "重采样",
            ["Line"] = "线",
            ["&Snap"] = "吸附",
            ["&Trace"] = "描迹",
            ["DataRange"] = "数据范围",
            ["&Draw"] = "绘制",
            ["Convert"] = "转换",
            ["Pause"] = "暂停",
            ["percentages"] = "百分比",
            ["log"] = "日志",
            ["Compute Devices"] = "计算设备",
            ["XNum"] = "X数量",
            ["YNum"] = "Y数量",
            ["Output Grid file"] = "输出网格文件",
            ["Show Layers"] = "显示图层",
            ["Show Property Grid"] = "显示属性网格",
            ["End"] = "结束",
            ["original x size"] = "原始X尺寸",
            ["Sampling"] = "采样",
            ["Show Location"] = "显示位置",
            ["Outlines"] = "轮廓",
            ["Graphics Objects"] = "图形对象",
            ["&Insert before"] = "前插入",
            ["aplly"] = "应用",
            ["Show Grid Point"] = "显示网格点",
            ["Vertical"] = "垂直",
            ["Horizontal"] = "水平",
            ["by"] = "按",
            ["hide unvisible"] = "隐藏不可见项",
            ["Show Background"] = "显示背景",
            ["First"] = "首项",
            ["Clear"] = "清空",
            ["Smooth Line"] = "平滑线",
            ["time left(s)"] = "剩余时间(秒)",
            ["Value"] = "值",
            ["Images"] = "图像",
            ["Geological Coordinates"] = "地质坐标",
            ["Slicer property"] = "切片属性",
            ["&ToUnclosed/Closed"] = "切换开闭合",
            ["Rendering"] = "渲染",
            ["Thickness"] = "厚度",
            ["YOZ"] = "YOZ",
            ["XOY"] = "XOY",
            ["XOZ"] = "XOZ",
            ["Export to"] = "导出到",
            ["Interpolation"] = "插值",
            ["Destination"] = "目标",
            ["Space"] = "空间",
            ["Grid Sample"] = "网格采样",
            ["Radius"] = "半径",
            ["Current 3D Engine"] = "当前3D引擎",
            ["Intersected Objects"] = "相交对象",
            ["InLine Num(y)"] = "内联数量(y)",
            ["Load From"] = "从文件加载",
            ["method"] = "方法",
            ["Type"] = "类型",
            ["Meshed Layers--from top to bottom"] = "网格层（从上到下）",
            ["Slicers Sampling Step"] = "切片采样步长",
            ["Source Column"] = "源列",
            ["Cartesian Coordinates"] = "笛卡尔坐标",
            ["x range"] = "X范围",
            ["y range"] = "Y范围",
            ["z range"] = "Z范围",
            ["Depth"] = "深度",
            ["PLY property"] = "PLY属性",
            ["V"] = "值",
            ["Hight"] = "高度",
            ["Meshes Sorted by Z values Asc"] = "网格按Z值升序",
            ["Restricted interfaces"] = "约束界面",
            ["Play"] = "播放",
            ["Reload"] = "重新加载",
            ["Mesh Grid Num"] = "网格网格数量",
            ["Do Overlap"] = "执行重叠",
            ["Export "] = "导出",
            ["alpha"] = "透明度",
            ["stepX"] = "X步长",
            ["stepY"] = "Y步长",
            ["X STEP"] = "X步长",
            ["Y STEP"] = "Y步长",
            ["Axis"] = "坐标轴",
            ["Image"] = "图像",
            ["Layers Triming"] = "图层裁剪",
            ["Transform"] = "变换",
            ["CrossLine Num(x)"] = "横线数量(x)",
            ["FloatNumber"] = "浮点精度",
            ["Stratum Objects"] = "地层对象",
            ["Gridding"] = "网格化",
            ["target"] = "目标",
            ["to Polygon2D"] = "到二维多边形",
            ["New Folder"] = "新建文件夹",
            ["Gridded Data"] = "网格化数据",
            ["current position"] = "当前位置",
            ["Boreholes Property"] = "钻孔属性",
            ["browse"] = "浏览",
            ["3DGrid property"] = "3D网格属性",
            ["Matching"] = "匹配",
            ["Ignore First Row"] = "忽略首行",
            ["Boreholes"] = "钻孔",
            ["From &Grid2D"] = "来自二维网格",
            ["&Import"] = "导入",
            ["Analyze"] = "分析",
            ["griding"] = "网格化",
            ["Gridding Method"] = "网格化方法",
            ["Memory Required"] = "所需内存",
            ["Transform To"] = "转换到",
            ["to Traced Polygon"] = "到描迹多边形",
            ["strip"] = "带状",
            ["By pixels"] = "按像素",
            ["GEO File"] = "GEO文件",
            ["Overlay Analysis"] = "叠加分析",
            ["BoreholeListForm"] = "钻孔列表",
            ["BoreholeWellDataForm"] = "钻孔井数据",
            ["BoreholesSampleForm"] = "钻孔采样",
            ["BoreholesCurvesInterpolationForm"] = "钻孔曲线插值",
            ["CoordsRotatingForm"] = "坐标旋转",
            ["CoordinateConvertingForm"] = "坐标转换",
            ["AxisLableForm"] = "坐标轴标签",
            ["ClosedValuesForm"] = "封闭值",
            ["ColorScaleForm"] = "色标",
            ["ColorScaleEditForm"] = "色标编辑",
            ["ColorPickerDlg"] = "颜色选择器",
            ["CreateMeshBufferForm"] = "创建网格缓冲",
            ["CreateMeshLayersFrom"] = "创建网格图层",
            ["CSlicerPropertyForm"] = "切片属性",
            ["CrossValidationForm"] = "交叉验证",
            ["C2DSlicerViewer"] = "二维切片查看器",
            ["C3DLinePropertyForm"] = "三维线属性",
            ["CModelRangeForm"] = "模型范围",
            ["CValueDistribution"] = "值分布",
            ["ConvertImageToEarthForm"] = "图像转地理坐标",
            ["CylinderIntersectForm"] = "圆柱求交",
            ["GridsOverlapForm"] = "网格重叠",
            ["PlyPropertyForm"] = "PLY属性",
            ["TracedObjectPropertyForm"] = "描迹对象属性",
            ["TestForm"] = "测试窗体",
            ["Drawing2D"] = "二维绘图",
            ["&Add"] = "添加",
            ["&Add new"] = "新增",
            ["&Add Traces"] = "添加描迹",
            ["&Cancel"] = "取消",
            ["&Color Picker"] = "颜色选择器",
            ["&Cutting"] = "切割",
            ["&Data Range"] = "数据范围",
            ["&Delete Grid Point"] = "删除网格点",
            ["&Delete Selected Objects"] = "删除选中对象",
            ["&Distribution"] = "分布",
            ["&Draw Curve"] = "绘制曲线",
            ["&DXF Files"] = "DXF文件",
            ["&Export"] = "导出",
            ["&Export Color Level"] = "导出色阶",
            ["&Export Image"] = "导出图像",
            ["&Export to file"] = "导出到文件",
            ["&Formatted File"] = "格式化文件",
            ["&Formatted Line Files"] = "格式化线文件",
            ["&Formatted Slicer File"] = "格式化切片文件",
            ["&Geological Profile"] = "地质剖面",
            ["&Geophysic profile"] = "地球物理剖面",
            ["&Geophysical Welling"] = "地球物理测井",
            ["&Graphic Device"] = "图形设备",
            ["&Import from file"] = "从文件导入",
            ["&Import Image"] = "导入图像",
            ["&Inclines"] = "倾角",
            ["&Interpolation"] = "插值",
            ["&Intersect With"] = "与...求交",
            ["&Load from"] = "从...加载",
            ["&Load Slicers"] = "加载切片",
            ["&Mouse Control"] = "鼠标控制",
            ["&New Line"] = "新建线",
            ["&New Project"] = "新建项目",
            ["&New Stratum"] = "新建地层",
            ["&Open Project"] = "打开项目",
            ["&Property Edit"] = "属性编辑",
            ["&Refresh"] = "刷新",
            ["&Reverse"] = "反转",
            ["&Scattered points"] = "散点",
            ["&Smoothing Line"] = "平滑线",
            ["&Strata"] = "地层",
            ["&Stratums"] = "地层",
            ["&Text Picker"] = "文本选择器",
            ["&Unselect All"] = "取消全选",
            ["3DGrid data"] = "3D网格数据",
            ["3DGrid Properties"] = "3D网格属性",
            ["2D Meshes"] = "二维网格",
            ["3D OBJECTS"] = "三维对象",
            ["3D Shape"] = "三维形体",
            ["Add Columns"] = "添加列",
            ["Add Slicers"] = "添加切片",
            ["AdgeDetecting"] = "边缘检测",
            ["ambient"] = "环境光",
            ["angle"] = "角度",
            ["Anticlockwise"] = "逆时针",
            ["as"] = "作为",
            ["As New"] = "作为新对象",
            ["As pixels"] = "按像素",
            ["As Same"] = "保持一致",
            ["Back"] = "后退",
            ["Backgound sample"] = "背景采样",
            ["Background Sampling"] = "背景采样",
            ["Background value"] = "背景值",
            ["Baseline from Coordinates"] = "从坐标生成基线",
            ["BigNumber"] = "大数值",
            ["Blank"] = "空白",
            ["blank "] = "空白",
            ["blue"] = "蓝色",
            ["Border &Tracing"] = "边界与描迹",
            ["Border keeping"] = "保留边界",
            ["Border Sampling"] = "边界采样",
            ["Borehole Properties"] = "钻孔属性",
            ["BoreholeCurvesForm"] = "钻孔曲线",
            ["Boreholes Slicer"] = "钻孔切片",
            ["BoreholesInclinesEditor"] = "钻孔倾角编辑",
            ["BottomLeft"] = "左下",
            ["BottomRight"] = "右下",
            ["Box"] = "盒体",
            ["BringTo"] = "置于",
            ["Buffer Files"] = "缓冲文件",
            ["buffer geometry"] = "缓冲几何",
            ["Buffer Size"] = "缓冲区大小",
            ["Buffers Overlap"] = "缓冲区重叠",
            ["BuffersOverlapForm"] = "缓冲区重叠",
            ["Center Position"] = "中心位置",
            ["centor x"] = "中心X",
            ["centor y"] = "中心Y",
            ["Central"] = "中央",
            ["centre"] = "中心",
            ["Centre Line"] = "中心线",
            ["ChangeOverlapColor"] = "修改重叠颜色",
            ["Channel"] = "通道",
            ["Checked Layers"] = "选中图层",
            ["Choose"] = "选择"
        };

        private static readonly Dictionary<string, string> ChineseWordMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["add"] = "添加",
            ["new"] = "新建",
            ["load"] = "加载",
            ["import"] = "导入",
            ["export"] = "导出",
            ["save"] = "保存",
            ["open"] = "打开",
            ["close"] = "关闭",
            ["delete"] = "删除",
            ["remove"] = "移除",
            ["refresh"] = "刷新",
            ["update"] = "更新",
            ["start"] = "开始",
            ["stop"] = "停止",
            ["pause"] = "暂停",
            ["play"] = "播放",
            ["clear"] = "清空",
            ["cancel"] = "取消",
            ["ok"] = "确定",
            ["yes"] = "是",
            ["no"] = "否",
            ["option"] = "选项",
            ["options"] = "选项",
            ["setting"] = "设置",
            ["settings"] = "设置",
            ["editor"] = "编辑器",
            ["edit"] = "编辑",
            ["color"] = "颜色",
            ["picker"] = "选择器",
            ["text"] = "文本",
            ["value"] = "值",
            ["values"] = "值",
            ["range"] = "范围",
            ["position"] = "位置",
            ["location"] = "位置",
            ["path"] = "路径",
            ["file"] = "文件",
            ["files"] = "文件",
            ["folder"] = "文件夹",
            ["image"] = "图像",
            ["images"] = "图像",
            ["object"] = "对象",
            ["objects"] = "对象",
            ["property"] = "属性",
            ["properties"] = "属性",
            ["grid"] = "网格",
            ["grids"] = "网格",
            ["gridding"] = "网格化",
            ["mesh"] = "网格",
            ["meshes"] = "网格",
            ["line"] = "线",
            ["lines"] = "线",
            ["polygon"] = "多边形",
            ["slicer"] = "切片",
            ["slicers"] = "切片",
            ["sample"] = "采样",
            ["sampling"] = "采样",
            ["interpolation"] = "插值",
            ["intersect"] = "求交",
            ["overlap"] = "重叠",
            ["analysis"] = "分析",
            ["analyze"] = "分析",
            ["coordinate"] = "坐标",
            ["coordinates"] = "坐标",
            ["convert"] = "转换",
            ["projection"] = "投影",
            ["rotate"] = "旋转",
            ["rotating"] = "旋转",
            ["axis"] = "轴",
            ["mouse"] = "鼠标",
            ["control"] = "控制",
            ["device"] = "设备",
            ["devices"] = "设备",
            ["engine"] = "引擎",
            ["graphics"] = "图形",
            ["background"] = "背景",
            ["trace"] = "描迹",
            ["snap"] = "吸附",
            ["draw"] = "绘制",
            ["outline"] = "轮廓",
            ["outlines"] = "轮廓",
            ["stratum"] = "地层",
            ["stratums"] = "地层",
            ["strata"] = "地层",
            ["borehole"] = "钻孔",
            ["boreholes"] = "钻孔",
            ["curve"] = "曲线",
            ["curves"] = "曲线",
            ["depth"] = "深度",
            ["target"] = "目标",
            ["source"] = "源",
            ["method"] = "方法",
            ["type"] = "类型",
            ["mode"] = "模式",
            ["info"] = "信息",
            ["information"] = "信息",
            ["warning"] = "警告",
            ["error"] = "错误",
            ["default"] = "默认",
            ["current"] = "当前",
            ["from"] = "从",
            ["to"] = "到",
            ["by"] = "按",
            ["with"] = "与",
            ["and"] = "和",
            ["x"] = "X",
            ["y"] = "Y",
            ["z"] = "Z",
            ["form"] = "窗体",
            ["dlg"] = "对话框",
            ["viewer"] = "查看器"
        };

        private static readonly Regex WordRegex = new Regex(@"[A-Za-z][A-Za-z0-9]*", RegexOptions.Compiled);
        private static readonly Regex CamelSplitRegex = new Regex(@"(?<!^)(?=[A-Z])", RegexOptions.Compiled);

        public static string CurrentCultureName => NormalizeCultureName(Properties.Settings.Default.UiLanguage);

        public static bool IsChinese => string.Equals(CurrentCultureName, ChineseCultureName, StringComparison.OrdinalIgnoreCase);

        public static CultureInfo CurrentCulture => CultureInfo.GetCultureInfo(CurrentCultureName);

        public static void ApplySavedLanguage()
        {
            ApplyCulture(CurrentCultureName, false);
        }

        public static void InstallAutoLocalization()
        {
            Application.Idle -= Application_Idle;
            Application.Idle += Application_Idle;
        }

        public static void SetLanguage(string cultureName)
        {
            string normalized = NormalizeCultureName(cultureName);
            if (string.Equals(Properties.Settings.Default.UiLanguage, normalized, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Thread.CurrentThread.CurrentUICulture.Name, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Properties.Settings.Default.UiLanguage = normalized;
            Properties.Settings.Default.Save();
            ApplyCulture(normalized, true);
        }

        public static string Translate(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (IsChinese)
            {
                if (ChineseTextMap.TryGetValue(text, out string translated))
                {
                    return translated;
                }

                if (ChineseUiFallbackMap.TryGetValue(text, out translated))
                {
                    return translated;
                }

                string trimmedText = text.Trim();
                if (!string.Equals(trimmedText, text, StringComparison.Ordinal) &&
                    ChineseUiFallbackMap.TryGetValue(trimmedText, out translated))
                {
                    return translated;
                }

                if (text.IndexOf('&') >= 0)
                {
                    string plain = text.Replace("&", string.Empty);
                    if (ChineseUiFallbackMap.TryGetValue(plain, out translated))
                    {
                        return text.StartsWith("&", StringComparison.Ordinal) ? "&" + translated : translated;
                    }
                }

                string translatedByWord = TranslateAsciiTextByWords(text);
                if (!string.Equals(translatedByWord, text, StringComparison.Ordinal))
                {
                    return translatedByWord;
                }

                if (text.StartsWith("Initiating Vulkan device failed.\n", StringComparison.OrdinalIgnoreCase))
                {
                    return "初始化 Vulkan 设备失败。\n" + text.Substring("Initiating Vulkan device failed.\n".Length);
                }
                if (text.StartsWith("Initiating OpenGL device failed.\n", StringComparison.OrdinalIgnoreCase))
                {
                    return "初始化 OpenGL 设备失败。\n" + text.Substring("Initiating OpenGL device failed.\n".Length);
                }
                if (text.StartsWith("Initiating OpenGLES device failed.\n", StringComparison.OrdinalIgnoreCase))
                {
                    return "初始化 OpenGLES 设备失败。\n" + text.Substring("Initiating OpenGLES device failed.\n".Length);
                }
                if (text.StartsWith("Initiating OpenGL failed.\n", StringComparison.OrdinalIgnoreCase))
                {
                    return "初始化 OpenGL 失败。\n" + text.Substring("Initiating OpenGL failed.\n".Length);
                }
                if (text.StartsWith("Load texture failed.\n", StringComparison.OrdinalIgnoreCase))
                {
                    return "加载纹理失败。\n" + text.Substring("Load texture failed.\n".Length);
                }
                if (text.StartsWith("Out of memory while rendering meshes: ", StringComparison.OrdinalIgnoreCase))
                {
                    return "渲染网格时内存不足：" + text.Substring("Out of memory while rendering meshes: ".Length);
                }
                if (text.StartsWith("Triangulating failed of ", StringComparison.OrdinalIgnoreCase))
                {
                    return "三角化失败：" + text.Substring("Triangulating failed of ".Length);
                }
                if (text.StartsWith("Writing to temporary files failed.", StringComparison.OrdinalIgnoreCase))
                {
                    return "写入临时文件失败。" + text.Substring("Writing to temporary files failed.".Length);
                }
                if (text.StartsWith("Recording stopped because of errors.", StringComparison.OrdinalIgnoreCase))
                {
                    return "录制因错误中断。" + text.Substring("Recording stopped because of errors.".Length);
                }
                if (text.StartsWith("No active screen recording.", StringComparison.OrdinalIgnoreCase))
                {
                    return "当前没有正在进行的屏幕录制。";
                }
                if (text.StartsWith("Recording failed! ", StringComparison.OrdinalIgnoreCase))
                {
                    return "录制失败！ " + text.Substring("Recording failed! ".Length);
                }
                if (text.StartsWith("Doing function ", StringComparison.OrdinalIgnoreCase))
                {
                    return "正在执行函数 " + text.Substring("Doing function ".Length);
                }
                if (text.StartsWith("Scripts interpreter started...", StringComparison.OrdinalIgnoreCase))
                {
                    return "脚本解释器已启动...";
                }
                if (text.StartsWith("Load grid data successfully.", StringComparison.OrdinalIgnoreCase))
                {
                    return "网格数据加载成功。" + text.Substring("Load grid data successfully.".Length);
                }
                if (text.StartsWith("Loading data failed.", StringComparison.OrdinalIgnoreCase))
                {
                    return "载入数据失败。" + text.Substring("Loading data failed.".Length);
                }
                if (text.StartsWith("Loading texture failed.", StringComparison.OrdinalIgnoreCase))
                {
                    return "载入纹理失败。" + text.Substring("Loading texture failed.".Length);
                }
                if (text.StartsWith("Project saved to ", StringComparison.OrdinalIgnoreCase))
                {
                    return "项目已保存到 " + text.Substring("Project saved to ".Length);
                }
                if (text.StartsWith("Failed to save project.", StringComparison.OrdinalIgnoreCase))
                {
                    return "项目保存失败。" + text.Substring("Failed to save project.".Length);
                }
                if (text.StartsWith("Failed to export data to file: ", StringComparison.OrdinalIgnoreCase))
                {
                    return "导出数据到文件失败：" + text.Substring("Failed to export data to file: ".Length);
                }
                if (text.StartsWith("Failed to save to file .", StringComparison.OrdinalIgnoreCase))
                {
                    return "保存到文件失败。" + text.Substring("Failed to save to file .".Length);
                }
            }
            else
            {
                if (EnglishTextMap.TryGetValue(text, out string translated))
                {
                    return translated;
                }

                if (text.StartsWith("打开文件错误！", StringComparison.Ordinal))
                {
                    return "Failed to open file!" + text.Substring("打开文件错误！".Length);
                }

                if (text.StartsWith("导入数据错误！", StringComparison.Ordinal))
                {
                    return "Error occurred while importing data!" + text.Substring("导入数据错误！".Length);
                }

                if (text.StartsWith("读取数据错误！", StringComparison.Ordinal))
                {
                    return "Error occurred while reading data!" + text.Substring("读取数据错误！".Length);
                }

                if (text.StartsWith("保存数据错误！", StringComparison.Ordinal))
                {
                    return "Error occurred while saving data!" + text.Substring("保存数据错误！".Length);
                }

                if (text.StartsWith("采样完成，采样点数", StringComparison.Ordinal))
                {
                    return "Sampling completed, sample count: " + text.Substring("采样完成，采样点数".Length).TrimStart('：', ':', ' ');
                }

                if (text.StartsWith("删除对象：", StringComparison.Ordinal))
                {
                    return "Delete object: " + text.Substring("删除对象：".Length);
                }
            }

            if (text.StartsWith("Light", StringComparison.OrdinalIgnoreCase) && int.TryParse(text.Substring(5), out int lightIndex))
            {
                return IsChinese
                    ? "灯光" + lightIndex.ToString(CultureInfo.InvariantCulture)
                    : "Light " + lightIndex.ToString(CultureInfo.InvariantCulture);
            }

            return text;
        }

        private static string TranslateAsciiTextByWords(string text)
        {
            if (text.IndexOf('\\') >= 0 || text.IndexOf('/') >= 0)
            {
                return text;
            }

            bool replacedAny = false;
            string replaced = WordRegex.Replace(text, match =>
            {
                string token = match.Value;
                if (ChineseWordMap.TryGetValue(token, out string direct))
                {
                    replacedAny = true;
                    return direct;
                }

                string[] parts = CamelSplitRegex
                    .Replace(token.Replace("_", " "), " ")
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length <= 1)
                {
                    return token;
                }

                bool changed = false;
                for (int i = 0; i < parts.Length; i++)
                {
                    if (ChineseWordMap.TryGetValue(parts[i], out string mapped))
                    {
                        parts[i] = mapped;
                        changed = true;
                    }
                }

                if (!changed)
                {
                    return token;
                }

                replacedAny = true;
                return string.Concat(parts);
            });

            return replacedAny ? replaced : text;
        }

        public static void ApplyToOpenForms()
        {
            foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
            {
                ApplyToControlTree(form);
            }
        }

        public static void ApplyToControlTree(Control root)
        {
            if (root == null)
            {
                return;
            }

            root.Text = Translate(root.Text);

            if (root is MenuStrip menuStrip)
            {
                ApplyToToolStripItems(menuStrip.Items);
            }
            else if (root is ToolStrip toolStrip)
            {
                ApplyToToolStripItems(toolStrip.Items);
            }

            foreach (Control child in root.Controls)
            {
                ApplyToControlTree(child);
            }
        }

        public static void ApplyToToolStripItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                item.Text = Translate(item.Text);
                if (!string.IsNullOrEmpty(item.ToolTipText))
                {
                    item.ToolTipText = Translate(item.ToolTipText);
                }
                if (item is ToolStripDropDownItem dropDown)
                {
                    ApplyToToolStripItems(dropDown.DropDownItems);
                }
            }
        }

        public static string NormalizeCultureName(string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                return EnglishCultureName;
            }

            if (cultureName.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            {
                return ChineseCultureName;
            }

            return EnglishCultureName;
        }

        private static void ApplyCulture(string cultureName, bool raiseEvent)
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(NormalizeCultureName(cultureName));
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            Resource1.Culture = culture;
            Properties.Resources.Culture = culture;

            ApplyToOpenForms();

            if (raiseEvent)
            {
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            ApplyToOpenForms();
        }
    }
}
