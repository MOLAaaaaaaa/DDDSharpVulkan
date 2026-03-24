using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace DataCollection
{
    #region 1. 箭头样式枚举
    public enum ArrowStyle
    {
        None = 0,   // 仅线段（与箭头箭杆等长）
        Left = 1,   // 左箭头+箭杆线
        Right = 2,   // 右箭头+箭杆线
        Both = 3,   // 左右箭头+箭杆线
    }
    #endregion

    #region 2. 自定义ComboBox（等长线段+纯图形）
    [DefaultProperty("CurrentArrowStyle")]
    [TypeConverter(typeof(ArrowComboBoxTypeConverter))]
    public class ArrowComboBox : ComboBox
    {
        // 核心属性
        private ArrowStyle _arrowStyle = ArrowStyle.Right;
        private Color _arrowColor = Color.Black;
        private int _arrowAreaWidth = 40;
        private int _arrowSize = 16;
        private int _dropdownArrowSize = 8;
        private int _dropdownRodLength = 15; // 所有样式共用该长度
        private int _arrowLineThickness = 1;
        private float _rodRatio = 0.6f;

        #region 仅暴露箭头相关公共属性
        [Category("箭头样式")]
        [DisplayName("箭头样式")]
        [Description("None=仅线段|Left=左箭头|Right=右箭头|Both=双向箭头")]
        [DefaultValue(ArrowStyle.Right)]
        [TypeConverter(typeof(ArrowStyleConverter))]
        [Editor(typeof(ArrowStyleEditor), typeof(UITypeEditor))]
        [Browsable(true)]
        public ArrowStyle CurrentArrowStyle
        {
            get => _arrowStyle;
            set
            {
                _arrowStyle = value;
                Invalidate();
            }
        }

        [Category("箭头样式")]
        [DisplayName("箭头颜色")]
        [Description("设置箭头/箭杆线的颜色")]
        [DefaultValue(typeof(Color), "Black")]
        [Browsable(false)]
        public Color ArrowColor
        {
            get => _arrowColor;
            set
            {
                _arrowColor = value;
                Invalidate();
            }
        }

        [Category("箭头样式")]
        [DisplayName("主控件箭头尺寸")]
        [Description("右侧箭头头部的固定尺寸（像素，8-30）")]
        [DefaultValue(16)]       
        [Browsable(false)]
        public int ArrowSize
        {
            get => _arrowSize;
            set
            {
               // if (value >= 8 && value <= 30)
                {
                    _arrowSize = value;
                    Invalidate();
                }
            }
        }

        [Category("箭头样式")]
        [DisplayName("下拉箭头尺寸")]
        [Description("下拉列表中箭头头部尺寸（像素，4-12）")]
        [DefaultValue(8)]       
        [Browsable(false)]
        public int DropdownArrowSize
        {
            get => _dropdownArrowSize;
            set
            {
                //if (value >= 4 && value <= 12)
                {
                    _dropdownArrowSize = value;
                    Invalidate();
                }
            }
        }

        [Category("箭头样式")]
        [DisplayName("箭头区域宽度")]
        [Description("ComboBox右侧箭头绘制区域宽度（像素，20-60）")]
        [DefaultValue(40)]       
        [Browsable(false)]
        public int ArrowAreaWidth
        {
            get => _arrowAreaWidth;
            set
            {
               // if (value >= 20 && value <= 60)
                {
                    _arrowAreaWidth = value;
                    Invalidate();
                }
            }
        }

        [Category("箭头样式")]
        [DisplayName("线条粗细")]
        [Description("箭杆线/箭头边框的粗细（像素，1-5）")]
        [DefaultValue(2)]        
        [Browsable(false)]
        public int ArrowLineThickness
        {
            get => _arrowLineThickness;
            set
            {
                //if (value >= 1 && value <= 5)
                {
                    _arrowLineThickness = value;
                    Invalidate();
                }
            }
        }
        #endregion

        #region 隐藏ComboBox所有原生属性
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Size Size { get => base.Size; set => base.Size = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Point Location { get => base.Location; set => base.Location = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Font Font { get => base.Font; set => base.Font = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Color BackColor { get => base.BackColor; set => base.BackColor = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Color ForeColor { get => base.ForeColor; set => base.ForeColor = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new int ItemHeight { get => base.ItemHeight; set => base.ItemHeight = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new int DropDownWidth { get => base.DropDownWidth; set => base.DropDownWidth = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new DrawMode DrawMode { get => base.DrawMode; set => base.DrawMode = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new ComboBoxStyle DropDownStyle { get => base.DropDownStyle; set => base.DropDownStyle = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new object SelectedItem { get => base.SelectedItem; set => base.SelectedItem = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new int SelectedIndex { get => base.SelectedIndex; set => base.SelectedIndex = value; }
        #endregion

        public ArrowComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            ItemHeight = 30;
            DropDownWidth = 120;
            base.Size = new Size(250, 45);
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        #region 核心绘制：纯图形+等长线段
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 绘制背景
            using (var backBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(backBrush, ClientRectangle);
            }

            // 绘制边框
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                SystemColors.ControlDark, ButtonBorderStyle.Solid);

            // 右侧箭头区域
            Rectangle arrowArea = new Rectangle(
                ClientRectangle.Width - _arrowAreaWidth,
                0,
                _arrowAreaWidth,
                ClientRectangle.Height
            );

            // 箭头区背景
            using (var arrowBackBrush = new SolidBrush(Color.FromArgb(245, 245, 245)))
            {
                e.Graphics.FillRectangle(arrowBackBrush, arrowArea);
            }

            // 绘制纯图形（无文本）
            DrawArrowWithRodInRightArea(e.Graphics, arrowArea);
        }

        /// <summary>
        /// 主控件右侧图形绘制：None样式线段与Left/Right箭杆等长
        /// </summary>
        private void DrawArrowWithRodInRightArea(Graphics g, Rectangle arrowArea)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int arrowCenterY = arrowArea.Y + arrowArea.Height / 2;
            int rodLength = (int)(arrowArea.Width * _rodRatio); // 统一箭杆/线段长度
            int arrowOffset = _arrowSize / 2 + 2;

            using (var pen = new Pen(_arrowColor, _arrowLineThickness))
            {
                pen.EndCap = LineCap.Flat;
                pen.StartCap = LineCap.Flat;
                if (_arrowStyle == ArrowStyle.None)
                {
                    // None样式：线段长度=rodLength（与Left/Right箭杆等长）
                    int noneStartX = arrowArea.X + (arrowArea.Width - rodLength) / 2;
                    int noneEndX = noneStartX + rodLength + ArrowSize / 2;
                    g.DrawLine(pen, noneStartX, arrowCenterY, noneEndX, arrowCenterY);
                }
                if (_arrowStyle == ArrowStyle.Right || _arrowStyle == ArrowStyle.Both)
                {
                    // Right样式：箭杆+箭头
                    int rightRodStartX = arrowArea.X + (arrowArea.Width - rodLength) / 2;
                    int rightRodEndX = arrowArea.X + arrowArea.Width - arrowOffset;
                    g.DrawLine(pen, rightRodStartX, arrowCenterY, rightRodEndX, arrowCenterY);

                    Point[] rightArrow = new[]
                    {
                            new Point(rightRodEndX, arrowCenterY - _arrowSize/2),
                            new Point(rightRodEndX + _arrowSize/2, arrowCenterY),
                            new Point(rightRodEndX, arrowCenterY + _arrowSize/2)
                        };
                    using (var brush = new SolidBrush(_arrowColor))
                    {
                        g.FillPolygon(brush, rightArrow);
                    }
                    g.DrawPolygon(pen, rightArrow);
                }

                if (_arrowStyle == ArrowStyle.Left || _arrowStyle == ArrowStyle.Both)
                {
                    // Left样式：箭头+箭杆
                    int leftRodStartX = arrowArea.X + arrowOffset;
                    int leftRodEndX = arrowArea.X + arrowArea.Width - (arrowArea.Width - rodLength) / 2;
                    g.DrawLine(pen, leftRodStartX, arrowCenterY, leftRodEndX, arrowCenterY);

                    Point[] leftArrow = new[]
                    {
                            new Point(leftRodStartX, arrowCenterY),
                            new Point(leftRodStartX + _arrowSize/2, arrowCenterY - _arrowSize/2),
                            new Point(leftRodStartX + _arrowSize/2, arrowCenterY + _arrowSize/2)
                        };
                    using (var brush = new SolidBrush(_arrowColor))
                    {
                        g.FillPolygon(brush, leftArrow);
                    }
                    g.DrawPolygon(pen, leftArrow);
                }

            }
        }

        /// <summary>
        /// 下拉项绘制：纯图形+None线段与箭头箭杆等长+无文本
        /// </summary>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);
            e.DrawBackground();

            if (e.Index >= 0)
            {
                ArrowStyle style = (ArrowStyle)Enum.GetValues(typeof(ArrowStyle)).GetValue(e.Index);
                int arrowSize = _dropdownArrowSize;
                int rodLength = _dropdownRodLength;    // 下拉统一长度（15px）
                int startX = e.Bounds.X + 10;          // 左对齐
                int centerY = e.Bounds.Y + e.Bounds.Height / 2;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(e.ForeColor, _arrowLineThickness))
                {
                    switch (style)
                    {
                        case ArrowStyle.None:
                            // None样式：线段长度=rodLength（与Left/Right箭杆等长）
                            e.Graphics.DrawLine(pen, startX, centerY, startX + rodLength+arrowSize/2, centerY);
                            break;
                        case ArrowStyle.Right:
                            // Right样式：箭杆+箭头
                            e.Graphics.DrawLine(pen, startX, centerY, startX + rodLength, centerY);
                            Point[] right = new[]
                            {
                                new Point(startX + rodLength, centerY - arrowSize/2),
                                new Point(startX + rodLength + arrowSize/2, centerY),
                                new Point(startX + rodLength, centerY + arrowSize/2)
                            };
                            using (var brush = new SolidBrush(e.ForeColor)) e.Graphics.FillPolygon(brush, right);
                            break;
                        case ArrowStyle.Left:
                            // Left样式：箭头+箭杆
                            Point[] left = new[]
                            {
                                new Point(startX, centerY),
                                new Point(startX + arrowSize/2, centerY - arrowSize/2),
                                new Point(startX + arrowSize/2, centerY + arrowSize/2)
                            };
                            using (var brush = new SolidBrush(e.ForeColor)) e.Graphics.FillPolygon(brush, left);
                            e.Graphics.DrawLine(pen, startX + arrowSize / 2, centerY, startX + arrowSize / 2 + rodLength, centerY);
                            break;
                    }
                }
            }

            e.DrawFocusRectangle();
        }
        #endregion

        // 下拉展开/收起
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            DroppedDown = !DroppedDown;
        }

        // 选择后更新样式
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (SelectedIndex >= 0)
            {
                _arrowStyle = (ArrowStyle)Enum.GetValues(typeof(ArrowStyle)).GetValue(SelectedIndex);
                Invalidate();
            }
        }
    }
    #endregion

    #region 3. 类型转换器（无报错+筛选属性）
    public class ArrowComboBoxTypeConverter : TypeConverter
    {
        private readonly string[] _keepProperties = {
            "CurrentArrowStyle", "ArrowColor", "ArrowSize",
            "DropdownArrowSize", "ArrowAreaWidth", "ArrowLineThickness"
        };

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection allProperties = TypeDescriptor.GetProperties(value, attributes);
            PropertyDescriptorCollection filteredProperties = new PropertyDescriptorCollection(null);

            foreach (string propName in _keepProperties)
            {
                PropertyDescriptor prop = allProperties.Find(propName, false);
                if (prop != null) filteredProperties.Add(prop);
            }

            return filteredProperties;
        }

        
    }
    #endregion

    #region 4. ArrowStyle转换器（无文本相关）
    public class ArrowStyleConverter : EnumConverter
    {
        public ArrowStyleConverter() : base(typeof(ArrowStyle)) { }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is ArrowStyle style)
            {
                return "";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string str)
            {
                return "";
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
    #endregion

    #region 5. UI编辑器（纯图形预览）
    public class ArrowStyleEditor : UITypeEditor
    {
        private IWindowsFormsEditorService _editorService;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                _editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (_editorService != null && value is ArrowStyle)
                {
                    var listBox = new ListBox
                    {
                        BorderStyle = BorderStyle.None,
                        SelectionMode = SelectionMode.One,
                        DrawMode = DrawMode.OwnerDrawFixed,
                        ItemHeight = 25,
                        Width = 100
                    };

                    foreach (ArrowStyle style in Enum.GetValues(typeof(ArrowStyle)))
                    {
                        listBox.Items.Add(style);
                        if (style == (ArrowStyle)value) listBox.SelectedIndex = listBox.Items.Count - 1;
                    }

                    // 编辑器纯图形绘制（无文本）
                    listBox.DrawItem += (s, e) =>
                    {
                        e.DrawBackground();
                        if (e.Index >= 0)
                        {
                            ArrowStyle style = (ArrowStyle)listBox.Items[e.Index];
                            int arrowSize = 8;
                            int rodLength = 15;    // 编辑器统一长度
                            int startX = e.Bounds.X + 8;
                            int centerY = e.Bounds.Y + e.Bounds.Height / 2;

                            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            using (var pen = new Pen(e.ForeColor, 1))
                            {
                                if (style == ArrowStyle.None)
                                {
                                    // None线段与箭头箭杆等长
                                    e.Graphics.DrawLine(pen, startX, centerY, startX + rodLength + arrowSize / 2, centerY);
                                }
                                if (style == ArrowStyle.Right || style == ArrowStyle.Both)
                                {
                                    e.Graphics.DrawLine(pen, startX, centerY, startX + rodLength, centerY);
                                    Point[] right = new[]
                                    {
                                            new Point(startX + rodLength, centerY - arrowSize/2),
                                            new Point(startX + rodLength + arrowSize/2, centerY),
                                            new Point(startX + rodLength, centerY + arrowSize/2)
                                        };
                                    using (var brush = new SolidBrush(e.ForeColor)) e.Graphics.FillPolygon(brush, right);
                                }
                                if (style == ArrowStyle.Left || style == ArrowStyle.Both)
                                {
                                    Point[] left = new[]
                                    {
                                            new Point(startX, centerY),
                                            new Point(startX + arrowSize/2, centerY - arrowSize/2),
                                            new Point(startX + arrowSize/2, centerY + arrowSize/2)
                                        };
                                    using (var brush = new SolidBrush(e.ForeColor)) e.Graphics.FillPolygon(brush, left);
                                    e.Graphics.DrawLine(pen, startX + arrowSize / 2, centerY, startX + arrowSize / 2 + rodLength, centerY);
                                }
                            }
                        }
                        e.DrawFocusRectangle();
                    };

                    listBox.SelectedValueChanged += (s, e) =>
                    {
                        if (listBox.SelectedItem is ArrowStyle selected)
                        {
                            value = selected;
                            _editorService.CloseDropDown();
                        }
                    };

                    _editorService.DropDownControl(listBox);
                }
            }
            return value;
        }

        // 预览区纯图形
        public override bool GetPaintValueSupported(ITypeDescriptorContext context) => true;

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is ArrowStyle style)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                int arrowSize = 8;
                int rodLength = 15;
                int startX = e.Bounds.X + 2;
                int centerY = e.Bounds.Y + e.Bounds.Height / 2;

                using (var pen = new Pen(Color.Black, 1))
                {
                    if (style == ArrowStyle.None)
                        e.Graphics.DrawLine(pen, startX, centerY, startX + rodLength, centerY);
                    if (style == ArrowStyle.Right || style == ArrowStyle.Both)
                    {
                        e.Graphics.DrawLine(pen, startX, centerY, startX + rodLength, centerY);
                        Point[] right = new[]
                        {
                                new Point(startX + rodLength- arrowSize/2, centerY - arrowSize/2),
                                new Point(startX + rodLength- arrowSize/2 + arrowSize/2, centerY),
                                new Point(startX + rodLength- arrowSize/2, centerY + arrowSize/2)
                            };
                        using (var brush = new SolidBrush(Color.Black)) e.Graphics.FillPolygon(brush, right);
                    }
                    if (style == ArrowStyle.Left || style == ArrowStyle.Both)
                    {
                        Point[] left = new[]
                        {
                                new Point(startX, centerY),
                                new Point(startX + arrowSize/2, centerY - arrowSize/2),
                                new Point(startX + arrowSize/2, centerY + arrowSize/2)
                            };
                        using (var brush = new SolidBrush(Color.Black)) e.Graphics.FillPolygon(brush, left);
                        e.Graphics.DrawLine(pen, startX + arrowSize / 2, centerY, startX + rodLength, centerY);
                    }
                }
            }
        }
    }
    #endregion
    
}