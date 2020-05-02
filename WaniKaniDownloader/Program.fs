// Learn more about F# at http://fsharp.org

open System.Text
open FSharp.Data
open FSharp.Json
open System.IO

type Pages = { per_page : int64
               next_url : string option
               previous_url: string option }

type Meaning = { meaning : string
                 primary : bool
                 accepted_answer : bool }

type AuxiliaryMeanings = {
    [<JsonField("type")>]
    typ : string
    meaning : string
}

type Reading = { primary : bool
                 reading : string
                 accepted_answer : bool }

type ContextSentence = {
    [<JsonField("en")>]
    english : string
    [<JsonField("ja")>]
    japanese : string
}

type PronunciationAudioMetadata = { gender : string
                                    source_id : int64
                                    pronunciation : string
                                    voice_actor_id : int64
                                    voice_actor_name : string
                                    voice_description : string }
type PronunciationAudio = { url : string
                            metadata : PronunciationAudioMetadata
                            content_type : string  }

type DataData = { created_at : string
                  level : int64
                  slug : string
                  hidden_at :  string option // todo: what is this for?
                  document_url : string
                  characters : string 
                  meanings : Meaning list
                  auxiliary_meanings : AuxiliaryMeanings list
                  readings : Reading list
                  parts_of_speech : string list
                  component_subject_ids : int64 list
                  meaning_mnemonic : string
                  reading_mnemonic : string
                  context_sentences : ContextSentence list
                  pronunciation_audios : PronunciationAudio list
                  lesson_position : int64 }

type Data = { id : int64
              object : string
              url : string
              data_updated_at : string
              data : DataData 
              }

type WaniKaniResult = { object : string
                        url : string
                        pages : Pages
                        total_count : int64
                        data_updated_at : string
                        data : Data list }
                        
                        
let dataDataGetMeaningsString (data : DataData)  =
    let result = data.meanings
                 |> List.fold (fun acc x -> acc + "/" + x.meaning) ""
    result.Substring(1)
    
          
let dataDataGetReadingsString (data : DataData) =
    let result = data.readings
                 |> List.fold (fun acc x -> acc + "/" + x.reading) ""
    result.Substring(1)
              
let dataGetCsvRow (data: DataData) =
    (dataDataGetMeaningsString data) + ";" +
    data.characters + ";" +
    (dataDataGetReadingsString data) + ";" +
    data.level.ToString()
                        
let resultToCsv result =
    result.data
    |> List.map (fun data -> dataGetCsvRow data.data)
    |> List.fold (fun acc x -> acc + "\n" + x) ""

// Run the HTTP web request
let getResult apiKey url =
    Http.RequestString
                ( url,
                httpMethod = "GET",
                query   = [ "types", "vocabulary"], // todo: move to command line arguments
                headers = [ "Authorization", "Bearer " + apiKey ] )
                
let initialUrl endpoint =
    "https://api.wanikani.com/v2/" + endpoint

let writeToFile content path =
    File.WriteAllText (path, content, Encoding.UTF8)


type DownloadArguments = { apiKey : string
                           endpoint : string
                           targetPath : string }



type Arguments = Download of DownloadArguments
               | Unknown

let isDownloading args =
    args
    |> Seq.contains "download"
    
let nextIndex i = i + 1
 
let getValue (key: string) (args: string []) =
    let index = args
                |> Seq.findIndex (fun x -> x.StartsWith(key))
                |> nextIndex
                
    args.[index]
           
 
let parseArguments args =
    if isDownloading args then
        let apiKey = getValue "--apiKey" args
        let endpoint = getValue "--endpoint" args
        let targetPath = (getValue "--targetPath" args)
        if Directory.Exists(targetPath) then
            let dla = { apiKey = apiKey; endpoint = endpoint; targetPath = targetPath }
            Download dla
        else
            Unknown
    else
        Unknown
        
/// Custom operator for combining paths
let (+/) path1 path2 = Path.Combine(path1, path2)
        
let getPath path time=
    path +/ time 
    
let jsonPath path time =
    getPath path (time + ".json")
    
let csvPath path time =
    getPath path (time + ".csv")
    
let validTimePath (time: string) =
    time.Replace(':', '_').Replace('.', '_')
    
let rec downloadAll apiKey url (data : Data list) =
    let json = getResult apiKey url
    let result = Json.deserialize<WaniKaniResult> json
    match result.pages.next_url with
    | Some url ->
        downloadAll apiKey url (data |> List.append result.data)
    | None ->
        {result with data = (data |> List.append result.data)}
     

[<EntryPoint>]
let main argv =
    match parseArguments argv with
    | Download d ->
        printfn "Downloading"
        let result = downloadAll d.apiKey (initialUrl d.endpoint) []
        let json = Json.serialize result
        let csv = resultToCsv result
        let date = validTimePath result.data_updated_at
        writeToFile json (jsonPath d.targetPath date)
        writeToFile csv (csvPath d.targetPath date)
        printfn "Finished"
    | Unknown -> printfn "unknown"
    0 // return an integer exit code
