﻿[<AutoOpen>]
module RssReaderFs.Wpf.ViewModel.Util

open System
open System.Collections.ObjectModel
open System.ComponentModel
open System.Windows
open System.Windows.Input

module Seq =
  let toObservableCollection (xs: seq<_>) =
    ObservableCollection(xs)

module NotifyPropertyChanged =
  let create sender =
    let ev              = Event<_, _>()
    let trigger name    = ev.Trigger(sender, PropertyChangedEventArgs(name))
    in (ev.Publish, trigger)

module Command =
  let create canExecute execute =
    let canExecuteChanged = Event<_, _>()
    let triggerCanExecuteChanged sender =
      canExecuteChanged.Trigger(sender, null)
    let command =
      { new ICommand with
          member this.CanExecute(_) = canExecute ()
          member this.Execute(_) = execute ()
          [<CLIEvent>]
          member this.CanExecuteChanged = canExecuteChanged.Publish
      }
    in (command, triggerCanExecuteChanged)

module WpfViewModel =
  type Base() as this =
    let (propertyChanged, raisePropertyChanged) =
      NotifyPropertyChanged.create this

    member internal this.RaisePropertyChanged(name) =
      name |> raisePropertyChanged

    interface INotifyPropertyChanged with
      [<CLIEvent>]
      member this.PropertyChanged = propertyChanged

  type DialogBase<'t>() =
    inherit Base()

    let mutable dataOpt = (None: option<'t>)

    member private this.Data
      with get ()       = dataOpt
      and  set value    =
        dataOpt <- value
        this.RaisePropertyChanged("Visibility")  // No need to notify change of `Data`

    member this.Visibility =
      match dataOpt with
      | Some _ -> Visibility.Visible
      | None   -> Visibility.Collapsed

    member this.Show(data) =
      this.Data <- Some data

    /// Returns if actually hidden or not.
    member this.Hide() =
      match dataOpt with
      | Some _  ->
          this.Data <- None
          true
      | None -> false
