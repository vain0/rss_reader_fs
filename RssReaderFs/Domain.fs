﻿namespace RssReaderFs

open System
open System.Collections.Generic
open System.Runtime.Serialization

[<AutoOpen>]
module Domain =
  type RssSource =
    {
      [<field: DataMember(Name = "name")>]
      Name: string

      [<field: DataMember(Name = "uri")>]
      Uri: Uri
      
      [<field: DataMember(Name = "lastUpdate")>]
      LastUpdate: DateTime

      // Category, UpdateSpan, etc.
    }

  type RssItem =
    {
      Title: string
      Desc: string option
      Link: string option
      Date: DateTime
      Uri: Uri
    }

  type ObserverId =
    | ObserverId of int

  type RssSubscriber =
    abstract member OnNewItems: RssItem [] -> unit

  type RssReader =
    {
      SourceMap     : Dictionary<Uri, RssSource>
      Subscriptions : Map<ObserverId, RssSubscriber>
    }
