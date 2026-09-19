using Avalonia;
using Avalonia.Media;
using CreateSkillNro.ChildWindows;
using CreateSkillNro.Classes;

namespace CreateSkillNro.Skills;

public class SkillRender(SkillPaint skillPaint, double scale, bool isEditor, Size size)
{
    private short _dartId;
    private short _dx0;
    private short _dx1;
    private short _dx2;
    private short _dy0;
    private short _dy1;
    private short _dy2;

    private EffectCharPaint _eff0;
    private EffectCharPaint _eff1;
    private EffectCharPaint _eff2;
    private short _i0;
    private short _i1;
    private short _i2;
    private bool _isPreviewAll;
    private Size _size = size;
    public short IndexSkill;
    public PlayerDart SkillDartPreview;
    public SkillPaint SkillPaint = skillPaint;

    public void Renew(SkillPaint skillPaint2)
    {
        SkillPaint = skillPaint2;
        SkillDartPreview?.Dispose();
        SkillDartPreview = null;
        _eff0 = _eff1 = _eff2 = null;
        IndexSkill = 0;
        _dartId = _dy2 = _dx2 = _dy1 = _dx1 = _dy0 = _dx0 = _i2 = _i1 = _i0 = 0;
    }

    public void SetPreviewAll()
    {
        _isPreviewAll = true;
    }

    public void Renew(Size size)
    {
        _size = size;
        SkillDartPreview?.Dispose();
        SkillDartPreview = null;
    }

    public void Dispose()
    {
        _eff0 = null;
        _eff1 = null;
        _eff2 = null;
        SkillDartPreview?.Dispose();
        SkillDartPreview = null;
    }

    public void Update()
    {
        try
        {
            if (SkillPaint.SkillStand == null || SkillPaint.SkillStand.Count == 0)
                return;
            if (isEditor && !FormSkillEffect.Instance.SkillPanelFrame.IsPreview)
                IndexSkill = (short)FormSkillEffect.Instance.IdSkillStandEdit;
            if (IndexSkill >= SkillPaint.SkillStand.Count) IndexSkill = 0;
            if (SkillPaint.SkillStand[IndexSkill].EffS0Id != 0 &&
                FormMainEffect.Instance.EffectCharPaints.TryGetValue(
                    (short)(SkillPaint.SkillStand[IndexSkill].EffS0Id - 1),
                    out var paint2))
            {
                _eff0 = paint2;
                if (!isEditor) _i0 = _dx0 = _dy0 = 0;
            }

            if (SkillPaint.SkillStand[IndexSkill].EffS1Id != 0 &&
                FormMainEffect.Instance.EffectCharPaints.TryGetValue(
                    (short)(SkillPaint.SkillStand[IndexSkill].EffS1Id - 1),
                    out var paint3))
            {
                _eff1 = paint3;
                if (!isEditor) _i1 = _dx1 = _dy1 = 0;
            }

            if (SkillPaint.SkillStand[IndexSkill].EffS2Id != 0 &&
                FormMainEffect.Instance.EffectCharPaints.TryGetValue(
                    (short)(SkillPaint.SkillStand[IndexSkill].EffS2Id - 1),
                    out var paint4))
            {
                _eff2 = paint4;
                if (!isEditor) _i2 = _dx2 = _dy2 = 0;
            }

            if (!isEditor && !_isPreviewAll) return;
            if (SkillPaint.SkillStand?[IndexSkill] == null || IndexSkill < 0 ||
                IndexSkill > SkillPaint.SkillStand.Count - 1) return;
            var arrowId = (short)(SkillPaint.SkillStand[IndexSkill].ArrowId - 100);
            if (arrowId < 0)
            {
                _dartId = 0;
                SkillDartPreview?.Dispose();
                SkillDartPreview = null;
                return;
            }

            SkillDartPreview?.Update();
            if (_dartId == arrowId) return;
            _dartId = arrowId;
            if (!FormMainEffect.Instance.DartInfos.TryGetValue(arrowId, out var dart))
                return;
            if (SkillDartPreview == null)
            {
                var xBase = _isPreviewAll ? _size.Width / 4 : _size.Width / 2;
                var xEnd = _isPreviewAll
                    ? (short)(_size.Width - _size.Width / 4)
                    : (short)(_size.Width - _size.Width * 0.28);
                SkillDartPreview = new PlayerDart(dart, 1,
                    (short)(xBase + SkillPaint.SkillStand[IndexSkill].Adx),
                    (short)(_size.Height / 1.2 + SkillPaint.SkillStand[IndexSkill].Ady * 8.5),
                    xEnd, (short)(_size.Height / 1.2 + SkillPaint.SkillStand[IndexSkill].Ady * 8.5));
            }
            else
            {
                SkillDartPreview.Dart = dart;
                SkillDartPreview.Renew();
            }
        }
        catch
        {
        }
    }

    public void Render(DrawingContext context)
    {
        try
        {
            if (SkillPaint.SkillStand == null || SkillPaint.SkillStand.Count == 0) return;
            var array = SkillPaint.SkillStand;
            if (_eff0 is { ArrEffInfo: not null } && _i0 < _eff0.ArrEffInfo.Count &&
                FormMainEffect.Instance.Images.TryGetValue(_eff0.ArrEffInfo[_i0].IdImg, out var paintImage) &&
                IndexSkill < array.Count)
            {
                if (_dx0 == 0) _dx0 = array[IndexSkill].E0dx;
                if (_dy0 == 0) _dy0 = array[IndexSkill].E0dy;
                Function.DrawSimple(context, paintImage, _dx0 + _eff0.ArrEffInfo[_i0].Dx,
                    _dy0 + _eff0.ArrEffInfo[_i0].Dy, isEditor, _size, scale, _isPreviewAll);
                _i0++;
                if (_i0 >= _eff0.ArrEffInfo.Count)
                {
                    _eff0 = null;
                    _i0 = _dx0 = _dy0 = 0;
                }
            }

            if (_eff1 is { ArrEffInfo: not null } && _i1 < _eff1.ArrEffInfo.Count &&
                FormMainEffect.Instance.Images.TryGetValue(_eff1.ArrEffInfo[_i1].IdImg, out var paintImage2) &&
                IndexSkill < array.Count)
            {
                if (_dx1 == 0) _dx1 = array[IndexSkill].E1dx;
                if (_dy1 == 0) _dy1 = array[IndexSkill].E1dy;
                Function.DrawSimple(context, paintImage2, _dx1 + _eff1.ArrEffInfo[_i1].Dx,
                    _dy1 + _eff1.ArrEffInfo[_i1].Dy, isEditor, _size, scale, _isPreviewAll);
                _i1++;
                if (_i1 >= _eff1.ArrEffInfo.Count)
                {
                    _eff1 = null;
                    _i1 = _dx1 = _dy1 = 0;
                }
            }

            if (_eff2 is { ArrEffInfo: not null } && _i2 < _eff2.ArrEffInfo.Count &&
                FormMainEffect.Instance.Images.TryGetValue(_eff2.ArrEffInfo[_i2].IdImg, out var paintImage3) &&
                IndexSkill < array.Count)
            {
                if (_dx2 == 0) _dx2 = array[IndexSkill].E2dx;
                if (_dy2 == 0) _dy2 = array[IndexSkill].E2dy;
                Function.DrawSimple(context, paintImage3, _dx2 + _eff2.ArrEffInfo[_i2].Dx,
                    _dy2 + _eff2.ArrEffInfo[_i2].Dy, isEditor, _size, scale, _isPreviewAll);
                _i2++;
                if (_i2 >= _eff2.ArrEffInfo.Count)
                {
                    _eff2 = null;
                    _i2 = _dx2 = _dy2 = 0;
                }
            }

            SkillDartPreview?.Render(context);
            if (!isEditor || FormSkillEffect.Instance.SkillPanelFrame.IsPreview) IndexSkill++;
        }
        catch
        {
        }
    }
}