﻿[<AutoOpen>]
module RssReaderFs.Wpf.ViewModel.Util

open System
open System.ComponentModel
open System.Windows
open System.Windows.Input

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

    member internal this.RaisePropertyChanged(names) =
      names |> Seq.iter raisePropertyChanged

    interface INotifyPropertyChanged with
      [<CLIEvent>]
      member this.PropertyChanged = propertyChanged
