using System;
using Avalonia.Controls;
using Avalonia.Input;
using ToolEffectNro.Classes;

namespace ToolEffectNro.ChildWindows;

public partial class FormSkillEffect : Window
{
    public static FormSkillEffect Instance;

    public FormSkillEffect()
    {
        InitializeComponent();
        Instance = this;
    }


    private void InputElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.F5) return;
        PanelMain.Data = FormMainEffect.Instance.ArrayFrame.ToArray();
        PanelMain.StartViewEff = !PanelMain.StartViewEff;
        if (PanelMain.StartViewEff)
            PanelMain.UpdateEffect.Start();
        else
            PanelMain.UpdateEffect.Stop();
        PanelMain.InvalidateVisual();
    }
}