﻿namespace RssReaderFs.Core

open System
open System.Linq
open Chessie.ErrorHandling

module RssFeed =
  let downloadAsync srcId url =
    async {
      let! xml = Net.downloadXmlAsync(url)
      return (xml |> Article.parseXml srcId)
    }

  let validate url =
    Trial.runRaisable (fun () ->
      // source id はダミー値で問題ない
      url |> downloadAsync 0L |> Async.RunSynchronously
      )
    |> Trial.lift (fun _ -> ())
    |> Trial.mapFailure (List.map ExnError)
