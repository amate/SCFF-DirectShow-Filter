﻿// Copyright 2012-2013 Alalf <alalf.iQLc_at_gmail.com>
//
// This file is part of SCFF-DirectShow-Filter(SCFF DSF).
//
// SCFF DSF is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SCFF DSF is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with SCFF DSF.  If not, see <http://www.gnu.org/licenses/>.

/// @file SCFF.GUI/Controls/Area.xaml.cs
/// クリッピング領域設定用UserControl

namespace SCFF.GUI.Controls {

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SCFF.Common;
using SCFF.Common.Ext;

/// クリッピング領域設定用UserControl
public partial class Area : UserControl, IUpdateByProfile {
  //===================================================================
  // 定数
  //===================================================================

  /// 仮想画面をスクリーン座標系で表したRect
  /// @todo(me) 現在Desktop/DesktopListViewで使い回ししているが、問題が発生する可能性あり
  private static readonly Rect VirtualScreenRect = new Rect {
    X       = User32.GetSystemMetrics(User32.SM_XVIRTUALSCREEN),
    Y       = User32.GetSystemMetrics(User32.SM_YVIRTUALSCREEN),
    Width   = User32.GetSystemMetrics(User32.SM_CXVIRTUALSCREEN),
    Height  = User32.GetSystemMetrics(User32.SM_CYVIRTUALSCREEN)
  };

  //===================================================================
  // コンストラクタ/Loaded/Closing/ShutdownStartedイベントハンドラ
  //===================================================================

  /// コンストラクタ
  public Area() {
    InitializeComponent();
  }

  //===================================================================
  // イベントハンドラ
  //===================================================================

  //-------------------------------------------------------------------
  // *Changed/Checked/Unchecked以外
  //-------------------------------------------------------------------

  /// Fit: Click
  private void Fit_Click(object sender, RoutedEventArgs e) {
    if (!this.Fit.IsChecked.HasValue) return;

    App.Profile.CurrentOutputLayoutElement.Fit = (bool)this.Fit.IsChecked;
    this.UpdateByCurrentProfile();
  }

  //-------------------------------------------------------------------

  /// 現在編集中のレイアウト要素のクリッピング領域/Fitオプションを変更する
  private void CommonAreaSelect(Rect boundScreenRect, WindowTypes nextWindowType) {
    // ダイアログの準備
    var dialog = new AreaSelectWindow();
    dialog.Left   = App.Profile.CurrentInputLayoutElement.ScreenClippingXWithFit;
    dialog.Top    = App.Profile.CurrentInputLayoutElement.ScreenClippingYWithFit;
    dialog.Width  = App.Profile.CurrentInputLayoutElement.ClippingWidthWithFit;
    dialog.Height = App.Profile.CurrentInputLayoutElement.ClippingHeightWithFit;

    // カラーの変更
    switch (nextWindowType) {
      case WindowTypes.Normal: {
        // 更に現在のTypeによって色を分ける
        switch (App.Profile.CurrentInputLayoutElement.WindowType) {
          case WindowTypes.DesktopListView: {
            dialog.WindowBorder.BorderBrush = BrushesAndPens.CurrentDesktopListViewBrush;
            dialog.WindowGrid.Background = BrushesAndPens.DesktopListViewBrush;
            break;
          }
          case WindowTypes.Desktop: {
            dialog.WindowBorder.BorderBrush = BrushesAndPens.CurrentDesktopBrush;
            dialog.WindowGrid.Background = BrushesAndPens.DesktopBrush;
            break;
          }
          default: {
            dialog.WindowBorder.BorderBrush = BrushesAndPens.CurrentNormalBrush;
            dialog.WindowGrid.Background = BrushesAndPens.NormalBrush;
            break;
          }
        }
        break;
      }
      case WindowTypes.DesktopListView: {
        dialog.WindowBorder.BorderBrush = BrushesAndPens.CurrentDesktopListViewBrush;
        dialog.WindowGrid.Background = BrushesAndPens.DesktopListViewBrush;
        break;
      }
      case WindowTypes.Desktop: {
        dialog.WindowBorder.BorderBrush = BrushesAndPens.CurrentDesktopBrush;
        dialog.WindowGrid.Background = BrushesAndPens.DesktopBrush;
        break;
      }
      default : {
        Debug.Fail("CommonAreaSelect: Invalid WindowTypes");
        break;
      }
    }

    // ダイアログの表示
    var result = dialog.ShowDialog();
    if (!result.HasValue || !(bool)result) return;

    // 結果をRECTにまとめる
    var nextScreenRect = new Rect(dialog.Left, dialog.Top, dialog.Width, dialog.Height);

    // ウィンドウの領域とIntersectをとる
    if (!nextScreenRect.IntersectsWith(boundScreenRect)) return;
    nextScreenRect.Intersect(boundScreenRect);

    // 結果をProfileに書き込み

    switch (nextWindowType) {
      case WindowTypes.Normal: {
        // nop
        break;
      }
      case WindowTypes.Desktop: {
        App.Profile.CurrentOutputLayoutElement.SetWindowToDesktop();
        break;
      }
      case WindowTypes.DesktopListView: {
        App.Profile.CurrentOutputLayoutElement.SetWindowToDesktopListView();
        break;
      }
      default : {
        Debug.Fail("CommonAreaSelect: Invalid WindowTypes");
        break;
      }
    }
    App.Profile.CurrentOutputLayoutElement.Fit = false;
    App.Profile.CurrentOutputLayoutElement.ClippingXWithoutFit =
        (int)(nextScreenRect.X - boundScreenRect.Left);
    App.Profile.CurrentOutputLayoutElement.ClippingYWithoutFit = 
        (int)(nextScreenRect.Y - boundScreenRect.Top);
    App.Profile.CurrentOutputLayoutElement.ClippingWidthWithoutFit =
        (int)nextScreenRect.Width;
    App.Profile.CurrentOutputLayoutElement.ClippingHeightWithoutFit =
        (int)nextScreenRect.Height;
    
    // コマンドをMainWindowに送信して関連するコントロールを更新
    UpdateCommands.UpdateTargetWindowByCurrentProfile.Execute(null, null);
  }

  /// AreaSelect: Click
  private void AreaSelect_Click(object sender, RoutedEventArgs e) {
    var boundScreenRect = new Rect {
      X = App.Profile.CurrentInputLayoutElement.ScreenWindowX,
      Y = App.Profile.CurrentInputLayoutElement.ScreenWindowY,
      Width = App.Profile.CurrentInputLayoutElement.WindowWidth,
      Height = App.Profile.CurrentInputLayoutElement.WindowHeight
    };
    this.CommonAreaSelect(boundScreenRect, WindowTypes.Normal);
  }

  /// ListView: Click
  private void ListView_Click(object sender, RoutedEventArgs e) {
    this.CommonAreaSelect(Area.VirtualScreenRect, WindowTypes.DesktopListView);
  }

  /// Desktop: Click
  private void Desktop_Click(object sender, RoutedEventArgs e) {
    this.CommonAreaSelect(Area.VirtualScreenRect, WindowTypes.Desktop);
  }

  //-------------------------------------------------------------------
  // Checked/Unchecked
  //-------------------------------------------------------------------

  /// Fit: Checked
  private void Fit_Checked(object sender, RoutedEventArgs e) {
    this.ClippingX.IsEnabled = false;
    this.ClippingY.IsEnabled = false;
    this.ClippingWidth.IsEnabled = false;
    this.ClippingHeight.IsEnabled = false;
  }

  /// Fit: Unchecked
  private void Fit_Unchecked(object sender, RoutedEventArgs e) {
    this.ClippingX.IsEnabled = true;
    this.ClippingY.IsEnabled = true;
    this.ClippingWidth.IsEnabled = true;
    this.ClippingHeight.IsEnabled = true;
  }

  //-------------------------------------------------------------------
  // *Changed/Collapsed/Expanded
  //-------------------------------------------------------------------

  /// 下限・上限つきでテキストボックスから値を取得する
  private bool TryParseClippingParameters(TextBox textBox,
      int lowerBound, int upperBound, out int parsedValue) {
    // Parse
    if (!int.TryParse(textBox.Text, out parsedValue)) {
      parsedValue = lowerBound;
      textBox.Text = lowerBound.ToString();
      return false;
    }

    // Validation
    if (parsedValue < lowerBound) {
      parsedValue = lowerBound;
      textBox.Text = lowerBound.ToString();
      return false;
    } else if (parsedValue > upperBound) {
      parsedValue = upperBound;
      textBox.Text = upperBound.ToString();
      return false;
    }

    return true;
  }

  /// ClippingX: TextChanged
  private void ClippingX_TextChanged(object sender, TextChangedEventArgs e) {
    var lowerBound = 0;
    var upperBound = App.Profile.CurrentInputLayoutElement.WindowWidth;
    int parsedValue;
    if (this.TryParseClippingParameters(this.ClippingX, lowerBound, upperBound, out parsedValue)) {
      // Profileに書き込み
      App.Profile.CurrentOutputLayoutElement.ClippingXWithoutFit = parsedValue;
    }
  }

  /// ClippingY: TextChanged
  private void ClippingY_TextChanged(object sender, TextChangedEventArgs e) {
    var lowerBound = 0;
    var upperBound = App.Profile.CurrentInputLayoutElement.WindowHeight;
    int parsedValue;
    if (this.TryParseClippingParameters(this.ClippingY, lowerBound, upperBound, out parsedValue)) {
      // Profileに書き込み
      App.Profile.CurrentOutputLayoutElement.ClippingYWithoutFit = parsedValue;
    }
  }

  /// ClippingWidth: TextChanged
  private void ClippingWidth_TextChanged(object sender, TextChangedEventArgs e) {
    var lowerBound = 0;
    var upperBound = App.Profile.CurrentInputLayoutElement.WindowWidth;
    int parsedValue;
    if (this.TryParseClippingParameters(this.ClippingWidth, lowerBound, upperBound, out parsedValue)) {
      // Profileに書き込み
      App.Profile.CurrentOutputLayoutElement.ClippingWidthWithoutFit = parsedValue;
    }
  }

  /// ClippingHeight: TextChanged
  private void ClippingHeight_TextChanged(object sender, TextChangedEventArgs e) {
    var lowerBound = 0;
    var upperBound = App.Profile.CurrentInputLayoutElement.WindowHeight;
    int parsedValue;
    if (this.TryParseClippingParameters(this.ClippingHeight, lowerBound, upperBound, out parsedValue)) {
      // Profileに書き込み
      App.Profile.CurrentOutputLayoutElement.ClippingHeightWithoutFit = parsedValue;
    }
  }

  //===================================================================
  // IUpdateByProfileの実装
  //===================================================================

  /// @copydoc IUpdateByProfile::UpdateByCurrentProfile
  public void UpdateByCurrentProfile() {
    // Enabled/Disabled
    switch (App.Profile.CurrentInputLayoutElement.WindowType) {
      case WindowTypes.Normal: {
        this.ListView.IsEnabled = true;
        this.Desktop.IsEnabled = true;
        break;
      }
      case WindowTypes.Desktop: {
        this.ListView.IsEnabled = true;
        this.Desktop.IsEnabled = false;
        break;
      }
      case WindowTypes.DesktopListView: {
        this.ListView.IsEnabled = false;
        this.Desktop.IsEnabled = true;
        break;
      }
    }

    // checkboxはclickがあるのでeventハンドラをattach/detachする必要はない
    this.Fit.IsChecked = App.Profile.CurrentInputLayoutElement.Fit;

    // *Changed
    this.DetachProfileChangedEventHandlers();
    this.ClippingX.Text = App.Profile.CurrentInputLayoutElement.ClippingXWithFit.ToString();
    this.ClippingY.Text = App.Profile.CurrentInputLayoutElement.ClippingYWithFit.ToString();
    this.ClippingWidth.Text = App.Profile.CurrentInputLayoutElement.ClippingWidthWithFit.ToString();
    this.ClippingHeight.Text = App.Profile.CurrentInputLayoutElement.ClippingHeightWithFit.ToString();
    this.AttachProfileChangedEventHandlers();
  }

  /// @copydoc IUpdateByProfile::UpdateByEntireProfile
  public void UpdateByEntireProfile() {
    // current以外のデータを表示する必要はない
    this.UpdateByCurrentProfile();
  }

  /// @copydoc IUpdateByProfile::AttachProfileChangedEventHandlers
  public void AttachProfileChangedEventHandlers() {
    this.ClippingX.TextChanged += ClippingX_TextChanged;
    this.ClippingY.TextChanged += ClippingY_TextChanged;
    this.ClippingWidth.TextChanged += ClippingWidth_TextChanged;
    this.ClippingHeight.TextChanged += ClippingHeight_TextChanged;
  }

  /// @copydoc IUpdateByProfile::DetachProfileChangedEventHandlers
  public void DetachProfileChangedEventHandlers() {
    this.ClippingX.TextChanged -= ClippingX_TextChanged;
    this.ClippingY.TextChanged -= ClippingY_TextChanged;
    this.ClippingWidth.TextChanged -= ClippingWidth_TextChanged;
    this.ClippingHeight.TextChanged -= ClippingHeight_TextChanged;
  }
}
}   // namespace SCFF.GUI.Controls
