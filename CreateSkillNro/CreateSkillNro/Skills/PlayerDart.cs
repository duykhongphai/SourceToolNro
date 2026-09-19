using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CreateSkillNro.ChildWindows;
using CreateSkillNro.Classes;

namespace CreateSkillNro.Skills;

public class PlayerDart
{
    private readonly double _scale;
    private int _angle;
    private List<SmallDart> _darts = [];
    private int _dxDart;
    private int _dyDart;
    private bool _isNearChar;
    private int _va;
    private int _vx;
    private int _vy;
    private short _xDart;
    private short _xDartRaw;
    private int _xEnd;
    private short _yDart;
    private short _yDartRaw;
    private int _yEnd;
    public DartInfo Dart;

    public PlayerDart(DartInfo dart, double scale, short x, short y, short xEnd, short yEnd)
    {
        _scale = scale;
        _xEnd = xEnd;
        _yEnd = yEnd;
        Dart = dart;
        _xDartRaw = _xDart = x;
        _yDartRaw = _yDart = y;
        _angle = Function.Angle(xEnd - _xDart, 0);
        _va = dart.Va;
        _vx = (_va * Function.Cos(_angle)) >> 10;
        _vy = (_va * Function.Sin(_angle)) >> 10;
        _isNearChar = false;
    }

    public void Renew(Rect bounds)
    {
        _xEnd = (short)(bounds.Width - bounds.Width * 0.28);
        _yEnd = (short)(bounds.Height / 2);
        _xDartRaw = _xDart = (short)(bounds.Width / 4);
        _yDartRaw = _yDart = (short)(bounds.Height / 2);
        _angle = Function.Angle(_xEnd - _xDart, 0);
        _vx = (_va * Function.Cos(_angle)) >> 10;
        _vy = (_va * Function.Sin(_angle)) >> 10;
        _isNearChar = false;
    }

    public void Renew(short x, short y, short xEnd, short yEnd)
    {
        _xEnd = xEnd;
        _yEnd = yEnd;
        _xDartRaw = _xDart = x;
        _yDartRaw = _yDart = y;
        _angle = Function.Angle(_xEnd - _xDart, 0);
        _vx = (_va * Function.Cos(_angle)) >> 10;
        _vy = (_va * Function.Sin(_angle)) >> 10;
        _isNearChar = false;
    }

    public void Render(DrawingContext context)
    {
        try
        {
            var num = Function.FindDirIndexFromAngle(360 - _angle);
            int num2 = Function.Frame[num];
            for (var i = _darts.Count / 2; i < _darts.Count; i++)
            {
                var smallDart = _darts[i];
                if (FormMainEffect.Instance.Images.TryGetValue(Dart.TailBorder[smallDart.Index], out var image))
                    DrawSimple(context, image, smallDart.X, smallDart.Y);
            }

            var num3 = Function.NextInt(Dart.HeadBorder.Count);
            if (FormMainEffect.Instance.Images.TryGetValue(Dart.HeadBorder[num3][num2], out var image2))
                DrawSimple(context, image2, _xDart, _yDart);

            foreach (var smallDart2 in _darts)
                if (FormMainEffect.Instance.Images.TryGetValue(Dart.Tail[smallDart2.Index], out var image3))
                    DrawSimple(context, image3, smallDart2.X, smallDart2.Y);

            if (FormMainEffect.Instance.Images.TryGetValue(Dart.Head[num3][num2], out var image4))
                DrawSimple(context, image4, _xDart, _yDart);
            foreach (var smallDart3 in _darts)
            {
                if (Math.Abs(Function.NextInt(0, 100)) >= Dart.XdPercent) continue;
                var idImage = Function.NextInt(0, 100) % 2 == 0
                    ? Dart.Xd1[smallDart3.Index]
                    : Dart.Xd2[smallDart3.Index];
                if (FormMainEffect.Instance.Images.TryGetValue(idImage, out var image5))
                    DrawSimple(context, image5, smallDart3.X, smallDart3.Y);
            }
        }
        catch
        {
        }
    }

    private void DrawSimple(DrawingContext context, Bitmap image, int x, int y)
    {
        var sourceSize = new Size(image.Size.Width, image.Size.Height);
        var scaledSize = sourceSize * _scale;
        var destRect = new Rect(x, y, scaledSize.Width, scaledSize.Height);
        var sourceRect = new Rect(sourceSize);
        context.DrawImage(image, sourceRect, destRect);
    }

    public void UpdatePos(short x, short y)
    {
        _xDartRaw = _xDart = x;
        _yDartRaw = _yDart = y;
        _yEnd = y;
        Renew();
    }

    public void Renew()
    {
        _xDart = _xDartRaw;
        _yDart = _yDartRaw;
        _angle = Function.Angle(_xEnd - _xDart, 0);
        _va = Dart.Va;
        _vx = (_va * Function.Cos(_angle)) >> 10;
        _vy = (_va * Function.Sin(_angle)) >> 10;
    }

    public void Update()
    {
        if (_isNearChar)
        {
            Renew();
            _isNearChar = false;
            return;
        }

        for (var i = 0; i < Dart.NUpdate; i++)
        {
            if (Dart.Tail.Count > 0) _darts.Add(new SmallDart(_xDart, _yDart));
            const int num = -10;
            _dxDart = (short)(_xEnd + num - _xDart);
            _dyDart = (short)(_yEnd - 16 - _yDart);
            if (Math.Abs(_dxDart) < 20 && Math.Abs(_dyDart) < 20) _isNearChar = true;
            var num2 = Function.Angle(_dxDart, _dyDart);
            if (Math.Abs(num2 - _angle) < 90 || _dxDart * _dxDart + _dyDart * _dyDart > 4096)
            {
                if (Math.Abs(num2 - _angle) < 15)
                    _angle = num2;
                else if ((num2 - _angle >= 0 && num2 - _angle < 180) || num2 - _angle < -180)
                    _angle = Function.FixAngle(_angle + 15);
                else
                    _angle = Function.FixAngle(_angle - 15);
            }

            if (_va < 8192) _va += 1024;
            _vx = (_va * Function.Cos(_angle)) >> 10;
            _vy = (_va * Function.Sin(_angle)) >> 10;
            _dxDart += _vx;
            var num3 = _dxDart >> 10;
            _xDart += (short)num3;
            _dxDart &= 1023;
            _dyDart += _vy;
            var num4 = _dyDart >> 10;
            _yDart += (short)num4;
            _dyDart &= 1023;
        }

        for (var j = 0; j < _darts.Count; j++)
        {
            var smallDart = _darts[j];
            smallDart.Index++;
            if (smallDart.Index >= Dart.Tail.Count) _darts.RemoveAt(j);
        }
    }

    public void Dispose()
    {
        Dart = null;
        _darts.Clear();
        _darts = null;
    }
}