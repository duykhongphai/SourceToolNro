using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ToolPart.Classes;
using ToolPart.Options;

namespace ToolPart.Windows;

public partial class WindowExecution : Window
{
    public WindowExecution()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        Instance = this;
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up:
            case Key.Down:
            case Key.Left:
            case Key.Right:
                UpdatePart();
                break;
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (_selectedImageToMove == null) return;
        const int moveAmount = 4;
        var (dx, dy) = e.Key switch
        {
            Key.Up => (0, -moveAmount),
            Key.Down => (0, moveAmount),
            Key.Left => (-moveAmount, 0),
            Key.Right => (moveAmount, 0),
            _ => (0, 0)
        };
        if (dx == 0 && dy == 0) return;
        var bounds = _selectedImageToMove.Bounds;
        _selectedImageToMove.Bounds = new Rect(bounds.X + dx, bounds.Y + dy, bounds.Width, bounds.Height);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Output.Instance?.Close();
        ImportImage.Instance?.Close();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        XOriginal = (int)(SizeHeight - ImgBong.Size.Height);
        YOriginal = (int)(SizeWeight / 2f - ImgBong.Size.Width / 2);
        XShadow = (int)(SizeHeight - ImgBong.Size.Height / 2);
        YShadow = SizeWeight / 2;
        InitPanels();
    }

    private void DragPanel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void MinimumButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void ImportDataButton_OnClick(object sender, RoutedEventArgs e)
    {
        var windowImportImage = new ImportData();
        var result = await windowImportImage.ShowDialog<string[]>(this);
        if (result == null) return;
        var headData = result[0];
        var bodyData = result[1];
        var legData = result[2];
        var iconPath = result[3];
        var isArray = result[4] == "1";
        var isIdDxDy = result[5] == "1";
        if (string.IsNullOrEmpty(headData) || string.IsNullOrEmpty(legData) || string.IsNullOrEmpty(bodyData) ||
            string.IsNullOrEmpty(iconPath))
        {
            await MessageBoxManager.GetMessageBoxStandard("Thông Báo", "Vui lòng chọn đủ thông tin", ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
            return;
        }

        PartHead = LoadParts(headData, PartHead, isArray, isIdDxDy, iconPath, BodyEnum.Head);
        PartBody = LoadParts(bodyData, PartBody, isArray, isIdDxDy, iconPath, BodyEnum.Body);
        PartLeg = LoadParts(legData, PartLeg, isArray, isIdDxDy, iconPath, BodyEnum.Leg);
    }

    private List<PartImage> LoadParts(string data, List<PartImage> target, bool isArray, bool isIdDxDy,
        string iconPath, BodyEnum type)
    {
        var parsed = JsonParser.ParseArray(data);
        if (parsed == null || parsed.Count == 0) return target;
        target ??= [];
        target.Clear();
        foreach (var item in parsed)
            target.Add(ParsePartImage(item, isArray, isIdDxDy, iconPath, type));
        return target;
    }

    private PartImage ParsePartImage(object data, bool isJsonArray, bool isIdDxDyOrder, string pathIcon, BodyEnum type)
    {
        if (isJsonArray)
        {
            if (data is List<object> arr && arr.Count >= 3 &&
                short.TryParse(arr[0]?.ToString(), out var arrId) &&
                short.TryParse(arr[1]?.ToString(), out var arrDx) &&
                short.TryParse(arr[2]?.ToString(), out var arrDy))
                return new PartImage
                {
                    Id = isIdDxDyOrder ? arrId : arrDy,
                    Dx = (sbyte)(isIdDxDyOrder ? arrDx : arrId),
                    Dy = (sbyte)(isIdDxDyOrder ? arrDy : arrDx),
                    Image = new Bitmap(Path.Combine(pathIcon, arrId + ".png")),
                    PartType = type
                };
        }
        else
        {
            if (data is Dictionary<string, object> obj &&
                obj.TryGetValue("id", out var value) &&
                obj.TryGetValue("dx", out var value2) &&
                obj.TryGetValue("dy", out var value3) &&
                short.TryParse(value?.ToString(), out var objId) &&
                sbyte.TryParse(value2?.ToString(), out var objDx) &&
                sbyte.TryParse(value3?.ToString(), out var objDy))
                return new PartImage
                {
                    Id = objId, Dx = objDx, Dy = objDy, Image = new Bitmap(Path.Combine(pathIcon, objId + ".png")),
                    PartType = type
                };
        }

        return null;
    }

    private async void ImportImage_OnClick(object sender, RoutedEventArgs e)
    {
        var windowImportImage = new ImportImage();
        var result = await windowImportImage.ShowDialog<List<List<PartImage>>>(this);
        if (result == null) return;
        PartHead = MergeParts(PartHead, result[0]);
        PartBody = MergeParts(PartBody, result[1]);
        PartLeg = MergeParts(PartLeg, result[2]);
        CharacterDisplayArea.InvalidateAll();
    }

    private static List<PartImage> MergeParts(List<PartImage> target, List<PartImage> source)
    {
        if (target == null) return source;
        for (var i = 0; i < target.Count; i++)
        {
            target[i].Image = source[i].Image;
            target[i].Id = source[i].Id;
        }

        return target;
    }

    private void ExportPart_OnClick(object sender, RoutedEventArgs e)
    {
        var windowOutput = new Output();
        windowOutput.Show();
    }

    private void TypePaint_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CharacterDisplayArea.InvalidateAll();
    }

    private PanelFrame CreatePanelCustom(byte value)
    {
        PanelFrame panel = new(value)
        {
            Width = SizeWeight,
            Height = SizeHeight,
            Margin = new Thickness(2)
        };
        panel.PointerMoved += PanelOnPointerMoved;
        panel.PointerReleased += PanelOnPointerReleased;
        panel.PointerPressed += PanelOnPointerPressed;
        return panel;
    }

    private void PanelOnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (_selectedImageToMove == null) return;
        UpdatePart();
        if (Output.Instance is { IsVisible: true }) Output.Instance.InitData();
    }

    private void PanelOnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is not PanelFrame panel) return;
        var position = e.GetPosition(panel);
        if (!sbyte.TryParse(panel.Tag?.ToString(), out var row)) return;
        if (_selectedImageToMove != null) _selectedImageToMove.IsSelected = false;
        TagPanelSelected = row;
        var selectedPart = GetPartImageInfo(position);
        if (selectedPart == null) return;
        _selectedImageToMove = selectedPart;
        _selectedImageToMove.IsSelected = true;
        Offset = new Point(position.X - _selectedImageToMove.Bounds.Left,
            position.Y - _selectedImageToMove.Bounds.Top);
        panel.InvalidateVisual();
    }

    private void PanelOnPointerMoved(object sender, PointerEventArgs e)
    {
        if (sender is not PanelFrame panel) return;
        var point = e.GetCurrentPoint(panel);
        var position = e.GetPosition(panel);
        if (!point.Properties.IsLeftButtonPressed || _selectedImageToMove == null) return;
        _selectedImageToMove.Bounds = new Rect(position.X - Offset.X, position.Y - Offset.Y,
            _selectedImageToMove.Bounds.Width, _selectedImageToMove.Bounds.Height);
        panel?.InvalidateVisual();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        if (Function.CountNotNull(PartHead) <= 0 || Function.CountNotNull(PartBody) <= 0 ||
            Function.CountNotNull(PartLeg) <= 0) return;
        if (sender is not Button button) return;
        var tagButton = int.Parse(button.Tag?.ToString() ?? "-1");
        InitPanel(tagButton);
    }

    #region Variable

    public static WindowExecution Instance { get; private set; }
    public static int XOriginal { get; private set; }
    public static int YOriginal { get; private set; }
    private static int XShadow { get; set; }
    private static int YShadow { get; set; }
    private const short SizeWeight = 200;
    private const short SizeHeight = 250;

    public static readonly int[][][] CharInfo =
    [
        [
            [
                0,
                -13,
                34
            ],
            [
                1,
                -8,
                10
            ],
            [
                1,
                -9,
                16
            ],
            [
                1,
                -9,
                45
            ]
        ],
        [
            new[]
            {
                0,
                -13,
                35
            },
            new[]
            {
                1,
                -8,
                10
            },
            new[]
            {
                1,
                -9,
                17
            },
            new[]
            {
                1,
                -9,
                46
            }
        ],
        new[]
        {
            new[]
            {
                1,
                -10,
                33
            },
            new[]
            {
                2,
                -10,
                11
            },
            new[]
            {
                2,
                -8,
                16
            },
            new[]
            {
                1,
                -12,
                49
            }
        },
        new[]
        {
            new[]
            {
                1,
                -10,
                32
            },
            new[]
            {
                3,
                -12,
                10
            },
            new[]
            {
                3,
                -11,
                15
            },
            new[]
            {
                1,
                -13,
                47
            }
        },
        new[]
        {
            new[]
            {
                1,
                -10,
                34
            },
            new[]
            {
                4,
                -8,
                11
            },
            new[]
            {
                4,
                -7,
                17
            },
            new[]
            {
                1,
                -12,
                47
            }
        },
        new[]
        {
            new[]
            {
                1,
                -10,
                34
            },
            new[]
            {
                5,
                -12,
                11
            },
            new[]
            {
                5,
                -9,
                17
            },
            new[]
            {
                1,
                -13,
                49
            }
        },
        new[]
        {
            new[]
            {
                1,
                -10,
                33
            },
            new[]
            {
                6,
                -10,
                10
            },
            new[]
            {
                6,
                -8,
                16
            },
            new[]
            {
                1,
                -12,
                47
            }
        },
        new[]
        {
            new[]
            {
                0,
                -9,
                36
            },
            new[]
            {
                7,
                -5,
                17
            },
            new[]
            {
                7,
                -11,
                25
            },
            new[]
            {
                1,
                -8,
                49
            }
        },
        new[]
        {
            new[]
            {
                0,
                -7,
                35
            },
            new[]
            {
                0,
                -18,
                22
            },
            new[]
            {
                7,
                -10,
                25
            },
            new[]
            {
                1,
                -7,
                48
            }
        },
        new[]
        {
            new[]
            {
                1,
                -11,
                35
            },
            new[]
            {
                10,
                -3,
                25
            },
            new[]
            {
                12,
                -10,
                26
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                1,
                -11,
                37
            },
            new[]
            {
                11,
                -3,
                25
            },
            new[]
            {
                12,
                -11,
                27
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -14,
                34
            },
            new[]
            {
                12,
                -8,
                21
            },
            new[]
            {
                9,
                -7,
                31
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -12,
                35
            },
            new[]
            {
                8,
                -5,
                14
            },
            new[]
            {
                8,
                -15,
                29
            },
            new[]
            {
                1,
                -9,
                49
            }
        },
        new[]
        {
            new[]
            {
                1,
                -9,
                34
            },
            new[]
            {
                9,
                -12,
                9
            },
            new[]
            {
                10,
                -7,
                19
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                1,
                -13,
                34
            },
            new[]
            {
                9,
                -12,
                9
            },
            new[]
            {
                11,
                -10,
                19
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                1,
                -8,
                32
            },
            new[]
            {
                9,
                -12,
                9
            },
            new[]
            {
                2,
                -6,
                15
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                1,
                -8,
                32
            },
            new[]
            {
                9,
                -12,
                9
            },
            new[]
            {
                13,
                -12,
                16
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -10,
                31
            },
            new[]
            {
                9,
                -12,
                9
            },
            new[]
            {
                7,
                -13,
                20
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -11,
                32
            },
            new[]
            {
                9,
                -12,
                9
            },
            new[]
            {
                8,
                -15,
                26
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -9,
                33
            },
            new[]
            {
                9,
                -12,
                9
            },
            new[]
            {
                14,
                -8,
                18
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -11,
                33
            },
            new[]
            {
                9,
                -12,
                9
            },
            new[]
            {
                15,
                -6,
                19
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -16,
                31
            },
            new[]
            {
                9,
                -12,
                9
            },
            new[]
            {
                9,
                -8,
                28
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -14,
                34
            },
            new[]
            {
                1,
                -8,
                10
            },
            new[]
            {
                8,
                -16,
                28
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -8,
                36
            },
            new[]
            {
                7,
                -5,
                17
            },
            new[]
            {
                0,
                -5,
                25
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -9,
                31
            },
            new[]
            {
                9,
                -12,
                9
            },
            new[]
            {
                0,
                -6,
                20
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                2,
                -9,
                36
            },
            new[]
            {
                13,
                -5,
                17
            },
            new[]
            {
                16,
                -11,
                25
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                1,
                -9,
                34
            },
            new[]
            {
                8,
                -5,
                13
            },
            new[]
            {
                10,
                -7,
                19
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                1,
                -13,
                34
            },
            new[]
            {
                8,
                -5,
                13
            },
            new[]
            {
                11,
                -10,
                19
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                1,
                -8,
                32
            },
            new[]
            {
                8,
                -5,
                13
            },
            new[]
            {
                2,
                -6,
                15
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                1,
                -8,
                32
            },
            new[]
            {
                8,
                -5,
                13
            },
            new[]
            {
                13,
                -12,
                16
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -9,
                33
            },
            new[]
            {
                8,
                -5,
                13
            },
            new[]
            {
                14,
                -8,
                18
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -11,
                33
            },
            new[]
            {
                8,
                -5,
                13
            },
            new[]
            {
                15,
                -6,
                19
            },
            new int[3]
        },
        new[]
        {
            new[]
            {
                0,
                -16,
                32
            },
            new[]
            {
                8,
                -5,
                13
            },
            new[]
            {
                9,
                -8,
                29
            },
            new int[3]
        }
    ];

    private PartImage _selectedImageToMove;
    private sbyte TagPanelSelected { get; set; }
    private sbyte TypePartSelected { get; set; }
    private Point Offset { get; set; }

    public List<PartImage> PartHead { get; private set; }
    public List<PartImage> PartBody { get; private set; }
    public List<PartImage> PartLeg { get; private set; }
    private readonly Dictionary<sbyte, List<PanelFrame>> _panels = new();
    public BodyEnum TypeSort = BodyEnum.DxDyId;

    public static readonly Bitmap ImgBong =
        ImageHelper.LoadFromResource(new Uri("avares://ToolPart/Assets/Resources/bong.png"));

    public static readonly Bitmap ImgNull =
        ImageHelper.LoadFromResource(new Uri("avares://ToolPart/Assets/Resources/h3.png"));

    #endregion

    #region Function

    private void InitPanels()
    {
        _panels.PutItem(1, CreatePanelCustom(0));

        _panels.PutItem(2, CreatePanelCustom(2));
        _panels.PutItem(2, CreatePanelCustom(3));
        _panels.PutItem(2, CreatePanelCustom(4));
        _panels.PutItem(2, CreatePanelCustom(5));
        _panels.PutItem(2, CreatePanelCustom(6));

        _panels.PutItem(3, CreatePanelCustom(12));

        _panels.PutItem(4, CreatePanelCustom(7));
        _panels.PutItem(4, CreatePanelCustom(8));

        _panels.PutItem(5, CreatePanelCustom(9));
        _panels.PutItem(5, CreatePanelCustom(10));
        _panels.PutItem(5, CreatePanelCustom(11));
        _panels.PutItem(5, CreatePanelCustom(13));
        _panels.PutItem(5, CreatePanelCustom(14));
        _panels.PutItem(5, CreatePanelCustom(15));
        _panels.PutItem(5, CreatePanelCustom(16));
        _panels.PutItem(5, CreatePanelCustom(17));
        _panels.PutItem(5, CreatePanelCustom(18));
        _panels.PutItem(5, CreatePanelCustom(19));
        _panels.PutItem(5, CreatePanelCustom(20));
        _panels.PutItem(5, CreatePanelCustom(21));
        _panels.PutItem(5, CreatePanelCustom(22));

        _panels.PutItem(6, CreatePanelCustom(24));

        _panels.PutItem(7, CreatePanelCustom(26));
        _panels.PutItem(7, CreatePanelCustom(27));
        _panels.PutItem(7, CreatePanelCustom(28));
        _panels.PutItem(7, CreatePanelCustom(29));
        _panels.PutItem(7, CreatePanelCustom(30));
        _panels.PutItem(7, CreatePanelCustom(31));
        _panels.PutItem(7, CreatePanelCustom(32));
    }

    private void UpdatePart()
    {
        if (_selectedImageToMove != null) UpdatePart(_selectedImageToMove);
        UpdatePointAllImageInPanel();
        CharacterDisplayArea.InvalidateAll();
    }

    private void UpdateImagePosition(PartImage image, int xOffset, int yOffset)
    {
        image.Dx = (int)((image.Bounds.X - YShadow) / 4 - xOffset);
        image.Dy = (int)((image.Bounds.Y - XShadow) / 4 + yOffset);
    }


    private void UpdatePart(PartImage image)
    {
        if (image == null) return;
        var panelPart = _panels.GetValueOrDefault(TypePartSelected)[TagPanelSelected];
        if (panelPart == null) return;
        var charInfo = CharInfo[panelPart.Cf];
        switch (image.PartType)
        {
            case BodyEnum.Head:
                UpdateImagePosition(image, charInfo[0][1], charInfo[0][2]);
                break;
            case BodyEnum.Body:
                UpdateImagePosition(image, charInfo[2][1], charInfo[2][2]);
                break;
            case BodyEnum.Leg:
                UpdateImagePosition(image, charInfo[1][1], charInfo[1][2]);
                break;
        }
    }

    private void UpdatePointAllImageInPanel()
    {
        foreach (var panelPart in _panels.GetValueOrDefault(TypePartSelected))
            if (panelPart != null)
            {
                var charInfo = CharInfo[panelPart.Cf];
                var head = panelPart.GetPartImage(BodyEnum.Head);
                var body = panelPart.GetPartImage(BodyEnum.Body);
                var leg = panelPart.GetPartImage(BodyEnum.Leg);
                head.UpdatePoint((head.Dx + charInfo[0][1]) * 4 + YShadow, (head.Dy - charInfo[0][2]) * 4 + XShadow);
                body.UpdatePoint((body.Dx + charInfo[2][1]) * 4 + YShadow, (body.Dy - charInfo[2][2]) * 4 + XShadow);
                leg.UpdatePoint((leg.Dx + charInfo[1][1]) * 4 + YShadow, (leg.Dy - charInfo[1][2]) * 4 + XShadow);
            }
    }

    private PartImage GetPartImageInfo(Point e)
    {
        var panel = _panels.GetValueOrDefault(TypePartSelected)[TagPanelSelected];
        if (panel == null) return null;
        var head = panel.GetPartImage(BodyEnum.Head);
        var body = panel.GetPartImage(BodyEnum.Body);
        var leg = panel.GetPartImage(BodyEnum.Leg);
        if (TypePaint.SelectedIndex == 0)
        {
            if (head.Bounds.Contains(e)) return head;
            if (body.Bounds.Contains(e)) return body;
        }
        else
        {
            if (body.Bounds.Contains(e)) return body;
            if (head.Bounds.Contains(e)) return head;
        }

        return leg.Bounds.Contains(e) ? leg : null;
    }

    private void InitPanel(int tag)
    {
        var tagPanel = int.Parse(CharacterDisplayArea.Tag?.ToString() ?? "-1");
        if (tagPanel != -1 && tagPanel == tag) return;
        if (_selectedImageToMove != null) _selectedImageToMove.IsSelected = false;
        _selectedImageToMove = null;
        CharacterDisplayArea.Children.Clear();
        CharacterDisplayArea.Tag = tag;
        TypePartSelected = (sbyte)tag;
        var list = _panels[(sbyte)tag];
        foreach (var p in list) CharacterDisplayArea.Children.Add(p);
        UpdatePart();
    }

    #endregion
}