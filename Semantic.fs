module dandeiion.Semantic

open System.Collections.Generic

type ValueName = string
type InnerType = string

[<Struct>]
type Constant = {
    name: string
    innerType: InnerType
}

[<Struct>]
type Value = {
    innerName: ValueName
    innerType: InnerType
    allocated: bool
}

type ValueBlockState = {
        values: Dictionary<ValueName, Value>
        innerValuesName: HashSet<ValueName>
        labels: HashSet<string>
        lastRegisterNumber: uint64
        parent: Option<ValueBlockState>
    } with
        member this.setRegister newRegister =
            let parent =
                this.parent
                |> Option.map(fun p -> p.setRegister newRegister)
            { this with  lastRegisterNumber = newRegister; parent = parent }

        member this.incRegister() =
            { this with lastRegisterNumber = this.lastRegisterNumber + 1UL }
            
        member this.setInnerName innerName =
            this.innerValuesName.Add(innerName) |> ignore
            let parent =
                this.parent
                |> Option.map(fun p -> p.setInnerName innerName)
            { this with parent = parent }
            
        member this.getValueName valueName =
            if this.values.ContainsKey valueName then
                Some(this.values[valueName])
            else if this.parent.IsSome then
                match this.parent with
                | Some parent -> parent.getValueName valueName
                | None -> None
            else
                None
                
        member this.getSetNextLabel label =
            if this.labels.Contains label then
                this.labels.Add label |> ignore
                label
            else
                let labelAttr = label.Split "."
                let name =
                    if labelAttr.Length = 2 then
                        let count = System.UInt64.Parse labelAttr[1]
                        $"{labelAttr[0]}.{count + 1UL}"
                    else
                        $"{labelAttr[0]}.0"
                    
                if this.labels.Contains name then
                    this.getSetNextLabel name
                else
                    this.labels.Add name |> ignore
                    name
                
            
let initValueBlockState parent : ValueBlockState =
    let lastRegisterNumber =
        parent
        |> Option.map(fun p -> p.lastRegisterNumber)
        |> Option.defaultValue 0UL
        
    let innerValuesName = 
        parent
        |> Option.map(fun p -> p.innerValuesName)
        |> Option.defaultValue(HashSet())
        
    let labels = 
        parent
        |> Option.map(fun p -> p.labels)
        |> Option.defaultValue(HashSet())
            
    {
        values              = Dictionary()
        innerValuesName   = innerValuesName
        labels              = labels
        lastRegisterNumber = lastRegisterNumber
        parent              = parent
    }