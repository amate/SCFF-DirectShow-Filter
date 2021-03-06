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

/// @file SCFF.GUI/MainWindow.xaml.cs
/// @copydoc SCFF::GUI::MainWindow

namespace SCFF.GUI {

    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;
    using Microsoft.Win32;
    using Microsoft.Windows.Shell;
    using SCFF.Common;
    using SCFF.Common.GUI;
    using System.Text;
    using Common.Ext;
    using System.Threading;/// MainWindowのコードビハインド
    public partial class MainWindow
    : Window, IBindingProfile, IBindingOptions, IBindingRuntimeOptions {
  //===================================================================
  // コンストラクタ/Dispose/デストラクタ
  //===================================================================

  /// コンストラクタ
  public MainWindow() {
    this.InitializeComponent();

    // SCFF.Common.ClientApplicationのイベントハンドラ登録
    App.Impl.OnErrorOccured += this.OnErrorOccured;
    App.Impl.OnProfileChanged += this.OnProfileChanged;

    App.Impl.OnProfileClosing += this.OnProfileClosing;
    App.Impl.OnProfileNew += this.OnProfileNew;
    App.Impl.OnProfileOpening += this.OnProfileOpening;
    App.Impl.OnProfileOpened += this.OnProfileOpened;
    App.Impl.OnProfileSaving += this.OnProfileSaving;
    App.Impl.OnProfileSaved += this.OnProfileSaved;
    App.Impl.OnProfileSent += this.OnProfileSent;

    App.Impl.OnDirectoryRefreshed += this.OnDirectoryRefreshed;
    App.Impl.OnCurrentEntryChanged += this.OnCurrentEntryChanged;

    App.ScreenCaptureTimer.Tick += LayoutEdit.OnScreenCaptured;

    TargetWindow.OnDragHereSetTargetWindow += this.OnDragHereSetTargetWindow;
    Area.OnDragHereSetTargetWindow += this.OnDragHereSetTargetWindow;

    this.NotifyOptionsChanged();
    this.NotifyRuntimeOptionsChanged();
    this.NotifyProfileChanged();
  }

  /// デストラクタ
  ~MainWindow() {
    // SCFF.Common.ClientApplicationのイベントハンドラ登録解除
    App.Impl.OnErrorOccured -= this.OnErrorOccured;
    App.Impl.OnProfileChanged -= this.OnProfileChanged;

    App.Impl.OnProfileClosing -= this.OnProfileClosing;
    App.Impl.OnProfileNew -= this.OnProfileNew;
    App.Impl.OnProfileOpening -= this.OnProfileOpening;
    App.Impl.OnProfileOpened -= this.OnProfileOpened;
    App.Impl.OnProfileSaving -= this.OnProfileSaving;
    App.Impl.OnProfileSaved -= this.OnProfileSaved;
    App.Impl.OnProfileSent -= this.OnProfileSent;

    App.Impl.OnDirectoryRefreshed -= this.OnDirectoryRefreshed;
    App.Impl.OnCurrentEntryChanged -= this.OnCurrentEntryChanged;

    App.ScreenCaptureTimer.Tick -= LayoutEdit.OnScreenCaptured;
  }

  //===================================================================
  // SCFF.Common.ClientApplicationイベントハンドラ
  //===================================================================

  /// @copybrief SCFF::Common::ClientApplication::OnErrorOccured
  /// @param[in] sender 使用しない
  /// @param[in] e エラー表示用のデータが格納されたオブジェクト
  private void OnErrorOccured(object sender, ErrorOccuredEventArgs e) {
    if (e.Quiet) return;
    MessageBox.Show(e.Message, "SCFF.GUI",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
  }

  /// @copybrief SCFF::Common::ClientApplication::OnProfileChanged
  /// @param[in] sender 使用しない
  /// @param[in] e 使用しない
  void OnProfileChanged(object sender, System.EventArgs e) {
    //-----------------------------------------------------------------
    // Notify self
    this.OnRuntimeOptionsChanged();
    // Notify other controls
    this.Apply.OnRuntimeOptionsChanged();
    //-----------------------------------------------------------------
  }

  /// @copybrief SCFF::Common::ClientApplication::OnProfileClosing
  /// @param[in] sender 使用しない
  /// @param[in,out] e e.Actionに動作を指定する
  void OnProfileClosing(object sender, ProfileClosingEventArgs e) {
    var message = string.Format("Do you want to save changes to {0}?", e.ProfileName);
    var result =  MessageBox.Show(message,
                                  "SCFF.GUI",
                                  MessageBoxButton.YesNoCancel,
                                  MessageBoxImage.Warning,
                                  MessageBoxResult.Yes);
    switch (result) {
      case MessageBoxResult.Yes: e.Action = CloseActions.Save; break;
      case MessageBoxResult.No: e.Action = CloseActions.Abandon; break;
      case MessageBoxResult.Cancel: e.Action = CloseActions.Cancel; break;
    }
  }

  /// @copybrief SCFF::Common::ClientApplication::OnProfileNew
  /// @param[in] sender 使用しない
  /// @param[in] e 使用しない
  private void OnProfileNew(object sender, System.EventArgs e) {
    //-----------------------------------------------------------------
    // Notify self
    this.OnRuntimeOptionsChanged();
    // Notify other controls
    this.Apply.OnRuntimeOptionsChanged();
    this.NotifyProfileChanged();
    //-----------------------------------------------------------------
  }

  /// @copybrief SCFF::Common::ClientApplication::OnProfileOpening
  /// @param[in] sender 使用しない
  /// @param[in,out] e e.Cancelでキャンセル可能
  private void OnProfileOpening(object sender, ProfileOpeningEventArgs e) {
    // パスが指定されている = ダイアログを開いてパスを指定する必要はない
    if (e.Path != null && e.Path != string.Empty) return;

    // ダイアログでパスを指定
    var dialog = new OpenFileDialog();
    dialog.Title = "SCFF.GUI";
    dialog.Filter = "SCFF Profile|*" + Constants.ProfileExtension;
    dialog.InitialDirectory = e.InitialDirectory;
    var result = dialog.ShowDialog();
    if (result.HasValue && (bool)result) {
      e.Path = dialog.FileName;
    } else {
      // e.Cancelをfalseからtrueに
      e.Cancel = true;
    }
  }

  /// @copybrief SCFF::Common::ClientApplication::OnProfileOpening
  /// @param[in] sender 使用しない
  /// @param[in] e 使用しない
  private void OnProfileOpened(object sender, System.EventArgs e) {
    //-----------------------------------------------------------------
    // Notify self
    this.OnRuntimeOptionsChanged();
    // Notify other controls
    this.Apply.OnRuntimeOptionsChanged();
    this.MainMenu.OnOptionsChanged();
    this.NotifyProfileChanged();
    //-----------------------------------------------------------------
  }

  /// @copybrief SCFF::Common::ClientApplication::OnProfileSaving
  /// @param[in] sender 使用しない
  /// @param[in] e e.Cancelでキャンセル可能
  private void OnProfileSaving(object sender, ProfileSavingEventArgs e) {
    // [保存]で既に一回以上ファイルに保存されている場合はパスの指定は必要ない
    if (e.Action == SaveActions.Save &&
        e.Path != null && e.Path != string.Empty) return;

    // ダイアログでパスを指定
    var dialog = new SaveFileDialog();
    dialog.Title = "SCFF.GUI";
    dialog.Filter = "SCFF Profile|*" + Constants.ProfileExtension;
    dialog.InitialDirectory = e.InitialDirectory;
    dialog.FileName = e.FileName;
    var result = dialog.ShowDialog();
    if (result.HasValue && (bool)result) {
      e.Path = dialog.FileName;
    } else {
      // e.Cancelをfalseからtrueに
      e.Cancel = true;
    }
  }

  /// @copybrief SCFF::Common::ClientApplication::OnProfileSaved
  /// @param[in] sender 使用しない
  /// @param[in] e 使用しない
  private void OnProfileSaved(object sender, System.EventArgs e) {
    //-----------------------------------------------------------------
    // Notify self
    this.OnRuntimeOptionsChanged();
    // Notify other controls
    this.MainMenu.OnOptionsChanged();
    //-----------------------------------------------------------------
  }

  /// @copybrief SCFF::Common::ClientApplication::OnProfileSent
  /// @param[in] sender 使用しない
  /// @param[in] e 使用しない
  private void OnProfileSent(object sender, System.EventArgs e) {
    //-----------------------------------------------------------------
    // Notify self
    // Notify other controls
    this.Apply.OnRuntimeOptionsChanged();
    //-----------------------------------------------------------------
  }

  /// @copybrief SCFF::Common::ClientApplication::OnDirectoryRefreshed
  /// @param[in] sender 使用しない
  /// @param[in] e 使用しない
  private void OnDirectoryRefreshed(object sender, System.EventArgs e) {
    //-----------------------------------------------------------------
    // Notify self
    Commands.SampleSizeChanged.Execute(null, this);
    // Notify other controls
    this.SCFFEntries.OnRuntimeOptionsChanged();
    //-----------------------------------------------------------------
  }

  /// @copybrief SCFF::Common::ClientApplication::OnCurrentEntryChanged
  /// @param[in] sender 使用しない
  /// @param[in] e 使用しない
  private void OnCurrentEntryChanged(object sender, System.EventArgs e) {
    //-----------------------------------------------------------------
    // Notify self
    Commands.SampleSizeChanged.Execute(null, this);
    // Notify other controls
    this.SCFFEntries.OnRuntimeOptionsChanged();
    //-----------------------------------------------------------------
  }

  //===================================================================
  // イベントハンドラ
  //===================================================================

  /// アプリケーション終了時に発生するClosingイベントハンドラ
  protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
    base.OnClosing(e);
    if (!App.Options.SaveProfileOnExit) {
      if (!App.Impl.CloseProfile()) {
        e.Cancel = true;
        return;
      }
    } else {
      if (App.Impl.HasModified) {
        App.Impl.SaveProfile(SaveActions.Save);
      }
    }

    this.SaveTemporaryOptions();
  }

  /// Deactivated
  /// @param e 使用しない
  protected override void OnDeactivated(System.EventArgs e) {
    base.OnDeactivated(e);

    App.ScreenCaptureTimer.TimerPeriod = Math.Max(
        Constants.DefaultLayoutPreviewInterval,
        App.Options.LayoutPreviewInterval);
  }

  /// Activated
  /// @param e 使用しない
  protected override void OnActivated(System.EventArgs e) {
    base.OnActivated(e);

    App.ScreenCaptureTimer.TimerPeriod = App.Options.LayoutPreviewInterval;
  }

  /// Drop
  /// @param e ドラッグアンドドロップされた内容が入っている
  protected override void OnDrop(DragEventArgs e) {
    base.OnDrop(e);
    var files = e.Data.GetData(DataFormats.FileDrop) as string[];
    if (files == null || files.Length == 0) return;
    var path = files[0];
    App.Impl.OpenProfile(path);
  }

  /// StateChanged
  /// @param e 使用しない
  protected override void OnStateChanged(System.EventArgs e) {
    if (!this.CanChangeOptions) return;

    base.OnStateChanged(e);
    App.Options.WindowState = (SCFF.Common.WindowState)this.WindowState;

    //-----------------------------------------------------------------
    // Notify self
    // Notify other controls
    Commands.ProfileVisualChanged.Execute(null, this);
    //-----------------------------------------------------------------
  }

  //-------------------------------------------------------------------
  // *Changed/Checked/Unchecked以外
  //-------------------------------------------------------------------

  //-------------------------------------------------------------------
  // Checked/Unchecked
  //-------------------------------------------------------------------

  //-------------------------------------------------------------------
  // *Changed/Collapsed/Expanded
  //-------------------------------------------------------------------

  /// AreaExpander: Collapsed
  private void AreaExpander_Collapsed(object sender, RoutedEventArgs e) {
    if (!this.CanChangeOptions) return;
    App.Options.AreaIsExpanded = false;
  }
  /// AreaExpander: Expanded
  private void AreaExpander_Expanded(object sender, RoutedEventArgs e) {
    if (!this.CanChangeOptions) return;
    App.Options.AreaIsExpanded = true;
  }
  /// OptionsExpander: Collapsed
  private void OptionsExpander_Collapsed(object sender, RoutedEventArgs e) {
    if (!this.CanChangeOptions) return;
    App.Options.OptionsIsExpanded = false;
  }
  /// OptionsExpander: Expanded
  private void OptionsExpander_Expanded(object sender, RoutedEventArgs e) {
    if (!this.CanChangeOptions) return;
    App.Options.OptionsIsExpanded = true;
  }
  /// ResizeMethodExpander: Collapsed
  private void ResizeMethodExpander_Collapsed(object sender, RoutedEventArgs e) {
    if (!this.CanChangeOptions) return;
    App.Options.ResizeMethodIsExpanded = false;
  }
  /// ResizeMethodExpander: Expanded
  private void ResizeMethodExpander_Expanded(object sender, RoutedEventArgs e) {
    if (!this.CanChangeOptions) return;
    App.Options.ResizeMethodIsExpanded = true;
  }
  /// LayoutExpander: Collapsed
  private void LayoutExpander_Collapsed(object sender, RoutedEventArgs e) {
    if (!this.CanChangeOptions) return;

    //-----------------------------------------------------------------
    // Notify self
    this.UpdateTmpSize();
    App.Options.LayoutIsExpanded = false;
    this.FixMinMaxSize();
    this.FixSize();
    // Notify other controls
    Commands.ProfileVisualChanged.Execute(null, this);
    //-----------------------------------------------------------------
  }
  /// LayoutExpander: Expanded
  private void LayoutExpander_Expanded(object sender, RoutedEventArgs e) {
    if (!this.CanChangeOptions) return;

    //-----------------------------------------------------------------
    // Notify self
    this.UpdateTmpSize();
    App.Options.LayoutIsExpanded = true;
    this.FixMinMaxSize();
    this.FixSize();
    // Notify other controls
    Commands.ProfileVisualChanged.Execute(null, this);
    //-----------------------------------------------------------------
  }

  //===================================================================
  // IBindingOptionsの実装
  //===================================================================

  // 1. Normal        : !LayoutIsExpanded && !CompactView
  // 2. NormalLayout  : LayoutIsExpanded && !CompactView
  // 3. Compact       : !LayoutIsExpanded && CompactView
  // 4. CompactLayout : LayoutIsExpanded && CompactView

  /// Expanderの表示を調整
  private void FixExpanders() {
    if (App.Options.CompactView) {
      this.OptionsExpander.Visibility = Visibility.Collapsed;
      this.ResizeMethodExpander.Visibility = Visibility.Collapsed;
    } else {
      this.OptionsExpander.Visibility = Visibility.Visible;
      this.ResizeMethodExpander.Visibility = Visibility.Visible;
    }
  }

  /// Tmp*の更新
  private void UpdateTmpSize() {
    var isNormal = this.WindowState == System.Windows.WindowState.Normal;
    if (!App.Options.LayoutIsExpanded && !App.Options.CompactView) {
      App.Options.TmpNormalWidth = isNormal ? this.Width : this.RestoreBounds.Width;
      App.Options.TmpNormalHeight = isNormal ? this.Height : this.RestoreBounds.Height;
    } else if (App.Options.LayoutIsExpanded && !App.Options.CompactView) {
      App.Options.TmpNormalLayoutWidth = isNormal ? this.Width : this.RestoreBounds.Width;
      App.Options.TmpNormalLayoutHeight = isNormal ? this.Height : this.RestoreBounds.Height;
    } else if (!App.Options.LayoutIsExpanded && App.Options.CompactView) {
      App.Options.TmpCompactWidth = isNormal ? this.Width : this.RestoreBounds.Width;
      App.Options.TmpCompactHeight = isNormal ? this.Height : this.RestoreBounds.Height;
    } else {
      App.Options.TmpCompactLayoutWidth = isNormal ? this.Width : this.RestoreBounds.Width;
      App.Options.TmpCompactLayoutHeight = isNormal ? this.Height : this.RestoreBounds.Height;
    }
  }

  /// Width/Heightの設定
  private void FixSize() {
    if (!App.Options.LayoutIsExpanded && !App.Options.CompactView) {
      this.Width = App.Options.TmpNormalWidth;
      this.Height = App.Options.TmpNormalHeight;
    } else if (App.Options.LayoutIsExpanded && !App.Options.CompactView) {
      this.Width = App.Options.TmpNormalLayoutWidth;
      this.Height = App.Options.TmpNormalLayoutHeight;
    } else if (!App.Options.LayoutIsExpanded && App.Options.CompactView) {
      this.Width = App.Options.TmpCompactWidth;
      this.Height = App.Options.TmpCompactHeight;
    } else {
      this.Width = App.Options.TmpCompactLayoutWidth;
      this.Height = App.Options.TmpCompactLayoutHeight;
    }
  }

  /// Max/MinWidthの設定
  private void FixMinMaxSize() {
    this.MinHeight = Constants.MinHeight;
    if (!App.Options.LayoutIsExpanded && !App.Options.CompactView) {
      this.MinWidth = Constants.NoLayoutMinWidth;
      this.MaxWidth = Constants.NoLayoutMaxWidth;
      this.MaxHeight = Constants.NormalMaxHeight;
    } else if (App.Options.LayoutIsExpanded && !App.Options.CompactView) {
      this.MinWidth = Constants.LayoutMinWidth;
      this.MaxWidth = Constants.LayoutMaxWidth;;
      this.MaxHeight = Constants.LayoutMaxHeight;
    } else if (!App.Options.LayoutIsExpanded && App.Options.CompactView) {
      this.MinWidth = Constants.NoLayoutMinWidth;
      this.MaxWidth = Constants.NoLayoutMaxWidth;
      this.MaxHeight = Constants.CompactMaxHeight;
    } else {
      this.MinWidth = Constants.LayoutMinWidth;
      this.MaxWidth = Constants.LayoutMaxWidth;;
      this.MaxHeight = Constants.LayoutMaxHeight;
    }
  }

  //-------------------------------------------------------------------

  /// thisを含むすべての子コントロールにOptionsChangedイベント発生を伝える
  public void NotifyOptionsChanged() {
    this.OnOptionsChanged();
    this.Apply.OnOptionsChanged();
    this.LayoutToolbar.OnOptionsChanged();
    this.LayoutEdit.OnOptionsChanged();
    this.MainMenu.OnOptionsChanged();
  }

  /// @copydoc Common::GUI::IBindingOptions::CanChangeOptions
  public bool CanChangeOptions { get; private set; }
  /// @copydoc Common::GUI::IBindingOptions::OnOptionsChanged
  public void OnOptionsChanged() {
    this.CanChangeOptions = false;

    // Temporary
    this.Left         = App.Options.TmpLeft;
    this.Top          = App.Options.TmpTop;
    this.WindowState  = (System.Windows.WindowState)App.Options.WindowState;

    // MainWindow.Controls
    this.AreaExpander.IsExpanded          = App.Options.AreaIsExpanded;
    this.OptionsExpander.IsExpanded       = App.Options.OptionsIsExpanded;
    this.ResizeMethodExpander.IsExpanded  = App.Options.ResizeMethodIsExpanded;
    this.LayoutExpander.IsExpanded        = App.Options.LayoutIsExpanded;

    this.FixMinMaxSize();
    this.FixSize();
    this.FixExpanders();

    App.Impl.SetAero();

    this.CanChangeOptions = true;
  }

  /// UIから設定にデータを保存
  private void SaveTemporaryOptions() {
    // Tmp接頭辞のプロパティだけはここで更新する必要がある
    var isNormal = this.WindowState == System.Windows.WindowState.Normal;
    App.Options.TmpLeft = isNormal ? this.Left : this.RestoreBounds.Left;
    App.Options.TmpTop = isNormal ? this.Top : this.RestoreBounds.Top;
    this.UpdateTmpSize();
  }

  //===================================================================
  // IBindingRuntimeOptionsの実装
  //===================================================================

  /// thisを含むすべての子コントロールにRuntimeOptionsChangedイベント発生を伝える
  public void NotifyRuntimeOptionsChanged() {
    this.OnRuntimeOptionsChanged();
    this.Apply.OnRuntimeOptionsChanged();
    this.LayoutEdit.OnRuntimeOptionsChanged();
    this.LayoutParameter.OnRuntimeOptionsChanged();
    this.SCFFEntries.OnRuntimeOptionsChanged();
  }

  /// @copydoc Common::GUI::IBindingRuntimeOptions::CanChangeRuntimeOptions
  public bool CanChangeRuntimeOptions { get; private set; }
  /// @copydoc Common::GUI::IBindingRuntimeOptions::OnRuntimeOptionsChanged
  public void OnRuntimeOptionsChanged() {
    this.CanChangeRuntimeOptions = false;

    this.WindowTitle.Content = App.Impl.Title;
    this.Title = App.Impl.Title;

    this.CanChangeRuntimeOptions = true;
  }

  //===================================================================
  // IBindingProfileの実装
  //===================================================================

  /// thisを含むすべての子コントロールにCurrentLayoutElementChangedイベント発生を伝える
  public void NotifyCurrentLayoutElementChanged() {
    this.OnCurrentLayoutElementChanged();
    this.TargetWindow.OnCurrentLayoutElementChanged();
    this.Area.OnCurrentLayoutElementChanged();
    this.Options.OnCurrentLayoutElementChanged();
    this.ResizeMethod.OnCurrentLayoutElementChanged();
    this.LayoutParameter.OnCurrentLayoutElementChanged();
    this.LayoutTab.OnCurrentLayoutElementChanged();
    this.LayoutEdit.OnCurrentLayoutElementChanged();
  }

  /// thisを含むすべての子コントロールにRuntimeOptionsChangedイベント発生を伝える
  public void NotifyProfileChanged() {
    this.OnProfileChanged();
    this.TargetWindow.OnProfileChanged();
    this.Area.OnProfileChanged();
    this.Options.OnProfileChanged();
    this.ResizeMethod.OnProfileChanged();
    this.LayoutParameter.OnProfileChanged();
    this.LayoutTab.OnProfileChanged();
    this.LayoutEdit.OnProfileChanged();
  }

  /// @copydoc Common::GUI::IBindingProfile::CanChangeProfile
  public bool CanChangeProfile { get; private set; }

  /// @copydoc Common::GUI::IBindingProfile::OnCurrentLayoutElementChanged
  public void OnCurrentLayoutElementChanged() {
    // Currentのみの更新には対応していない
    this.OnProfileChanged();
  }

  /// @copydoc Common::GUI::IBindingProfile::OnProfileChanged
  public void OnProfileChanged() {
    this.CanChangeProfile = false;
    // nop
    this.CanChangeProfile = true;
  }

  //===================================================================
  // コマンドイベントハンドラ
  //===================================================================

  //-------------------------------------------------------------------
  // ApplicationCommands
  //-------------------------------------------------------------------

  /// New
  private void OnNew(object sender, ExecutedRoutedEventArgs e) {
    App.Impl.NewProfile();
  }

  /// Open
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnOpen(object sender, ExecutedRoutedEventArgs e) {
    var path = e.Parameter as string;
    App.Impl.OpenProfile(path);
  }

  /// Save
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnSave(object sender, ExecutedRoutedEventArgs e) {
    App.Impl.SaveProfile(SaveActions.Save);
  }

  /// SaveAs
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnSaveAs(object sender, ExecutedRoutedEventArgs e) {
    App.Impl.SaveProfile(SaveActions.SaveAs);
  }

  //-------------------------------------------------------------------
  // Windows.Shell.SystemCommands
  //-------------------------------------------------------------------
  
  /// CloseWindow
  private void OnCloseWindow(object sender, ExecutedRoutedEventArgs e) {
    SystemCommands.CloseWindow(this);
  }
  /// MaximizeWindow
  private void OnMaximizeWindow(object sender, ExecutedRoutedEventArgs e) {
    SystemCommands.MaximizeWindow(this);
  }
  /// MinimizeWindow
  private void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs e) {
    SystemCommands.MinimizeWindow(this);
  }
  /// RestoreWindow
  private void OnRestoreWindow(object sender, ExecutedRoutedEventArgs e) {
    SystemCommands.RestoreWindow(this);
  }

  //-------------------------------------------------------------------
  // SCFF.GUI.Commands
  //-------------------------------------------------------------------

  /// @copybrief Commands::CurrentLayoutElementVisualChanged
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnCurrentLayoutElementVisualChanged(object sender, ExecutedRoutedEventArgs e) {
    this.LayoutEdit.OnCurrentLayoutElementChanged();
  }
  /// @copybrief Commands::ProfileVisualChanged
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnProfileVisualChanged(object sender, ExecutedRoutedEventArgs e) {
    this.LayoutEdit.OnOptionsChanged();
    // 内部でOnProfileChangedと同じ処理が走る
  }
  /// @copybrief Commands::ProfileStructureChanged
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnProfileStructureChanged(object sender, ExecutedRoutedEventArgs e) {
    // tabの選択を変えないといけないのでEntireじゃなければいけない
    this.NotifyProfileChanged();
  }
  /// @copybrief Commands::LayoutParameterChanged
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnLayoutParameterChanged(object sender, ExecutedRoutedEventArgs e) {
    this.LayoutParameter.OnCurrentLayoutElementChanged();
  }
  /// @copybrief Commands::TargetWindowChanged
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnTargetWindowChanged(object sender, ExecutedRoutedEventArgs e) {
    this.TargetWindow.OnCurrentLayoutElementChanged();
    // CurrentLayoutElementVisualChanged
    this.LayoutEdit.OnCurrentLayoutElementChanged();
  }
  /// @copybrief Commands::AreaChanged
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnAreaChanged(object sender, ExecutedRoutedEventArgs e) {
    this.Area.OnCurrentLayoutElementChanged();
    // CurrentLayoutElementVisualChanged
    this.LayoutEdit.OnCurrentLayoutElementChanged();
  }
  /// @copybrief Commands::SampleSizeChanged
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnSampleSizeChanged(object sender, ExecutedRoutedEventArgs e) {
    this.LayoutEdit.OnRuntimeOptionsChanged();
    this.LayoutParameter.OnRuntimeOptionsChanged();
  }

  //-------------------------------------------------------------------

  /// @copybrief Commands::AddLayoutElement
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnAddLayoutElement(object sender, ExecutedRoutedEventArgs e) {
    App.Profile.Add();
    // tabの選択を変えないといけないので全てに通知
    this.NotifyProfileChanged();
  }

  /// @copybrief Commands::AddLayoutElement
  /// @warning CanExecuteは処理負荷が高い
  /// @param sender 使用しない
  /// @param e 実行可能かどうかをCanExecuteに設定可能
  private void CanAddLayoutElement(object sender, CanExecuteRoutedEventArgs e) {
    e.CanExecute = App.Profile.CanAdd;
  }

  /// @copybrief Commands::RemoveCurrentLayoutElement
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnRemoveCurrentLayoutElement(object sender, ExecutedRoutedEventArgs e) {
    App.Profile.RemoveCurrent();
    // tabの選択を変えないといけないので全てに通知
    this.NotifyProfileChanged();
  }

  /// @copybrief Commands::RemoveCurrentLayoutElement
  /// @warning CanExecuteは処理負荷が高い
  /// @param sender 使用しない
  /// @param e 実行可能かどうかをCanExecuteに設定可能
  private void CanRemoveCurrentLayoutElement(object sender, CanExecuteRoutedEventArgs e) {
    e.CanExecute = App.Profile.CanRemoveCurrent;
  }

  /// @copybrief Commands::BringCurrentLayoutElementForward
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnBringCurrentLayoutElementForward(object sender, ExecutedRoutedEventArgs e) {
    App.Profile.BringCurrentForward();
    // tabの選択を変えないといけないので全てに通知
    this.NotifyProfileChanged();
  }

  /// @copybrief Commands::BringCurrentLayoutElementForward
  /// @warning CanExecuteは処理負荷が高い
  /// @param sender 使用しない
  /// @param e 実行可能かどうかをCanExecuteに設定可能
  private void CanBringCurrentLayoutElementForward(object sender, CanExecuteRoutedEventArgs e) {
    e.CanExecute = App.Profile.CanBringCurrentForward;
  }

  /// @copybrief Commands::SendCurrentLayoutElementBackward
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnSendCurrentLayoutElementBackward(object sender, ExecutedRoutedEventArgs e) {
    App.Profile.SendCurrentBackward();
    // tabの選択を変えないといけないので全てに通知
    this.NotifyProfileChanged();
  }

  /// @copybrief Commands::BringCurrentLayoutElementForward
  /// @warning CanExecuteは処理負荷が高い
  /// @param sender 使用しない
  /// @param e 実行可能かどうかをCanExecuteに設定可能
  private void CanSendCurrentLayoutElementBackward(object sender, CanExecuteRoutedEventArgs e) {
    e.CanExecute = App.Profile.CanSendCurrentBackward;
  }

  /// @copybrief Commands::FitCurrentBoundRect
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnFitCurrentBoundRect(object sender, ExecutedRoutedEventArgs e) {
    if (!App.Profile.Current.IsWindowValid) {
      Debug.WriteLine("Invalid Window", "[Command] FitCurrentBoundRect");
      return;
    }

    // Profileの設定を変える
    App.Profile.Open();
    App.Profile.Current.FitBoundRelativeRect(
        App.RuntimeOptions.CurrentSampleWidth,
        App.RuntimeOptions.CurrentSampleHeight);
    App.Profile.Close();

    //-----------------------------------------------------------------
    // Notify self
    // Notify other controls
    this.LayoutParameter.OnCurrentLayoutElementChanged();
    this.LayoutEdit.OnCurrentLayoutElementChanged();
    //-----------------------------------------------------------------
  }

  //-------------------------------------------------------------------

  /// @copybrief Commands::SendProfile
  private void OnSendProfile(object sender, ExecutedRoutedEventArgs e) {
    Debug.Assert(App.RuntimeOptions.IsCurrentProcessIDValid);
    App.Impl.SendProfile(false, false);
  }
  /// @copybrief Commands::SendProfile
  private void CanSendProfile(object sender, CanExecuteRoutedEventArgs e) {
    e.CanExecute = App.RuntimeOptions.IsCurrentProcessIDValid;
  }
  /// @copybrief Commands::SendNullProfile
  private void OnSendNullProfile(object sender, ExecutedRoutedEventArgs e) {
    Debug.Assert(App.RuntimeOptions.IsCurrentProcessIDValid);
    App.Impl.SendProfile(false, true);
  }
  /// @copybrief Commands::SendNullProfile
  private void CanSendNullProfile(object sender, CanExecuteRoutedEventArgs e) {
    e.CanExecute = App.RuntimeOptions.IsCurrentProcessIDValid;
  }

  //-------------------------------------------------------------------

  /// @copybrief SCFF::Common::ClientApplication::SetAero
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnSetAero(object sender, ExecutedRoutedEventArgs e) {
    App.Options.ForceAeroOn = (bool)e.Parameter;
    Debug.WriteLine("Execute", "[Command] SetAero");
    App.Impl.SetAero();
  }

  /// Compact表示に変更する
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnSetCompactView(object sender, ExecutedRoutedEventArgs e) {
    Debug.WriteLine("Execute", "[Command] SetCompactView");

    // App.Options.CompactViewの変更前にウィンドウの幅と高さを保存しておく
    this.UpdateTmpSize();
    App.Options.CompactView = (bool)e.Parameter;
    this.FixMinMaxSize();
    this.FixSize();
    this.FixExpanders();
  }

    private void OnDragHereSetTargetWindow(object sender, EventArgs e)
    {
        if (sender == null) {
            if (targetWindowWatchingTimer != null) {
                targetWindowWatchingTimer.Dispose();
                targetWindowWatchingTimer = null;
                targetWindow = UIntPtr.Zero;
            }
            this.TargetWindow.WindowCaption.Text = "(Desktop)";

         } else {
            UIntPtr window = (UIntPtr)sender;
            targetWindow = window;
            // サイズ
            App.Profile.Current.Fit = true;
            var nextScreenRect = App.Profile.Current.ScreenClippingRectWithFit;
            lastTargetWindowRect = nextScreenRect;

            // Profile更新
            App.Profile.Open();
            App.Profile.Current.SetWindowToDesktop();
            App.Profile.Current.SetClippingRectByScreenRect(nextScreenRect);
            App.Profile.Current.ClearBackupParameters();
            App.Profile.Close();

            this.Area.OnCurrentLayoutElementChanged();
            Commands.TargetWindowChanged.Execute(null, this.Area);


            var windowCaption = "n/a";
            if (Common.Utilities.IsWindowValid(window))
            {
                StringBuilder className = new StringBuilder(256);
                User32.GetClassName(window, className, 256);
                windowCaption = className.ToString();
            }
            this.TargetWindow.WindowCaption.Text = "(Desktop) " + windowCaption;

            var context = SynchronizationContext.Current; // UIThread
            targetWindowWatchingTimer = new Timer((state) =>
            {
                var context2 = state as SynchronizationContext;
                context2.Post(this.TargetWindowWatchingCallback, null);
            }, context, 0, 1000);
        }
    }

    private void TargetWindowWatchingCallback(object state)
    {
       if (Common.Utilities.IsWindowValid(targetWindow) == false) {
            if (targetWindowWatchingTimer != null) {
                targetWindowWatchingTimer.Dispose();
                targetWindowWatchingTimer = null;
                targetWindow = UIntPtr.Zero;
            }
            return;
       }
       // サイズ
       App.Profile.Current.Fit = true;
       App.Profile.Current.SetWindow(targetWindow);
       var nextScreenRect = App.Profile.Current.ScreenClippingRectWithFit;
        if (lastTargetWindowRect.X != nextScreenRect.X || lastTargetWindowRect.Y != nextScreenRect.Y
                    || lastTargetWindowRect.Height != nextScreenRect.Height
                    || lastTargetWindowRect.Width != nextScreenRect.Width)
        {
            lastTargetWindowRect = nextScreenRect;
            // Profile更新
            App.Profile.Open();
            App.Profile.Current.SetWindowToDesktop();
            App.Profile.Current.SetClippingRectByScreenRect(nextScreenRect);
            App.Profile.Current.ClearBackupParameters();
            App.Profile.Close();

            this.Area.OnCurrentLayoutElementChanged();
            Commands.TargetWindowChanged.Execute(null, this.Area);

            var windowCaption = "n/a";
            StringBuilder className = new StringBuilder(256);
            User32.GetClassName(targetWindow, className, 256);
            windowCaption = className.ToString();
            this.TargetWindow.WindowCaption.Text = "(Desktop) " + windowCaption;
        }
        else {
            App.Profile.Current.SetWindowToDesktop();
            App.Profile.Current.SetClippingRectByScreenRect(nextScreenRect);
        }
    }

    private UIntPtr targetWindow;
    private Timer targetWindowWatchingTimer;
    private ScreenRect lastTargetWindowRect;
  }
}   // namespace SCFF.GUI
