using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using DrawMap.ChildWindows;
using DrawMap.Classes;

namespace DrawMap.Options;

public class PanelMap : Control
{
    #region Constants

    private const double FontSize = 10;
    private const int GridSize = 24;
    private const int UpdateIntervalMs = 100;
    private const int TileTypeIndicatorOffset = 5;
    private const int TileTypeIndicatorEnd = 19;
    private const int TextOffsetY = 20;
    private const int ActorTextOffsetY = 30;
    private const int NpcTextOffsetY = 70;

    #endregion

    #region Static Resources

    private static readonly Pen BorderPen = new(Brushes.Blue);
    private static readonly Pen GridPen1 = new(Color.FromArgb(25, 0, 0, 0).ToUInt32());
    private static readonly Pen GridPen3 = new(Color.FromArgb(45, 255, 255, 255).ToUInt32());
    private static readonly Pen AquaPen = new(Brushes.Aqua);
    private static readonly Pen RedPen = new(Brushes.Red, 2);
    private static readonly Typeface ArialTypeface = new("Arial");

    #endregion

    #region Private Fields

    private DispatcherTimer _updateEffect;
    private bool _canDrop;
    private bool _hasProcessedClick;
    private bool _isDragging;
    private Point _mouseLocation;

    #endregion

    #region Constructor

    public PanelMap()
    {
        InitializeEventHandlers();
        InitializeUpdateTimer();
    }

    private void InitializeEventHandlers()
    {
        PointerMoved += OnPointerMoved;
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
    }

    private void InitializeUpdateTimer()
    {
        _updateEffect = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(UpdateIntervalMs)
        };
        _updateEffect.Tick += UpdateEffect_Tick;
        _updateEffect.Start();
    }

    #endregion

    #region Event Handlers

    private void UpdateEffect_Tick(object sender, EventArgs e)
    {
        if (ManagerDrawMap.Instance.EffectRadio.IsChecked != true)
            return;

        UpdateEffects();
    }

    private static void UpdateEffects()
    {
        foreach (var effect in Fields.ResourceEffect.Values) effect.UpdateEffect();
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        StartDragging();
    }

    private void StartDragging()
    {
        _isDragging = true;
        _hasProcessedClick = false;
        ProcessMapInteraction();
        _hasProcessedClick = true;
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        StopDragging();
    }

    private void StopDragging()
    {
        _isDragging = false;
        _hasProcessedClick = false;
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        UpdateMouseLocation(e);
        UpdateCanDropState();
        HandleDragOperation(e);
        InvalidateVisual();
    }

    private void UpdateMouseLocation(PointerEventArgs e)
    {
        _mouseLocation = e.GetPosition(this);
    }

    private void UpdateCanDropState()
    {
        if (ChildWindows.DrawMap.Instance.ImageSelected == null)
        {
            _canDrop = false;
            return;
        }

        var manager = ManagerDrawMap.Instance;
        _canDrop = _mouseLocation.X >= 0 && _mouseLocation.X <= manager.WMedium &&
                   _mouseLocation.Y >= 0 && _mouseLocation.Y <= manager.HMedium;
    }

    private void HandleDragOperation(PointerEventArgs e)
    {
        if (!ShouldProcessDragOperation(e))
            return;

        var imageSelected = ChildWindows.DrawMap.Instance.ImageSelected;
        if (imageSelected != null && CanDragType(imageSelected.TypePicture)) ProcessMapInteraction();
    }

    private bool ShouldProcessDragOperation(PointerEventArgs e)
    {
        return _isDragging &&
               _hasProcessedClick &&
               e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
    }

    private static bool CanDragType(Enum typePicture)
    {
        return typePicture is Enum.TileMap or Enum.Eraser;
    }

    #endregion

    #region Rendering

    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH",
        MessageId = "type: Entry[Avalonia.PropertyStore.EffectiveValue][]; size: 143MB")]
    public sealed override void Render(DrawingContext context)
    {
        base.Render(context);

        try
        {
            var manager = ManagerDrawMap.Instance;
            RenderMapContainer(context, manager);
            RenderMapLayers(context, manager);
            RenderSelectedImagePreview(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Render error: {ex.Message}");
        }
    }

    private void RenderMapContainer(DrawingContext context, ManagerDrawMap manager)
    {
        var containerRect = new Rect(0, 0, manager.WMedium - 1, manager.HMedium - 1);
        context.FillRectangle(Brushes.Transparent, containerRect);
        context.DrawRectangle(BorderPen, containerRect);
    }

    private void RenderMapLayers(DrawingContext context, ManagerDrawMap manager)
    {
        RenderBackground(context, manager);
        RenderGrid(context, manager);
        RenderTileMap(context, manager);
        RenderWaypoint(context, manager);

        if (manager.ViewItemBackgroundCheck.IsChecked == true)
            RenderItemBackground(context, manager);

        if (manager.ViewEffectCheck.IsChecked == true)
            RenderEffects(context, manager);

        if (manager.ViewActorCheck.IsChecked == true)
            RenderActors(context, manager);
    }

    private void RenderGrid(DrawingContext context, ManagerDrawMap manager)
    {
        if (ShouldShowGrid(manager)) DrawGrid(context, manager.WMedium, manager.HMedium);
    }

    private static bool ShouldShowGrid(ManagerDrawMap manager)
    {
        return manager.ViewGridCheck.IsChecked == true &&
               !string.IsNullOrEmpty(manager.WidthTextBox.Text) &&
               !string.IsNullOrEmpty(manager.HeightTextBox.Text);
    }

    private void RenderBackground(DrawingContext context, ManagerDrawMap manager)
    {
        if (!ShouldRenderBackground(manager))
            return;

        var backgrounds = Fields.ResourceBackground[manager.BackgroundId];
        RenderBackgroundLayers(context, manager, backgrounds);
    }

    private static bool ShouldRenderBackground(ManagerDrawMap manager)
    {
        return manager.ViewBackgroundCheck.IsChecked == true &&
               Fields.ResourceBackground.Count > 0;
    }

    private void RenderBackgroundLayers(DrawingContext context, ManagerDrawMap manager, List<PictureCustom> backgrounds)
    {
        for (var i = backgrounds.Count - 1; i > 0; i--)
        {
            var img = backgrounds[i];
            if (img.Source == null) continue;

            RenderBackgroundLayer(context, manager, img, i);
        }
    }

    private void RenderBackgroundLayer(DrawingContext context, ManagerDrawMap manager, PictureCustom img,
        int layerIndex)
    {
        var sourceWidth = img.Source.Size.Width;
        var sourceHeight = img.Source.Size.Height;

        for (var j = -((0 >> Fields.LayerSpeed[layerIndex]) % sourceWidth);
             j < manager.WMedium;
             j += sourceWidth)
        {
            var rect = new Rect(
                j,
                ChildWindows.DrawMap.Instance.Yb[layerIndex] - (0 >> Fields.DeltaY[layerIndex]),
                sourceWidth,
                sourceHeight);
            context.DrawImage(img.Source, rect);
        }
    }

    private int _cachedIdTitle = -1;
    private Dictionary<int, PictureCustom> _tileLookup;

    private void RenderTileMap(DrawingContext context, ManagerDrawMap manager)
    {
        var dataMap = manager.DataMap;

        if (_cachedIdTitle != manager.IdTitle)
        {
            _cachedIdTitle = manager.IdTitle;
            _tileLookup = Fields.ResourceTitleMap[manager.IdTitle].ToDictionary(t => t.GetId());
        }

        for (var i = 0; i < dataMap.Length; i++)
        {
            var titleMap = dataMap[i];
            if (titleMap == 0) continue;

            RenderTile(context, manager, titleMap, i);
        }
    }

    private void RenderTile(DrawingContext context, ManagerDrawMap manager, int titleMap, int index)
    {
        if (!_tileLookup.TryGetValue(titleMap, out var info)) return;
        var pointImage = IndexToLocation(index);

        context.DrawImage(info.Source!, new Rect(pointImage, info.Bounds.Size));

        if (manager.ViewGridCheck.IsChecked == true) RenderTileGrid(context, info, pointImage);

        if (manager.TitleTypeCheck.IsChecked == true) RenderTileTypeIndicators(context, info, pointImage);
    }

    private static void RenderTileGrid(DrawingContext context, PictureCustom info, Point pointImage)
    {
        context.DrawRectangle(AquaPen, new Rect(pointImage, new Size(info.Width, info.Height)));
    }

    private static void RenderTileTypeIndicators(DrawingContext context, PictureCustom info, Point pointImage)
    {
        var typeBlock = info.TypeBlock;
        var width = info.Width;

        var indicators = new[]
        {
            (Fields.T_TOP, new Point(pointImage.X + TileTypeIndicatorOffset, pointImage.Y + TileTypeIndicatorOffset),
                new Point(pointImage.X + width - TileTypeIndicatorOffset, pointImage.Y + TileTypeIndicatorOffset)),
            (Fields.T_BOTTOM, new Point(pointImage.X + TileTypeIndicatorOffset, pointImage.Y + TileTypeIndicatorEnd),
                new Point(pointImage.X + width - TileTypeIndicatorOffset, pointImage.Y + TileTypeIndicatorEnd)),
            (Fields.T_LEFT, new Point(pointImage.X + TileTypeIndicatorOffset, pointImage.Y + TileTypeIndicatorOffset),
                new Point(pointImage.X + TileTypeIndicatorOffset, pointImage.Y + TileTypeIndicatorEnd)),
            (Fields.T_RIGHT, new Point(pointImage.X + TileTypeIndicatorEnd, pointImage.Y + TileTypeIndicatorOffset),
                new Point(pointImage.X + TileTypeIndicatorEnd, pointImage.Y + TileTypeIndicatorEnd))
        };

        foreach (var (flag, start, end) in indicators)
            if (typeBlock.Contains(flag))
                context.DrawLine(RedPen, start, end);
    }

    private static void RenderItemBackground(DrawingContext context, ManagerDrawMap manager)
    {
        foreach (var bgItem in manager.BgItem)
        {
            var pictureBox = Fields.ResourceItemBackground[bgItem.Id];
            if (pictureBox.Source == null) continue;

            context.DrawImage(pictureBox.Source,
                new Rect(bgItem.X, bgItem.Y, pictureBox.Width, pictureBox.Height));
        }
    }

    private static void RenderEffects(DrawingContext context, ManagerDrawMap manager)
    {
        foreach (var effectMap in manager.EffectMap) RenderEffect(context, effectMap);
    }

    private static void RenderEffect(DrawingContext context, EffectMap effectMap)
    {
        var data = Fields.ResourceEffect[effectMap.Id];
        var text = $"{effectMap.Id}.{effectMap.Layer}.{effectMap.X}.{effectMap.Y + data.Height}";

        var formattedText = CreateFormattedText(text, Brushes.Yellow);
        var textPosition = new Point(
            effectMap.X - formattedText.Width / 2,
            effectMap.Y - data.Height - TextOffsetY);

        context.DrawText(formattedText, textPosition);
        data.Paint(context, 0, effectMap.X, effectMap.Y);
    }

    private static void RenderWaypoint(DrawingContext context, ManagerDrawMap manager)
    {
        foreach (var waypoint in manager.WaypointMap) RenderWaypointItem(context, waypoint);
    }

    private static void RenderWaypointItem(DrawingContext context, ActorMap waypoint)
    {
        var pic = Fields.PictureWaypoint;
        var text = $"Min: {waypoint.X + 70} - {waypoint.Y + 70}\nMax: {waypoint.X + 94} - {waypoint.Y + 94}";
        var formattedText = CreateFormattedText(text, Brushes.Yellow);

        context.DrawText(formattedText, new Point(waypoint.X, waypoint.Y - 30));
        context.DrawImage(pic.Source, new Rect(waypoint.X, waypoint.Y, pic.Width, pic.Height));
    }

    private static void RenderActors(DrawingContext context, ManagerDrawMap manager)
    {
        RenderMonsters(context, manager);
        RenderNpcs(context, manager);
    }

    private static void RenderMonsters(DrawingContext context, ManagerDrawMap manager)
    {
        foreach (var monster in manager.MonsterMap) RenderMonster(context, monster);
    }

    private static void RenderMonster(DrawingContext context, ActorMap monster)
    {
        var pic = Fields.ResourceMonster[monster.Id];
        var text = $"{monster.Id} - {pic.Image.Tag}\n{monster.X} - {monster.Y - 11}";
        var formattedText = CreateFormattedText(text, Brushes.Yellow);

        var textPosition = new Point(
            monster.X - formattedText.Width / 2,
            monster.Y - pic.Height - ActorTextOffsetY);

        context.DrawText(formattedText, textPosition);
        pic.Paint(context, 0, monster.X, monster.Y);
    }

    private static void RenderNpcs(DrawingContext context, ManagerDrawMap manager)
    {
        foreach (var npc in manager.NpcMap) RenderNpc(context, npc);
    }

    private static void RenderNpc(DrawingContext context, ActorMap npc)
    {
        var pic = Fields.ResourceNpc[npc.Id];
        var text = $"{npc.Id} - {pic.Tag}\n{npc.X} - {npc.Y + 32}";
        var formattedText = CreateFormattedText(text, Brushes.Yellow);

        var textPosition = new Point(npc.X - pic.Width / 2, npc.Y - NpcTextOffsetY);

        context.DrawText(formattedText, textPosition);
        pic.RenderNpc(context, npc.X, npc.Y);
    }

    private void RenderSelectedImagePreview(DrawingContext context)
    {
        var imageSelected = ChildWindows.DrawMap.Instance.ImageSelected;
        if (!_canDrop || imageSelected == null) return;

        var renderer = CreatePreviewRenderer(imageSelected);
        renderer?.Invoke(context, imageSelected);
    }

    private Action<DrawingContext, PictureCustom> CreatePreviewRenderer(PictureCustom imageSelected)
    {
        return imageSelected.TypePicture switch
        {
            Enum.TileMap or Enum.Eraser => RenderTileMapPreview,
            Enum.Monster => RenderMonsterPreview,
            Enum.Effect => RenderEffectPreview,
            Enum.ItemBackground => RenderItemBackgroundPreview,
            Enum.Npc => RenderNpcPreview,
            Enum.Waypoint => RenderWaypointPreview,
            _ => null
        };
    }

    private void RenderTileMapPreview(DrawingContext context, PictureCustom imageSelected)
    {
        var snapPoint = SnapToGrid(CalculateCenteredPosition(imageSelected));
        context.DrawImage(imageSelected.Source, new Rect(snapPoint, imageSelected.Bounds.Size));
    }

    private void RenderMonsterPreview(DrawingContext context, PictureCustom imageSelected)
    {
        var position = CalculateCenteredPosition(imageSelected);
        imageSelected.EffectData.Paint(context, 0, position.X, position.Y);
    }

    private void RenderEffectPreview(DrawingContext context, PictureCustom imageSelected)
    {
        imageSelected.EffectData.Paint(context, 0, _mouseLocation.X, _mouseLocation.Y);
    }

    private void RenderItemBackgroundPreview(DrawingContext context, PictureCustom imageSelected)
    {
        var basePosition = CalculateCenteredPosition(imageSelected);
        var snapPoint = SnapToGrid(new Point(
            basePosition.X + imageSelected.GetDx(),
            basePosition.Y + imageSelected.GetDy()));
        context.DrawImage(imageSelected.Source, new Rect(snapPoint, imageSelected.Bounds.Size));
    }

    private void RenderNpcPreview(DrawingContext context, PictureCustom imageSelected)
    {
        var position = CalculateCenteredPosition(imageSelected);
        imageSelected.RenderNpc(context, position.X, position.Y);
    }

    private void RenderWaypointPreview(DrawingContext context, PictureCustom imageSelected)
    {
        var position = CalculateCenteredPosition(imageSelected);
        context.DrawImage(imageSelected.Source, new Rect(position, imageSelected.Bounds.Size));
    }

    private Point CalculateCenteredPosition(PictureCustom imageSelected)
    {
        return new Point(
            _mouseLocation.X - imageSelected.Width / 2,
            _mouseLocation.Y - imageSelected.Height / 2);
    }

    private static FormattedText CreateFormattedText(string text, IBrush brush)
    {
        return new FormattedText(text, CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight, ArialTypeface, FontSize, brush);
    }

    #endregion

    #region Grid Operations

    private static Point SnapToGrid(Point point)
    {
        var x = GridSize * Math.Round(point.X / GridSize);
        var y = GridSize * Math.Round(point.Y / GridSize);
        return new Point(x, y);
    }

    private static void DrawGrid(DrawingContext context, int width, int height)
    {
        DrawHorizontalGridLines(context, width, height);
        DrawVerticalGridLines(context, width, height);
    }

    private static void DrawHorizontalGridLines(DrawingContext context, int width, int height)
    {
        for (var y = 0; y < height; y += GridSize)
        {
            context.DrawLine(GridPen1, new Point(0, y), new Point(width, y));
            context.DrawLine(GridPen3, new Point(0, y + 1), new Point(width, y + 1));
        }
    }

    private static void DrawVerticalGridLines(DrawingContext context, int width, int height)
    {
        for (var x = 0; x < width; x += GridSize)
        {
            context.DrawLine(GridPen1, new Point(x, 0), new Point(x, height));
            context.DrawLine(GridPen3, new Point(x + 1, 0), new Point(x + 1, height));
        }
    }

    #endregion

    #region Coordinate Conversion

    private int LocationToIndex(Point point)
    {
        var row = point.Y / GridSize;
        var col = point.X / GridSize;
        return (int)(row * ManagerDrawMap.Instance.Tmw + col);
    }

    private Point IndexToLocation(int index)
    {
        try
        {
            var width = ManagerDrawMap.Instance.Tmw;
            var row = index / width;
            var col = index % width;
            return new Point(col * GridSize, row * GridSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"IndexToLocation error: {ex.Message}");
            return new Point(0, 0);
        }
    }

    #endregion

    #region Map Data Operations

    private void ProcessMapInteraction()
    {
        var imageSelected = ChildWindows.DrawMap.Instance.ImageSelected;
        if (!_canDrop || imageSelected == null) return;

        try
        {
            var manager = ManagerDrawMap.Instance;
            var handler = CreateInteractionHandler(imageSelected.TypePicture);
            handler?.Invoke(manager, imageSelected);
            InvalidateVisual();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Map interaction error: {ex.Message}");
        }
    }

    private Action<ManagerDrawMap, PictureCustom> CreateInteractionHandler(Enum typePicture)
    {
        return typePicture switch
        {
            _ when typePicture == Enum.TileMap => HandleTileMapInteraction,
            _ when typePicture == Enum.ItemBackground => HandleItemBackgroundInteraction,
            _ when typePicture == Enum.Eraser => HandleEraserInteraction,
            _ when typePicture == Enum.Effect => HandleEffectInteraction,
            _ when typePicture == Enum.Monster => HandleMonsterInteraction,
            _ when typePicture == Enum.Npc => HandleNpcInteraction,
            _ when typePicture == Enum.Waypoint => HandleWaypointInteraction,
            _ => null
        };
    }

    private void HandleTileMapInteraction(ManagerDrawMap manager, PictureCustom imageSelected)
    {
        var locationGrid = GetSnappedGridLocation(imageSelected);
        manager.DataMap[LocationToIndex(locationGrid)] = (byte)imageSelected.GetId();
    }

    private void HandleItemBackgroundInteraction(ManagerDrawMap manager, PictureCustom imageSelected)
    {
        var locationGrid = GetSnappedGridLocation(imageSelected);
        manager.BgItem.Add(new BgItem(imageSelected.GetId(),
            (int)(locationGrid.X + imageSelected.GetDx()),
            (int)(locationGrid.Y + imageSelected.GetDy())));
    }

    private void HandleEraserInteraction(ManagerDrawMap manager, PictureCustom imageSelected)
    {
        var locationGrid = GetSnappedGridLocation(imageSelected);

        var eraserHandlers = new Dictionary<Func<bool>, Action>
        {
            { () => manager.TileMapRadio.IsChecked == true, () => EraseTileMap(manager, locationGrid) },
            { () => manager.ItemBackgroundRadio.IsChecked == true, () => EraseItemBackground(manager) },
            { () => manager.EffectRadio.IsChecked == true, () => EraseEffect(manager) },
            { () => manager.WaypointRadio.IsChecked == true, () => EraseWaypoint(manager) },
            { () => manager.ViewActorCheck.IsChecked == true, () => EraseActor(manager) }
        };

        foreach (var (condition, action) in eraserHandlers)
        {
            if (!condition()) continue;
            action();
            break;
        }
    }

    private void EraseTileMap(ManagerDrawMap manager, Point locationGrid)
    {
        manager.DataMap[LocationToIndex(locationGrid)] = 0;
    }

    private void EraseItemBackground(ManagerDrawMap manager)
    {
        var itemToRemove = Function.FindItemUnderMouse(
            (int)(_mouseLocation.X - ChildWindows.DrawMap.Instance.ImageSelected.Width / 2),
            (int)(_mouseLocation.Y - ChildWindows.DrawMap.Instance.ImageSelected.Height / 2));

        if (itemToRemove != null)
            manager.BgItem.Remove(itemToRemove);
    }

    private void EraseEffect(ManagerDrawMap manager)
    {
        var itemToRemove = Function.FindItemEffectUnderMouse((int)_mouseLocation.X, (int)_mouseLocation.Y);
        if (itemToRemove != null)
            manager.EffectMap.Remove(itemToRemove);
    }

    private void EraseWaypoint(ManagerDrawMap manager)
    {
        var itemToRemove = Function.FindWaypointMapUnderMouse((int)_mouseLocation.X, (int)_mouseLocation.Y);
        if (itemToRemove != null)
            manager.WaypointMap.Remove(itemToRemove);
    }

    private void EraseActor(ManagerDrawMap manager)
    {
        var itemToRemove = Function.FindActorMapUnderMouse((int)_mouseLocation.X, (int)_mouseLocation.Y);
        if (itemToRemove == null) return;

        var actorRemovers = new Dictionary<int, Action>
        {
            { 0, () => manager.MonsterMap.Remove(itemToRemove) },
            { 1, () => manager.NpcMap.Remove(itemToRemove) }
        };

        if (actorRemovers.TryGetValue(itemToRemove.TypeActor, out var remover)) remover();
    }

    private void HandleEffectInteraction(ManagerDrawMap manager, PictureCustom imageSelected)
    {
        manager.EffectMap.Add(new EffectMap(imageSelected.EffectData.Id,
            manager.LayerEffectFieldComboBox.SelectedIndex + 1,
            (int)_mouseLocation.X, (int)_mouseLocation.Y));
    }

    private void HandleMonsterInteraction(ManagerDrawMap manager, PictureCustom imageSelected)
    {
        var position = GetActorPosition(imageSelected);
        manager.MonsterMap.Add(new ActorMap(imageSelected.GetId(), position.Item1, position.Item2, 0));
    }

    private void HandleNpcInteraction(ManagerDrawMap manager, PictureCustom imageSelected)
    {
        var position = GetActorPosition(imageSelected);
        manager.NpcMap.Add(new ActorMap(imageSelected.GetId(), position.Item1, position.Item2, 1));
    }

    private void HandleWaypointInteraction(ManagerDrawMap manager, PictureCustom imageSelected)
    {
        var position = GetActorPosition(imageSelected);
        manager.WaypointMap.Add(new ActorMap(-1, position.Item1, position.Item2, 2));
    }

    private Point GetSnappedGridLocation(PictureCustom imageSelected)
    {
        return SnapToGrid(new Point(
            _mouseLocation.X - imageSelected.Width / 2,
            _mouseLocation.Y - imageSelected.Height / 2));
    }

    private (int x, int y) GetActorPosition(PictureCustom imageSelected)
    {
        return ((int)(_mouseLocation.X - imageSelected.Width / 2),
            (int)(_mouseLocation.Y - imageSelected.Height / 2));
    }

    #endregion
}