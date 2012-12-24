﻿// Copyright 2012 Alalf <alalf.iQLc_at_gmail.com>
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

/// @file SCFF.GUI/Controls/Options.cs
/// 拡大縮小時のオプション

namespace SCFF.GUI.Controls {

using System.Windows;
using System.Windows.Controls;

/// 拡大縮小時のオプション
public partial class Options : UserControl, IProfileToControl {

  /// コンストラクタ
  public Options() {
    InitializeComponent();
  }

  //===================================================================
  // IProfileToControlの実装
  //===================================================================

  public void UpdateByProfile() {
    // checkboxはclickがあるのでeventハンドラをattach/detachする必要はない
    this.ShowCursor.IsChecked = App.Profile.CurrentLayoutElement.ShowCursor;
    this.ShowLayeredWindow.IsChecked = App.Profile.CurrentLayoutElement.ShowLayeredWindow;
    this.KeepAspectRatio.IsChecked = App.Profile.CurrentLayoutElement.KeepAspectRatio;
    this.Stretch.IsChecked = App.Profile.CurrentLayoutElement.Stretch;
    // @todo(me) overSampingとthreadCountはまだDSFでも実装されていない
  }

  public void AttachChangedEventHandlers() {
    // nop
  }

  public void DetachChangedEventHandlers() {
    // nop
  }

  //===================================================================
  // イベントハンドラ
  //===================================================================

  //-------------------------------------------------------------------
  // *Changed/Checked/Unchecked以外
  //-------------------------------------------------------------------

  private void showCursor_Click(object sender, RoutedEventArgs e) {
    if (this.ShowCursor.IsChecked.HasValue) {
      App.Profile.CurrentLayoutElement.ShowCursor = (bool)this.ShowCursor.IsChecked;
    }
  }

  private void showLayeredWindow_Click(object sender, RoutedEventArgs e) {
    if (this.ShowLayeredWindow.IsChecked.HasValue) {
      App.Profile.CurrentLayoutElement.ShowLayeredWindow = (bool)this.ShowLayeredWindow.IsChecked;
    }
  }

  private void keepAspectRatio_Click(object sender, RoutedEventArgs e) {
    if (this.KeepAspectRatio.IsChecked.HasValue) {
      App.Profile.CurrentLayoutElement.KeepAspectRatio = (bool)this.KeepAspectRatio.IsChecked;
    }
  }

  private void stretch_Click(object sender, RoutedEventArgs e) {
    if (this.Stretch.IsChecked.HasValue) {
      App.Profile.CurrentLayoutElement.Stretch = (bool)this.Stretch.IsChecked;
    }
  }
}
}