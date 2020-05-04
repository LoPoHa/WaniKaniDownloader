namespace WaniKaniDownloader.Gui

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout

module MainGUI = 
    
    type State = { settings: Settings.State }
    let init = { settings = Settings.init }

    type Msg = SettingsMsg of Settings.Msg

    let update (msg: Msg) (state: State) : State =
        match msg with
        | SettingsMsg smsg ->
            { state with settings = Settings.update smsg state.settings }

    let view (state: State) (dispatch) =
        TabControl.create [
            TabControl.tabStripPlacement Dock.Left
            TabControl.viewItems [
                TabItem.create [
                    TabItem.header "Settings"
                    TabItem.content (Settings.view state.settings (SettingsMsg >> dispatch))
                ]
                TabItem.create [
                    TabItem.header "TODO"
                ]
            ]
        ]