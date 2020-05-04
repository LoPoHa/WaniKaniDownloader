namespace WaniKaniDownloader.Gui

open Avalonia.Controls
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout

module Settings = 
    
    type State = { wanikani_apikey: string }
    let init = { wanikani_apikey = "ABC" }

    type Msg = SetWaniKaniApiKey of string
             | Reset

    let update (msg: Msg) (state: State) : State =
        match msg with
        | SetWaniKaniApiKey key -> { state with wanikani_apikey = key }
        | Reset -> init

    let view (state: State) (dispatch) =
        DockPanel.create [
            DockPanel.children [
                TextBox.create [
                    TextBox.watermark "WaniKani API Key V2 (Read only is sufficient)"
                    TextBox.text state.wanikani_apikey
                    TextBox.fontSize 48.0
                    TextBox.dock Dock.Top
                ]
                Button.create [
                    Button.dock Dock.Bottom
                    Button.onClick (fun _ -> dispatch Reset)
                    Button.content "reset"
                ]                
                Button.create [
                    Button.dock Dock.Bottom
                    Button.onClick (fun _ -> dispatch (SetWaniKaniApiKey "todo"))
                    Button.content "+"
                ]
                TextBlock.create [
                    TextBlock.dock Dock.Top
                    TextBlock.fontSize 48.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.text (string state.wanikani_apikey)
                ]
            ]
        ]
