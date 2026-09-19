using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DrawMap.Classes;
using DrawMap.Options;

namespace DrawMap.ChildWindows;

public partial class DrawMap : Window
{
    public static DrawMap Instance;
    private readonly ManagerDrawMap _manager = new();
    public readonly int[] BgH = new int[5];
    public readonly int[] Yb = new int[5];
    public PictureCustom ImageSelected;

    public DrawMap()
    {
        InitializeComponent();
        Instance = this;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _manager.SetParentForm(this);
    }

    public void UpdateSize()
    {
        PanelMap.Height = _manager.HMedium;
        PanelMap.Width = _manager.WMedium;
    }

    public void GetItemMap(int id, bool isNew = true)
    {
        try
        {
            ImageSelected = null;
            if (isNew) _manager.DataMap = new byte[_manager.Tmw * _manager.Tmh];
            _manager.IdTitle = id;
            PanelItems.Children.Clear();
            CreatePicture(Fields.ResourceTitleMap[id]);
            TileIdText.Text = $"Tile ID: {_manager.IdTitle}/{Fields.ResourceTitleMap.Keys.Count}";
            PanelMap.InvalidateVisual();
        }
        catch
        {
        }
    }

    public void GetItemBackground(int page)
    {
        try
        {
            ImageSelected = null;
            PanelItems.Children.Clear();
            foreach (var item in Fields.ResourceItemBackground
                         .OrderBy(kvp => kvp.Key)
                         .Skip(page * Settings.numItemInPage)
                         .Take(Settings.numItemInPage))
                CreatePicture(item.Value);
            TileIdText.Text = $"Page: {page}/{Fields.ResourceItemBackground.Count / Settings.numItemInPage}";
            PanelMap.InvalidateVisual();
        }
        catch
        {
        }
    }

    public void GetItemEffect(int page)
    {
        try
        {
            ImageSelected = null;
            PanelItems.Children.Clear();
            foreach (var item in Fields.ResourceEffect
                         .OrderBy(kvp => kvp.Key)
                         .Skip(page * Settings.numEffectInPage)
                         .Take(Settings.numEffectInPage))
                CreatePicture(item.Value.Image);
            TileIdText.Text = $"Page: {page}/{Fields.ResourceEffect.Count / Settings.numEffectInPage}";
            PanelMap.InvalidateVisual();
        }
        catch
        {
        }
    }

    public void GetItemMonster(int page)
    {
        try
        {
            ManagerDrawMap.Instance.PageMonster = page;
            ImageSelected = null;
            PanelItems.Children.Clear();
            foreach (var item in Fields.ResourceMonster
                         .OrderBy(kvp => kvp.Key)
                         .Skip(page * Settings.numActorInPage)
                         .Take(Settings.numActorInPage))
                CreatePicture(item.Value.Image);
            TileIdText.Text = $"Page: {page}/{Fields.ResourceMonster.Count / Settings.numActorInPage}";
            PanelMap.InvalidateVisual();
        }
        catch
        {
        }
    }

    public void GetItemNpc(int page)
    {
        try
        {
            ImageSelected = null;
            PanelItems.Children.Clear();
            foreach (var item in Fields.ResourceNpc
                         .OrderBy(kvp => kvp.Key)
                         .Skip(page * Settings.numActorInPage)
                         .Take(Settings.numActorInPage))
                CreatePicture(item.Value);
            TileIdText.Text = $"Page: {page}/{Fields.ResourceNpc.Count / Settings.numActorInPage}";
            PanelMap.InvalidateVisual();
        }
        catch
        {
        }
    }

    public void GetItemWaypoint()
    {
        try
        {
            ImageSelected = null;
            PanelItems.Children.Clear();
            PanelItems.Children.Add(Fields.PictureWaypoint);
            TileIdText.Text = "Waypoint";
            PanelMap.InvalidateVisual();
        }
        catch
        {
        }
    }

    private void CreatePicture(List<PictureCustom> list)
    {
        list.Sort();
        foreach (var t in list) CreatePicture(t);
    }


    private void CreatePicture(PictureCustom pic)
    {
        PanelItems.Children.Add(pic);
    }

    private void PrevPageBtn_OnClick(object sender, RoutedEventArgs e)
    {
        NavigatePage(false);
        PanelMap.InvalidateVisual();
    }

    private void NextPageBtn_OnClick(object sender, RoutedEventArgs e)
    {
        NavigatePage(true);
        PanelMap.InvalidateVisual();
    }

    private void NavigatePage(bool isNext)
    {
        if (_manager.TileMapRadio.IsChecked == true)
            HandleTileMapNavigation(isNext);
        else if (_manager.ItemBackgroundRadio.IsChecked == true)
            HandlePageNavigation(ref _manager.PageItemBg, Fields.ResourceItemBackground.Count, Settings.numItemInPage,
                GetItemBackground, isNext);
        else if (_manager.EffectRadio.IsChecked == true)
            HandlePageNavigation(ref _manager.PageEffect, Fields.ResourceEffect.Count, Settings.numEffectInPage,
                GetItemEffect, isNext);
        else if (_manager.NpcRadio.IsChecked == true)
            HandlePageNavigation(ref _manager.PageNpc, Fields.ResourceNpc.Count, Settings.numActorInPage, GetItemNpc,
                isNext);
        else if (_manager.MonsterRadio.IsChecked == true)
            HandlePageNavigation(ref _manager.PageMonster, Fields.ResourceMonster.Count, Settings.numActorInPage,
                GetItemMonster, isNext);
    }

    public void PicOnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is PictureCustom image) ImageSelected = image;
    }

    private void HandleTileMapNavigation(bool isNext)
    {
        var keys = Fields.ResourceTitleMap.Keys;
        if (isNext)
        {
            var maxId = keys.Max();
            _manager.IdTitle = _manager.IdTitle < maxId ? _manager.IdTitle + 1 : keys.Min();
        }
        else
        {
            var minId = keys.Min();
            _manager.IdTitle = _manager.IdTitle > minId ? _manager.IdTitle - 1 : keys.Max();
        }

        GetItemMap(_manager.IdTitle);
    }

    private void HandlePageNavigation(ref int currentPage, int totalItems, int itemsPerPage, Action<int> loadAction,
        bool isNext)
    {
        var maxPage = totalItems / itemsPerPage;

        if (isNext)
            currentPage = currentPage < maxPage ? currentPage + 1 : 0;
        else
            currentPage = currentPage > 0 ? currentPage - 1 : maxPage;

        loadAction(currentPage);
    }
}