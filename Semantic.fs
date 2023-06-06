module dandeiion.Semantic

open Codegen
open System.Collections.Generic

type ValueName = string
type  InnerType = string

type StateErrorKind = 
    | ConstantAlreadyExist
    | TypeAlreadyExist
    | FunctionAlreadyExist
    | ValueNotFound
    | FunctionNotFound
    | ReturnNotFound
    

type StateErrorLocation = {
    line: uint64
    column: uint64
}

type StateErrorResult = {
    kind: StateErrorKind
    value: string
    location: StateErrorLocation
}
type StateResult<'T> = Result<'T, StateErrorResult>
type StateResults<'T> = Result<'T, StateErrorResult[]>

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
            Some this.values[valueName]
        else
            match this.parent with
            | Some parent -> parent.getValueName valueName
            | Option.None -> Option.None
            
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
                
    member this.getNextInnerName(name: string): string =
        let nameAttr = name.Split "."
        let newName =
            if nameAttr.Length = 2 then
                let count = System.UInt64.Parse nameAttr[1]
                $"{nameAttr[0]}.{count + 1UL}"
            else
                $"{nameAttr[0]}.0"
        if this.innerValuesName.Contains newName then
            this.getNextInnerName newName
        else
            newName           
            
let initValueBlockState parent : ValueBlockState =
    let lastRegisterNumber, innerValuesName, labels =
        parent
        |> Option.map(fun p ->
            (
                p.lastRegisterNumber,
                p.innerValuesName,
                p.labels
            ))
        |> Option.defaultValue (0UL, HashSet(), HashSet())
            
    {
        values = Dictionary()
        innerValuesName= innerValuesName
        labels = labels
        lastRegisterNumber = lastRegisterNumber
        parent = parent
    }
    
type Function = {
    innerName: string
    innerType: InnerType
    parameters: InnerType[]
}

type GlobalState = {
    constants: Dictionary<string, Constant>
    types: HashSet<InnerType>
    functions: Dictionary<string, Function>
}

type State = {
    globalState: GlobalState
    codegen: ICodegen
} with
    member this.types(data: Ast.StructTypes) =
        if this.globalState.types.Contains data.getName then
            Error  {
                kind = StateErrorKind.TypeAlreadyExist
                value = ""
                location = { line = 0UL; column = 0UL } 
            }
        else
            this.globalState.types.Add data.getName |> ignore
            this.codegen.setType data
            Ok ()
            
    member this.constant(data: Ast.Constant) =
        if this.globalState.constants.ContainsKey data.getName then
            Error  {
                kind = StateErrorKind.ConstantAlreadyExist
                value = ""
                location = { line = 0UL; column = 0UL } 
            }
        else
            this.globalState.constants.Add(data.getName, { name  = data.getName; innerType = data.constant_type.getName })
            this.codegen.setConstant data
            Ok ()

    member this.functions(data: Ast.FunctionStatement) =
        if this.globalState.functions.ContainsKey data.getName then
            Error  {
                kind = StateErrorKind.FunctionAlreadyExist
                value = ""
                location = { line = 0UL; column = 0UL } 
            }
        else
            this.globalState.functions.Add(data.getName, {
                innerName  = data.getName
                innerType = data.resultType.getName
                parameters = Array.map (fun (p: Ast.FunctionParameter) -> p.parameterType.getName) data.parameters
            })
            this.codegen.setFunction data
            Ok ()